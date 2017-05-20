using Nistec.Data.Advanced;
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Nistec.Data.Entities
{
    public abstract class DbContextCommand
    {

        IDbConnection _Connection;

        #region IDbConnection

        ///// <summary>
        ///// Create IDbCmd
        ///// </summary>
        ///// <returns></returns>
        //public static IDbCmd Create<Dbc>() where Dbc : IDbContext
        //{
        //    IDbContext db = ActivatorUtil.CreateInstance<Dbc>();//System.Activator.CreateInstance<Dbc>();
        //    if (db == null)
        //    {
        //        throw new Entities.EntityException("Create Instance of IDbContext was failed");
        //    }
        //    return Create(db.ConnectionString, db.Provider);
        //}

      
        ///// <summary>
        ///// Create IDbCmd
        ///// </summary>
        ///// <param name="db"></param>
        ///// <returns></returns>
        //public static IDbCmd Create(IDbContext db)
        //{
        //    if (db == null)
        //    {
        //        throw new ArgumentNullException("db");
        //    }
        //    return Create(db.ConnectionString, db.Provider);
        //}
        ///// <summary>
        ///// Create IDbCmd
        ///// </summary>
        ///// <param name="cnn"></param>
        ///// <returns></returns>
        //public static IDbCmd Create(IDbConnection cnn)
        //{
        //    if (cnn is SqlConnection)
        //        return new SqlClient.DbSqlCmd(cnn as SqlConnection);
        //    else if (cnn is OleDbConnection)
        //        return new OleDb.DbOleCmd(cnn as OleDbConnection);
        //    else
        //        throw new ArgumentException("Provider not supported");
        //}
        /*
        /// <summary>
        /// Create IDbConnection
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

        /// <summary>
        /// Create IDbConnection
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string connectionKey)
        {
            ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
            if (cnn == null)
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            DBProvider provider = GetProvider(cnn.ProviderName);
            if (provider == DBProvider.SqlServer)
                return new SqlConnection(cnn.ConnectionString);
            else if (provider == DBProvider.OleDb)
                return new OleDbConnection(cnn.ConnectionString);
            else
                throw new ArgumentException("Provider not supported");
        }

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
                default:
                    Provider = DBProvider.SqlServer;
                    break;
            }
            return Provider;
        }
        */
        #endregion

        #region Ctor
        /// <summary>
        /// ctor
        /// </summary>
        public DbContextCommand() { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cnn"></param>
        public DbContextCommand(IDbConnection cnn)
        {
            OwnsConnection = false;
            Database = cnn.Database;
            ConnectionString = cnn.ConnectionString;
            Provider = GetProvider(cnn);
            _Connection = cnn;
            //Init(cnn);
        }

        /// <summary>
        /// ctor DbContextCommand
        /// </summary>
        /// <param name="connectionKey"></param>
        public DbContextCommand(string connectionKey)
        {
            ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
            if (cnn == null)
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            Provider = DbFactory.GetProvider(cnn.ProviderName);
            OwnsConnection = false;
            ConnectionName = cnn.Name;
            ConnectionString = cnn.ConnectionString;
            _Connection = CreateConnection();
            if (_Connection!=null)
            Database = _Connection.Database;
        }
        /// <summary>
        /// ctor DbContextCommand
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbContextCommand(string connectionString, DBProvider provider)
        {
            OwnsConnection = false;
            ConnectionString = connectionString;
            Provider = provider;
            _Connection = CreateConnection();
            if (_Connection != null)
                Database = _Connection.Database;
            //Init(connectionString, provider);
        }

        #endregion

        #region Dispose

        private bool disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_Connection != null)
                    {
                        if (_Connection.State != ConnectionState.Closed)
                            _Connection.Close();
                        _Connection.Dispose();
                        _Connection = null;
                    }
                    ConnectionName = null;
                    ConnectionString = null;
                    Database = null;
                }
                disposed = true;
            }
        }
      
   
        #endregion

        #region override

        /// <summary>
        /// Create Connection
        /// </summary>
        /// <returns></returns>
        protected virtual IDbConnection CreateConnection()//string connectionString, DBProvider provider)
        {
            if(!HasConnection)
            {
                throw new Exception("Invalid connection string");
            }
            if (Provider == DBProvider.SqlServer)
                return new SqlConnection(ConnectionString);
            else if (Provider == DBProvider.OleDb)
                return new OleDbConnection(ConnectionString);
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// Get Provider
        /// </summary>
        /// <param name="cnn"></param>
        /// <returns></returns>
        protected virtual DBProvider GetProvider(IDbConnection cnn)
        {
            if (cnn is SqlConnection)
                return DBProvider.SqlServer;
            else if (cnn is OleDbConnection)
                return DBProvider.OleDb;
            else
                throw new ArgumentException("Provider not supported");
        }

        /// <summary>
        /// DB Constructor with IAutoBase
        /// </summary>
        internal void Init(IAutoBase dalBase)
        {
            ConnectionString = dalBase.Connection.ConnectionString;
            Provider = dalBase.DBProvider;
            OwnsConnection = false;
            _Connection = dalBase.Connection;
            if (_Connection != null)
                Database = _Connection.Database;
        }


        /// <summary>
        /// DB Constructor with IDbContext
        /// </summary>
        internal void Init<T>() where T : IDbContext
        {
            IDbContext db = System.Activator.CreateInstance<T>();
            ConnectionString = db.ConnectionString;
            Provider = db.Provider;
            OwnsConnection = false;
            _Connection = CreateConnection();
            if (_Connection != null)
                Database = _Connection.Database;
        }


        ///// <summary>
        ///// DB Constructor with connection string
        ///// </summary>
        //internal void Init(string connectionString, DBProvider provider, bool ownConnection=false)
        //{
        //    _Connection = new SqlConnection(ConnectionString);
        //    OwnConnection = ownConnection;
        //    ConnectionString = connectionString;
        //    Provider = provider;
        //}

        ///// <summary>
        ///// DB Constructor with connection
        ///// </summary>
        //internal void Init(IDbConnection connection, bool ownConnection = false)
        //{
        //    _Connection = connection;
        //    OwnConnection = ownConnection;
        //    ConnectionString = connection.ConnectionString;
        //    Provider = DbFactory.GetProvider(connection);

        //}


        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        protected virtual IDbCommand CreateCommand(string cmdText)
        {
            SqlFormatter.ValidateSql(cmdText, null);
            if (_Connection == null)
            {
                throw new ArgumentNullException("cnn");
            }

            IDbCommand cmd = _Connection.CreateCommand();
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
        protected virtual IDbCommand CreateCommand(string cmdText, IDbDataParameter[] parameters)
        {
            SqlFormatter.ValidateSql(cmdText, null);
           
            if (_Connection is SqlConnection)
            {
                SqlCommand cmd = new SqlCommand(cmdText, _Connection as SqlConnection);
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddSqlParameters(cmd, parameters);
                    //cmd.Parameters.AddRange(parameters);
                }
                return cmd;
            }
            else
            {
                IDbCommand cmd = _Connection.CreateCommand();
                cmd.CommandText = cmdText;
                if (parameters != null && parameters.Length > 0)
                {
                    DataParameter.AddParameters(cmd, parameters);
                }
                return cmd;
            }

        }

       

        #endregion

        #region Connection

        /// <summary>
        /// Validate if entity has connection properties
        /// </summary>
        /// <exception cref="EntityException"></exception>
        public void ValidateConnectionSettings()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new EntityException("Invalid ConnectionString");
            }
        }

        /// <summary>
        /// ConnectionOpen
        /// </summary>
        /// <param name="cmd"></param>
        internal void ConnectionOpen()
        {
            ValidateConnectionSettings();

            if (_Connection == null)
            {
                Connection.Open();
            }
            else if (_Connection.State == ConnectionState.Closed)
            {
                _Connection.Open();
            }
        }

        /// <summary>
        /// ConnectionAutoClose
        /// </summary>
        /// <param name="dispose"></param>
        internal void ConnectionAutoClose(bool dispose = true)
        {
            if (OwnsConnection)
            {
                return;
            }

            if (_Connection != null)
            {
                if (_Connection.State != ConnectionState.Closed)
                    _Connection.Close();
            }
            if (dispose)
                _Connection.Dispose();
        }

       

   
        /// <summary>
        /// ConnectionClose
        /// </summary>
        protected void ConnectionClose()
        {
            if (_Connection != null)
            {
                if (_Connection.State != ConnectionState.Closed)
                    _Connection.Close();
            }
        }

        #endregion

        #region properties

        ///// <summary>
        ///// Get indicate if DbEntites IsEmpty
        ///// </summary>
        //public bool IsEmpty
        //{
        //    get { return string.IsNullOrEmpty(ConnectionString) /*|| Items.Count == 0*/; }
        //}

        /// <summary>
        /// Get indicate if entity has connection properties
        /// </summary>
        public bool HasConnection
        {
            get
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Get indicate if the connection is valid
        /// </summary>
        public bool IsValidConnection
        {
            get
            {
                if (!HasConnection)
                    return false;
                if (_Connection == null || string.IsNullOrEmpty(_Connection.ConnectionString))
                    return false;
                return true;

            }
        }

        /// <summary>
        /// Get or Set if <see cref="DbContextCommand"/> own the Connection, Default is false
        /// </summary>
        public bool OwnsConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Get or Set ConnectionName
        /// </summary>
        public string ConnectionName
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get or Set Database
        /// </summary>
        public string Database
        {
            get;
            protected set;
        }

        DBProvider m_Provider = DBProvider.SqlServer;
        /// <summary>
        /// Get or Set DBProvider
        /// </summary>
        public DBProvider Provider
        {
            get { return m_Provider; }
            protected set { m_Provider = value; }
        }

        public IDbConnection Connection
        {
            get
            {
                ValidateConnectionSettings();

                if (_Connection == null || string.IsNullOrEmpty(_Connection.ConnectionString))
                {
                    _Connection = CreateConnection();
                }
                return _Connection;
            }
            protected set { _Connection = value; }
        }

        #endregion

        #region Execute none query

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        public int ExecuteCommandNonQuery(string cmdText)
        {
            return ExecuteCommandNonQuery(cmdText, null);
        }
        
        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public int ExecuteCommandNonQuery(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.commandText");
            }
            try
            {
                ConnectionOpen();
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }
        /// <summary>
        /// Executes a command NonQuery and returns <see cref="EntityCommandResult"/> OutptResults and the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public EntityCommandResult ExecuteCommandOutput(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.cmdText");
            }
            try
            {

                ConnectionOpen();

                int res = 0;
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    res = cmd.ExecuteNonQuery();
                    return RenderOutpuResult(cmd, res);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }

                /// <summary>
        /// Executes a command NonQuery and returns the ReturnValue from StoredProcedure.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public int ExecuteCommandReturnValue(string cmdText, IDbDataParameter[] parameters, int returnIfNull, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.cmdText");
            }
            if (parameters == null || parameters.Length==0)
            {
                throw new ArgumentNullException("ExecuteNonQuery.ExecuteCommandOutput.parameters");
            }
            
            try
            {

                var retuenParam= parameters.Where(p => p.Direction == ParameterDirection.ReturnValue).FirstOrDefault();
                if (retuenParam==null)
                {
                    throw new ArgumentException("Invalid ReturnValue Parameter");
                }
                ConnectionOpen();

                //T res = default(T);
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    var res = cmd.ExecuteNonQuery();
                    var result = retuenParam.Value;
                    return GenericTypes.Convert<int>(result, returnIfNull);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }


//        using (SqlConnection conn = new SqlConnection(getConnectionString()))
//using (SqlCommand cmd = conn.CreateCommand())
//{
//    cmd.CommandText = parameterStatement.getQuery();
//    cmd.CommandType = CommandType.StoredProcedure;
//    cmd.Parameters.AddWithValue("SeqName", "SeqNameValue");

//    var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
//    returnParameter.Direction = ParameterDirection.ReturnValue;

//    conn.Open();
//    cmd.ExecuteNonQuery();
//    var result = returnParameter.Value;
//}

 
        protected EntityCommandResult RenderOutpuResult(IDbCommand command, int affectedRecords)
        {
            var outputValues = new Dictionary<string, object>();
            foreach (IDbDataParameter prm in command.Parameters)
            {
                if (prm.Direction != ParameterDirection.Input)
                {
                    outputValues.Add(prm.ParameterName.Replace("@", ""), prm.Value);
                }
            }
            return new EntityCommandResult(affectedRecords, outputValues,null);
        }
      
        #endregion

        #region CommandScalar
        /*
        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        public T ExecuteCommandScalar<T>(string cmdText)
        {
            return ExecuteCommandScalar<T>(cmdText, null,default(T));
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
        public T ExecuteCommandScalar<T>(string cmdText, DataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteScalar.commandText");
            }
            try
            {
                ConnectionOpen();
                using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
                {

                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    object result = cmd.ExecuteScalar();
                    return GenericTypes.Convert<T>(result);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }
        */
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
        public T ExecuteCommandScalar<T>(string cmdText, IDbDataParameter[] parameters, T returnIfNull, CommandType commandType = CommandType.Text, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteScalar.commandText");
            }
            try
            {
                ConnectionOpen();
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {

                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }
                    object result = cmd.ExecuteScalar();

                    return GenericTypes.Convert<T>(result, returnIfNull);
                }

            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return returnIfNull;
            }
            finally
            {
                ConnectionAutoClose();
            }
        }


        #endregion

        #region Execute command

        //public T ExecuteCommand<T>(string mappingName, DataFilter filter, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
        //{
           
        //    if (mappingName == null)
        //    {
        //        throw new ArgumentNullException("ExecuteCommandFilter.mappingName");
        //    }
        //    try
        //    {
        //        string cmdText = filter == null ? SqlFormatter.SelectString(mappingName) : filter.Select(mappingName);
        //        IDbDataParameter[] parameters = filter == null ? null : filter.Parameters;
        //        ConnectionOpen();
        //        using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
        //        {
        //            cmd.CommandType = commandType;
        //            if (commandTimeOut > 0)
        //            {
        //                cmd.CommandTimeout = commandTimeOut;
        //            }
        //            return AdapterFactory.ExecuteDataOrScalar<T>(cmd, null, addWithKey);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        ConnectionAutoClose();
        //    }
        //}

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public T ExecuteCommand<T>(string cmdText, bool addWithKey = false)
        {
            IDbDataParameter[] parameters = null;
            return ExecuteCommand<T>(cmdText, parameters, CommandType.Text, 0, addWithKey);
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
        public TResult ExecuteCommand<TItem, TResult>(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            try
            {
                 ConnectionOpen();
                 using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                 {
                     cmd.CommandType = commandType;
                     if (commandTimeOut > 0)
                     {
                         cmd.CommandTimeout = commandTimeOut;
                     }
                     return ExecuteDataOrScalar<TItem, TResult>(cmd, null, addWithKey);
                 }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
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
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            try
            {
                 ConnectionOpen();
                 using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                 {

                     cmd.CommandType = commandType;
                     if (commandTimeOut > 0)
                     {
                         cmd.CommandTimeout = commandTimeOut;
                     }
                     return ExecuteDataOrScalar<T>(cmd, null, addWithKey);
                 }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
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
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            object value = default(T);
            try
            {
                if (AutoDataCache.DW.GetTryParse(cacheKey, out value))
                {
                    return (T)value;
                }
                ConnectionOpen();
                using (IDbCommand cmd = CreateCommand(cmdText, parameters))
                {
                    cmd.CommandType = commandType;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }

                    value = ExecuteDataOrScalar<T>(cmd, null, addWithKey);

                    if (!string.IsNullOrEmpty(cacheKey))
                    {
                        AutoDataCache.DW[cacheKey] = value;
                    }
                    return (T)value;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionAutoClose();
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
            string cmdText = SqlFormatter.SelectString(mappingName);
            return ExecuteCommand<DataTable>(cmdText, false);
        }
        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string mappingName, bool addWithKey = false)
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
        public DataTable ExecuteDataTable(string tableName, string cmdText, bool addWithKey = false)
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
            DataTable dt = ExecuteCommand<DataTable>(cmdText, null, CommandType.Text, commandTimeout, addWithKey);
            dt.TableName = tableName;
            return dt;
        }
        #endregion

        #region DataAdapter

        /// <summary>
        /// CreateIDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected virtual IDbDataAdapter CreateIDataAdapter(IDbCommand cmd)
        {
            if (cmd is SqlCommand)
                return new SqlDataAdapter((SqlCommand)cmd) as IDbDataAdapter;
            else if (cmd is OleDbCommand)
                return new OleDbDataAdapter((OleDbCommand)cmd) as IDbDataAdapter;
            else
                throw new DalException("Provider not supported,Only SQL Server and OleDb are valid database types");
        }

        /// <summary>
        /// Fill DataTable using DbDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected virtual DataTable ExecuteDataTable(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            DataTable dt = new DataTable(mappingName);
            using (DbDataAdapter dbAdapter = (DbDataAdapter)CreateIDataAdapter(cmd))
            {
                dbAdapter.SelectCommand = (DbCommand)cmd;
                if (addWithKey)
                    dbAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                dbAdapter.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// Fill DataTable using DbDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected virtual DataSet ExecuteDataSet(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(mappingName))
            {
                ds.DataSetName = mappingName;
            }
            using (DbDataAdapter dbAdapter = (DbDataAdapter)CreateIDataAdapter(cmd))
            {
                dbAdapter.SelectCommand = (DbCommand)cmd;
                if (addWithKey)
                    dbAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                dbAdapter.Fill(ds);

            }
            return ds;
        }

        /// <summary>
        /// Executes command and returns T value (DataSet|DataTable|DataRow|IEntityItem) or any type for scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected T ExecuteDataOrScalar<T>(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            Type type = typeof(T);
            object result = null;
            try
            {

                if (type == typeof(DataSet))
                {
                    result = ExecuteDataSet(cmd, mappingName, addWithKey);
                    return (T)result;
                }
                else if (type == typeof(DataTable))
                {
                    result = ExecuteDataTable(cmd, mappingName, addWithKey);
                    return (T)result;
                }
                else if (type == typeof(DataView))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)
                    {
                        result = ((DataTable)dt).DefaultView;
                    }
                    return (T)result;
                }
                else if (type == typeof(DataRow))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Rows[0];
                    return (T)result;
                }
                else if (type == typeof(System.Data.DataRow[]))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Select();
                    return (T)result;
                }
                else if (typeof(IEntityItem).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt.Rows[0].ToEntity<T>();
                    return (T)result;
                }
                else if (type == typeof(GenericRecord))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = GenericRecord.Parse(dt);
                    return (T)result;
                }
                else if (type == typeof(JsonResults))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.ToJsonResult();
                    return (T)result;
                }
                else //if (type == typeof(object))
                {
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message);
            }

            return InternalCmd.ReturnValue<T>(result);
        }


        /// <summary>
        /// Executes command and returns T value (DataSet|DataTable|DataRow|IEntityItem|List of IEntityItem) or any type for scalar.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        protected TResult ExecuteDataOrScalar<TItem, TResult>(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            Type type = typeof(TResult);
            object result = null;
            try
            {

                if (type == typeof(DataSet))
                {
                    result = ExecuteDataSet(cmd, mappingName, addWithKey);
                    return (TResult)result;
                }
                else if (type == typeof(DataTable))
                {
                    result = ExecuteDataTable(cmd, mappingName, addWithKey);
                    return (TResult)result;
                }
                else if (type == typeof(DataView))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)
                    {
                        result = ((DataTable)dt).DefaultView;
                    }
                    return (TResult)result;
                }
                else if (type == typeof(DataRow))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Rows[0];
                    return (TResult)result;
                }
                else if (type == typeof(System.Data.DataRow[]))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Select();
                    return (TResult)result;
                }
                else if (typeof(IEntityItem).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt.Rows[0].ToEntity<TResult>();
                    return (TResult)result;
                }
                else if (typeof(IEntityItem[]).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)// && dt.Rows.Count > 0)
                        result = dt.EntityList<TItem>().ToArray();
                    return (TResult)result;
                }
                else if (AdapterFactory.IsAssignableOfList(type, typeof(IEntityItem)))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null)//&& dt.Rows.Count > 0)
                        result = dt.EntityList<TItem>();
                    return (TResult)result;
                }
                else if (type == typeof(GenericRecord))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = GenericRecord.Parse(dt);
                    return (TResult)result;
                }
                else if (type == typeof(JsonResults))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.ToJsonResult();
                    return (TResult)result;
                }
                else //if (type == typeof(object))
                {
                    result = cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new DalException(ex.Message);
            }

            return InternalCmd.ReturnValue<TResult>(result);
        }

        #endregion

        #region Execute Reader

        /// <summary>
        /// GetCommand
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        internal IDbCommand GetCommand(string cmdText)
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

        #region static

        /// <summary>
        /// Create Command
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cnn"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static IDbCommand CreateCommand(string cmdText, IDbConnection cnn, IDbDataParameter[] parameters)
        {
            SqlFormatter.ValidateSql(cmdText, null);
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

        #endregion
    }

}
