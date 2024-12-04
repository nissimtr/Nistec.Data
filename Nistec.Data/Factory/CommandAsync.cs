//licHeader
//===============================================================================================================
// System  : Nistec.Data - Nistec.Data Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of data library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Threading;
using System.Data.Common;
using Nistec.Data;
#pragma warning disable CS1591
namespace Nistec.Data.Factory
{


    #region summary
    ///<remarks>
    ///<example>
    /// private delegate void AsyncFillDelegate(IDataReader reader);
    /// private delegate void AsyncStatusDelegate(string Text);
    /// protected void AsyncHandleCallback(IAsyncResult result)
    /// {
    ///    try
    ///    {
    ///         AsyncFillDelegate del = new AsyncFillDelegate(dalAsync.AsyncFillDataSource);
    ///         this.Invoke(del, dalAsync.AsyncExecuteEnd(result));
    ///
    ///          // Do not close the reader here, because it is being used in 
    ///         // a separate thread. Instead, have the procedure you have
    ///         // called close the reader once it is done with it.
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         this.Invoke(new AsyncStatusDelegate(dalAsync.AsyncStatus), "Error: " + ex.Message);
    ///   }
    ///}
    /// protected override bool Initialize()
    ///{
    ///    dalAsync = new CommandAsync(ConnectionString);
    ///    dalAsync.AsyncCompleted += new EventHandler(Async_AsyncCompleted);
    ///    dalAsync.AsyncExecuteBegin(new AsyncCallback(AsyncHandleCallback));
    ///}
    ///
    ///void Async_AsyncCompleted(object sender, EventArgs e)
    ///{
    ///    DataTable dt = dalAsync.AsyncResult_DataTable();
    ///    dt.TableName = "Accounts";
    ///    this.ctlNavBar.Init(dt);
    ///}
    ///
    /// </example>
    ///</remarks>
    /// 
    /// <summary>
    /// Use CommandAsync to invoke Asynchronous Processing
    /// you can use 2 ways AsyncExecute or AsyncExecuteBegin and AsyncExecuteEnd
    /// for win form you have simple way to accomplish this is to call the Invoke
    /// method of the form, which calls the delegate you supply
    /// from the form's thread. 
    /// </summary>
    #endregion

    public abstract  class CommandAsync : ICommandAsync,IDisposable
    {

         /// <summary>
        /// Factory Create Async provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ICommandAsync Factory(string provider, string connectionString)
        {

            if (provider.ToLower().IndexOf("sql") > -1)
            {
                return (ICommandAsync)new SqlClient.CommandAsync(connectionString);
            }
            else if (provider.ToLower().IndexOf("ole") > -1)
            {
                return (ICommandAsync)new OleDb.CommandAsync(connectionString);
            }
            else
            {
                throw new Data.DalException("Provider not supported " + provider);
            }
        }

        /// <summary>
        /// Factory Create Async provider
        /// </summary>
        /// <param name="provider">DBProvider</param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ICommandAsync Factory(DBProvider provider, string connectionString)
        {
            if (provider == DBProvider.SqlServer)
            {
                return (ICommandAsync)new SqlClient.CommandAsync(connectionString);
            }
            else if (provider == DBProvider.OleDb)
            {
                return (ICommandAsync)new OleDb.CommandAsync(connectionString);
            }
            else
            {
                throw new Data.DalException("Provider not supported " + provider);
            }
        }

       #region IDisposable implementation
		
		/// <summary>
		/// Disposed flag.
		/// </summary>
		protected bool m_disposed = false;
  
		/// <summary>
		/// Implementation of method of IDisposable interface.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose method with a boolean parameter indicating the source of calling.
		/// </summary>
        /// <param name="disposing">Indicates from whare the method is called.</param>
        protected virtual void Dispose(bool disposing)
		{
			if(!m_disposed)
			{
                if (disposing)
				{
					InnerDispose();
				}
				m_disposed = true;
            }
		}

		/// <summary>
		/// Inner implementation of Dispose method.
		/// </summary>
        protected virtual void InnerDispose()
		{

		}

		/// <summary>
		/// Class destructor.
		/// </summary>
        ~CommandAsync()
		{
			Dispose(false);
		}

		#endregion

        /// <summary>
        /// Provider
        /// </summary>
        internal DBProvider Provider;
        /// <summary>
        /// Status
        /// </summary>
        internal STATUS Status;

        /// <summary>
        /// GetParameters factory
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public IDataParameter[] GetParameters(string[] names,object[] values)
        {
            if (names.Length != values.Length)
            {
                throw new ArgumentException("names ans values should be equal");
            }
            if (Provider == DBProvider.SqlServer)
            {
                SqlParameter[] sqlParam = new SqlParameter[names.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    sqlParam[i] = new SqlParameter(names[i], values[i]);
                }

                return (IDataParameter[])sqlParam;
            }
            else if (Provider == DBProvider.OleDb)
            {
                OleDbParameter[] oleParam = new OleDbParameter[names.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    oleParam[i] = new OleDbParameter(names[i], values[i]);
                }

                return (IDataParameter[])oleParam;
            }
            return null;

        }
        /// <summary>
        /// CommandAsync ctor
        /// </summary>
        public CommandAsync()
        {
            Status = new STATUS("Loading");
        }

 
        /// <summary>
        /// Inner class member representing OleDbConnection string
        /// </summary>
        protected string m_connectionString = null;

   
        #region Async


        /// <summary>
        /// AsyncCompleted event occured when executing complited
        /// </summary>
        public event EventHandler AsyncCompleted;
        /// <summary>
        /// AsyncStatusChanged event occured when status is changed
        /// </summary>
        public event EventHandler AsyncStatusChanged;

        /// <summary>
        /// _AsyncIsExecuting
        /// </summary>
        internal bool _AsyncIsExecuting;
        /// <summary>
        /// _AsyncDataSource
        /// </summary>
        internal DataTable _AsyncDataSource;
       

        /// <summary>
        /// This flag ensures that the user does not attempt
        /// to restart the command or close the form while the 
        /// asynchronous command is executing.
        /// </summary>
        public bool AsyncIsExecuting
        {
            get { return _AsyncIsExecuting; }
        }
        /// <summary>
        /// Get Async Display Status
        /// </summary>
        public string AsyncDisplayStatus
        {
            get { return Status.displayStatus;/* _AsyncDisplayStatus;*/ }
        }
        /// <summary>
        /// Get Async Display Status
        /// </summary>
        public STATUS AsyncStatus
        {
            get { return Status; }
        }


        /// <summary>
        /// Get AsyncResult as DataTable
        /// </summary>
        public DataTable AsyncResult_DataTable
        {
            get{ return _AsyncDataSource;}
        }

        /// <summary>
        /// Get AsyncResult as DataRows array
        /// </summary>
        public DataRow[] AsyncResult_DataRows
        {
            get{return _AsyncDataSource==null ? null: _AsyncDataSource.Select();}
        }

        /// <summary>
        /// Get AsyncResult as SingleRow,if multi rows affected return the first row
        /// </summary>
        public DataRow AsyncResult_SingleRow
        {
            get
            {
                if (_AsyncDataSource == null)
                    return null;
                DataRow[] drs = _AsyncDataSource.Select();
                if (drs.Length > 0)
                    return drs[0];
                return null;
            }
        }

        /// <summary>
        /// SetAsyncStatus
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="priority"></param>
        public virtual void SetAsyncStatus(string Text, StatusPriority priority)
        {
            Status.displayStatus = Text;
            Status.statusPriority = priority;
            OnAsyncStatusChanged(EventArgs.Empty);
        }

        /// <summary>
        /// OnAsyncCompleted occured when executing complited
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAsyncCompleted(EventArgs e)
        {
            if (AsyncCompleted != null)
                AsyncCompleted(this, e);
        }
        /// <summary>
        /// OnAsyncStatusChanged occured when status changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAsyncStatusChanged(EventArgs e)
        {
            if (AsyncStatusChanged != null)
                AsyncStatusChanged(this, e);
        }


        /// <summary>
        /// AsyncFillDataSource
        /// You must Use this method as a target of invokation delegate
        /// to complit AsyncExecution ,
        /// fill the data source you need
        /// and close the IDaqtaReader as well.
        /// </summary>
        /// <param name="reader"></param>
        public void AsyncFillDataSource(IDataReader reader)
        {
            try
            {
                _AsyncDataSource = new DataTable();
                _AsyncDataSource.Load(reader);
                SetAsyncStatus("Ready", StatusPriority.Normal);
                OnAsyncCompleted(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Because you are guaranteed this procedure
                // is running from within the form's thread,
                // it can directly interact with members of the form.
                SetAsyncStatus(string.Format("Ready (last attempt failed: {0})", ex.Message), StatusPriority.Error);
            }
            finally
            {
                // Closing the reader also closes the connection,
                // because this reader was created using the 
                // CommandBehavior.CloseConnection value.
                if (reader != null)
                {
                    reader.Close();
                }
                _AsyncIsExecuting = false;
            }
        }

        /// <summary>
        /// FillDataSourceSchema
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="tableName"></param>
        public void FillDataSourceSchema(IDataReader reader, string tableName)
        {
            _AsyncDataSource = DataUtil.GetTableSchema(reader, tableName);
        }

      
        /// <summary>
        /// AsyncExecuteBegin , use this method to begin invoke 
        /// Async procedure call, doing so makes it easier
        /// to call AsyncExecuteEnd in the callback procedure.
        /// </summary>
        /// <param name="callback">AsyncCallback</param>
        /// <param name="sql"></param>
        /// <param name="parameters">parameters </param>
        /// <param name="timeOut">Set time out, default =0</param>
        /// <param name="WaitForDelay">a few seconds before retrieving the real data use for a long-running query, default =0</param>
        public abstract void AsyncExecuteBegin(AsyncCallback callback, string sql, IDataParameter[] parameters, int timeOut, int WaitForDelay);

        /// <summary>
        /// AsyncHandleCallback
        /// Retrieve the original command object, passed
        /// to this procedure in the AsyncState property
        /// of the IAsyncResult parameter.
        /// </summary>
        /// <param name="result"></param>
        public abstract IDataReader AsyncExecuteEnd(IAsyncResult result);

        /// <summary>
        /// AsyncExecute , use this method to invoke 
        /// Async procedure call until is IsCompleted 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">parameters </param>
        /// <param name="timeOut">Set time out, default =0</param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        /// <param name="WaitForDelay">a few seconds before retrieving the real data use for a long-running query, default =0</param>
        public abstract void AsyncExecute(string sql, IDataParameter[] parameters, int interval, int timeOut, int WaitForDelay);
 
       /// <summary>
        /// AsyncExecute , use this method to invoke 
        /// Async procedure call until is IsCompleted 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">parameters </param>
        /// <param name="timeOut">Set time out, default =0</param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        public abstract void Execute(string sql, IDataParameter[] parameters, int interval, int timeOut);
 
        #endregion

    }
}
