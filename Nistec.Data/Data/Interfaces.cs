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
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using Nistec.Data;
using Nistec.Data.Factory;
#pragma warning disable CS1591
namespace Nistec.Data
{
     #region IAutoBase
    
    /// <summary>
    /// IAutoBase interface
    /// </summary>
    public interface IAutoBase : IDisposable
    {
  
        /// <summary>
        /// DBProvider
        /// </summary>
        DBProvider DBProvider { get;}
 
     
  
        #region Public members
        /// <summary>
        /// Get Initilaized
        /// </summary>
         bool Initilaized { get;}
         /// <summary>
         /// It true then the object owns its connection
         /// and disposes it on its own disposal.
         /// </summary>
         bool OwnsConnection { get; }

         /// <summary>
         /// If true then the object's connection is closed each time 
         /// after sql command execution.
         /// </summary>
         bool AutoCloseConnection { get; }

        /// <summary>
        /// Sql connection property.
        /// </summary>
         IDbConnection Connection { get; }

         /// <summary>
         /// Sql transaction property.
         /// </summary>
         IDbTransaction Transaction { get; }

      
         /// <summary>
         /// Begins sql transaction with a specified isolation level.
         /// </summary>
         /// <param name="iso"></param>
         /// <returns></returns>
         IDbTransaction BeginTransaction(IsolationLevel iso);

         /// <summary>
         /// Rolls back the current transaction.
         /// </summary>
         void RollbackTransaction();

         /// <summary>
         /// Commits the current transaction.
         /// </summary>
         void CommitTransaction();
 
        #endregion


     }
    #endregion

     #region IAutoDb

     /// <summary>
     /// Interface that every dal class must inherit. When DBLayer creates 
     /// a dal class it uses see cref="Common.DalDB class as its base class
     /// </summary>
     public interface IAutoDb
     {
         /// <summary>
         /// Connection property
         /// </summary>
         IDbConnection Connection { get;set;}

         /// <summary>
         /// Transaction property
         /// </summary>
         IDbTransaction Transaction { get;set;}

         /// <summary>
         /// AutoCloseConnection property
         /// </summary>
         bool AutoCloseConnection { get;set;}

     }


     #endregion

     #region IDalAsync

     /// <summary>
     /// ICommandAsync is inertace of CommandAsync to invoke Asynchronous Processing
    /// </summary>
    public interface ICommandAsync
    {
        /// <summary>
        /// AsyncCompleted event occured when executing complited
        /// </summary>
        event EventHandler AsyncCompleted;
        /// <summary>
        /// AsyncStatusChanged event occured when status is changed
        /// </summary>
        event EventHandler AsyncStatusChanged;

        /// <summary>
        /// This flag ensures that the user does not attempt
        /// to restart the command or close the form while the 
        /// asynchronous command is executing.
        /// </summary>
        bool AsyncIsExecuting { get;}
        /// <summary>
        /// Get Async Display Status
        /// </summary>
        string AsyncDisplayStatus { get;}
        /// <summary>
        /// Get Async Status struct
        /// </summary>
        STATUS AsyncStatus { get;}
        /// <summary>
        /// Get AsyncResult as DataTable
        /// </summary>
        DataTable AsyncResult_DataTable { get;}
        /// <summary>
        /// Get AsyncResult as DataRows array
        /// </summary>
        DataRow[] AsyncResult_DataRows { get;}
        /// <summary>
        /// Get AsyncResult as SingleRow,if multi rows affected return the first row
        /// </summary>
        DataRow AsyncResult_SingleRow { get;}
        /// <summary>
        /// SetAsyncStatus
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="priority"></param>
        void SetAsyncStatus(string Text, StatusPriority priority);
        /// <summary>
        /// AsyncFillDataSource
        /// You must Use this method as a target of invokation delegate
        /// to complit AsyncExecution ,
        /// fill the data source you need
        /// and close the IDaqtaReader as well.
        /// </summary>
        /// <param name="reader"></param>
        void AsyncFillDataSource(IDataReader reader);
        /// <summary>
        /// AsyncExecute , use this method to invoke 
        /// Async procedure call until is IsCompleted 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">parameters </param>
        /// <param name="timeOut">Set time out, default =0</param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        /// <param name="WaitForDelay">a few seconds before retrieving the real data use for a long-running query, default =0</param>
        void AsyncExecute(string sql, IDataParameter[] parameters, int interval, int timeOut, int WaitForDelay);
        /// <summary>
        /// AsyncExecute , use this method to invoke 
        /// Async procedure call until is IsCompleted 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">parameters </param>
        /// <param name="timeOut">Set time out, default =0</param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        void Execute(string sql, IDataParameter[] parameters, int interval, int timeOut);
        /// <summary>
        /// GetParameters factory
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        IDataParameter[] GetParameters(string[] names, object[] values);
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
        void AsyncExecuteBegin(AsyncCallback callback, string sql, IDataParameter[] parameters, int timeOut, int WaitForDelay);

        /// <summary>
        /// AsyncHandleCallback
        /// Retrieve the original command object, passed
        /// to this procedure in the AsyncState property
        /// of the IAsyncResult parameter.
        /// </summary>
        /// <param name="result"></param>
        IDataReader AsyncExecuteEnd(IAsyncResult result);

    }
     #endregion

     #region IActiveCommand

    internal interface IActiveCommand
    {
        string BuildCmdText(DBCommandType commandType, string TableName, string cmdPart1, string cmdPart2, string cmdPart3, string autNumberField);

        IDbAdapter CreateAdapter();

    }

    #endregion
}
