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
    

    /// <summary>
    /// Represent Command methods interface
    /// </summary>
    public interface IDbCmd : IDisposable
    {
        #region properties
        /// <summary>
        /// Get DBProvider
        /// </summary>
        DBProvider DBProvider { get; }
        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        string ConnectionString { get; set; }
        /// <summary>
        ///  Get or Set Connection
        /// </summary>
        System.Data.IDbConnection Connection { get; set; }
        /// <summary>
        /// Get Adapter
        /// </summary>
        IDbAdapter Adapter { get; }
        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        int ExecuteNonQuery(string cmdText);
     
        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        int ExecuteNonQuery(string cmdText, IDbDataParameter[] parameters, CommandType commandType= CommandType.Text, int commandTimeOut=0);

        #endregion

        #region CommandScalar

        /// <summary>
        /// Executes Command and returns T value as scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <returns></returns> 
        T ExecuteScalar<T>(string cmdText);
  
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
        T ExecuteScalar<T>(string cmdText, IDbDataParameter[] parameters, T returnIfNull, CommandType commandType= CommandType.Text, int commandTimeOut=0);

        #endregion

        #region Execute Command

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        T ExecuteCommand<T>(string cmdText, bool addWithKey=false);


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
        T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false);

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow|IEntityItem) .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cacheKey">Get or Set data from/to cache.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        T ExecuteCommand<T>(string cmdText, IDbDataParameter[] parameters, string cacheKey, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false);

        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow|IEntityItem|List of IEntityItem) .
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns> 
        TResult ExecuteCommand<TItem, TResult>(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0, bool addWithKey = false);

        #endregion

        #region ExecuteDataTable

        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <returns></returns>
        DataTable ExecuteDataTable(string mappingName);

        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        DataTable ExecuteDataTable(string mappingName, bool addWithKey);

        /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cmdText"></param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
        /// <returns></returns>
        DataTable ExecuteDataTable(string tableName, string cmdText, bool addWithKey);

         /// <summary>
        /// Executes Adapter and returns DataTable.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cmdText"></param>
        /// <param name="commandTimeout">Timeout in seconds</param>
        /// <param name="addWithKey">Adds the primary key columns to complete the schema.</param>
         /// <returns></returns>
        DataTable ExecuteDataTable(string tableName, string cmdText, int commandTimeout, bool addWithKey = false);

        #endregion

        #region ExecuteReader

        /// <summary>
        /// Execute Reader
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        IDataReader ExecuteReader(string cmdText, CommandBehavior behavior);

        /// <summary>
        /// Create Reader
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="behavior"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        IDataReader ExecuteReader(string cmdText, CommandBehavior behavior, IDbDataParameter[] parameters);
        #endregion

        #region LookupQuery

        /// <summary>
        /// Execute lookup function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Field">field name for return value</param>
        /// <param name="Table">Table name or View name</param>
        /// <param name="Where">Sql Where string,each paramet should start with @ </param>
        /// <param name="returnIfNull">Default value to return if no recored affected</param>
        /// <param name="values">Values of Parameters</param>
        /// <returns></returns>
        T LookupQuery<T>(string Field, string Table, string Where, T returnIfNull, object[] values);

        #endregion

        #region Multi commands

        /// <summary>
        /// Execute multiple commands Non Query
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="failOnFirstError"></param>
        void MultiExecuteNonQuery(string[] commands, bool failOnFirstError);


        /// <summary>
        /// Execute multiple commands Scalar
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="failOnFirstError"></param>
        /// <returns></returns>
        object[] MultiExecuteScalar(string[] commands, bool failOnFirstError);

        #endregion

        #region Dlookup methods

        DataRow DRow(string fields, string table, string where, object[] parameters);

        DataView DList(string valueMemeber, string displayMember, string table, string Where, object[] parameters);

        DataView DList(string valueMemeber, string displayMember, string table, string Where, DataParameter[] parameters);

        IDbCommand GetCommand(string cmdText);
        #endregion
    }
    
}
