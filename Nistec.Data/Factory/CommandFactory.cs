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
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using Nistec.Data.Entities;
using System.Collections.Generic;


namespace Nistec.Data.Factory
{

    /// <summary>
    /// Base class for CommandFactory class. You can inherint your classes from 
    /// this base class.
    /// </summary>
    public abstract class CommandFactory : AutoDb, IAutoDb
    {

        #region abstract

        /// <summary>
        /// Create Adapter as <see cref="IDbAdapter"/>
        /// </summary>
        /// <returns></returns>
        protected abstract IDbAdapter CreateAdapter();
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
        internal abstract string BuildCommandText(IDbCommand command, DBCommandType commandType, string TableName, string cmdPart1, string cmdPart2, string cmdPart3, string cmdPart4, string autNumberField);

        #endregion

        #region AutoCmd implementation

     
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
        internal object ExecuteInternal(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
        {
            if (method == null)
            {
                // this is done because this method can be called explicitly from code.
                method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            }
            object result = null;

            // create command object
            IDbCommand command = connection.CreateCommand(); // new SqlCommand();
            command.Connection = connection;
            command.Transaction = transaction;
            if (commandTimeOut > 0)
            {
                command.CommandTimeout = commandTimeOut;
            }
            // define default command properties (command text, command type and missing schema action)
            string commandText = method.Name;
            DBCommandType commandType = DBCommandType.StoredProcedure;
            MissingSchemaAction missingSchemaAction = MissingSchemaAction.Add;

            // try to get command properties from calling method attribute
            DBCommandAttribute commandAttribute = (DBCommandAttribute)Attribute.GetCustomAttribute(method, typeof(DBCommandAttribute));
            if (commandAttribute != null)
            {
                if (commandAttribute.CommandText.Length > 0)
                {
                    commandText = commandAttribute.CommandText;
                }
                commandType = commandAttribute.CommandType;
                missingSchemaAction = commandAttribute.MissingSchemaAction;
                command.CommandTimeout = commandAttribute.Timeout;
            }

            // set command text
            command.CommandText = commandText;

            // set command type
            switch (commandType)
            {
                case DBCommandType.Lookup:
                case DBCommandType.Insert:
                case DBCommandType.Update:
                case DBCommandType.Delete:
                case DBCommandType.InsertOrUpdate:
                case DBCommandType.InsertNotExists:
                case DBCommandType.Text:
                    command.CommandType = CommandType.Text; break;
                case DBCommandType.StoredProcedure:
                    command.CommandType = CommandType.StoredProcedure; break;
                default:
                    command.CommandType = CommandType.Text; break;
            }

            // define command parameters.
            // In this step command text can be changed.
            int[] indexes = null;
            if (values != null && values.Length > 0)
            {
                indexes = new int[values.Length];
                SetParameters(command, method, values, indexes, commandType);
            }


            // execute command
            //object result = null;
            result = RunCommand(command, method.ReturnType, autoCloseConnection, missingSchemaAction, commandType == DBCommandType.Lookup);

            if (values != null && values.Length > 0)
            {
                // return command parameter values
                for (int i = 0; i < values.Length; ++i)
                {
                    int sqlParamIndex = indexes[i];
                    IDbDataParameter prm = (IDbDataParameter)command.Parameters[i];
                    if (sqlParamIndex >= 0 && prm.Direction != ParameterDirection.Input)
                    {
                        values[i] = prm.Value;
                    }
                }
            }
            return InternalCmd.ReturnValue(commandType, result, method.ReturnType, commandAttribute.ReturnIfNull);

        }

        /// <summary>
        /// Generates command parameters. 
        /// For some command types the command text can be changed during parameter generating.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="method"><see cref="MethodInfo"/> type object</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <param name="commandType"><see cref="DBCommandType"/> enumeration value</param>
        /// <param name="indexes">Array of parameter indexes.</param>
        internal void SetParameters(IDbCommand command, MethodInfo method, object[] values, int[] indexes, DBCommandType commandType)
        {
            #region InsertUpdate parts declaration
            string cmdPart1 = "";
            string cmdPart2 = "";
            string cmdPart3 = ""; 
            string cmdPart4 = "";
            string autNumberField = "";

            #endregion

            ParameterInfo[] methodParameters = method.GetParameters();

            int sqlParamIndex = 0;

            for (int paramIndex = 0; paramIndex < methodParameters.Length; ++paramIndex)
            {
                indexes[paramIndex] = -1;
                ParameterInfo paramInfo = methodParameters[paramIndex];

                // create command parameter
                IDbDataParameter sqlParameter = command.CreateParameter();// new SqlParameter();

                // set default values
                string paramName = paramInfo.Name;
                DalParamType paramCustType = DalParamType.Default;
                object v = values[paramIndex];

                // get parameter attribute and set command parameter settings
                DbFieldAttribute paramAttribute = (DbFieldAttribute)Attribute.GetCustomAttribute(paramInfo, typeof(DbFieldAttribute));
                if (paramAttribute != null)
                {
                    paramCustType = paramAttribute.ParameterType;

                    if (paramAttribute.IsNameDefined)
                        paramName = paramAttribute.Name;

                    if (paramAttribute.IsTypeDefined)
                        sqlParameter.DbType = paramAttribute.SqlDbType;

                    if (paramAttribute.IsSizeDefined)
                        sqlParameter.Size = paramAttribute.Size;

                    if (paramAttribute.IsScaleDefined)
                        sqlParameter.Scale = paramAttribute.Scale;

                    if (paramAttribute.IsPrecisionDefined)
                        sqlParameter.Precision = paramAttribute.Precision;

                    if (InternalCmd.CompareAsNullValues(paramAttribute.AsNull, v, paramInfo.ParameterType))
                    {
                        v = DBNull.Value;
                    }

                }

                // parameter direction
                if (paramCustType == DalParamType.SPReturnValue)
                {
                    sqlParameter.Direction = ParameterDirection.ReturnValue;
                    sqlParameter.DbType = DbType.Int32;
                }
                else if (paramInfo.ParameterType.IsByRef)
                {
                    sqlParameter.Direction = paramInfo.IsOut ? ParameterDirection.Output : ParameterDirection.InputOutput;
                }
                else
                {
                    sqlParameter.Direction = ParameterDirection.Input;
                }

                // generate parts of InsertUpdate expresion
                #region generate parts of InsertUpdateDelete expresion

                if (paramCustType == DalParamType.Identity)
                {
                    if (autNumberField.Length > 0)
                    {
                        throw new DalException("Only one identity parameter is possible");
                    }
                    autNumberField = paramName;
                    Type reftype = InternalCmd.GetRefType(paramInfo.ParameterType);
                    if (reftype == null)
                    {
                        throw new DalException("Identity parameter must be ByRef parameter");
                    }
                    sqlParameter.DbType = DbType.Int32;

                    // check default value
                    if (paramAttribute.AsNull.ToString() == DbFieldAttribute.NullValueToken)
                    {
                        if (Convert.ToInt64(v) <= 0)
                        {
                            v = DBNull.Value;
                        }
                    }
                }

                if (commandType == DBCommandType.InsertOrUpdate)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;

                    if (paramCustType != DalParamType.Identity)
                    {
                        cmdPart1 = InternalCmd.AddWithDelim(cmdPart1, ", ", fieldName);
                        cmdPart2 = InternalCmd.AddWithDelim(cmdPart2, ", ", cmdparamName);
                        cmdPart3 = InternalCmd.AddWithDelim(cmdPart3, ", ", fieldName + "=" + cmdparamName);
                    }
                    if (paramCustType == DalParamType.Key)
                    {
                        cmdPart4 = InternalCmd.AddWithDelim(cmdPart4, " and ", fieldName + "=" + cmdparamName);
                    }
                }
                else if (commandType == DBCommandType.InsertNotExists)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;

                    if (paramCustType != DalParamType.Identity)
                    {
                        cmdPart1 = InternalCmd.AddWithDelim(cmdPart1, ", ", fieldName);
                        cmdPart2 = InternalCmd.AddWithDelim(cmdPart2, ", ", cmdparamName);
                    }
                    if (paramCustType == DalParamType.Key)
                    {
                        cmdPart4 = InternalCmd.AddWithDelim(cmdPart4, " and ", fieldName + "=" + cmdparamName);
                    }
                }
                else if (commandType == DBCommandType.Insert)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;

                    if (paramCustType != DalParamType.Identity)
                    {
                        cmdPart1 = InternalCmd.AddWithDelim(cmdPart1, ", ", fieldName);
                        cmdPart2 = InternalCmd.AddWithDelim(cmdPart2, ", ", cmdparamName);
                    }
                }

                else if (commandType == DBCommandType.Update)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;

                    if ((paramCustType == DalParamType.Key) || (paramCustType == DalParamType.Identity))
                    {
                        cmdPart2 = InternalCmd.AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
                    }
                    else if (paramCustType != DalParamType.Identity)
                    {
                        cmdPart1 = InternalCmd.AddWithDelim(cmdPart1, ", ", fieldName + "=" + cmdparamName);
                    }
                }
                else if (commandType == DBCommandType.Delete)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;
                    cmdPart1 = "";
                    if ((paramCustType == DalParamType.Key) || (paramCustType == DalParamType.Identity))
                    {
                        cmdPart2 = InternalCmd.AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
                    }
                }

                #endregion

                // set parameter name
                sqlParameter.ParameterName = "@" + paramName.Replace("@", "");


                // set parameter value
                if (v == null)
                {
                    v = DBNull.Value;
                }
                sqlParameter.Value = values[paramIndex];// this is to set a proper data type
                sqlParameter.Value = v;

                // add parameter to the command object
                command.Parameters.Add(sqlParameter);
                indexes[paramIndex] = sqlParamIndex;
                sqlParamIndex++;
            }

            #region CommandBuilder

            string TableName = command.CommandText;
            BuildCommandText(command,commandType, TableName, cmdPart1, cmdPart2,cmdPart3,cmdPart4, autNumberField);

            #endregion

        }

        /// <summary>
        /// Executes a command object according to the return type.
        /// </summary>
        /// <param name="cmd">The command object.</param>
        /// <param name="retType">Return type</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="missingSchemaAction">Determines <see cref="MissingSchemaAction"/> type value in case of filling a datasets.</param>
        /// <param name="isScalar">Determines if the command is a lookup function.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal object RunCommand(IDbCommand cmd, Type retType, bool autoCloseConnection, MissingSchemaAction missingSchemaAction, bool isScalar)
        {
            try
            {
                object result = null;

                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                if (isScalar)
                {
                    return cmd.ExecuteScalar();
                }
                if (retType == typeof(int))
                {
                    result = cmd.ExecuteNonQuery();
                }
                else if (retType.FullName == "System.Void")
                {
                    cmd.ExecuteNonQuery();
                    result = null;
                }
                else if (retType == typeof(System.Data.DataSet))
                {
                       result = AdapterFactory.ExecuteDataSet(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);
                }
                else if (retType == typeof(System.Data.DataTable))
                {
                    result = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);
                }
                else if (retType == typeof(System.Data.DataRow[]))
                {
                    DataTable dt = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);

                    if (dt != null)
                        result = dt.Select();
                }
                else if (retType == typeof(System.Data.DataRow))
                {
                    DataTable dt = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);

                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Rows[0];
                }
                else if (typeof(IEntityItem).IsAssignableFrom(retType))
                {
                    DataTable dt = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.Rows[0].ToEntity(retType);
                }
                else if (typeof(IEntityItem[]).IsAssignableFrom(retType))
                {
                    DataTable dt = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        result = dt.EntityList(retType.GetElementType()).ToArray();
                    }
                }
                else if (AdapterFactory.IsAssignableOfList(retType, typeof(IEntityItem), false, ref retType))
                {
                    DataTable dt = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        result = dt.EntityList(retType);
                    }
                }

                else if (retType == typeof(System.Data.IDataReader))//System.Data.SqlClient.SqlDataReader))
                {
                    IDataReader dr = null;
                    if (autoCloseConnection)
                        dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    else
                        dr = cmd.ExecuteReader();

                    result = dr;
                }
                else if (retType == typeof(System.Data.IDbDataAdapter))//System.Data.SqlClient.SqlDataAdapter))
                {
                    IDbDataAdapter da = AdapterFactory.CreateIDataAdapter(cmd);
                    da.MissingSchemaAction = missingSchemaAction;
                    result = da;
                }
                else if (retType == typeof(System.Data.IDbCommand))//System.Data.SqlClient.SqlCommand))
                {
                    result = cmd;
                }
                else if (retType == typeof(System.Object))
                {

                    result = cmd.ExecuteScalar();
                }
                else
                {

                    result = cmd.ExecuteNonQuery();
                }


                return result;
            }
            catch (System.Data.DataException dex)
            {
                throw new DalException(dex.Message);
            }
            catch (Exception dbex)
            {
                throw new DalException(dbex.Message);
            }
            finally
            {
                if (autoCloseConnection && !(retType == typeof(System.Data.IDataReader)))//System.Data.SqlClient.SqlDataReader)))
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes a command object according to the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd">The command object.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="missingSchemaAction">Determines <see cref="MissingSchemaAction"/> type value in case of filling a datasets.</param>
        /// <param name="isScalar">Determines if the command is a lookup function.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal T RunCommand<T>(IDbCommand cmd, bool autoCloseConnection, MissingSchemaAction missingSchemaAction, bool isScalar)
        {
            Type retType = typeof(T);
            try
            {

                object result = RunCommand(cmd, retType, autoCloseConnection, missingSchemaAction, isScalar);
                return InternalCmd.ReturnValue<T>(result);
            }
            catch (Exception dbex)
            {
                throw new DalException(dbex.Message);
            }
        }


        #endregion

    }

}
