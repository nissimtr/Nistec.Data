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
#pragma warning disable CS1591
namespace Nistec.Data.Factory
{
    public interface IDbAdapter : IDisposable
    {
        #region Fill methods

        /// <summary>
        /// Fill Data Table
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        DataTable FillDataTable(string mappingName, bool addWithKey);

        /// <summary>
        /// Fill Data Table
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        DataTable FillDataTable(IDbCommand cmd, bool addWithKey);
        /// <summary>
        /// Fill Data Table
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tableName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        DataTable FillDataTable(IDbCommand cmd, string tableName, bool addWithKey);
        /// <summary>
        /// Fill Data Set
        /// </summary>
        /// <returns></returns>
        DataSet FillDataSet();

        /// <summary>
        /// Fill Data Set
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        DataSet FillDataSet(IDbCommand cmd, bool addWithKey);


        /// <summary>
        /// Executes command and returns T value (DataSet|DataTable|DataRow) or any type for scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        T FillDataOrScalar<T>(IDbCommand cmd, bool addWithKey);


        #endregion

        #region Schema
        /// <summary>
        /// Get DB Schema
        /// </summary>
        /// <returns></returns>
        DataSet GetSchemaDB();

        /// <summary>
        /// Fill data set with DB schema
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableSchema"></param>
        /// <param name="prefix"></param>
        /// <param name="addAdvancedColumns"></param>
        void FillSchemaDB(DataSet dataSet, DataTable tableSchema, string prefix, bool addAdvancedColumns);
        /// <summary>
        /// Get Schema Table
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        DataTable GetSchemaTable(string sql, SchemaType type);
        /// <summary>
        /// Get all DB tables
        /// </summary>
        /// <returns></returns>
        DataTable GetSchemaTable();
        /// <summary>
        /// Get all DB views
        /// </summary>
        /// <returns></returns>
        DataTable GetSchemaView();

        #endregion

        #region dataAdpter
        /// <summary>
        /// Get DataTable Mapping
        /// </summary>
        ITableMappingCollection DataTableMapping { get; }

        //DataTable FillSchema(DataTable dataTable, SchemaType type);

        /// <summary>
        /// Fill Schema
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        DataTable[] FillSchema(DataSet dataSet, SchemaType type);

        //DataTable[] FillSchema(DataSet dataSet, SchemaType type, string srcTable);

        /// <summary>
        /// Create Data Adapter
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        IDbDataAdapter CreateDataAdapter(string strSQL);
        /// <summary>
        /// GetChanges
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        DataTable GetChanges(DataTable dataTable);
        /// <summary>
        /// GetChanges
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        DataSet GetChanges(DataSet dataSet);

        #endregion

        #region UpdateChanges Methods
        /// <summary>
        /// UpdateChanges
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="dbTableName"></param>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        int UpdateChanges(DataTable dataTable, string dbTableName, string selectCommand);
        /// <summary>
        /// UpdateChanges
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="dbTableName"></param>
        /// <param name="selectCommand"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        int UpdateChanges(DataTable dataTable, string dbTableName, string selectCommand, SchemaType type);
        /// <summary>
        /// UpdateChanges
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableName"></param>
        /// <param name="dbTableName"></param>
        /// <param name="selectCommand"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        int UpdateChanges(DataSet dataSet, string tableName, string dbTableName, string selectCommand, SchemaType type);
        /// <summary>
        /// UpdateChanges
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="selectCommand"></param>
        /// <returns></returns>
        int UpdateChanges(DataSet dataSet, string selectCommand);
        /// <summary>
        /// UpdateChanges
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="dbTableName"></param>
        /// <returns></returns>
        int UpdateChanges(DataTable dataTable, string dbTableName);
        /// <summary>
        /// UpdateChanges
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        int UpdateChanges(DataTable dataTable);

        #endregion

    }
}
