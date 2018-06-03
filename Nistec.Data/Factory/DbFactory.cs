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
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using Nistec.Data;
using System.Configuration;
using Nistec.Generic;
using Nistec.Data.Advanced;
using Nistec.Runtime;



namespace Nistec.Data.Factory
{

    [Serializable]
    public abstract class DbFactory : DbBase, IDbCmd
    {
        #region IDbCmd

        /// <summary>
        /// Create IDbCmd
        /// </summary>
        /// <returns></returns>
        public static IDbCmd Create<Dbc>() where Dbc : Nistec.Data.Entities.IDbContext
        {
            Nistec.Data.Entities.IDbContext db = ActivatorUtil.CreateInstance<Dbc>();
            if (db == null)
            {
                throw new Entities.EntityException("Create Instance of IDbContext was failed");
            }
            return Create(db.ConnectionString, db.Provider);
        }

        /// <summary>
        /// Create IDbCmd
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static IDbCmd Create(Ado.ConnectionProvider cp)
        {
            if (cp == null)
            {
                throw new ArgumentNullException("cp");
            }
            cp.CreateConnectionString();
            return Create(cp.ConnectionString, cp.Provider);
        }
        /// <summary>
        /// Create IDbCmd
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IDbCmd Create(Entities.IDbContext db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            return Create(db.ConnectionString, db.Provider);
        }
        /// <summary>
        /// Create IDbCmd
        /// </summary>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static IDbCmd Create(IDbConnection cnn)
        {
            if (cnn is SqlConnection)
                return new SqlClient.DbSqlCmd(cnn as SqlConnection);
            else if (cnn is OleDbConnection)
                return new OleDb.DbOleCmd(cnn as OleDbConnection);
            else
                throw new ArgumentException("Provider not supported");
        }
        /// <summary>
        /// Create IDbCmd
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IDbCmd Create(string connectionString, DBProvider provider)
        {
            if (provider == DBProvider.SqlServer)
                return new SqlClient.DbSqlCmd(connectionString);
            else if (provider == DBProvider.OleDb)
                return new OleDb.DbOleCmd(connectionString);
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// Create IDbCmd
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <returns></returns>
        public static IDbCmd Create(string connectionKey)
        {
            ConnectionStringSettings cnn= NetConfig.ConnectionContext(connectionKey);
            if (cnn == null)
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            DBProvider provider = GetProvider(cnn.ProviderName);
            if (provider == DBProvider.SqlServer)
                return new SqlClient.DbSqlCmd(cnn.ConnectionString);
            else if (provider == DBProvider.OleDb)
                return new OleDb.DbOleCmd(cnn.ConnectionString);
            else
                throw new ArgumentException("Provider not supported");
        }
        #endregion
            
        #region internal Factory methods

        /// <summary>
        /// BuildCommandText
        /// </summary>
        /// <param name="command"></param>
        /// <param name="provider"></param>
        /// <param name="commandType"></param>
        /// <param name="TableName"></param>
        /// <param name="cmdPart1"></param>
        /// <param name="cmdPart2"></param>
        /// <param name="cmdPart3"></param>
        /// <param name="cmdPart4"></param>
        /// <param name="autNumberField"></param>
        /// <returns></returns>
        internal static string BuildCommandText(IDbCommand command, DBProvider provider, DBCommandType commandType, string TableName, string cmdPart1, string cmdPart2, string cmdPart3, string cmdPart4, string autNumberField)
        {
            if (provider == DBProvider.SqlServer)
                return SqlClient.DbCommand.BuildCommandTextInternal(command,commandType, TableName, cmdPart1, cmdPart2,cmdPart3, cmdPart4, autNumberField);
            else if (provider == DBProvider.OleDb)
                return OleDb.DbCommand.BuildCommandTextInternal(command,commandType, TableName, cmdPart1, cmdPart2,cmdPart3,cmdPart4, autNumberField);
            else
                throw new ArgumentException("Provider not supported");
        }

        #endregion

        #region static methods

        /// <summary>
        /// Execute query by using Default connection name and sql query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static T Query<T>(string query, IDbDataParameter[] parameters=null)
        {
            using (IDbCmd cmd = DbFactory.Create(NetConfig.DefaultConnectionName))
            {
                return cmd.ExecuteCommand<T>(query, parameters);
            }
        }

        /// <summary>
        /// Execute query by given connection name and sql query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="query"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static T Query<T>(string connectionName, string query, IDbDataParameter[] parameters = null)
        {
            using (IDbCmd cmd = DbFactory.Create(connectionName))
            {
                return cmd.ExecuteCommand<T>(query, parameters);
            }
        }

        /// <summary>
        /// execute query using filetr.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionName"></param>
        /// <param name="entity"></param>
        /// <param name="filter"><see cref="DataFilter"/></param>
        /// <returns></returns>
        public T Query<T>(string connectionName, string entity, DataFilter filter)// = null)
        {
            if (connectionName == null)
                connectionName= NetConfig.DefaultConnectionName;

            IDbDataParameter[] parameters = filter == null ? null : filter.Parameters;
            string cmdText = filter == null ? SqlFormatter.SelectString(entity) : filter.Select(entity);
            using (IDbCmd cmd = DbFactory.Create(connectionName))
            {
                return cmd.ExecuteCommand<T>(cmdText,parameters);
            }
        }


        /// <summary>
        /// Create Connection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string connectionString, DBProvider provider)
        {
            if (provider == DBProvider.SqlServer)
                return new SqlConnection(connectionString);
            else if (provider == DBProvider.OleDb)
                return new OleDbConnection(connectionString);
            else
                throw new ArgumentException("Provider not supported");
        }

        private string EnsureCommandText(string cmdText, CommandType cmdType)
        {
            if (cmdText == null)
                return null;
            if (Regx.RegexValidateIgnoreCase(@"^(\s+|)\*\w+", cmdText))
            {
                return "select * from " + cmdText;
            }
            
            if (Regx.RegexValidateIgnoreCase("^(\\s+|)(select|exec|insert|update)", cmdText))
            {
                return cmdText;
            }
            if (cmdType == CommandType.Text)
                return "select * from " + cmdText;
            return cmdText;
        }

        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static IDbCommand CreateCommand(string cmdText, IDbConnection cnn)
        {
            SqlFormatter.ValidateSql(cmdText, null);
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn");
            }

            IDbCommand cmd = cnn.CreateCommand();
            cmd.CommandText = cmdText;
            return cmd;

        }
        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cnn"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static IDbCommand CreateCommand(string cmdText, IDbConnection cnn, IDbDataParameter[] parameters)
        {
            SqlFormatter.ValidateSql(cmdText,null);
            if (cnn == null)
            {
                throw new ArgumentNullException("cnn");
            }
            if (cnn is SqlConnection)
            {
                SqlCommand cmd = new SqlCommand(cmdText, cnn as SqlConnection);
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddSqlParameters(cmd, parameters);
                    //cmd.Parameters.AddRange(parameters);
                }
                return cmd;
            }
            else
            {
                IDbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = cmdText;
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddParameters(cmd, parameters);
                }
                return cmd;
            }

        }
       
  
        /// <summary>
        /// Add Parameters to command
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        public static void ParametersAddRange(IDbCommand cmd, IDbDataParameter[] parameters)
        {
            DataParameter.AddParameters(cmd, parameters);
        }

        /// <summary>
        /// Get Provider
        /// </summary>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static DBProvider GetProvider(IDbConnection cnn)
        {
            if (cnn is SqlConnection)
                return DBProvider.SqlServer;
            else if (cnn is OleDbConnection)
                return DBProvider.OleDb;
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// Get Provider
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static DBProvider GetProvider(IDbCommand cmd)
        {
            if (cmd is SqlCommand)
                return DBProvider.SqlServer;
            else if (cmd is OleDbCommand)
                return DBProvider.OleDb;
            else
                throw new ArgumentException("Provider not supported");
        }


        /// <summary>
        /// Get Provider
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DBProvider GetProvider(string provider)
        {
            DBProvider Provider;
            switch (provider.ToLower())
            {
                case "system.data.Sqlclient":
                case "Sqlclient":
                case "sqlserver":
                    Provider = DBProvider.SqlServer; break;
                case "oledb":
                    Provider = DBProvider.OleDb; break;
                case "db2":
                    Provider = DBProvider.DB2; break;
                case "firebird":
                    Provider = DBProvider.Firebird; break;
                case "mysql":
                    Provider = DBProvider.MySQL; break;
                case "oracle":
                    Provider = DBProvider.Oracle; break;
                case "sybasease":
                    Provider = DBProvider.SybaseASE; break;
                case "sqlite":
                    Provider = DBProvider.SQLite; break;
                case "sqlce":
                    Provider = DBProvider.SqlCe; break;
                default:
                    Provider = DBProvider.SqlServer;
                    break;
            }
            return Provider;
        }
        
 
        #endregion

        #region Ctor
        /// <summary>
        /// ctor
        /// </summary>
        public DbFactory() { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cnn"></param>
        public DbFactory(IDbConnection cnn)
        //    : base(cnn, true)
        {
            //conn = cnn;
            base.Init(cnn, true);
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbFactory(string connectionString, DBProvider provider)
        //    : base(connectionString, provider)
        {
            base.Init(connectionString, provider,true);
        }
      

        #endregion

        #region Properties
       

        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get { return m_connectionString; }
            set { m_connectionString = value; }
        }

        /// <summary>
        /// Get Adapter
        /// </summary>
        public IDbAdapter Adapter
        {
            get { return AdapterFactory.CreateAdapter(m_connectionString, DBProvider); }
        }

        #endregion

        #region Execute Reader
        /// <summary>
        /// GetCommand
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public IDbCommand GetCommand(string cmdText)
        {
            SqlFormatter.ValidateSql(cmdText, "drop|alter");
            return DbFactory.CreateCommand(cmdText, this.Connection);
        }
        /// <summary>
        /// Execute Reader
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string cmdText, CommandBehavior behavior)
        {
            return this.GetCommand(cmdText).ExecuteReader(behavior);
        }

        /// <summary>
        /// Execute Reader
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="behavior"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string cmdText, CommandBehavior behavior, IDbDataParameter[] parameters)
        {
            IDbCommand cmd = CreateCommand(cmdText, this.Connection, parameters);
            return cmd.ExecuteReader(behavior);
        }

        #endregion

        #region Execute none query

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        public int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(cmdText, null);
        }
  

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public int ExecuteNonQuery(string cmdText, IDbDataParameter[] parameters, CommandType commandType= CommandType.Text, int commandTimeOut=0)
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
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose(cmd);//ConnectionClose(cmd);
            }
        }

        #endregion

        #region CommandScalar

        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        public T ExecuteScalar<T>(string cmdText)
        {
            return ExecuteScalar<T>(cmdText, null);
        }
  
        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public T ExecuteScalar<T>(string cmdText, DataParameter[] parameters, CommandType commandType= CommandType.Text, int commandTimeOut=0) 
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

                return GenericTypes.Convert<T>(result);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose(cmd);//ConnectionClose(cmd);
            }
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
        public T ExecuteScalar<T>(string cmdText, IDbDataParameter[] parameters, T returnIfNull, CommandType commandType, int commandTimeOut)
        {
            return base.ExecuteScalarInternal<T>(cmdText, parameters, returnIfNull, commandType, commandTimeOut);
        }


        #endregion

        #region Execute command

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public T ExecuteCommand<T>(string cmdText, bool addWithKey=false)
        {
            return ExecuteCommand<T>(cmdText, null, CommandType.Text, 0, addWithKey);
        }
   
        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) .
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public TResult ExecuteCommand<TItem,TResult>(string cmdText, IDbDataParameter[] parameters, CommandType commandType= CommandType.Text, int commandTimeOut=0,bool addWithKey=false)
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
                return AdapterFactory.ExecuteDataOrScalar<TItem,TResult>(cmd, null, addWithKey);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose(cmd);
            }
        }


        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
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
                return AdapterFactory.ExecuteDataOrScalar<T>(cmd, null, addWithKey);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose(cmd);
            }
        }

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cacheKey">Get or Set data from/to cache.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, string cacheKey, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
        {
            object value = default(T);
            IDbCommand cmd = null;
            try
            {
                if (AutoDataCache.DW.GetTryParse(cacheKey, out value))
                {
                    return (T)value;
                }

                cmd = DbFactory.CreateCommand(cmdText, m_connection, parameters);

                cmd.CommandType = commandType;
                if (commandTimeOut > 0)
                {
                    cmd.CommandTimeout = commandTimeOut;
                }
                ConnectionOpen(cmd);
                value = AdapterFactory.ExecuteDataOrScalar<T>(cmd, null, addWithKey);

                if (!string.IsNullOrEmpty(cacheKey))
                {
                    AutoDataCache.DW[cacheKey] = value;
                }
                return (T)value;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose(cmd);
            }
        }

        #endregion

        #region Execute dataTable

        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string mappingName)
        {
            string cmdText =SqlFormatter.SelectString(mappingName);
            return ExecuteCommand<DataTable>(cmdText, false);
        }
        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string mappingName, bool addWithKey=false)
        {
            string cmdText = SqlFormatter.SelectString(mappingName);
            return ExecuteCommand<DataTable>(cmdText, addWithKey);
        }
        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cmdText"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string tableName, string cmdText, bool addWithKey=false)
        {
            DataTable dt = ExecuteCommand<DataTable>(cmdText, addWithKey);
            dt.TableName = tableName;
            return dt;
        }

        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cmdText"></param>
        /// <param name="commandTimeout">Timeout in seconds</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
         /// <returns></returns>
        public DataTable ExecuteDataTable(string tableName, string cmdText, int commandTimeout, bool addWithKey = false)
        {
            DataTable dt = ExecuteCommand<DataTable>(cmdText, null,CommandType.Text, commandTimeout, addWithKey);
            dt.TableName = tableName;
            return dt;
        }
        #endregion

        #region Dlist

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
        public new T LookupQuery<T>(string Field, string Table, string Where, T returnIfNull, object[] values)
        {
            DataParameter[] parameters = null;
            try
            {
                Type retType = typeof(T);
                string sql = SqlFormatter.SelectString(Field, Table, Where);
                if (!string.IsNullOrEmpty(Where))
                {
                    parameters = DataFilter.CreateFilter(Where, values);
                }
                return ExecuteScalarInternal<T>(sql, parameters, returnIfNull, CommandType.Text, 0);
            }
            catch (Exception)
            {
                return returnIfNull;
            }
        }

        public DataRow DRow(string fields, string table, string where, object[] parameters)
        {
            string sql = SqlFormatter.SelectString(fields, table, where);
            DataParameter[] dparameters = DataFilter.CreateFilter(where, parameters);
            return ExecuteCommand<DataRow>(sql, DataParameter.CreateParameters(dparameters, DBProvider));
        }


        public DataView DList(string valueMemeber, string displayMember, string table, string where, object[] parameters)
        {
            DataParameter[] dparameters = DataFilter.CreateFilter(where, parameters);

            return DList(valueMemeber, displayMember, table, where, dparameters);
        }

        public DataView DList(string valueMemeber, string displayMember, string table, string where, DataParameter[] parameters)
        {

            try
            {
                string fields = SqlFormatter.FieldsString(valueMemeber, displayMember);
                string sql = SqlFormatter.SelectString(fields, table, where);
 
                DataTable dt = ExecuteCommand<DataTable>(sql, DataParameter.CreateParameters(parameters,DBProvider));
                if (dt == null)
                    return null;
                return dt.DefaultView;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Multi commands

        /// <summary>
        /// Execute multiple commands Non Query
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="failOnFirstError"></param>
        public void MultiExecuteNonQuery(string[] commands, bool failOnFirstError)
        {

            IDbCommand cmd = null;
            try
            {
                cmd = Connection.CreateCommand();

                this.ConnectionOpen();
                cmd.Connection = this.Connection;
                foreach (string cmdText in commands)
                {
                    SqlFormatter.ValidateSql(cmdText, null);
                    cmd.CommandText = cmdText;
                    if (failOnFirstError)
                    {
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch { }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionClose(cmd);
            }
        }

        /// <summary>
        /// Execute multiple commands Scalar
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="failOnFirstError"></param>
        /// <returns></returns>
        public object[] MultiExecuteScalar(string[] commands, bool failOnFirstError)
        {
            IDbCommand cmd = null;
            object[] Retuens = new object[commands.Length];
            try
            {
                cmd = Connection.CreateCommand();
                this.ConnectionOpen();
                cmd.Connection = this.Connection;
                int index = 0;
                foreach (string cmdText in commands)
                {
                    SqlFormatter.ValidateSql(cmdText, null);
                    cmd.CommandText = cmdText;

                    if (failOnFirstError)
                    {
                        Retuens[index] = cmd.ExecuteScalar();
                    }
                    else
                    {
                        try
                        {
                            Retuens[index] = cmd.ExecuteScalar();
                        }
                        catch
                        {
                            Retuens[index] = -1;
                        }
                    }
                    index++;
                }

                return Retuens;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionClose(cmd);
            }
        }

        #endregion

    }
}
