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
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using Nistec.Data.Factory;

namespace Nistec.Data.OleDb
{
    public class DbAdapter : AdapterFactory
    {

        #region Ctor

        public DbAdapter(OleDbConnection cnn)
            : base(cnn)
        {
        }
        public DbAdapter(OleDbCommand cmd, MissingSchemaAction schemaAction)
            : base(cmd.Connection)
        {
        }
        public DbAdapter(string connectionString)
            : base(connectionString, DBProvider.OleDb)
        {
        }
      

        #endregion

        #region Properties

        public override DBProvider Provider
        {
            get { return DBProvider.OleDb; }
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
            m_dataAdapter = new System.Data.OleDb.OleDbDataAdapter();
            m_dataAdapter.SelectCommand = cmd;
            m_dataAdapter.MissingSchemaAction = SchemaAction;

            System.Data.OleDb.OleDbCommandBuilder cb = new System.Data.OleDb.OleDbCommandBuilder((OleDbDataAdapter)m_dataAdapter);
            return m_dataAdapter;
        }

        protected override void SetAdapterSelectCommand(IDbCommand cmd)
        {
            ((OleDbDataAdapter)DataAdapter).SelectCommand = cmd as OleDbCommand;
        }

        public override IDbDataAdapter CreateIAdapter(IDbCommand cmd)
        {
            return new OleDbDataAdapter((OleDbCommand)cmd);
        }

        public override DataTable FillDataTable(IDbCommand cmd, bool addWithKey)
        {
            return FillDataTable(cmd, "", addWithKey);
        }

        public override DataTable FillDataTable(IDbCommand cmd, string tableName, bool addWithKey)
        {
            DataTable dt = new DataTable(tableName);
            OleDbDataAdapter da = new OleDbDataAdapter((OleDbCommand)cmd);
            if (addWithKey)
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(dt);

            return dt;
        }

        public void AdapterCommandBuilder(OleDbDataAdapter dataAdapter)
        {
            if (dataAdapter.UpdateCommand == null)
            {
                System.Data.OleDb.OleDbCommandBuilder cb = new System.Data.OleDb.OleDbCommandBuilder(dataAdapter);
            }
        }

        public override int Update(DataTable dataTable)
        {
            OleDbDataAdapter da = (OleDbDataAdapter)DataAdapter;
            return da.Update(dataTable);
        }
        public override int Update(DataSet dataSet, string srcTable)
        {
            OleDbDataAdapter da = (OleDbDataAdapter)DataAdapter;
            return da.Update(dataSet, srcTable);
        }

        #endregion

        #region override schema

        public override DataTable FillSchema(DataTable dataTable, SchemaType type)
        {
            OleDbDataAdapter da = (OleDbDataAdapter)DataAdapter;
            return da.FillSchema(dataTable, type);
        }

        public override DataTable[] FillSchema(DataSet dataSet, SchemaType type, string srcTable)
        {
            OleDbDataAdapter da = (OleDbDataAdapter)DataAdapter;
            return da.FillSchema(dataSet, type, srcTable);
        }

        public override DataTable GetSchemaTable(IDbConnection conn)
        {
            conn.Open();
            DataTable schemaTable = ((OleDbConnection)conn).GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "TABLE" });
            conn.Close();
            return schemaTable;
        }

        public override DataTable GetSchemaView(IDbConnection conn)
        {

            conn.Open();
            DataTable schemaTable = ((OleDbConnection)conn).GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                new object[] { null, null, null, "VIEW" });
            conn.Close();
            return schemaTable;

        }

        #endregion

    }
}
