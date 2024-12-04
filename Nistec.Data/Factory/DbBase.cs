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
using Nistec.Runtime;
#pragma warning disable CS1591
namespace Nistec.Data.Factory
{


    /// <summary>
    /// Base class for every dal class. You can inherint your classes from 
    /// this base class or <see cref="IAutoDb">IAutoDb</see> interface
    /// </summary>
    public abstract class DbBase //:IAutoDb 
    {

        #region IDisposable implementation
		
		/// <summary>
		/// Disposed flag.
		/// </summary>
        internal bool m_disposed = false;
  
		/// <summary>
		/// Implementation of method of IDisposable interface.
		/// </summary>
		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose method with a boolean parameter indicating the source of calling.
		/// </summary>
        /// <param name="disposing">Indicates from whare the method is called.</param>
        protected void Dispose(bool disposing)
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
		internal void InnerDispose()
		{
			if(m_connection != null)
			{
				if((m_connection.State != ConnectionState.Closed))// && m_ownsConnection) 
				{
					try
					{
						m_connection.Close();
                        m_connection.Dispose();
					}
					catch{}
				}
                m_connection = null;
			}
            if (m_AsyncConnection != null)
            {
                if ((m_AsyncConnection.State != ConnectionState.Closed))// && m_ownsConnection) 
                {
                    try
                    {
                        m_AsyncConnection.Close();
                        m_AsyncConnection.Dispose();
                    }
                    catch { }
                }
                m_AsyncConnection = null;
            }
            if (m_Command != null)
            {
                m_Command.Dispose();
                m_Command = null;
            }
            if (m_transaction != null)
            {
                m_transaction.Dispose();
                m_transaction = null;
            }
		}

		#endregion

        #region DalDB members

        /// <summary>
        /// Inner class member representing Connection string
        /// </summary>
        internal string m_connectionString = null;

        /// <summary>
        /// Inner class member representing auto CloseConnection
        /// </summary>
        protected bool m_autoCloseConnection = true;

        ///// <summary>
        ///// DataSet representing the Schema of Database.
        ///// </summary>
        //internal DalSchema m_DataSet = null;


        /// <summary>
        /// EnableCancelExecuting
        /// </summary>
        internal bool m_EnableCancelExecuting = false;

        /// <summary>
        /// IDbCommand
        /// </summary>
        internal IDbCommand m_Command;

        /// <summary>
        /// DBProvider
        /// </summary>
        internal DBProvider m_DBProvider;
             

        /// <summary>
        /// Get DBProvider property
        /// </summary>
        public DBProvider DBProvider
        {
            get
            {
                return m_DBProvider;
            }
        }

        #endregion

        #region Connection

        /// <summary>
        /// ConnectionOpen
        /// </summary>
        /// <param name="cmd"></param>
        internal void ConnectionOpen(IDbCommand cmd)
        {
             if (cmd.Connection.State == ConnectionState.Closed)
            {
                cmd.Connection.Open();
            }
        }

        /// <summary>
        /// ConnectionAutoClose
        /// </summary>
        /// <param name="cmd"></param>
        internal void ConnectionAutoClose(IDbCommand cmd)
        {
             if (cmd == null)
                return;
             if (!m_autoCloseConnection)
             {
                 cmd.Dispose();
                 return;
             }

            if (cmd.Connection != null)
            {
                if (cmd.Connection.State != ConnectionState.Closed)
                    cmd.Connection.Close();
            }
            cmd.Dispose();
        }

        /// <summary>
        /// ConnectionClose
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dispose">optional</param>
        internal void ConnectionClose(IDbCommand cmd, bool dispose = true)
        {
            if (cmd == null)
                return;
            if (cmd.Connection != null)
            {
                if (cmd.Connection.State != ConnectionState.Closed)
                    cmd.Connection.Close();
            }
            if (dispose)
            {
                cmd.Dispose();
            }
        }

        /// <summary>
        /// ConnectionOpen
        /// </summary>
        protected void ConnectionOpen()
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
        }
        /// <summary>
        /// ConnectionClose
        /// </summary>
        protected void ConnectionClose()
        {
            if (Connection != null)
            {
                if (Connection.State != ConnectionState.Closed)
                    Connection.Close();
            }
        }

        #endregion

        #region DAggregate

        /// <summary>
        /// Sum function if no record found return 0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string </param>
        /// <param name="values">parameters values </param>
        /// <returns>retun object result</returns>
        internal protected T DSum<T>(string Field, string Table, string Where, object[] values)
        {
            return DAggregate<T>("SUM", Field, Table, Where, values);

        }
        /// <summary>
        /// Max function if no record found return 0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string </param>
        /// <param name="values">parameters values </param>
        /// <returns>retun object result</returns>
        internal protected T DMax<T>(string Field, string Table, string Where, object[] values)
        {
            return DAggregate<T>("MAX", Field, Table, Where, values);

        }
        /// <summary>
        /// Min function if no record found return 0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string </param>
        /// <param name="values">parameters values </param>
        /// <returns>retun object result</returns>
        internal protected T DMin<T>(string Field, string Table, string Where, object[] values)
        {
            return DAggregate<T>("MIN", Field, Table, Where, values);

        }
        /// <summary>
        /// Avg function if no record found return 0
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string </param>
        /// <param name="values">parameters values </param>
        /// <returns>retun object result</returns>
        internal protected T DAvg<T>(string Field, string Table, string Where, object[] values)
        {
            return DAggregate<T>("AVG", Field, Table, Where, values);

        }

        /// <summary>
        /// Count function if no record found return 0
        /// </summary>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string </param>
        /// <param name="values">parameters values </param>
        /// <returns>retun int result</returns>
        internal protected int DCount(string Field, string Table, string Where, object[] values)
        {
            try
            {
                return (int)DAggregate<int>("COUNT", Field, Table, Where, values);
            }
            catch
            {
                return 0;
            }

        }

        private T DAggregate<T>(string AggregateMode, string Field, string Table, string Where, object[] values)
        {
            DataParameter[] parameters = null;
            try
            {
                string sql =SqlFormatter.AggregateString(AggregateMode, Field, Table, Where);
                T def = ActivatorUtil.CreateInstance<T>();

                if (!string.IsNullOrEmpty(Where))
                {
                    parameters = DataFilter.CreateFilter(Where, values);
                }
                return ExecuteScalarInternal<T>(sql, parameters, def, CommandType.Text, 0);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

 
        #region init

        /// <summary>
        /// DB Constructor with IAutoBase
        /// </summary>
        internal void Init(IAutoBase dalBase)
        {
  
            m_connectionString = dalBase.Connection.ConnectionString;
            m_DBProvider = dalBase.DBProvider;
            m_connection = DbFactory.CreateConnection(m_connectionString, m_DBProvider);
            m_autoCloseConnection = true;

        }


         /// <summary>
        /// DB Constructor with IDbContext
        /// </summary>
        internal void Init<T>() where T : IDbContext
        {
            IDbContext db = ActivatorUtil.CreateInstance<T>();
            m_connectionString = db.ConnectionString;
            m_DBProvider = db.Provider;
            m_connection = DbFactory.CreateConnection(m_connectionString, m_DBProvider);
            m_autoCloseConnection = true;

        }
        

        /// <summary>
        /// DB Constructor with connection string
        /// </summary>
        internal void Init(string connectionString, DBProvider provider, bool autoCloseConnection)
        {
            m_connection = DbFactory.CreateConnection(connectionString, provider);
            m_autoCloseConnection = autoCloseConnection;
            m_connectionString = connectionString;
            m_DBProvider = provider;
        }

        /// <summary>
        /// DB Constructor with connection
        /// </summary>
        internal void Init(IDbConnection connection, bool autoCloseConnection)
        {
            m_connection = connection;
            m_autoCloseConnection = autoCloseConnection;
            m_connectionString = connection.ConnectionString;
            m_DBProvider = DbFactory.GetProvider(connection);

        }

        #endregion

        #region DalDB

        /// <summary>
        /// Inner class member representing IDbConnection object
        /// </summary>
        protected IDbConnection m_connection = null;

        /// <summary>
        /// Inner class member representing IDbTransaction object
        /// </summary>
        protected IDbTransaction m_transaction = null;

        /// <summary>
        /// Async Connection object.
        /// </summary>
        private SqlConnection m_AsyncConnection = null;
  
        /// <summary>
        /// Get async connection
        /// </summary>
        protected SqlConnection AsyncConnection
        {
            get
            {
                if (m_AsyncConnection == null)
                {
                    string connAsyncString = string.Format("{0};Asynchronous Processing=true", m_connection.ConnectionString);
                    m_AsyncConnection = new SqlConnection(connAsyncString);
                }
                return m_AsyncConnection;
            }
        }

        /// <summary>
        /// Gte or set Connection property
        /// </summary>
        public virtual IDbConnection Connection
        {
            get
            {
                return m_connection as IDbConnection;
            }
            set
            {
                m_connection = value as IDbConnection;
            }
        }

        /// <summary>
        /// Gte or set Transaction property
        /// </summary>
        public virtual IDbTransaction Transaction
        {
            get
            {
                return m_transaction as IDbTransaction;
            }
            set
            {
                m_transaction = value as IDbTransaction;
            }
        }


        #endregion

        #region lookup query


        /// <summary>
        /// Execute lookup function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string,each paramet name should start with @ </param>
        /// <param name="returnIfNull">Default value to return if no recored affected</param>
        /// <param name="parameters">Values of Parameters, use method DataParameter.Get(params object[] keyValueParameters)</param>
        /// <returns></returns>
        protected virtual T LookupQuery<T>(string Field, string Table, string Where, T returnIfNull, IDbDataParameter[] parameters)
        {
            try
            {
                Type retType = typeof(T);// method.ReturnType;
                string sql = SqlFormatter.SelectString(Field, Table, Where);
                return ExecuteScalarInternal<T>(sql, parameters, returnIfNull, CommandType.Text, 0);
            }
            catch (Exception)
            {
                return returnIfNull;// throw ex;
            }
        }

        /// <summary>
        /// Execute lookup function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string,each paramet name should start with @ </param>
        /// <param name="returnIfNull">Default value to return if no recored affected</param>
        /// <param name="values">Values of Parameters</param>
        /// <returns></returns>
        protected virtual T LookupQuery<T>(string Field, string Table, string Where, T returnIfNull, object[] values)
        {
            DataParameter[] parameters = null;
            try
            {
                Type retType = typeof(T);// method.ReturnType;
                string sql = SqlFormatter.SelectString(Field, Table, Where);
                if (!string.IsNullOrEmpty(Where))
                {
                    parameters = DataFilter.CreateFilter(Where, values);
                }
                return ExecuteScalarInternal<T>(sql, parameters, returnIfNull, CommandType.Text, 0);
            }
            catch (Exception)
            {
                return returnIfNull;// throw ex;
            }
        }

       
        /// <summary>
        /// Lookup function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string </param>
        /// <param name="defaultValue">Default value to return if no recored affected</param>
        /// <returns>retun object result</returns>
        [Obsolete("Use LookupQuery insted, this is not secure")]
        protected virtual T Dlookup<T>(string Field, string Table, string Where, T defaultValue)
        {
            
            try
            {
                string sql = SqlFormatter.SelectString(Field, Table, Where);
                return ExecuteScalarInternal<T>(sql, null, defaultValue, CommandType.Text, 0);//(sql, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;// throw ex;
            }
        }

        /// <summary>
        /// Lookup function return true if exists
        /// </summary>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string </param>
        /// <param name="values">parameters values </param>
        /// <returns>retun Boolean result</returns>
        protected bool DExists(string Field, string Table, string Where, object[] values)
        {
            DataParameter[] parameters = null;
            try
            {
                string sql = SqlFormatter.SelectString(Field, Table, Where);
                if (!string.IsNullOrEmpty(Where))
                {
                    parameters = DataFilter.CreateFilter(Where, values);
                }
                return ExecuteScalarInternal(sql, parameters, CommandType.Text, 0) != null;
            }
            catch (Exception)
            {
                return false;
            }

        }
        #endregion


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
        internal T ExecuteScalarInternal<T>(string cmdText,  IDbDataParameter[] parameters,T returnIfNull, CommandType commandType, int commandTimeOut)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);

                cmd.CommandType = commandType;
                if (commandTimeOut > 0)
                {
                    cmd.CommandTimeout = commandTimeOut;
                }
                ConnectionOpen(cmd);
                object result = cmd.ExecuteScalar();
               
                return GenericTypes.Convert<T>(result, returnIfNull);

            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return returnIfNull;
            }
            finally
            {
                ConnectionAutoClose(cmd);// ConnectionClose(cmd);
            }
        }


        /// <summary>
        /// Executes Command and returns object value as scalar or null.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        internal object ExecuteScalarInternal(string cmdText, IDbDataParameter[] parameters, CommandType commandType, int commandTimeOut)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);

                cmd.CommandType = commandType;
                if (commandTimeOut > 0)
                {
                    cmd.CommandTimeout = commandTimeOut;
                }
                ConnectionOpen(cmd);
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }
            finally
            {
                ConnectionAutoClose(cmd);//ConnectionClose(cmd);
            }
        }

        #region Data constraint

                /// <summary>
        /// Set DataSer tables mapping
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="enforceConstraints"></param>
        /// <param name="tableMapping"></param>
        /// <returns></returns>
        protected DataSet DataSetTableMapping(DataSet ds, bool enforceConstraints, params string[] tableMapping)
        {
            return AdapterFactory.DataSetTableMapping(ds, enforceConstraints, tableMapping);
        }

        /// <summary>
        /// Get Item from DataCache. 
        /// </summary>
        /// <param name="cacheKey">Get or Set object from/to cache.</param>
        /// <returns></returns>
        protected object DataCacheItem(string cacheKey)
        {
            object value = null;
            if (AutoDataCache.DW.GetTryParse(cacheKey, out value))
            {
                return value;
            }
            return value;
        }

        /// <summary>
        /// Remove Item from DataCache. 
        /// </summary>
        /// <param name="cacheKey">Get or Set object from/to cache.</param>
        /// <returns></returns>
        protected void RemoveCacheItem(string cacheKey)
        {
            AutoDataCache.DW.Remove(cacheKey);
        }

        /// <summary>
        /// Executes sql string and returns DataTable object,User can cancel executing.
        /// </summary>
        /// <param name="sql">Sql string.</param>
        /// <param name="constraint">Constraint.</param>
        /// <returns></returns>
        protected DataTable ExecuteDataConstraint(string sql, CONSTRAINT[] constraint)
        {
            IDbCommand cmd = DbFactory.CreateCommand(sql, m_connection);
            DataTable dt = (DataTable)InternalCmd.RunCommand<DataTable>(cmd, m_autoCloseConnection, MissingSchemaAction.Add);
            if (constraint != null)
            {
                CONSTRAINT.SetConstraint(dt, constraint);
            }
            return dt;
        }
        /// <summary>
        /// Executes sql string and returns DataTable object,User can cancel executing.
        /// </summary>
        /// <param name="sql">Sql string.</param>
        /// <param name="constraint">Constraint.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns>
        protected DataTable ExecuteDataConstraint(string sql, CONSTRAINT[] constraint, int commandTimeOut)
        {
            IDbCommand cmd = DbFactory.CreateCommand(sql, m_connection);
            cmd.CommandTimeout = commandTimeOut;
            DataTable dt = (DataTable)InternalCmd.RunCommand<DataTable>(cmd, m_autoCloseConnection, MissingSchemaAction.Add);
            if (constraint != null)
            {
                CONSTRAINT.SetConstraint(dt, constraint);
            }
            return dt;
        }


        #endregion
    }

}
