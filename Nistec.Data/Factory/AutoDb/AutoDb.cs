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
using System.Reflection;
using System.Diagnostics;
using Nistec.Data.Entities;
using System.Collections.Generic;
using Nistec.Data;
using Nistec.Data.Advanced;


namespace Nistec.Data.Factory
{

    /// <summary>
    /// Base class for every dal class. You can inherint your classes from 
    /// this base class or <see cref="IAutoDb">IAutoDb</see> interface
    /// </summary>
    public class AutoDb : DbBase, IAutoDb
    {

        #region IAutoDb

        /// <summary>
        /// CancelExecuting
        /// </summary>
        public void CancelExecuting()
        {
            try
            {
                if (m_Command != null)
                    m_Command.Cancel();
            }
            catch { }
        }
        /// <summary>
        /// Get or set EnableCancelExecuting property
        /// </summary>
        public bool EnableCancelExecuting
        {
            get
            {
                return m_EnableCancelExecuting;
            }
            set
            {
                m_EnableCancelExecuting = value;
            }
        }

        /// <summary>
        /// Get or set AutoCloseConnection property
        /// </summary>
        public bool AutoCloseConnection
        {
            get
            {
                return m_autoCloseConnection;
            }
            set
            {
                m_autoCloseConnection = value;
            }
        }


        #endregion

        #region DalDB


        /// <summary>
        /// DB Constructor
        /// </summary>
        public AutoDb()
        {
           
        }

        /// <summary>
        /// DB Constructor with IAutoBase
        /// </summary>
        public AutoDb(IAutoBase dalBase)
        {
            base.Init(dalBase);
        }

        /// <summary>
        /// DB Constructor with connection string
        /// </summary>
        public AutoDb(string connectionString, DBProvider provider)
        {
            base.Init(connectionString, provider, true);
        }

        /// <summary>
        /// DB Constructor with connection
        /// </summary>
        public AutoDb(IDbConnection connection, bool autoCloseConnection)
        {
            base.Init(connection, autoCloseConnection);
        }

        #endregion

        #region Execute

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object Execute()
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            return ExecuteCmd(m_connection, m_transaction, method, null, m_autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object Execute(bool autoCloseConnection)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            return ExecuteCmd(m_connection, m_transaction, method, null, autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object Execute(bool autoCloseConnection, object[] values)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            return ExecuteCmd(m_connection, m_transaction, method, values, autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object Execute(int commandTimeOut, object[] values)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            return ExecuteCmd(m_connection, m_transaction, method, values, m_autoCloseConnection, commandTimeOut);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object Execute(params object[] values)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            return ExecuteCmd(m_connection, m_transaction, method, values, m_autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="cacheKey">Get or Set object from/to cache.</param>
        /// <param name="reload">Should reload item to cache.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object Execute(string cacheKey, bool reload, params object[] values)
        {
            object value = null;
            if (!reload && AutoDataCache.DW.GetTryParse(cacheKey, out value))
            {
                return value;
            }
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            value = ExecuteCmd(m_connection, m_transaction, method, values, m_autoCloseConnection, 0);

            AutoDataCache.DW[cacheKey] = value;

            return value;
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="transaction">Transaction property.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object ExecuteTrans(IDbTransaction transaction, bool autoCloseConnection, object[] values)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());
            return ExecuteCmd(m_connection, transaction, method, values, autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="transaction">Transaction property.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected object ExecuteTrans(IDbTransaction transaction, bool autoCloseConnection, int commandTimeOut, object[] values)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());
            return ExecuteCmd(m_connection, transaction, method, values, autoCloseConnection, commandTimeOut);

        }

             /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="connection">Connection property.</param>
        /// <param name="transaction">Transaction property.</param>
        /// <param name="method"><see cref="MethodInfo"/> type object from which the command object is built.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal virtual object ExecuteCmd(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
        {
            return InternalCmd.ExecuteCmd(m_connection, transaction, method, values, autoCloseConnection, commandTimeOut);
        }
        #endregion

        #region Execute Generic <T>

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected T Execute<T>()
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());
            return ExecuteCmd<T>(m_connection, m_transaction, method, null, m_autoCloseConnection, 0);
        }
        
        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected T Execute<T>(params object[] values)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());
            return ExecuteCmd<T>(m_connection, m_transaction, method, values, m_autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">Get or Set object from/to cache.</param>
        /// <param name="reload">Should reload item to cache.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected T Execute<T>(string cacheKey, bool reload, params object[] values)
        {
            object value = null;
            if (!reload && AutoDataCache.DW.GetTryParse(cacheKey, out value))
            {
                return (T)value;
            }
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            value = ExecuteCmd<T>(m_connection, m_transaction, method, values, m_autoCloseConnection, 0);

            AutoDataCache.DW[cacheKey] = value;

            return (T)value;
        }

       
        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Connection property.</param>
        /// <param name="transaction">Transaction property.</param>
        /// <param name="method"><see cref="MethodInfo"/> type object from which the command object is built.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal virtual T ExecuteCmd<T>(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
        {
            return InternalCmd.ExecuteCmd<T>(m_connection, transaction, method, values, autoCloseConnection, commandTimeOut);
        }
        #endregion

        #region Execute Generic <TItem, TResult>

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected TResult Execute<TItem, TResult>()
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());
            return ExecuteCmd<TItem, TResult>(m_connection, m_transaction, method, null, m_autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected TResult Execute<TItem, TResult>(params object[] values)
        {
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());
            return ExecuteCmd<TItem, TResult>(m_connection, m_transaction, method, values, m_autoCloseConnection, 0);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cacheKey">Get or Set object from/to cache.</param>
        /// <param name="reload">Should reload item to cache.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        protected TResult Execute<TItem, TResult>(string cacheKey, bool reload, params object[] values)
        {
            object value = null;
            if (!reload && AutoDataCache.DW.GetTryParse(cacheKey, out value))
            {
                return (TResult)value;
            }
            // this is done because this method can be called explicitly from code.
            MethodInfo method = (System.Reflection.MethodInfo)(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod());

            value = ExecuteCmd<TItem, TResult>(m_connection, m_transaction, method, values, m_autoCloseConnection, 0);

            AutoDataCache.DW[cacheKey] = value;

            return (TResult)value;
        }
  

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="connection">Connection property.</param>
        /// <param name="transaction">Transaction property.</param>
        /// <param name="method"><see cref="MethodInfo"/> type object from which the command object is built.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal virtual TResult ExecuteCmd<TItem, TResult>(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
        {
            return InternalCmd.ExecuteCmd<TItem, TResult>(m_connection, transaction, method, values, autoCloseConnection, commandTimeOut);
        }
        #endregion

       
        #region Execute scalar

        /// <summary>
        /// Executes sql string and returns Scalar value,User can cancel executing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql string.</param>
        /// <returns></returns>
        protected T ExecuteScalar<T>(string cmdText)
        {
            return ExecuteScalar<T>(cmdText,null, typeof(T).IsValueType ? Activator.CreateInstance<T>() : default(T));
        }

        /// <summary>
        /// Executes sql string and returns Scalar value,User can cancel executing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql string.</param>
        /// <param name="returnIfNull">The value will return if result is null.</param>
        /// <returns></returns>
        protected T ExecuteScalar<T>(string cmdText, T returnIfNull)
        {
            return ExecuteScalar<T>(cmdText,null,returnIfNull);
        }
   

        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="returnIfNull">The value will return if result is null.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        protected T ExecuteScalar<T>(string cmdText, IDbDataParameter[] parameters, T returnIfNull, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            IDbCommand cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);
            cmd.CommandTimeout = commandTimeOut;
            if (m_EnableCancelExecuting)
                m_Command = cmd;
            return InternalCmd.RunCommandScalar<T>(cmd, m_autoCloseConnection, returnIfNull);
        }

        #endregion

        #region Execute none query

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected ,User can cancel executing.
        /// </summary>
        /// <param name="cmdText">Sql string.</param>
        /// <returns>return the number of rows affected.</returns>
        protected int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(cmdText, null);
        }
   
        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected ,User can cancel executing.
        /// </summary>
        /// <param name="cmdText">Sql string.</param>
        /// <param name="parameters">Command parameters.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns>return the number of rows affected.</returns>
        protected virtual int ExecuteNonQuery(string cmdText, IDbDataParameter[] parameters, CommandType commandType= CommandType.Text, int commandTimeOut=0)
        {
            IDbCommand cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);
            cmd.CommandType = commandType;
            cmd.CommandTimeout = commandTimeOut;
            if (m_EnableCancelExecuting)
                m_Command = cmd;
            return InternalCmd.RunCommandNonQuery(cmd, true);
        }
      
        #endregion

        #region Execute commands

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow),User can cancel executing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql string.</param>
        /// <returns></returns>
        protected T ExecuteCommand<T>(string cmdText)
        {
            return ExecuteCommand<T>(cmdText, null);
        }

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow),User can cancel executing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql string.</param>
        /// <param name="parameters">Command parameters.</param>
        /// <returns></returns>
        protected virtual T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters)
        {
            IDbCommand cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);
            if (m_EnableCancelExecuting)
                m_Command = cmd;
            return InternalCmd.RunCommand<T>(cmd, true, MissingSchemaAction.Add);
        }

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow),User can cancel executing.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmdText">Sql string.</param>
        /// <param name="parameters">Command parameters.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <returns></returns>
        protected virtual TResult ExecuteCommand<TItem, TResult>(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text)
        {
            IDbCommand cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);
            cmd.CommandType = commandType;
            if (m_EnableCancelExecuting)
                m_Command = cmd;
            return InternalCmd.RunCommand<TItem, TResult>(cmd, true, MissingSchemaAction.Add, false);
        }
   
        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow),User can cancel executing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql string.</param>
        /// <param name="parameters">Command parameters.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="missingSchemaAction"></param>
        /// <returns></returns>
        protected virtual T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, CommandType commandType= CommandType.Text, int commandTimeOut=0, MissingSchemaAction missingSchemaAction= MissingSchemaAction.Add)
        {
            IDbCommand cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);
            cmd.CommandType = commandType;
            cmd.CommandTimeout = commandTimeOut;
            if (m_EnableCancelExecuting)
                m_Command = cmd;
            return InternalCmd.RunCommand<T>(cmd, true, missingSchemaAction);
        }
   
        #endregion
  
    }

}
