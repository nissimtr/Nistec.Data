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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Serialization;
using Nistec.Runtime;
#pragma warning disable CS1591
namespace Nistec.Data.Factory
{
   
    public abstract class AdapterFactory : IDbAdapter
    {

        #region Dispose

        private bool disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (m_Connection != null)
                    {
                        if ((m_Connection.State != ConnectionState.Closed))// && m_ownsConnection) 
                        {
                            try
                            {
                                m_Connection.Close();
                                m_Connection.Dispose();
                            }
                            catch { }
                        }
                        m_Connection = null;
                    }
                    m_connectionString = null;
                   
                }

                //dispose unmanaged resources here.
                disposed = true;
            }
        }
        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);     
         
            // take this object off the finalization queue     
            GC.SuppressFinalize(this); 
        }

        #endregion

        #region Members
        internal System.Data.IDbDataAdapter m_dataAdapter;
        internal System.Data.IDbConnection m_Connection;
        internal System.Data.IDbTransaction m_transaction;
        internal string m_connectionString;
        internal MissingSchemaAction m_MissingSchemaAction = MissingSchemaAction.Add;
        private string[] m_tableMapping;

        #endregion

        #region Ctor

        internal AdapterFactory(IDbConnection cnn)
        {
            m_Connection = cnn;
        }
        internal AdapterFactory(IDbCommand cmd, MissingSchemaAction schemaAction)
        {
            m_Connection = cmd.Connection;
            m_MissingSchemaAction = schemaAction;
            this.CreateDataAdapter(cmd);
        }
        internal AdapterFactory(string connectionString, DBProvider provider)
        {
            m_connectionString = connectionString;
            m_Connection = new System.Data.SqlClient.SqlConnection(connectionString);
        }
       
        private void ConnectionOpen()
        {
            if (m_Connection == null)
                return;
            if (m_Connection.State == ConnectionState.Closed)
            {
                m_Connection.Open();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get DBProvider
        /// </summary>
        public abstract DBProvider Provider { get; }
        /// <summary>
        /// Get or Set SchemaAction
        /// Specifies the action to take when adding data to the System.Data.DataSet
        /// and the required System.Data.DataTable or System.Data.DataColumn is missing.
        /// </summary>
        public MissingSchemaAction SchemaAction
        {
            get { return m_MissingSchemaAction; }
            set { m_MissingSchemaAction = value; }
        }
        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get { return m_connectionString; }
            set { m_connectionString = value; }
        }
        /// <summary>
        /// Get or Set Connection
        /// </summary>
        public System.Data.IDbConnection Connection
        {
            get { return m_Connection; }
            set { m_Connection = value; }
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
        /// <summary>
        /// Get Valid connection
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public System.Data.IDbConnection DbConnection
        {
            get
            {
                if (Connection == null)
                {
                    throw new InvalidOperationException("Invalid connection, connection not initilaized");
                }
                return m_Connection;
            }
        }
        /// <summary>
        /// SetConnection
        /// </summary>
        /// <param name="connectionString"></param>
        public void SetConnection(string connectionString)
        {
            if (m_Connection != null)
            {
                if (m_Connection.State == ConnectionState.Open)
                    m_Connection.Close();
                m_Connection = null;
            }
            m_connectionString = connectionString;
            m_Connection = DbFactory.CreateConnection(m_connectionString, Provider);
        }
        /// <summary>
        /// Get DataAdapter
        /// </summary>
        ///<exception cref="InvalidOperationException"></exception>
        public System.Data.IDbDataAdapter DataAdapter
        {
            get
            {
                if (m_dataAdapter == null)
                {
                    throw new InvalidOperationException("DataAdapter not initilaized");
                }
                return m_dataAdapter;
            }
            set { m_dataAdapter = value; }
        }
        /// <summary>
        /// Get DataTableMapping
        /// </summary>
        public ITableMappingCollection DataTableMapping
        {
            get { return DataAdapter.TableMappings; }
        }

        /// <summary>
        /// Get or Set the order of readed tables for DataSet adapter
        /// </summary>
        public string[] TableMappings
        {
            get
            {
                return m_tableMapping;
            }
            set
            {
                m_tableMapping = value;
            }
        }
        #endregion

        #region static IDbAdapter
        /// <summary>
        /// CreateAdapter
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static IDbAdapter CreateAdapter(Ado.ConnectionProvider cp)
        {
            cp.CreateConnectionString();
            return CreateAdapter(cp.ConnectionString, cp.Provider);
        }
        /// <summary>
        /// CreateAdapter
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        public static IDbAdapter CreateAdapter(Entities.IDbContext cn)
        {
            if (cn == null)
            {
                throw new ArgumentNullException("cn");
            }
            return CreateAdapter(cn.ConnectionString, cn.Provider);
        }
        /// <summary>
        /// CreateAdapter
        /// </summary>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static IDbAdapter CreateAdapter(IDbConnection cnn)
        {
            if (cnn is SqlConnection)
                return new SqlClient.DbAdapter(cnn as SqlConnection);
            else if (cnn is OleDbConnection)
                return new OleDb.DbAdapter(cnn as OleDbConnection);
            else
                throw new ArgumentException("Provider not supported");
        }
        /// <summary>
        /// CreateAdapter
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IDbAdapter CreateAdapter(string connectionString, DBProvider provider)
        {

            if (provider == DBProvider.SqlServer)
                return new SqlClient.DbAdapter(connectionString);
            else if (provider == DBProvider.OleDb)
                return new Nistec.Data.OleDb.DbAdapter(connectionString);
            else
                throw new ArgumentException("Provider not supported");
        }
        /// <summary>
        /// CreateAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="schemaAction"></param>
        /// <returns></returns>
        public static IDbAdapter CreateAdapter(IDbCommand cmd, MissingSchemaAction schemaAction)
        {
            if (cmd is SqlCommand)
                return new SqlClient.DbAdapter(cmd as SqlCommand, schemaAction);
            else if (cmd is OleDbCommand)
                return new OleDb.DbAdapter(cmd as OleDbCommand, schemaAction);
            else
                throw new DalException("Provider not supported,Only SQL Server and OleDb are valid database types");
        }
        /// <summary>
        /// CreateIDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static IDbDataAdapter CreateIDataAdapter(IDbCommand cmd)
        {
            if (cmd is SqlCommand)
                return new SqlDataAdapter((SqlCommand)cmd) as IDbDataAdapter;
            else if (cmd is OleDbCommand)
                return new OleDbDataAdapter((OleDbCommand)cmd) as IDbDataAdapter;
            else
                throw new DalException("Provider not supported,Only SQL Server and OleDb are valid database types");
        }

        #endregion

        #region Fill
        /// <summary>
        /// GetAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual IDbDataAdapter GetAdapter(IDbCommand cmd)
        {
            return AdapterFactory.CreateIDataAdapter(cmd);
        }
 
        /// <summary>
        /// GetSchemaTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable GetSchemaTable(string sql, SchemaType type)
        {
            if (this.m_dataAdapter == null)
            {
                CreateDataAdapter(sql);
            }
            DataSet tableSchema = new DataSet();
            m_dataAdapter.FillSchema(tableSchema, type);
            if (tableSchema.Tables.Count > 0)
                return tableSchema.Tables[0];
            return null;
        }

        /// <summary>
        /// FillSchema
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable[] FillSchema(DataSet dataSet, SchemaType type)
        {
            return DataAdapter.FillSchema(dataSet, type);
        }

  

        /// <summary>
        /// CreateDataAdapter
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public abstract IDbDataAdapter CreateDataAdapter(string strSQL);
        

        /// <summary>
        /// CreateDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            if (m_dataAdapter != null)
            {
                return m_dataAdapter;
            }
            if (m_Connection.State != ConnectionState.Open)
            {
                m_Connection.Open();
            }
            m_dataAdapter = CreateIDataAdapter(cmd);
            m_dataAdapter.SelectCommand = cmd;
            m_dataAdapter.MissingSchemaAction = SchemaAction;

            return m_dataAdapter;
        }

       
        /// <summary>
        /// GetCommand
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public IDbCommand GetCommand(string cmdText)
        {
            IDbCommand cmd = this.DbConnection.CreateCommand();
            cmd.CommandText = cmdText;
            return cmd;
        }

 
        #endregion

        #region Fill methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public DataTable FillDataTable(string mappingName, bool addWithKey)
        {
            IDbCommand cmd = DbConnection.CreateCommand();
            cmd.CommandText = SqlFormatter.SelectString("*",mappingName,null);
            DataTable dt = new DataTable(mappingName);
            return FillDataTable(cmd, mappingName, addWithKey);
        }
        /// <summary>
        /// FillDataTable
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="addWithKey"></param>
        /// <returns></returns>
        public abstract DataTable FillDataTable(IDbCommand cmd, bool addWithKey);
        /// <summary>
        /// FillDataTable
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tableName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public abstract DataTable FillDataTable(IDbCommand cmd, string tableName, bool addWithKey);
        /// <summary>
        /// FillDataSet
        /// </summary>
        /// <returns></returns>
        public DataSet FillDataSet()
        {
            DataSet ds = new DataSet();
            IDbDataAdapter da = DataAdapter;
            da.MissingSchemaAction = SchemaAction;
            da.Fill(ds);
            return ds;
        }
        /// <summary>
        /// FillDataSet
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="addWithKey"></param>
        /// <returns></returns>
        public DataSet FillDataSet(IDbCommand cmd, bool addWithKey)
        {
            DataSet ds = new DataSet();
            IDbDataAdapter da = CreateIAdapter(cmd);
            if (addWithKey)
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds);
            return ds;
        }


        /// <summary>
        /// Fill Typed DataSet
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="addWithKey"></param>
        /// <param name="ds"></param>
        /// <param name="dsType"></param>
        /// <param name="enforceConstraints"></param>
        /// <returns></returns>
        public object FillTypedDataSet(IDbCommand cmd, bool addWithKey,DataSet ds, Type dsType, bool enforceConstraints)
        {
            //DataSet ds = new DataSet();
            if (ds == null)
            {
                throw new ArgumentNullException("ds");
            }
            IDbDataAdapter da = CreateIAdapter(cmd);
            if (addWithKey)
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            if (dsType == null)
            {
                da.Fill(ds);
            }
            else
            {
                DbDataAdapter dbAdapter = (DbDataAdapter)da;

                ds.EnforceConstraints = enforceConstraints;

                //strongly typed dataset, need to figure out the table name to fill
                //from the class name
                string tableNamePrefix = ds.DataSetName;

                if (TableMappings != null && TableMappings.Length > 0)
                {
                    for (int i = 0; i < TableMappings.Length; i++)
                    {
                        string genericTableName = (i == 0) ? tableNamePrefix : tableNamePrefix + i.ToString();
                        dbAdapter.TableMappings.Add(genericTableName, TableMappings[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < ds.Tables.Count; i++)
                    {
                        string genericTableName = (i == 0) ? tableNamePrefix : tableNamePrefix + i.ToString();
                        dbAdapter.TableMappings.Add(genericTableName, ds.Tables[i].TableName);
                    }
                }

                dbAdapter.Fill(ds, dsType.Name);
            }


            return ds;
        }

      

        /// <summary>
        /// Executes command and returns T value (DataSet|DataTable|DataRow|IEntityItem) or any type for scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public T FillDataOrScalar<T>(IDbCommand cmd, bool addWithKey)
        {
            Type type = typeof(T);
            object result = null;
            try
            {
                if (type == typeof(DataSet))
                {
                    result = FillDataSet(cmd, addWithKey);
                    return (T)result;
                }
                else if (type == typeof(DataTable))
                {
                    result = FillDataTable(cmd, addWithKey);
                    return (T)result;
                }
                else if (type == typeof(DataView))
                {
                    DataTable dt = FillDataTable(cmd, addWithKey);
                    if (dt != null)
                    {
                        result = ((DataTable)dt).DefaultView;
                    }
                    return (T)result;
                }
                else if (type == typeof(DataRow))
                {
                    DataTable dt = FillDataTable(cmd, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Rows[0];
                    return (T)result;
                }
                else if (type == typeof(System.Data.DataRow[]))
                {
                    DataTable dt = FillDataTable(cmd, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Select();
                    return (T)result;
                }
                else if (typeof(IEntityItem).IsAssignableFrom(type))
                {
                    DataTable dt = FillDataTable(cmd, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        return dt.Rows[0].ToEntity<T>();
                    return (T)result;
                }
                else if (type == typeof(GenericRecord))
                {
                    DataTable dt = FillDataTable(cmd, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = GenericRecord.Parse(dt);
                    return (T)result;
                }
                else if (typeof(IDataTableAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = FillDataTable(cmd, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        T instance = ActivatorUtil.CreateInstance<T>();
                        ((IDataTableAdaptor)instance).Prepare(dt);
                        return instance;
                    }
                    return (T)result;
                }
                else if (typeof(IDataRowAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = FillDataTable(cmd, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        T instance = ActivatorUtil.CreateInstance<T>();
                        ((IDataRowAdaptor)instance).Prepare(dt.Rows[0]);
                        return instance;
                    }
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

        #endregion

        #region adapter abstract

        //public abstract DataTable FillDataTable();

        /// <summary>
        /// Fill Schema
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract DataTable FillSchema(DataTable dataTable, SchemaType type);
        /// <summary>
        /// Fill Schema
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="type"></param>
        /// <param name="srcTable"></param>
        /// <returns></returns>
        public abstract DataTable[] FillSchema(DataSet dataSet, SchemaType type, string srcTable);
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public abstract int Update(DataTable dataTable);
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="srcTable"></param>
        /// <returns></returns>
        public abstract int Update(DataSet dataSet, string srcTable);

        //public abstract int UpdateChanges(DataTable dataTable, string dbTableName, string strSql);

        //public abstract T FillDataOrScalar<T>(IDbCommand cmd, bool addWithKey);

        /// <summary>
        /// CreateIAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public abstract IDbDataAdapter CreateIAdapter(IDbCommand cmd);



        #endregion

        #region UpdateChanges Methods
        /// <summary>
        /// Close Connection
        /// </summary>
        protected void Close()
        {
            if (m_Connection != null)
            {
                m_Connection.Close();
            }
            m_dataAdapter = null;
        }
        /// <summary>
        /// UpdateChanges
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted,
        /// updated, or deleted row in the specified System.Data.DataSet from a System.Data.DataTable
        /// named "Table".
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="dbTableName"></param>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        public int UpdateChanges(DataTable dataTable, string dbTableName, string selectCommand)
        {
            int res = 0;
            System.Data.DataTable dsChanges = GetChanges(dataTable);
            if (dsChanges == null)
                return 0;

            CreateDataAdapter(selectCommand);
            try
            {
                if (m_dataAdapter.TableMappings.Count == 0)
                {
                    m_dataAdapter.TableMappings.Add(dbTableName, dataTable.TableName);
                }
                res = Update(dsChanges);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
            return res;
        }
        /// <summary>
        /// UpdateChanges
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted,
        /// updated, or deleted row in the specified System.Data.DataSet from a System.Data.DataTable
        /// named "Table".
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="dbTableName"></param>
        /// <param name="selectCommand"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int UpdateChanges(DataTable dataTable, string dbTableName, string selectCommand, SchemaType type)
        {
            int res = 0;
            System.Data.DataTable dsChanges = GetChanges(dataTable);
            if (dsChanges == null)
                return 0;

            CreateDataAdapter(selectCommand);
            FillSchema(dataTable, type);
            try
            {
                if (m_dataAdapter.TableMappings.Count == 0)
                {
                    m_dataAdapter.TableMappings.Add(dbTableName, dataTable.TableName);
                }
                res = Update(dsChanges);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
            return res;
        }
        /// <summary>
        /// UpdateChanges
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted,
        /// updated, or deleted row in the specified System.Data.DataSet from a System.Data.DataTable
        /// named "Table".
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableName"></param>
        /// <param name="dbTableName"></param>
        /// <param name="selectCommand"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int UpdateChanges(DataSet dataSet, string tableName, string dbTableName, string selectCommand, SchemaType type)
        {
            int res = 0;
            System.Data.DataSet dsChanges = GetChanges(dataSet);
            if (dsChanges == null)
                return 0;

            CreateDataAdapter(selectCommand);
            FillSchema(dataSet, type, tableName);
            try
            {
                if (m_dataAdapter.TableMappings.Count == 0)
                {
                    m_dataAdapter.TableMappings.Add(dbTableName, tableName);
                }
                res = Update(dsChanges, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
            return res;
        }
        /// <summary>
        /// UpdateChanges
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted,
        /// updated, or deleted row in the specified System.Data.DataSet from a System.Data.DataTable
        /// named "Table".
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        public int UpdateChanges(DataSet dataSet, string selectCommand)
        {
            int res = 0;
            System.Data.DataSet dsChanges = GetChanges(dataSet);
            if (dsChanges == null)
                return 0;

            CreateDataAdapter(selectCommand);
            try
            {
                res = m_dataAdapter.Update(dsChanges);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
            return res;
        }
        /// <summary>
        /// GetChanges
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public virtual DataTable GetChanges(DataTable dataTable)
        {
            System.Data.DataTable dsChanges = dataTable.GetChanges();
            if (dsChanges != null && dsChanges.HasErrors)
            {
                throw new Exception("Data Changes Has Errors");
            }
            return dsChanges;
        }
        /// <summary>
        /// GetChanges
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public virtual DataSet GetChanges(DataSet dataSet)
        {
            System.Data.DataSet dsChanges = dataSet.GetChanges();
            if (dsChanges != null && dsChanges.HasErrors)
            {
                throw new Exception("Data Changes Has Errors");
            }
            return dsChanges;
        }
        /// <summary>
        /// Update changes
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted,
        /// updated, or deleted row in the specified System.Data.DataSet from a System.Data.DataTable
        /// named "Table".
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="dbTableName"></param>
        /// <returns></returns>
        public virtual int UpdateChanges(DataTable dataTable, string dbTableName)
        {
            string strSql = SqlFormatter.SelectCommand(dataTable, dbTableName,null);
            return UpdateChanges(dataTable, dbTableName, strSql);
        }
        /// <summary>
        /// Update changes
        /// Calls the respective INSERT, UPDATE, or DELETE statements for each inserted,
        /// updated, or deleted row in the specified System.Data.DataSet from a System.Data.DataTable
        /// named "Table".
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public virtual int UpdateChanges(DataTable dataTable)
        {
            string strSql = SqlFormatter.SelectCommand(dataTable, dataTable.TableName,null);
            return UpdateChanges(dataTable, strSql);
        }

        #endregion

        #region schema
        /// <summary>
        /// Set Adapter SelectCommand
        /// </summary>
        /// <param name="cmd"></param>
        protected abstract void SetAdapterSelectCommand(IDbCommand cmd);
        /// <summary>
        /// GetSchemaTable
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public abstract DataTable GetSchemaTable(IDbConnection conn);
        /// <summary>
        /// GetSchemaView
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public abstract DataTable GetSchemaView(IDbConnection conn);
        /// <summary>
        /// GetSchemaTable
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable()
        {
            return GetSchemaTable(Connection);
        }
        /// <summary>
        /// GetSchemaView
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaView()
        {
            return GetSchemaView(Connection);
        }

        //public abstract DataSet GetSchemaDB();

        /// <summary>
        /// Get DB Schema
        /// </summary>
        /// <returns></returns>
        public virtual DataSet GetSchemaDB()
        {
            DataSet ds = null;
            if (m_Connection == null)
            {
                throw new DalException("Invalid connection, Can not create adapter");
            }

            ds = new DataSet();

            DataTable dt = GetSchemaTable();
            dt.TableName = "TABLES";
            ds.Tables.Add(dt);

            DataTable dv = GetSchemaView();
            dv.TableName = "VIEWS";
            ds.Tables.Add(dv);

            return ds;
        }
        /// <summary>
        /// Fill Schema DB
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableSchema"></param>
        /// <param name="prefix"></param>
        /// <param name="addAdvancedColumns"></param>
        public void FillSchemaDB(DataSet dataSet, DataTable tableSchema, string prefix, bool addAdvancedColumns)
        {
            try
            {

                //IDbDataAdapter da = new SqlDataAdapter();
                IDbCommand cmd = Connection.CreateCommand();// new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = Connection;
                string src = null;
                CreateDataAdapter(cmd);

                if (addAdvancedColumns)
                {
                    DataTable dtSrc = null;
                    DataTable dt = null;

                    foreach (DataRow dr in tableSchema.Rows)
                    {
                        src = (string)dr["TABLE_NAME"];
                        dtSrc = new DataTable(src);
                        cmd.CommandText = SqlFormatter.SelectString(src);
                        SetAdapterSelectCommand(cmd);

                        FillSchema(dtSrc, SchemaType.Source);

                        dt = new DataTable(src);
                        dt.Prefix = prefix;

                        dt.Columns.Add("ColumnName");
                        dt.Columns.Add("DataType");
                        dt.Columns.Add("MaxLength");

                        foreach (DataColumn c in dtSrc.Columns)
                        {
                            dt.Rows.Add(new object[] { c.ColumnName, c.DataType.Name, c.MaxLength });
                        }
                        dataSet.Tables.Add(dt);
                    }
                }
                else
                {
                    foreach (DataRow dr in tableSchema.Rows)
                    {
                        src = (string)dr["TABLE_NAME"];
                        cmd.CommandText =SqlFormatter.SelectString( src);
                        SetAdapterSelectCommand(cmd);
                        DataTable[] dt = FillSchema(dataSet, SchemaType.Source, src);
                        foreach (DataTable d in dt)
                        {
                            d.Prefix = prefix;
                        }
                        m_dataAdapter.Fill(dataSet);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        #endregion

        #region static

        /// <summary>
        /// Fill DataTable using DbDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            DataTable dt = new DataTable(mappingName);
            using (DbDataAdapter dbAdapter = (DbDataAdapter)AdapterFactory.CreateIDataAdapter(cmd))
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
        public static DataSet ExecuteDataSet(IDbCommand cmd, string mappingName, bool addWithKey)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(mappingName))
            {
                ds.DataSetName = mappingName;
            }
            using (DbDataAdapter dbAdapter = (DbDataAdapter)AdapterFactory.CreateIDataAdapter(cmd))
            {
                dbAdapter.SelectCommand = (DbCommand)cmd;
                if (addWithKey)
                    dbAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                dbAdapter.Fill(ds);

            }
            return ds;
        }

        /// <summary>
        /// Fill DataTable using DbDataAdapter
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <param name="enforceConstraints"></param>
        /// <param name="tableMapping"></param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(IDbCommand cmd, string mappingName, bool addWithKey, bool enforceConstraints, string[] tableMapping)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(mappingName))
            {
                ds.DataSetName = mappingName;
            }
            using (DbDataAdapter dbAdapter = (DbDataAdapter)AdapterFactory.CreateIDataAdapter(cmd))
            {
                dbAdapter.SelectCommand = (DbCommand)cmd;
                if (addWithKey)
                    dbAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                ds.EnforceConstraints = enforceConstraints;

                string tableNamePrefix = ds.DataSetName;

                if (tableMapping != null && tableMapping.Length > 0)
                {
                    for (int i = 0; i < tableMapping.Length; i++)
                    {
                        string genericTableName = (i == 0) ? tableNamePrefix : tableNamePrefix + i.ToString();
                        dbAdapter.TableMappings.Add(genericTableName, tableMapping[i]);
                    }
                }
 
                dbAdapter.Fill(ds);
            }
            return ds;
        }

        /// <summary>
        /// Set DataSer tables mapping
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="enforceConstraints"></param>
        /// <param name="tableMapping"></param>
        /// <returns></returns>
        public static DataSet DataSetTableMapping(DataSet ds, bool enforceConstraints, params string[] tableMapping)
        {

            ds.EnforceConstraints = enforceConstraints;

            if (tableMapping != null && tableMapping.Length > 0)
            {
                for (int i = 0; i < tableMapping.Length; i++)
                {
                    ds.Tables[i].TableName = tableMapping[i];
                }
            }
            return ds;
        }

        /// <summary>
        /// Fill Typed DataSet
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="addWithKey"></param>
        /// <param name="ds"></param>
        /// <param name="dsType"></param>
        /// <param name="enforceConstraints"></param>
        /// <param name="tableMapping"></param>
        /// <returns></returns>
        public static object ExecuteTypedDataSet(IDbCommand cmd, bool addWithKey, DataSet ds, Type dsType, bool enforceConstraints, string[] tableMapping)
        {
            if (ds == null)
            {
                throw new ArgumentNullException("ds");
            }

            using (DbDataAdapter dbAdapter = (DbDataAdapter)AdapterFactory.CreateIDataAdapter(cmd))
            {
                if (addWithKey)
                    dbAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                if (dsType == null)
                {
                    dbAdapter.Fill(ds);
                }
                else
                {

                    ds.EnforceConstraints = enforceConstraints;

                    string tableNamePrefix = ds.DataSetName;

                    if (tableMapping != null && tableMapping.Length > 0)
                    {
                        for (int i = 0; i < tableMapping.Length; i++)
                        {
                            string genericTableName = (i == 0) ? tableNamePrefix : tableNamePrefix + i.ToString();
                            dbAdapter.TableMappings.Add(genericTableName, tableMapping[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ds.Tables.Count; i++)
                        {
                            string genericTableName = (i == 0) ? tableNamePrefix : tableNamePrefix + i.ToString();
                            dbAdapter.TableMappings.Add(genericTableName, ds.Tables[i].TableName);
                        }
                    }

                    dbAdapter.Fill(ds, dsType.Name);
                }
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
        public static T ExecuteDataOrScalar<T>(IDbCommand cmd,string mappingName, bool addWithKey)
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
                else if (typeof(IDataTableAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        T instance = ActivatorUtil.CreateInstance<T>();
                        ((IDataTableAdaptor)instance).Prepare(dt);
                        return instance;
                    }
                    return (T)result;
                }
                else if (typeof(IDataRowAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        T instance = ActivatorUtil.CreateInstance<T>();
                        ((IDataRowAdaptor)instance).Prepare(dt.Rows[0]);
                        return instance;
                    }
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
        public static TResult ExecuteDataOrScalar<TItem,TResult>(IDbCommand cmd, string mappingName, bool addWithKey)
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
                else if (IsAssignableOfList(type, typeof(IEntityItem)))
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
                else if (typeof(IDataTableAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        TResult instance = ActivatorUtil.CreateInstance<TResult>();
                        ((IDataTableAdaptor)instance).Prepare(dt);
                        return instance;
                    }
                    return (TResult)result;
                }
                else if (typeof(IDataRowAdaptor).IsAssignableFrom(type))
                {
                    DataTable dt = ExecuteDataTable(cmd, mappingName, addWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        TResult instance = ActivatorUtil.CreateInstance<TResult>();
                        ((IDataRowAdaptor)instance).Prepare(dt.Rows[0]);
                        return instance;
                    }
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

        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static bool IsAssignableOfList(Type type, Type interfaceType)
        {
            if (type == null)
            {
                return false;
            }
            Type[] args = type.GetGenericArguments();
            foreach (Type t in args)
            {
                if (interfaceType.IsAssignableFrom(t))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAssignableOfList(Type type, Type interfaceType, bool isGeneric , ref Type elementType)
        {
            if (type == null)
            {
                return false;
            }
            Type listype = (isGeneric) ? typeof(IList<>) : typeof(System.Collections.IList);
            if (listype.IsAssignableFrom(type))
            {
                Type[] args = type.GetGenericArguments();
                foreach (Type t in args)
                {
                    if (interfaceType.IsAssignableFrom(t))
                    {
                        elementType = t;
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<T> ConvertList<T>(object o)
        {
            return ((object[])o).Cast<T>().ToList<T>();
        }
        public static List<T> ConvertList<T>(object[] array)
        {
            return array.Cast<T>().ToList<T>();
        }
        public static List<T> ConvertList<T>(List<object> list)
        {
            return list.Cast<T>().ToList<T>();
        }
        public static T[] ConvertArray<T>(object[] array)
        {
            return Array.ConvertAll(array, item => (T)item);
        }
        public static T[] ConvertArray<T>(object o)
        {
            return Array.ConvertAll((object[])o, item => (T)item);
        }
        public static T ConvertItem<T>(object o)
        {
             return new object[] { o }.Cast<T>().FirstOrDefault();
        }

        public static T ConvertTo<T>(object o)
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                try
                {
                    return (T)Convert.ChangeType(o, typeof(T));
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }

        }
    }
}
