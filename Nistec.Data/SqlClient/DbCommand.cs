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
using System.Collections;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

using Debug = System.Diagnostics.Debug;
using StackTrace = System.Diagnostics.StackTrace;
using Nistec.Data.Entities;
using Nistec.Data.Factory;
using System.Text;

namespace Nistec.Data.SqlClient
{

    #region generic DbCommand

    #region summary
    /// <summary>
    /// Represent an object that execute sql commands and implements <see cref="IAutoDb"/>.
    /// </summary>
    /// <example >
    /// <code >
    /// 
    ///public class AdventureWorksCommand : Nistec.Data.SqlClient.DbCommand<AdventureWorks>
    ///{
    ///
    ///    public static AdventureWorksCommand Instance
    ///    {
    ///        get { return new AdventureWorksCommand(); }
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Text, "SELECT * FROM Person.Contact", null, MissingSchemaAction.AddWithKey)]
    ///    public DataTable Contacts()
    ///    {
    ///        return (DataTable)base.Execute();
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailAddress FROM Person.Contact where ContactID=@ContactID", null)]
    ///    public string Contact_Email(int ContactID)
    ///    {
    ///        return (string)base.Execute(ContactID);
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailPromotion FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    public int Contact_EmailPromotion(int ContactID)
    ///    {
    ///        return (int)base.Execute(ContactID);
    ///    }
    ///
    ///    [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    public DataRow Contact(int ContactID)
    ///    {
    ///        return (DataRow)base.Execute(ContactID);
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Update, "Person.Contact")]
    ///    public int Contact_Update
    ///        (
    ///        [DalParam(DalParamType.Key)] int ContactID,
    ///        [DalParam()]DateTime ModifiedDate,
    ///        [DalParam(24)]string Phone
    ///        )
    ///    {
    ///        return (int)base.Execute(ContactID, ModifiedDate, Phone);
    ///
    ///    }
    ///}
    ///
    /// </code>
    /// </example>
    #endregion

    public class DbCommand<T> : DbCommand where T : IDbContext
    {
        public DbCommand()
        {
            base.Init<T>();
        }
    }
    #endregion

    #region summary
    /// <summary>
    /// Represent an object that execute sql commands and implements <see cref="IAutoDb"/>.
    /// </summary>
    /// <example >
    /// <code >
    /// 
    ///public class AdventureWorksCommand : Nistec.Data.SqlClient.DbCommand
    ///{
    ///
    ///    public AdventureWorksCommand()
    ///      : base(DBConfig.ConnectionString)
    ///    {
    ///    }
    ///   
    ///    public static AdventureWorksCommand Instance
    ///    {
    ///        get { return new AdventureWorksCommand(); }
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Text, "SELECT * FROM Person.Contact", null, MissingSchemaAction.AddWithKey)]
    ///    public DataTable Contacts()
    ///    {
    ///        return (DataTable)base.Execute();
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailAddress FROM Person.Contact where ContactID=@ContactID", null)]
    ///    public string Contact_Email(int ContactID)
    ///    {
    ///        return (string)base.Execute(ContactID);
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailPromotion FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    public int Contact_EmailPromotion(int ContactID)
    ///    {
    ///        return (int)base.Execute(ContactID);
    ///    }
    ///
    ///    [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    public DataRow Contact(int ContactID)
    ///    {
    ///        return (DataRow)base.Execute(ContactID);
    ///    }
    ///
    ///    [DBCommand(DBCommandType.Update, "Person.Contact")]
    ///    public int Contact_Update
    ///        (
    ///        [DalParam(DalParamType.Key)] int ContactID,
    ///        [DalParam()]DateTime ModifiedDate,
    ///        [DalParam(24)]string Phone
    ///        )
    ///    {
    ///        return (int)base.Execute(ContactID, ModifiedDate, Phone);
    ///
    ///    }
    ///}
    ///
    /// </code>
    /// </example>
    #endregion
    public class DbCommand : Nistec.Data.Factory.CommandFactory, IDisposable//,IDalCommand
    {

        #region Constructor
        internal DbCommand()
        {
        }

        /// <summary>
        /// DbCommand Constructor with AutoBase. 
        /// </summary>
        public DbCommand(IAutoBase dalBase)//:base(dalBase)
        {
            base.Init(dalBase);
        }


        /// <summary>
        /// DbCommand Constructor with connection string
        /// </summary>
        public DbCommand(string connectionString)
       {
            base.Init(connectionString, DBProvider.SqlServer, true);
        }


        /// <summary>
        /// DbCommand Constructor with connection
        /// </summary>
        public DbCommand(SqlConnection connection, bool autoCloseConnection)
        {
            base.Init(connection, autoCloseConnection);
        }

        /// <summary>
        /// Class destructor.
        /// </summary>
        ~DbCommand()
        {
            Dispose(false);
        }

        #endregion

        #region override Common.DbCommand

        /// <summary>
        /// Build Command Text
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="TableName"></param>
        /// <param name="cmdPart1"></param>
        /// <param name="cmdPart2"></param>
        /// <param name="cmdPart3"></param>
        /// <param name="cmdPart4"></param>
        /// <param name="autNumberField"></param>
        /// <returns></returns>
        internal override string BuildCommandText(IDbCommand command, DBCommandType commandType, string TableName, string cmdPart1, string cmdPart2,string cmdPart3,string cmdPart4, string autNumberField)
        {
            return BuildCommandTextInternal(command, commandType, TableName, cmdPart1, cmdPart2,cmdPart3,cmdPart4, autNumberField);
        }
        /// <summary>
        /// Create Adapter as <see cref="IDbAdapter"/>
        /// </summary>
        /// <returns></returns>
        protected override IDbAdapter CreateAdapter()
        {
            IDbAdapter adapter = new DbAdapter(m_connection as SqlConnection);
            return adapter;
        }

        internal static string BuildCommandTextInternal(IDbCommand command, DBCommandType commandType, string TableName, string cmdPart1, string cmdPart2, string cmdPart3, string cmdPart4, string autNumberField)
        {
            string cmdString = null;
            string CommandText = "";

            if (commandType == DBCommandType.InsertOrUpdate)
            {

                cmdString = SqlFormatter.InsertOrUpdateString(TableName, cmdPart1, cmdPart2, cmdPart3,cmdPart4);

                if (autNumberField == "")
                {
                    CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                }
                else
                {
                    CommandText += String.Format("if(@{0} is NULL) begin {1} select @{0} = SCOPE_IDENTITY() end ", autNumberField, cmdString);
                    CommandText += String.Format("else begin {0} end", cmdString);
                }

                command.CommandText = CommandText;
            }

            else if (commandType == DBCommandType.InsertNotExists)
            {

                cmdString = SqlFormatter.InsertNotExistsString(TableName, cmdPart1, cmdPart2, cmdPart4);

                if (autNumberField == "")
                {
                    CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                }
                else
                {
                    CommandText += String.Format("if(@{0} is NULL) begin {1} select @{0} = SCOPE_IDENTITY() end ", autNumberField, cmdString);
                    CommandText += String.Format("else begin {0} end", cmdString);
                }

                command.CommandText = CommandText;
            }
            else if (commandType == DBCommandType.Insert)
            {

                cmdString = SqlFormatter.InsertString(TableName, cmdPart1, cmdPart2);// String.Format(" INSERT INTO [{0}]({1}) VALUES({2}) ", TableName, cmdPart1, cmdPart2);

                if (autNumberField == "")
                {
                    CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                }
                else
                {
                    CommandText += String.Format("if(@{0} is NULL) begin {1} select @{0} = SCOPE_IDENTITY() end ", autNumberField, cmdString);
                    CommandText += String.Format("else begin {0} end", cmdString);
                }

                command.CommandText = CommandText;
            }
            else if (commandType == DBCommandType.Update)
            {
                if (cmdPart2 == "")
                {
                    throw new DalException("No Identity or Autoincrement field is defined.");
                }

                cmdString = SqlFormatter.UpdateString(TableName, cmdPart1, cmdPart2);// String.Format(" UPDATE [{0}] SET {1} WHERE {2} ", TableName, cmdPart1, cmdPart2);
               CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                command.CommandText = CommandText;

            }
            else if (commandType == DBCommandType.Delete)
            {
                if (cmdPart2 == "")
                {
                    throw new DalException("No Identity or Autoincrement field is defined.");
                }

                cmdString = SqlFormatter.DeleteString(TableName, cmdPart2);// String.Format(" DELETE FROM {0} where {1} ", TableName, cmdPart2);
                CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                command.CommandText = CommandText;
            }
            return command.CommandText;

        }
        #endregion

        #region override AutoDb
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
        internal override object ExecuteCmd(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
        {
            return ExecuteInternal(connection, transaction, method, values, autoCloseConnection, commandTimeOut);
        }

        #endregion

        #region Async execute
        /// <summary>
        /// Executes Async sql string and returns the number of row affected.
        /// </summary>
        /// <param name="sql">Sql string.</param>
        /// <param name="interval"></param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="WaitForDelay">a few seconds before retrieving the real data use for a long-running query, default =0</param>
        /// <returns>number of row affected</returns>
        protected virtual int ExecuteAsyncCommand(string sql, int interval, int commandTimeOut, int WaitForDelay)
        {
            SqlCommand cmd = null;
            string cmdText = sql;
            if (WaitForDelay > 0)
            {
                cmdText = cmdText.Insert(0, string.Format("WAITFOR DELAY '00:00:{0}';", WaitForDelay));
            }
            cmd = new SqlCommand(sql, (SqlConnection)m_connection);
            cmd.CommandTimeout = commandTimeOut;
            cmd.Connection = AsyncConnection;
            return InternalCmd.RunAsyncCommand(cmd, m_autoCloseConnection, interval);
        }

        /// <summary>
        /// AsyncExecute
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        /// <param name="WaitForDelay">a few seconds before retrieving the real data use for a long-running query, default =0</param>
        public IDataReader ExecuteAsyncReader(string sql, int interval, int commandTimeOut, int WaitForDelay)
        {

            SqlCommand cmd = null;
            try
            {
                string cmdText = sql;
                if (WaitForDelay > 0)
                {
                    cmdText = cmdText.Insert(0, string.Format("WAITFOR DELAY '00:00:{0}';", WaitForDelay));
                }
                cmd = new SqlCommand(sql, (SqlConnection)m_connection);
                if (commandTimeOut > 0)
                {
                    cmd.CommandTimeout = commandTimeOut;
                }
                 cmd.Connection = AsyncConnection;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RunAsyncReader(cmd, interval);
        }

        /// <summary>
        /// RunAsyncReader, asynchronously execute
        /// the specified command against the connection. 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="interval">Waiting time for async thread, default =0</param>
        /// <returns>IDataReader</returns>
        public static IDataReader RunAsyncReader(SqlCommand cmd, int interval)
        {
            using (cmd.Connection)
            {
                try
                {
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }

                    IAsyncResult aresult = cmd.BeginExecuteReader(CommandBehavior.CloseConnection);

                    //int count = 0;
                    while (!aresult.IsCompleted)
                    {
                        //Console.WriteLine("Waiting ({0})", count++);
                        // Wait for 1/10 second, so the counter
                        // does not consume all available resources 
                        // on the main thread.
                        System.Threading.Thread.Sleep(interval);
                    }

                    SqlDataReader reader = cmd.EndExecuteReader(aresult);
                    return reader;

                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new DalException(ex.Message);
                }
                catch (InvalidOperationException oex)
                {
                    throw new DalException(oex.Message);
                }
                catch (DalException dbex)
                {
                    throw new DalException(dbex.Message);
                }
                catch (Exception e)
                {
                    throw new DalException(e.Message);
                }
            }
        }

        #endregion

        #region BulkCopy

        /// <summary>
        /// Execute Bulk Copy
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="columnMapping"></param>
        protected virtual void ExecuteBulkCopy(string sql, string destinationTableName, MAPPING[] columnMapping)
        {
            SqlCommand cmd = new SqlCommand(sql, m_connection as SqlConnection);
            SqlDataReader reader = (SqlDataReader)InternalCmd.RunCommand<IDataReader>(cmd, m_autoCloseConnection, MissingSchemaAction.Add);

            using (SqlBulkCopy bcp =
                    new SqlBulkCopy(m_connection as SqlConnection))
            {
                try
                {
                    if (m_connection.State == ConnectionState.Closed)
                    {
                        m_connection.Open();
                    }
                    if (columnMapping != null)
                    {
                        foreach (MAPPING m in columnMapping)
                        {
                            bcp.ColumnMappings.Add(m.SourceColumnName, m.DestColumnName);
                        }
                    }

                    bcp.DestinationTableName =
                        destinationTableName;
                    // Write from the source to 
                    // the destination.

                    bcp.WriteToServer(reader);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new DalException(ex.Message);
                }
                catch (Exception dbex)
                {
                    throw new DalException(dbex.Message);
                }
                finally
                {
                    if (this.AutoCloseConnection)
                    {
                        if (m_connection.State == ConnectionState.Open)
                            m_connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Execute Bulk Copy
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="columnMapping"></param>
        /// <param name="batchSize"></param>
        /// <param name="timeout"></param>
        protected virtual void ExecuteBulkCopy(string sql, string destinationTableName, MAPPING[] columnMapping, int batchSize, int timeout)
        {
            SqlCommand cmd = new SqlCommand(sql, m_connection as SqlConnection);
            SqlDataReader reader = (SqlDataReader)InternalCmd.RunCommand<IDataReader>(cmd, m_autoCloseConnection, MissingSchemaAction.Add);

            using (SqlBulkCopy bcp =
                    new SqlBulkCopy(m_connection as SqlConnection))
            {
                try
                {
                    if (m_connection.State == ConnectionState.Closed)
                    {
                        m_connection.Open();
                    }
                    if (columnMapping != null)
                    {
                        foreach (MAPPING m in columnMapping)
                        {
                            bcp.ColumnMappings.Add(m.SourceColumnName, m.DestColumnName);
                        }
                    }

                    bcp.BatchSize = batchSize;
                    bcp.BulkCopyTimeout = timeout;

                    bcp.DestinationTableName =
                        destinationTableName;
                    // Write from the source to 
                    // the destination.

                    bcp.WriteToServer(reader);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new DalException(ex.Message);
                }
                catch (Exception dbex)
                {
                    throw new DalException(dbex.Message);
                }
                finally
                {
                    if (this.AutoCloseConnection)
                    {
                        if (m_connection.State == ConnectionState.Open)
                            m_connection.Close();
                    }
                }
            }
        }


        /// <summary>
        /// Execute Bulk Copy
        /// </summary>
        /// <param name="table"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="columnMapping"></param>
        protected virtual void ExecuteBulkCopy(DataTable table, string destinationTableName, MAPPING[] columnMapping)
        {
            using (SqlBulkCopy bcp =
                   new SqlBulkCopy(m_connection as SqlConnection))
            {
                try
                {
                    if (m_connection.State == ConnectionState.Closed)
                    {
                        m_connection.Open();
                    }
                    if (columnMapping != null)
                    {
                        foreach (MAPPING m in columnMapping)
                        {
                            bcp.ColumnMappings.Add(m.SourceColumnName, m.DestColumnName);
                        }
                    }
                    bcp.DestinationTableName =
                        destinationTableName;
                    bcp.WriteToServer(table);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new DalException(ex.Message);
                }
                catch (Exception dbex)
                {
                    throw new DalException(dbex.Message);
                }
                finally
                {
                    if (this.AutoCloseConnection)
                    {
                        if (m_connection.State == ConnectionState.Open)
                            m_connection.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Execute Bulk Copy
        /// </summary>
        /// <param name="table"></param>
        /// <param name="state"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="columnMapping"></param>
        protected virtual void ExecuteBulkCopy(DataTable table, DataRowState state, string destinationTableName, MAPPING[] columnMapping)
        {
            using (SqlBulkCopy bcp =
                   new SqlBulkCopy(m_connection as SqlConnection))
            {
                try
                {
                    if (m_connection.State == ConnectionState.Closed)
                    {
                        m_connection.Open();
                    }
                    if (columnMapping != null)
                    {
                        foreach (MAPPING m in columnMapping)
                        {
                            bcp.ColumnMappings.Add(m.SourceColumnName, m.DestColumnName);
                        }
                    }
                    bcp.DestinationTableName =
                        destinationTableName;
                    bcp.WriteToServer(table, state);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new DalException(ex.Message);
                }
                catch (Exception dbex)
                {
                    throw new DalException(dbex.Message);
                }
                finally
                {
                    if (this.AutoCloseConnection)
                    {
                        if (m_connection.State == ConnectionState.Open)
                            m_connection.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Execute Bulk Copy
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="columnMapping"></param>
        protected virtual void ExecuteBulkCopy(DataRow[] rows, string destinationTableName, MAPPING[] columnMapping)
        {
            using (SqlBulkCopy bcp =
                   new SqlBulkCopy(m_connection as SqlConnection))
            {
                try
                {
                    if (m_connection.State == ConnectionState.Closed)
                    {
                        m_connection.Open();
                    }
                    if (columnMapping != null)
                    {
                        foreach (MAPPING m in columnMapping)
                        {
                            bcp.ColumnMappings.Add(m.SourceColumnName, m.DestColumnName);
                        }
                    }
                    bcp.DestinationTableName =
                        destinationTableName;
                    bcp.WriteToServer(rows);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new DalException(ex.Message);
                }
                catch (Exception dbex)
                {
                    throw new DalException(dbex.Message);
                }
                finally
                {
                    if (this.AutoCloseConnection)
                    {
                        if (m_connection.State == ConnectionState.Open)
                            m_connection.Close();
                    }
                }

            }
        }

        #endregion

 
    }
}



