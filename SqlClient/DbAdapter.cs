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
using Nistec.Data.Factory;

namespace Nistec.Data.SqlClient
{
    public class DbAdapter : AdapterFactory
    {

  		#region Ctor

        public DbAdapter(SqlConnection cnn)
            : base(cnn)
        {
        }
        public DbAdapter(SqlCommand cmd, MissingSchemaAction schemaAction)
            : base(cmd.Connection)
        {
        }
        public DbAdapter(string connectionString)
            : base(connectionString, DBProvider.SqlServer)
        {
        }
          
		#endregion

        #region Properties

        public override DBProvider Provider
        {
            get { return  DBProvider.SqlServer; }
        }
      
        #endregion

        #region override adapter factory

        /// <summary>
        /// CreateDataAdapter
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public override IDbDataAdapter CreateDataAdapter(string strSQL)
        {
            if (m_dataAdapter != null)
            {
                return m_dataAdapter;
            }
            if (m_Connection.State != ConnectionState.Open)
            {
                m_Connection.Open();
            }
            IDbCommand cmd = DbFactory.CreateCommand(strSQL, m_Connection);
            if (Transaction != null)
            {
                cmd.Transaction = Transaction;
            }
            m_dataAdapter = new System.Data.SqlClient.SqlDataAdapter();
            m_dataAdapter.SelectCommand = cmd;
            m_dataAdapter.MissingSchemaAction = SchemaAction;

            System.Data.SqlClient.SqlCommandBuilder cb = new System.Data.SqlClient.SqlCommandBuilder((SqlDataAdapter)m_dataAdapter);
            return m_dataAdapter;
        }
        protected override void SetAdapterSelectCommand(IDbCommand cmd)
        {
            ((SqlDataAdapter)DataAdapter).SelectCommand = cmd as SqlCommand;
        }

        public override IDbDataAdapter CreateIAdapter(IDbCommand cmd)
        {
            return new SqlDataAdapter((SqlCommand)cmd);
        }

        public override DataTable FillDataTable(IDbCommand cmd, bool addWithKey)
        {
            return FillDataTable(cmd, "", addWithKey);
        }

        public override DataTable FillDataTable(IDbCommand cmd, string tableName, bool addWithKey)
        {
            DataTable dt = new DataTable(tableName);
            SqlDataAdapter da = new SqlDataAdapter((SqlCommand)cmd);
            if (addWithKey)
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(dt);

            return dt;
        }

        public void AdapterCommandBuilder(SqlDataAdapter dataAdapter)
        {
            if (dataAdapter.UpdateCommand == null)
            {
                System.Data.SqlClient.SqlCommandBuilder cb = new System.Data.SqlClient.SqlCommandBuilder(dataAdapter);
            }
        }

        public override int Update(DataTable dataTable)
        {
            SqlDataAdapter da = (SqlDataAdapter)DataAdapter;
            return da.Update(dataTable);
        }
        public override int Update(DataSet dataSet, string srcTable)
        {
            SqlDataAdapter da = (SqlDataAdapter)DataAdapter;
            return da.Update(dataSet, srcTable);
        }

        #endregion

        #region override schema

        public override DataTable FillSchema(DataTable dataTable, SchemaType type)
        {
            SqlDataAdapter da = (SqlDataAdapter)DataAdapter;
            return da.FillSchema(dataTable, type);
        }

        public override DataTable[] FillSchema(DataSet dataSet, SchemaType type, string srcTable)
        {
            SqlDataAdapter da = (SqlDataAdapter)DataAdapter;
            return da.FillSchema(dataSet, type, srcTable);
        }

        public override DataTable GetSchemaTable(IDbConnection conn)
        {

            SqlDataAdapter schemaDA = new SqlDataAdapter("SELECT * FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_TYPE = 'BASE TABLE' " +
                "ORDER BY TABLE_TYPE",
                conn as SqlConnection);

            DataTable schemaTable = new DataTable();
            schemaDA.Fill(schemaTable);
            return schemaTable;

        }

        public override DataTable GetSchemaView(IDbConnection conn)
        {
            SqlDataAdapter schemaDA = new SqlDataAdapter("SELECT * FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_TYPE = 'VIEW' " +
                "ORDER BY TABLE_TYPE",
                conn as SqlConnection);

            DataTable schemaTable = new DataTable();
            schemaDA.Fill(schemaTable);
            return schemaTable;

        }

        #endregion

    }
}
