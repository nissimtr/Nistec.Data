﻿//licHeader
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
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Threading;
using System.Runtime.Remoting.Messaging;

using Nistec.Threading;
using Nistec.Data.Factory;
using Nistec.Generic;

#pragma warning disable CS1591
namespace Nistec.Data.Advanced
{


    /// <example>
    ///    public void RunCmd(){
    ///    mControl.Data.AsyncCommand acmd1 = new mControl.Data.AsyncCommand(connectionString, mControl.Data.DBProvider.SqlServer);
    ///    acmd1.AsyncProgressLevel = mControl.Data.AsyncProgressLevel.All;
    ///    acmd1.ExecutingResultEvent += new mControl.Data.ExecutingResultEventHandler(acmd1_ExecutingResultEvent);
    ///    acmd1.ExecutingTraceEvent +=new mControl.Data.ExecutingTraceEventHandler(acmd1_ExecutingTraceEvent);
    ///    acmd1.RunCmdScript("select * from Accounts", "Accounts");
    ///    }
    ///    void acmd1_ExecutingTraceEvent(object sender, mControl.Data.ExecutingTraceEventArgs e)
    ///    {
    ///        string s = e.Message;
    ///    }
    ///
    ///    void acmd1_ExecutingResultEvent(object sender, mControl.Data.ExecutingResultEventArgs e)
    ///    {
    ///        DataTable d = e.Table;
    ///   }          
    /// </example>

    /// <summary>
    /// AsyncCommand
    /// </summary>
    public class AsyncReader : IDisposable//:Component
    {
        #region members
        protected Nistec.Threading.ThreadTimer ExecutionTimer;

        public event EventHandler StartProgressEvent;
        public event EventHandler StopProgressEvent;
        public event EventHandler AsyncCancelExecuting;
        public event AsyncDataResultEventHandler AsyncCompleted;
        public event AsyncProgressEventHandler AsyncProgress;

        private IDbCmd dbcmd;
        private AsyncProgressLevel _MessageLevel;
        private IDbConnection connection;
        CommandBehavior _CommandBehavior;
        private bool _AutoCloseConnection;
        #endregion

        #region ctor

        public AsyncReader(IDbConnection cnn)
            : this(cnn, AsyncProgressLevel.None)
        {
        }
        public AsyncReader(string cnn, DBProvider provider)
        {
            _AutoCloseConnection = true;
            _CommandBehavior = CommandBehavior.Default;
            _currentAsyncState = AsyncState.None;
            _MessageLevel = AsyncProgressLevel.None;
            dbcmd = DbFactory.Create(cnn, provider);
            connection = dbcmd.Connection;
            InitializeComponent();

        }
        public AsyncReader(IDbConnection cnn, AsyncProgressLevel level)
        {
            _AutoCloseConnection = true;
            _CommandBehavior = CommandBehavior.Default;
            _currentAsyncState = AsyncState.None;
            _MessageLevel = level;
            connection = cnn;
            dbcmd = DbFactory.Create(cnn);
            InitializeComponent();
        }
        public AsyncReader(IAutoBase dal, AsyncProgressLevel level)
        {
            _AutoCloseConnection = true;
            _CommandBehavior = CommandBehavior.Default;
            _currentAsyncState = AsyncState.None;
            _MessageLevel = level;
            connection = dal.Connection;
            dbcmd = DbFactory.Create(connection);
            InitializeComponent();
        }

        public virtual void Dispose()//bool disposing)
        {
            if (_currentCommand != null)
            {
                _currentCommand.Dispose();
                _currentCommand = null;
            }

            //base.Dispose(disposing);
        }

        #endregion

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExecutionTimer = new Nistec.Threading.ThreadTimer();// (this.components);
            // 
            // ExecutionTimer
            // 
            this.ExecutionTimer.Interval = 500;
            this.ExecutionTimer.Elapsed += new System.Timers.ElapsedEventHandler(ExecutionTimer_Elapsed);
        }

        #endregion

        #region current memebers

        protected DataTable _currentDataTable = null;
        protected string _currentMappingName = null;
        protected DateTime _currentExecutionStart;
        protected DateTime _currentExecutionEnd;
        protected TimeSpan _currentExecutionTime;
        protected IAsyncResult _asyncResult;
        protected Exception _currentException;
        protected IDbCommand _currentCommand;
        protected string _currentTextScript;
        protected string _currentTime;
        protected string _currentMessage;
        protected AsyncState _currentAsyncState;
        protected IDataReader _currentReader = null;

        private int currentCounter = 0;
        private bool stopLoading = false;
        private int fastFirstRows = 0;
        private int maxRows = 0;
        const int loadingRowBatch = 100;
        public event AsyncDataResultEventHandler FastFirstRowsEvent;
        public event AsyncDataResultEventHandler LoadingRowBatchEvent;
        public event GenericEventHandler<string> ErrorOcurred;

        private delegate IDataReader RunAsyncCallDelegate(IDbCommand dbCommand, string cmdName);

        #endregion

        #region properties

        public bool AutoCloseConnection
        {
            get { return _AutoCloseConnection; }
            set { _AutoCloseConnection = value; }
        }

        public CommandBehavior CommandBehavior
        {
            get { return _CommandBehavior; }
            set { _CommandBehavior = value; }
        }

        public AsyncProgressLevel AsyncProgressLevel
        {
            get { return _MessageLevel; }
            set { _MessageLevel = value; }
        }
        public AsyncState AsyncState
        {
            get { return _currentAsyncState; }
        }
        public IAsyncResult AsyncResult
        {
            get { return _asyncResult; }
        }
        public string CurrentTime
        {
            get { return _currentTime; }
        }
        public DataTable AsyncDataSource
        {
            get { return _currentDataTable; }
        }
        public Exception CurrentException
        {
            get { return _currentException; }
        }
        public IDbCommand CurrentCommand
        {
            get { return _currentCommand; }
        }
        public string CurrentMessage
        {
            get { return _currentMessage; }
        }
        public IDataReader CurrentReader
        {
            get { return _currentReader; }
        }

        public int CurrentCounter
        {
            get { return currentCounter; }
        }
        public bool StopLoading
        {
            get { return stopLoading; }
        }
        public int FastFirstRows
        {
            get { return fastFirstRows; }
        }
        public int MaxRows
        {
            get { return maxRows; }
        }


        #endregion

        #region public methods

        public void AsyncBeginInvoke(string cmdText, string tableName)
        {
            try
            {
                if (string.IsNullOrEmpty(cmdText))
                {
                    throw new ArgumentException("Invalid command");
                }

                _currentTextScript = cmdText;
                IDbCommand cmd = DbFactory.CreateCommand(cmdText, connection);// dbcmd.GetCommand(command);
                AsyncBeginInvoke(cmd, tableName);
            }
            catch (Exception ex)
            {
                ExecutingTrace(ex.Message, AsyncProgressLevel.Error);
            }
        }

        public void AsyncBeginInvoke(IDbCommand dbCommand, string tableName)
        {
            try
            {
                if (dbCommand == null)
                {
                    throw new ArgumentException("Invalid command");
                }
                if (_CommandBehavior == CommandBehavior.CloseConnection)
                {
                    _AutoCloseConnection = false;
                }
                _currentAsyncState = AsyncState.Started;
                _currentCommand = dbCommand;
                _currentException = null;
                _currentExecutionStart = DateTime.Now;
                _currentTime = "00:00:00";
                OnStartProgress(EventArgs.Empty);

                _currentMappingName = tableName;
                _currentTextScript = dbCommand.CommandText;
                ExecutingTrace("Executing command " + _currentTextScript, AsyncProgressLevel.Info);

                DateTime dt = DateTime.Now;
                RunAsyncCallDelegate msc = new RunAsyncCallDelegate(RunAsyncCall);
                AsyncCallback cb = new AsyncCallback(RunAsyncCallback);

                if (dbCommand.Connection.State == ConnectionState.Closed)
                {
                    dbCommand.Connection.Open();
                }

                _asyncResult = msc.BeginInvoke(dbCommand, tableName, cb, null);
                ExecutionTimer.Enabled = true;
                OnExecuting();
            }
            catch (Exception ex)
            {
                OnStopProgress(EventArgs.Empty);
                ExecutingTrace(ex.Message, AsyncProgressLevel.Error);
            }
        }

        public void StopCurrentExecution()
        {

            ExecutingTrace("Stop Executing ", AsyncProgressLevel.Info);

            try
            {
                OnStopProgress(EventArgs.Empty);
                ExecutionTimer.Enabled = false;
                ExecutingTrace("Execution terminated.", AsyncProgressLevel.Info);
                _currentCommand.Cancel();
            }
            catch
            {
                ExecutingTrace("Unable to stop execution", AsyncProgressLevel.Error);
            }
            _currentAsyncState = AsyncState.Canceled;
            OnCancelExecuting(EventArgs.Empty);

        }
        #endregion

        #region private methods

        private void RunAsyncCallback(IAsyncResult ar)
        {
            Thread th = Thread.CurrentThread;
            RunAsyncCallDelegate msc = (RunAsyncCallDelegate)((AsyncResult)ar).AsyncDelegate;
            _currentReader = msc.EndInvoke(ar);
        }

        private IDataReader RunAsyncCall(IDbCommand dbCommand, string tableName)
        {
            try
            {
                Thread th = Thread.CurrentThread;
                _currentDataTable = new DataTable();
                _currentDataTable.TableName = _currentMappingName;

                IDataReader reader = null;
                reader = dbCommand.ExecuteReader(_CommandBehavior);
                ExecuteReader(reader);
                return reader;
            }
            catch (Exception ex)
            {
                _currentException = ex;
                OnErrorOcurred(new GenericEventArgs<string>(ex.Message));
                return null;
            }
        }

        private void ExecuteReader(IDataReader reader)
        {
            bool hasTableEndLoading = false;
            stopLoading = false;
            try
            {

                if (fastFirstRows == 0)
                {
                    _currentDataTable.Load(reader, LoadOption.Upsert);
                }
                else
                {
                    currentCounter = 0;
                    hasTableEndLoading = true;
                    int fieldsCount = reader.FieldCount;
                    object[] values = new object[fieldsCount];
                    _currentDataTable.BeginLoadData();

                    while (!stopLoading && reader.Read() && ((currentCounter < maxRows) || (maxRows == 0)))
                    {
                        values.Initialize();
                        for (int i = 0; i < fieldsCount; i++)
                        {
                            values[i] = (object)reader[i];
                        }

                        _currentDataTable.LoadDataRow(values, LoadOption.Upsert);
                        if (currentCounter == fastFirstRows)
                        {
                            OnFastFirstRows(new AsyncDataResultEventArgs(_currentDataTable));//EventArgs.Empty);
                        }
                        if (currentCounter > fastFirstRows && currentCounter % loadingRowBatch == 0)
                        {
                            OnLoadingRowBatch(new AsyncDataResultEventArgs(_currentDataTable));//EventArgs.Empty);
                        }
                        currentCounter++;
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>(ex.Message));
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>(ex.Message));
            }
            finally
            {
                try
                {
                    if (hasTableEndLoading)
                    {
                        _currentDataTable.EndLoadData();
                    }
                    if (reader != null && !reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
                catch (Exception e)
                {
                    OnErrorOcurred(new GenericEventArgs<string>(e.Message));
                }

            }
            //return tbl;
        }

        void ExecutionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OnExecutionTimerTick(e);
        }

        protected virtual void OnExecutionTimerTick(System.Timers.ElapsedEventArgs e)
        {

            DateTime dtn = DateTime.Now;
            TimeSpan ts = dtn.Subtract(_currentExecutionStart);
            _currentTime = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
            ExecutingTrace(_currentTime, AsyncProgressLevel.Progress);
            OnExecuting();

            if (_asyncResult.IsCompleted)
            {
                ExecutionTimer.Enabled = false;
                _currentExecutionEnd = DateTime.Now;
                _currentExecutionTime = _currentExecutionEnd.Subtract(_currentExecutionStart);
                ExecutionResult(_currentExecutionTime);//_currentDataTable,
            }
        }

        protected virtual void ExecutionResult(TimeSpan _currentExecutionTime)
        {
            try
            {
                OnExecuting();

                if (_currentException != null)
                    throw _currentException;

                string msgTrace = string.Format("Execute Data: {0} \r\n ExecutionTime: {1} Rows Found: {2}  \r\n", _currentDataTable.TableName, _currentExecutionTime.TotalMilliseconds, _currentDataTable.Rows.Count);
                ExecutingTrace(msgTrace, AsyncProgressLevel.Info);

            }
            catch (Exception ex)
            {
                ExecutingTrace(ex.Message, AsyncProgressLevel.Error);
            }
            finally
            {
                ExecutionTimer.Enabled = false;
                OnStopProgress(EventArgs.Empty);
                OnAsyncCompleted(new AsyncDataResultEventArgs(_currentDataTable));
            }

        }
        #endregion

        #region override

        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, e);
        }
        protected virtual void OnFastFirstRows(AsyncDataResultEventArgs e)
        {
            if (FastFirstRowsEvent != null)
                FastFirstRowsEvent(this, e);
        }

        protected virtual void OnLoadingRowBatch(AsyncDataResultEventArgs e)
        {
            if (LoadingRowBatchEvent != null)
                LoadingRowBatchEvent(this, e);
        }


        protected virtual void OnCancelExecuting(EventArgs e)
        {
            if (AsyncCancelExecuting != null)
                AsyncCancelExecuting(this, e);
        }

        protected virtual void OnStartProgress(EventArgs e)
        {
            if (StartProgressEvent != null)
                StartProgressEvent(this, e);
        }
        protected virtual void OnStopProgress(EventArgs e)
        {
            if (StopProgressEvent != null)
                StopProgressEvent(this, e);
        }
        protected virtual void OnExecuting()
        {

        }
        private void ExecutingTrace(string s, AsyncProgressLevel lvl)
        {
            if (lvl == AsyncProgressLevel.Progress)
            {
                _currentTime = s;
                if (AsyncProgress != null && (_MessageLevel == AsyncProgressLevel.Progress || _MessageLevel == AsyncProgressLevel.All))
                    OnAsyncProgress(new AsyncProgressEventArgs(s, lvl));
            }
            else if (lvl == AsyncProgressLevel.Info)
            {
                _currentMessage = s;
                if (AsyncProgress != null && (_MessageLevel == AsyncProgressLevel.Info || _MessageLevel == AsyncProgressLevel.All))
                    OnAsyncProgress(new AsyncProgressEventArgs(s, lvl));
            }
            else if (lvl == AsyncProgressLevel.Error)
            {
                _currentMessage = s;
                _currentAsyncState = AsyncState.Canceled;
                if (AsyncProgress != null && (_MessageLevel == AsyncProgressLevel.Error || _MessageLevel == AsyncProgressLevel.All))
                    OnAsyncProgress(new AsyncProgressEventArgs(s, lvl));
            }
        }

        protected virtual void OnAsyncProgress(AsyncProgressEventArgs e)
        {
            if (AsyncProgress != null)
                AsyncProgress(this, e);
        }

        protected virtual void OnAsyncCompleted(AsyncDataResultEventArgs e)
        {
            _currentAsyncState = AsyncState.Completed;

            if (_AutoCloseConnection && connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            if (AsyncCompleted != null)
                AsyncCompleted(this, e);
        }

        #endregion

    }
}

