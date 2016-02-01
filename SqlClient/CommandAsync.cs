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
using System.Threading;
using Nistec.Data;
using Nistec.Generic;

namespace Nistec.Data.SqlClient
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
    /// </remarks>
    /// <summary>
    /// Use CommandAsync to invoke Asynchronous Processing
    /// you can use 2 ways AsyncExecute or AsyncExecuteBegin and AsyncExecuteEnd
    /// for win form you have simple way to accomplish this is to call the Invoke
    /// method of the form, which calls the delegate you supply
    /// from the form's thread. 
    /// </summary>
    #endregion

    public class CommandAsync : Nistec.Data.Factory.CommandAsync
    {
        /// <summary>
        /// InnerDispose
        /// </summary>
        protected override void InnerDispose()
        {
            base.InnerDispose();
            if (m_AsyncConnection != null)
            {
                if ((m_AsyncConnection.State != ConnectionState.Closed))// && m_ownsConnection) 
                {
                    try
                    {
                        m_AsyncConnection.Close();
                    }
                    catch { }
                }
            }
            m_AsyncConnection = null;
        }

        /// <summary>
        /// Async Connection object.
        /// </summary>
        protected SqlConnection m_AsyncConnection = null;
       
        /// <summary>
        /// CommandAsync Ctor
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="mode"></param>
        internal CommandAsync(string connection, string mode)
        {
            if (connection.Contains("Asynchronous Processing"))
            {
                m_connectionString = connection;
            }
            else
            {
                m_connectionString = string.Format("{0};Asynchronous Processing=true", connection);
            }

            m_AsyncConnection = new SqlConnection(m_connectionString);
            Provider = DBProvider.SqlServer;
        }

        /// <summary>
        /// CommandAsync Ctor
        /// </summary>
        /// <param name="connection"></param>
        public CommandAsync(string connection)
        {

            if (connection.Contains("Asynchronous Processing"))
            {
                m_connectionString = connection;
            }
            else
            {
                m_connectionString = string.Format("{0};Asynchronous Processing=true", connection);
            }

            m_AsyncConnection = new SqlConnection(m_connectionString);
            Provider = DBProvider.SqlServer;
        }

    
        #region Async

        private void ConnectionAsyncOpen(SqlCommand cmd)
        {
            cmd.Connection = m_AsyncConnection;
            if (m_AsyncConnection.State == ConnectionState.Closed)
            {
                cmd.Connection.Open();
            }
        }

 
        /// <summary>
        /// OnAsyncCompleted occured when executing complited
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAsyncCompleted(EventArgs e)
        {
            base.OnAsyncCompleted(e);
        }
        /// <summary>
        /// OnAsyncStatusChanged occured when status changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAsyncStatusChanged(EventArgs e)
        {
            base.OnAsyncStatusChanged(e);
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
        public override void AsyncExecuteBegin(AsyncCallback callback, string sql, IDataParameter[] parameters, int timeOut, int WaitForDelay)
        {
            if (_AsyncIsExecuting)
            {
                return;
            }
            else
            {
                SqlCommand command = null;
                try
                {
                    SetAsyncStatus("Connecting...", StatusPriority.Normal);
                    //connection = new SqlConnection(GetConnectionString());
                    // To emulate a long-running query, wait for 
                    // a few seconds before retrieving the real data.
                    //command = new SqlCommand("WAITFOR DELAY '0:0:5';" + sql, connection);
                    string cmdText = sql;
                    if (WaitForDelay > 0)
                    {
                        cmdText = cmdText.Insert(0, string.Format("WAITFOR DELAY '00:00:{0}';", WaitForDelay));
                    }
                    command = new SqlCommand(cmdText);
                    if (timeOut >= 0)
                    {
                        command.CommandTimeout = timeOut;
                    }
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    ConnectionAsyncOpen(command);

                    SetAsyncStatus("Executing...", StatusPriority.Normal);
                    _AsyncIsExecuting = true;
                    // Although it is not required that you pass the 
                    // SqlCommand object as the second parameter in the 
                    // BeginExecuteReader call, doing so makes it easier
                    // to call EndExecuteReader in the callback procedure.
                    //AsyncCallback callback = new AsyncCallback(AsyncHandleCallback);
                    command.BeginExecuteReader(callback, command,CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    SetAsyncStatus("Error: " + ex.Message, StatusPriority.Error);
                }
             }
        }

        /// <summary>
        /// AsyncHandleCallback
        /// Retrieve the original command object, passed
        /// to this procedure in the AsyncState property
        /// of the IAsyncResult parameter.
        /// </summary>
        /// <param name="result"></param>
        public override IDataReader AsyncExecuteEnd(IAsyncResult result)
        {
            try
            {
                 
                // Retrieve the original command object, passed
                // to this procedure in the AsyncState property
                // of the IAsyncResult parameter.
                SqlCommand command = (SqlCommand)result.AsyncState;
                SqlDataReader reader = command.EndExecuteReader(result);
                return reader as IDataReader;

            }
            catch (Exception ex)
            {
                SetAsyncStatus("Error: " + ex.Message, StatusPriority.Error);
                return null;
            }
            finally
            {
                _AsyncIsExecuting = false;
            }
        }

        /// <summary>
        /// AsyncExecute , use this method to invoke 
        /// Async procedure call until is IsCompleted 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">parameters </param>
        /// <param name="timeOut">Set time out, default =0</param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        /// <param name="WaitForDelay">a few seconds before retrieving the real data use for a long-running query, default =0</param>
        public override void AsyncExecute(string sql, IDataParameter[] parameters, int interval, int timeOut, int WaitForDelay)
        {
            if (_AsyncIsExecuting)
            {
                return;
            }
            else
            {
                SqlCommand command = null;
                try
                {
                    SetAsyncStatus("Connecting...", StatusPriority.Normal);
                    //connection = new SqlConnection(GetConnectionString());
                    // To emulate a long-running query, wait for 
                    // a few seconds before retrieving the real data.
                    //command = new SqlCommand("WAITFOR DELAY '0:0:5';" + sql, connection);

                    string cmdText = sql;
                    if (WaitForDelay > 0)
                    {
                        cmdText = cmdText.Insert(0, string.Format("WAITFOR DELAY '00:00:{0}';", WaitForDelay));
                    }
                    command = new SqlCommand(cmdText);
                    if (timeOut >= 0)
                    {
                        command.CommandTimeout = timeOut;
                    }
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    ConnectionAsyncOpen(command);

                    SetAsyncStatus("Executing...", StatusPriority.Normal);
                    _AsyncIsExecuting = true;

                    IAsyncResult aresult = command.BeginExecuteReader(CommandBehavior.CloseConnection);

                    while (!aresult.IsCompleted)
                    {
                        //Console.WriteLine("Waiting ({0})", count++);
                        // Wait for 1/10 second, so the counter
                        // does not consume all available resources 
                        // on the main thread.
                        System.Threading.Thread.Sleep(interval);
                    }

                    using (SqlDataReader reader = command.EndExecuteReader(aresult))
                    {
                        AsyncFillDataSource(reader);
                    }

                }
                catch (Exception ex)
                {
                    SetAsyncStatus("Error: " + ex.Message, StatusPriority.Error);
                }
                finally
                {
                    _AsyncIsExecuting = false;
                }
             }
        }

        /// <summary>
        /// AsyncExecute , use this method to invoke 
        /// Async procedure call until is IsCompleted 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">parameters </param>
        /// <param name="timeOut">Set time out, default =0</param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        public override void Execute(string sql, IDataParameter[] parameters, int interval, int timeOut)
        {
            if (_AsyncIsExecuting)
            {
                return;
            }
            else
            {
                SqlCommand command = null;
                try
                {
                    SetAsyncStatus("Connecting...", StatusPriority.Normal);
                    string cmdText = sql;
                     command = new SqlCommand(cmdText);
                    if (timeOut >= 0)
                    {
                        command.CommandTimeout = timeOut;
                    }
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    ConnectionAsyncOpen(command);

                    SetAsyncStatus("Executing...", StatusPriority.Normal);
                    _AsyncIsExecuting = true;

                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        AsyncFillDataSource(reader);
                    }

                }
                catch (Exception ex)
                {
                    SetAsyncStatus("Error: " + ex.Message, StatusPriority.Error);
                }
                finally
                {
                    _AsyncIsExecuting = false;
                }
            }
        }

        #endregion

    }
}
