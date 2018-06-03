using Nistec.Data.Advanced;
using Nistec.Data.Entities;
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Nistec.Data.Sqlite
{
    public class DbCommand : DbContextCommand, IDisposable
    {
        #region Ctor
        /// <summary>
        /// ctor
        /// </summary>
        public DbCommand() : base() { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cnn"></param>
        public DbCommand(IDbConnection cnn)
            : base(cnn)
        {
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbCommand(string connectionString)
            : base(connectionString)
        {
        }

        #endregion

        #region Dispose

        //private bool disposed = false;

        //protected void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            if (_Command != null)
        //            {
        //                _Command.Dispose();
        //                _Command = null;
        //            }
        //            ConnectionName = null;
        //            ConnectionString = null;
        //            Database = null;
        //        }
        //        disposed = true;
        //    }
        //}

        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // take this object off the finalization queue     
            GC.SuppressFinalize(this);
        }

        ~DbCommand()
        {
            Dispose(false);
        }

        #endregion
    }

    public abstract class DbContextCommand
    {

        IDbConnection _Connection;

        #region IDbConnection

      
        ///// <summary>
        ///// Create IDbConnection like "Data Source=MyDatabase.sqlite;Version=3;"
        ///// </summary>
        ///// <param name="connectionString"></param>
        ///// <param name="provider"></param>
        ///// <returns></returns>
        //public static IDbConnection CreateConnection(string connectionString)
        //{
        //        return new SQLiteConnection(connectionString);
        //}

        /// <summary>
        /// Create IDbConnection
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="isConnectionString"></param>
        /// <returns></returns>
        public static IDbConnection CreateConnection(string connectionKey, bool isConnectionString=false)
        {
            if(isConnectionString)
                return new SQLiteConnection(connectionKey);
            ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
            if (cnn == null)
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            return new SQLiteConnection(cnn.ConnectionString);
        }
       

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
            Init(cnn);
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbContextCommand(string connectionString)
        {
            Init(connectionString);
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

        #region init
              

        /// <summary>
        /// DB Constructor with IDbContext
        /// </summary>
        internal void Init<T>() where T : IDbContext
        {
            IDbContext db = System.Activator.CreateInstance<T>();
            ConnectionString = db.ConnectionString;
            //Provider = db.Provider;
            _Connection = new SQLiteConnection(ConnectionString);
            OwnConnection = false;
        }


        /// <summary>
        /// DB Constructor with connection string
        /// </summary>
        internal void Init(string connectionString, bool ownConnection=false)
        {
            _Connection = new SQLiteConnection(ConnectionString);
            OwnConnection = ownConnection;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// DB Constructor with connection
        /// </summary>
        internal void Init(IDbConnection connection, bool ownConnection = false)
        {
            _Connection = connection;
            OwnConnection = ownConnection;
            ConnectionString = connection.ConnectionString;

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
        /// <param name="cmd"></param>
        internal void ConnectionAutoClose(bool dispose = true)
        {
            if (OwnConnection)
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

        /// <summary>
        /// Get indicate if DbEntites IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(ConnectionString) /*|| Items.Count == 0*/; }
        }

        /// <summary>
        /// Get or Set if <see cref="DbContextCommand"/> own the Connection, Default is false
        /// </summary>
        public bool OwnConnection
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
            set;
        }

        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Get or Set Database
        /// </summary>
        public string Database
        {
            get;
            set;
        }

        DBProvider m_Provider = DBProvider.SQLite;
        /// <summary>
        /// Get or Set DBProvider
        /// </summary>
        public DBProvider Provider
        {
            get { return m_Provider; }
            set { m_Provider = value; }
        }

        public IDbConnection Connection
        {
            get
            {
                ValidateConnectionSettings();

                if (_Connection == null)
                {
                    _Connection = CreateConnection(ConnectionString);
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
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        public int ExecuteCommandNonQuery(string cmdText, IDbDataParameter[] parameters, int commandTimeOut = 0)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.commandText");
            }
            try
            {
                ConnectionOpen();
                using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
                {
                    cmd.CommandType = CommandType.Text;
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

        #endregion

        #region CommandScalar

        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        public T ExecuteCommandScalar<T>(string cmdText)
        {
            return ExecuteCommandScalar<T>(cmdText, null);
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
        public T ExecuteCommandScalar<T>(string cmdText, DataParameter[] parameters, int commandTimeOut = 0)
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

                    cmd.CommandType = CommandType.Text;
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
        public T ExecuteCommandScalar<T>(string cmdText, IDbDataParameter[] parameters, T returnIfNull, int commandTimeOut)
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

                    cmd.CommandType = CommandType.Text;
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
            return ExecuteCommand<T>(cmdText, parameters, 0, addWithKey);
        }

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) .
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        public TResult ExecuteCommand<TItem, TResult>(string cmdText, IDbDataParameter[] parameters,  int commandTimeOut = 0, bool addWithKey = false)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            try
            {
                 ConnectionOpen();
                 using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
                 {
                     cmd.CommandType = CommandType.Text;
                     if (commandTimeOut > 0)
                     {
                         cmd.CommandTimeout = commandTimeOut;
                     }
                     return AdapterFactory.ExecuteDataOrScalar<TItem, TResult>(cmd, null, addWithKey);
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
        public T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, int commandTimeOut = 0, bool addWithKey = false)
        {
            if (cmdText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            try
            {
                 ConnectionOpen();
                 using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
                 {

                     cmd.CommandType = CommandType.Text;
                     if (commandTimeOut > 0)
                     {
                         cmd.CommandTimeout = commandTimeOut;
                     }
                     return AdapterFactory.ExecuteDataOrScalar<T>(cmd, null, addWithKey);
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
        public T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, string cacheKey, int commandTimeOut = 0, bool addWithKey = false)
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
                using (IDbCommand cmd = DbFactory.CreateCommand(cmdText, _Connection, parameters))
                {
                    cmd.CommandType = CommandType.Text;
                    if (commandTimeOut > 0)
                    {
                        cmd.CommandTimeout = commandTimeOut;
                    }

                    value = AdapterFactory.ExecuteDataOrScalar<T>(cmd, null, addWithKey);

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
            DataTable dt = ExecuteCommand<DataTable>(cmdText, null, commandTimeout, addWithKey);
            dt.TableName = tableName;
            return dt;
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

            IDbCommand cmd = cnn.CreateCommand();
            cmd.CommandText = cmdText;
            if (parameters != null && parameters.Length > 0)
            {
                DataParameter.AddParameters(cmd, parameters);
            }
            return cmd;

        }

        #endregion
    }

}
