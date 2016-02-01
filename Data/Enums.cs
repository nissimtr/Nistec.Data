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
using System.Text;

namespace Nistec.Data
{

    #region base enum
  

    /// <summary>
    /// Return type list by excute command
    /// </summary>
    public enum DalReturnType
    {
        /// <summary>
        /// int use for ExecuteNonQuey
        /// </summary>
        Int,
        /// <summary>
        /// DalSchema as System.Data.DataSet
        /// </summary>
        DalSchema,
        /// <summary>
        /// System.Data.DataSet
        /// </summary>
        DataSet,
        /// <summary>
        /// System.Data.DataTable
        /// </summary>
        DataTable,
        /// <summary>
        /// System.Data.DataRow[]  array
        /// </summary>
        DataRows,
        /// <summary>
        /// System.Data.DataRow  Single row
        /// </summary>
        DataRow,
        /// <summary>
        /// System.Data.SqlClient.SqlDataReader/System.Data.OleDb.OleDbDataReader
        /// </summary>
        DataReader,
        /// <summary>
        /// System.Data.SqlClient.SqlDataAdapter/System.Data.OleDb.OleDbDataAdapter
        /// </summary>
        DataAdapter,
        /// <summary>
        /// System.Data.SqlClient.SqlCommand/System.Data.OleDb.OleDbCommand
        /// </summary>
        Command,
        /// <summary>
        /// System.Object use for scalar command
        /// </summary>
        Object,
        /// <summary>
        /// return none use for ExecuteNonQuey
        /// </summary>
        Default
    }

    #endregion

    #region Cmd enums

    /// <summary>
    /// Command type enumeration for <see cref="DBCommandAttribute.CommandType"/> property
    /// of <see cref="DBCommandAttribute"/> attribute.
    /// </summary>
    public enum DBCommandType
    {
        /// <summary>
        /// Equals to <b>CommandType.Text</b> value. If <see cref="DBCommandAttribute"/> is defined 
        /// then it is a default value of <see cref="DBCommandAttribute.CommandType"/> property.
        /// </summary>
        Text,
        /// <summary>
        /// Equals to <b>CommandType.StoredProcedure</b> value. 
        /// </summary>
        StoredProcedure,
        /// <summary>
        /// The command implements insert operation for a table specified.
        /// </summary>
        Insert,
        /// <summary>
        /// The command implements update operation for a table specified.
        /// </summary>
        Update,
        /// <summary>
        /// The command implements Delete operation for a table specified.
        /// </summary>
        Delete,
        /// <summary>
        /// The command implements Lookup sclar function.
        /// </summary>
        Lookup,
        /// <summary>
        /// The command implements if not exists, insert operation else update for a table specified.
        /// </summary>
        InsertOrUpdate,
        /// <summary>
        /// The command implements insert operation if not exists for a table specified.
        /// </summary>
        InsertNotExists
    }

    /// <summary>
    /// Command type enumeration for ActiveCommandBuilder property
    /// </summary>
    public enum UpdateCommandType
    {
        /// <summary>
        /// The command implements insert operation for a table specified.
        /// </summary>
        Insert,
        /// <summary>
        /// The command implements update operation for a table specified.
        /// </summary>
        Update,
        /// <summary>
        /// The command implements Delete operation for a table specified.
        /// </summary>
        Delete,
        /// <summary>
        /// Equals to <b>CommandType.StoredProcedure</b> value. 
        /// </summary>
        StoredProcedure
    }


    /// <summary>
    /// Parameter type enumeration for <see cref="Sql.DbFieldAttribute.ParameterType"/> property
    /// of <see cref="Sql.DbFieldAttribute"/> attribute.
    /// </summary>
    public enum DalParamType
    {
        /// <summary>
        /// The parameter is defaul and has not special role.
        /// </summary>
        Default,

        /// <summary>
        /// The parameter is a return value from a stored procedure.
        /// </summary>
        SPReturnValue,

        /// <summary>
        /// This parameter is a part of a table key 
        /// and is applicable only 
        /// for <see cref="DBCommandType.Insert">DBCommandType.Insert </see> or <see cref="DBCommandType.Update">DBCommandType.Update</see> value 
        /// of method's <see cref="DBCommandAttribute.CommandType">DBCommandAttribute.CommandType</see> property.
        /// </summary>
        Key,

        /// <summary>
        /// This parameter is a part of a table autoincremental field 
        /// and is applicable only 
        /// for <see cref="DBCommandType.Insert">DBCommandType.Insert </see> or <see cref="DBCommandType.Update">DBCommandType.Update</see> value 
        /// of method's <see cref="DBCommandAttribute.CommandType">DBCommandAttribute.CommandType</see> property.
        /// This parameter requires to be only passed by refference.
        /// </summary>
        Identity,

        /// <summary>
        /// This parameter is a Array of  
        /// and is applicable only 
        /// for <see cref="DBCommandType.Text">DBCommandType.Text </see> value 
        /// of method's <see cref="DBCommandAttribute.CommandType">DBCommandAttribute.CommandType</see> property.
        /// This parameter requires for Where X In() operation.
        /// </summary>
        Array

    }
    /// <summary>
    /// Parameter type info for Parameter property
    /// </summary>
    public enum ParamDir
    {
        /// <summary>
        /// The parameter is defaul and has not special role.
        /// </summary>
        ByValue = 0,
        /// <summary>
        /// The parameter is by ref.
        /// </summary>
        ByRef = 1,
        /// <summary>
        /// The parameter is by out.
        /// </summary>
        ByOut = 2

    }



    /// <summary>
    /// Parameter type enumeration for <see cref="AggregationMode"/> property
    /// </summary>
    public enum AggregationMode
    {
        /// <summary>
        /// Count Aggregation mode.
        /// </summary>
        Count,
        /// <summary>
        /// Sum Aggregation mode.
        /// </summary>
        Sum,
        /// <summary>
        /// Minimum Aggregation mode.
        /// </summary>
        Min,
        /// <summary>
        /// Maximum Aggregation mode.
        /// </summary>
        Max,
        /// <summary>
        /// Avg Aggregation mode.
        /// </summary>
        Avg

    }

    #endregion

    public enum PermsLevel
    {
        DenyAll = 0,
        ReadOnly = 1,
        EditOnly = 2,
        FullControl = 3
    }

    public enum Aggregate
    {
        None,
        Sum,
        Count,
        Min,
        Max,
        First,
        Last
    }

    public enum DBProvider
    {
        /// <summary>
        /// OleDb
        /// </summary>
        OleDb,
        /// <summary>
        /// SqlServer
        /// </summary>
        SqlServer,
         /// <summary>
        /// OracleOleDb
        /// </summary>
        Oracle,
        /// <summary>
        /// MySQL
        /// </summary>
        MySQL,
        /// <summary>
        /// SybaseASE
        /// </summary>
        SybaseASE,
        /// <summary>
        /// Firebird
        /// </summary>
        Firebird,
        /// <summary>
        /// DB2
        /// </summary>
        DB2
    }

    public enum DataResult
    {
        Error = -1,
        None =0,
        Commit = 1
    }
   

}
