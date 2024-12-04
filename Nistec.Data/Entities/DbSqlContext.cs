using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{
    /// <summary>
    /// DbSqlContext is a DbContext of SqlServer with the ability to pass DataTable to Procedure
    /// </summary>
    public class DbSqlContext
    {
        /// <summary>
        /// Getnew instance of DbSqlContext
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <returns></returns>
        public static DbSqlContext Get<Dbc>() where Dbc : IDbContext
        {
            DbContextAttribute[] attributes = typeof(Dbc).GetCustomAttributes<DbContextAttribute>();
            if (attributes == null || attributes.Length == 0)
                return null;
            return new DbSqlContext(attributes[0]);
        }
        /// <summary>
        /// ctor
        /// </summary>
        protected DbSqlContext(DbContextAttribute attr)
        {
            ConnectionString = attr.ConnectionString;
            ConnectionKey = attr.ConnectionKey;
        }
        /// <summary>
        /// ConnectionString
        /// </summary>
        public string ConnectionString { get; private set; }
        /// <summary>
        /// ConnectionKey
        /// </summary>
        public string ConnectionKey { get; private set; }
        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command,
        /// The time in seconds to wait for the command to execute. The default is 30 seconds.
        /// </summary>
        public int CommandTimeout { get; set; }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionKey"></param>
        public DbSqlContext(string connectionKey)
        {
            SetCnn(connectionKey);
            //ConnectionString = NetConfig.AppSettings[connectionKey];
        }

        /// <summary>
        /// GetParameters
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        protected List<SqlParameter> GetParameters(object[] keyValueParameters)
        {
            if (keyValueParameters == null)
                throw new ArgumentNullException("DbSqlContext.GetParameters");

            var parameters = Nistec.Data.DataParameter.GetSqlList(keyValueParameters);
            foreach (var parameter in parameters)
            {
                if (parameter.Value != null && parameter.Value.GetType() == typeof(DataTable))
                {
                    parameter.SqlDbType = System.Data.SqlDbType.Structured;
                    parameter.TypeName = ((DataTable)parameter.Value).TableName;// "dbo.typTransItems";
                }
            }
            return parameters;
        }
        /// <summary>
        /// ValidateArgumets
        /// </summary>
        /// <param name="procName"></param>
        protected void ValidateArgumets(string procName)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new ArgumentNullException("DbSqlContext.ValidateArgumets.ConnectionString");
            if (string.IsNullOrEmpty(procName))
                throw new ArgumentNullException("DbSqlContext.ValidateArgumets.procName");
            if (CommandTimeout <= 0)
                CommandTimeout = 30;
        }

       /// <summary>
       /// Set connection string
       /// </summary>
       /// <param name="connectionKey"></param>
        protected void SetCnn(string connectionKey)
        {
            //Max Pool Size=100;
            this.ConnectionKey = connectionKey;
            var connectionSettings=NetConfig.ConnectionContext(ConnectionKey);
            if (connectionSettings == null)
            {
                throw new InvalidOperationException("Invalid connection settings for key:" + ConnectionKey);
            }
            this.ConnectionString = connectionSettings.ConnectionString;
            //return NetConfig.AppSettings[ConnectionKey];
        }
        /// <summary>
        /// ExecProcReturnValue
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public int ExecProcReturnValue(string procName, params object[] keyValueParameters)
        {
            int result;

            ValidateArgumets(procName);
            var parameters = GetParameters(keyValueParameters);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(procName, connection))
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters.ToArray());
                    var returnParameter = command.Parameters.Add("@ReturnVal", System.Data.SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    result = Types.ToInt(returnParameter.Value);
                }
            }
            return result;
        }
        /// <summary>
        /// ExecProcScalar
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public T ExecProcScalar<T>(string procName, T returnIfNull, params object[] keyValueParameters)
        {
            T result;
            ValidateArgumets(procName);
            var parameters = GetParameters(keyValueParameters);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(procName, connection))
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters.ToArray());
                    connection.Open();
                    result = Nistec.GenericTypes.Convert<T>(command.ExecuteScalar(), returnIfNull);
                }
            }
            return result;
        }
        /// <summary>
        /// ExecProcDataRow
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public DataRow ExecProcDataRow(string procName, params object[] keyValueParameters)
        {
            DataTable result = ExecProcDataTable(procName, keyValueParameters);
            if (result != null && result.Rows.Count > 0)
                return result.Rows[0];
            return null;
        }
        /// <summary>
        /// ExecProcEnitiy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public T ExecProcEnitiy<T>(string procName, params object[] keyValueParameters)
        {
            DataRow result = ExecProcDataRow(procName, keyValueParameters);
            if (result != null)
                return Nistec.Data.Entities.EntityDataExtension.DataRowToEntity<T>(result);
            return Nistec.GenericTypes.Default<T>();
        }
        /// <summary>
        /// ExecProcJson
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public string ExecProcJson(string procName, params object[] keyValueParameters)
        {
            DataTable result = ExecProcDataTable(procName, keyValueParameters);
            if (result != null && result.Rows.Count > 0)
                return Nistec.Serialization.JsonSerializer.Serialize(result);
            return null;
        }
        /// <summary>
        /// ExecProcJsonRecord
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public string ExecProcJsonRecord(string procName, params object[] keyValueParameters)
        {
            DataRow result = ExecProcDataRow(procName, keyValueParameters);
            if (result != null)
                return Nistec.Serialization.JsonSerializer.Serialize(result);
            return null;
        }
        /// <summary>
        /// ExecuteDictionary
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public IList<Dictionary<string, object>> ExecuteDictionary(string procName, params object[] keyValueParameters)
        {
            DataTable result = ExecProcDataTable(procName, keyValueParameters);
            if (result != null && result.Rows.Count > 0)
                return Nistec.Data.DataExtension.ToListDictionary(result);
            return null;
        }
        /// <summary>
        /// ExecuteDictionaryRecord
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public Dictionary<string, object> ExecuteDictionaryRecord(string procName, params object[] keyValueParameters)
        {
            IList<Dictionary<string, object>> result = ExecuteDictionary(procName, keyValueParameters);
            if (result != null && result.Count > 0)
                return result[0];
            return null;
        }
        /// <summary>
        /// ExecProcDataTable
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public DataTable ExecProcDataTable(string procName, params object[] keyValueParameters)
        {
            DataTable result = new DataTable();
            ValidateArgumets(procName);
            var parameters = GetParameters(keyValueParameters);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(procName, connection))
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters.ToArray());
                    using (SqlDataAdapter adp = new SqlDataAdapter(command))
                    {
                        adp.Fill(result);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// ExecProcDataSet
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public DataSet ExecProcDataSet(string procName, params object[] keyValueParameters)
        {
            DataSet result = new DataSet();
            ValidateArgumets(procName);
            var parameters = GetParameters(keyValueParameters);
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(procName, connection))
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(parameters.ToArray());
                    using (SqlDataAdapter adp = new SqlDataAdapter(command))
                    {
                        adp.Fill(result);
                    }
                }
            }
            return result;
        }
    }
}
