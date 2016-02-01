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
using System.Threading;
using System.Linq;

using Debug = System.Diagnostics.Debug;
using StackTrace = System.Diagnostics.StackTrace;
using System.Data.OleDb;
using Nistec.Data;
using Nistec.Data.Entities;
using System.Collections.Generic;
using Nistec.Generic;
using Nistec.Runtime;

namespace Nistec.Data.Factory
{
	#region summary
	/// <summary>
	/// This class contains static method Execute to create and execute Sql commands
	/// </summary>
	/// <example >
	/// <code >
	/// 
	///	public sealed class Data: Nistec.Data.OleDb.AutoBase  
	/// {
	///		public ClassDB dalDB{get{return (ClassDB)GetDalDB();}}
	///		public CheqsProperty dalCheq{get{return (CheqsProperty)GetDalDB();}}
	/// }
	/// 
	///	public abstract class ClassDB : Nistec.Data.OleDb.DalDB
	///	{
	///
	///		[DBCommand("SELECT ID FROM CheqProperty WHERE PropertyName=@name")]
	///		public abstract object GetCheqPropertyID(string name);
	///
	///		[DBCommand("SELECT PropertyName FROM CheqProperty ")]
	///		public abstract ArrayList GetCheqPropertyList();
	///
	///		[DBCommand("SELECT ID,PropertyName FROM CheqProperty")]
	///		public abstract Record[] GetCheqPropertyRecords();
	///	}
	/// 
	/// private int GetPropertyID(string name)
	/// {
	/// 	return (int)App.DB.dalDB.GetCheqPropertyID (name) ; 
	/// }
	/// 
	///	private void GetRecordList()
	/// {
	///		Nistec.Data.Record[] rcd=App.DB.dalDB.GetCheqPropertyRecords();
	///		System.Text.StringBuilder sb=new System.Text.StringBuilder (); 
	///		string s="";
	///		for(int i=0;i  rcd.Length ;i++)
	///		{
	///		s="DisplayMember : " + ((Nistec.Data.Record)rcd.GetValue (i)).DisplayMember.ToString () ; 
	///		sb.Append (s);
	///		s=" ,  ValueMember : " + ((Nistec.Data.Record)rcd.GetValue (i)).ValueMember.ToString ()+ "\n\r" ; 
	///		sb.Append (s);
	///		}
	///		MessageBox.Show (sb.ToString ());
	/// }
	///	private void GetList()
	///	{
	///
	///		System.Text.StringBuilder sb=new System.Text.StringBuilder (); 
	///		ArrayList list=App.DB.dalDB.GetCheqPropertyList ();
	///		for(int i=0;i list.Count ;i++)
	///		{
	///          sb.Append (list[i].ToString () + "\n\r");
	///               
	///		}
	///		MessageBox.Show (sb.ToString ());
	/// }
	/// </code>
	/// </example>
	#endregion

    internal static class InternalCmd
	{
        static readonly object m_lock = new object();

		#region Execute Methods
 
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
        public static object ExecuteCmd(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
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

            return ReturnValue(commandType, result, method.ReturnType, commandAttribute.ReturnIfNull);
        }

               /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Connection property.</param>
        /// <param name="transaction">Transaction property.</param>
        /// <param name="method"><see cref="MethodInfo"/> type object from which the command object is built.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        public static T ExecuteCmd<T>(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
        {
            return ExecuteCmd<T, T>(connection, transaction, method, values, autoCloseConnection, commandTimeOut);
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="connection">Connection property.</param>
        /// <param name="transaction">Transaction property.</param>
        /// <param name="method"><see cref="MethodInfo"/> type object from which the command object is built.</param>
        /// <param name="values">Array of values for the command parameters.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        public static TResult ExecuteCmd<TItem, TResult>(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection, int commandTimeOut)
        {
            if (method == null)
            {
                // this is done because this method can be called explicitly from code.
                method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            }
            TResult result = default(TResult);

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
            result = RunCommand<TItem, TResult>(command, /*method.ReturnType,*/ autoCloseConnection, missingSchemaAction, commandType == DBCommandType.Lookup);

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
            
            if (result == null)
            {
                return GenericTypes.ImplicitConvert<TResult>(result, GenericTypes.ImplicitConvert<TResult>(commandAttribute.ReturnIfNull));
            }

            return result;
        }

        #endregion

        #region execute cmd

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="schemaAction"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="parameters"></param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut"></param>
        /// <returns></returns>
        internal static object ExecuteCmd<T>(IDbConnection connection, IDbTransaction transaction, DBCommandType commandType, string commandText, MissingSchemaAction schemaAction, T returnIfNull, DataParameter[] parameters, bool autoCloseConnection, int commandTimeOut)
        {
            Type returnType = typeof(T);
            object result = null;
            int[] indexes = null;

            // create command object
            IDbCommand command = CreateCommand(connection, commandType, commandText, commandTimeOut, parameters, out indexes);

            // execute command
            //object result = null;
            result = RunCommand(command, returnType, autoCloseConnection, schemaAction, commandType == DBCommandType.Lookup);

            if (parameters != null)
            {
                // return command parameter values
                for (int i = 0; i < parameters.Length; ++i)
                {
                    int sqlParamIndex = indexes[i];
                    if (sqlParamIndex >= 0)
                    {
                        parameters[i].Value = ((IDbDataParameter)command.Parameters[i]).Value;
                    }
                }
            }
            return ReturnValue<T>(commandType, result, returnIfNull);
        }

        #endregion

        #region set parameters

 
        /// <summary>
		/// Generates command parameters. 
		/// For some command types the command text can be changed during parameter generating.
		/// </summary>
		/// <param name="command">Command object.</param>
		/// <param name="method"><see cref="MethodInfo"/> type object</param>
		/// <param name="values">Array of values for the command parameters.</param>
		/// <param name="commandType"><see cref="DBCommandType"/> enumeration value</param>
        /// <param name="indexes">Array of parameter indexes.</param>
        internal static void SetParameters(IDbCommand command, MethodInfo method, object[] values, int[] indexes, DBCommandType commandType)
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

			for(int paramIndex = 0; paramIndex < methodParameters.Length; ++paramIndex)
			{
				indexes[paramIndex] = -1;
				ParameterInfo paramInfo = methodParameters[paramIndex];

				// create command parameter
                IDbDataParameter sqlParameter = command.CreateParameter();

				// set default values
				string paramName = paramInfo.Name;
				DalParamType paramCustType = DalParamType.Default;
				object v = values[paramIndex];

				// get parameter attribute and set command parameter settings
				DbFieldAttribute paramAttribute = (DbFieldAttribute) Attribute.GetCustomAttribute(paramInfo, typeof(DbFieldAttribute));
				if(paramAttribute != null)
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

					if(CompareAsNullValues(paramAttribute.AsNull, v, paramInfo.ParameterType))
					{
						v = DBNull.Value;
					}

				}

				// parameter direction
				if(paramCustType == DalParamType.SPReturnValue)
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

				if(paramCustType == DalParamType.Identity)
				{
					if(autNumberField.Length > 0)
					{
						throw new DalException("Only one identity parameter is possible");
					}
					autNumberField = paramName;
					Type reftype = GetRefType(paramInfo.ParameterType);
					if(reftype == null)
					{
						throw new DalException("Identity parameter must be ByRef parameter");
					}
                    sqlParameter.DbType = DbType.Int32;

					// check default value
					if(paramAttribute.AsNull.ToString() == DbFieldAttribute.NullValueToken)
					{
						if(Convert.ToInt64(v) <= 0)
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
                        cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName);
                        cmdPart2 = AddWithDelim(cmdPart2, ", ", cmdparamName);
                        cmdPart3 = AddWithDelim(cmdPart3, ", ", fieldName + "=" + cmdparamName);
                    }
                    if (paramCustType == DalParamType.Key)
                    {
                        cmdPart4 = AddWithDelim(cmdPart4, " and ", fieldName + "=" + cmdparamName);
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

				else if(commandType == DBCommandType.Insert )
				{
					string fieldName = "[" + paramName + "]";
					string cmdparamName = "@" + paramName;

					if(paramCustType != DalParamType.Identity)
					{
						cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName);
						cmdPart2 = AddWithDelim(cmdPart2, ", ", cmdparamName);
					}
				}

				else if(commandType == DBCommandType.Update )
				{
					string fieldName = "[" + paramName + "]";
					string cmdparamName = "@" + paramName;

					if((paramCustType == DalParamType.Key) || (paramCustType == DalParamType.Identity))
					{
						cmdPart2 = AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
					}
					else if(paramCustType != DalParamType.Identity)
					{
						cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName + "=" + cmdparamName);
					}
				}
				else if(commandType == DBCommandType.Delete )
				{
					string fieldName = "[" + paramName + "]";
					string cmdparamName = "@" + paramName;
					cmdPart1 ="";
					if((paramCustType == DalParamType.Key) || (paramCustType == DalParamType.Identity))
					{
						cmdPart2 = AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
					}
				}

				#endregion

				// set parameter name
                sqlParameter.ParameterName = "@" + paramName.Replace("@", "");


				// set parameter value
				if(v == null)
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

			//string cmdString="";
			string TableName = command.CommandText;
            DBProvider provider = DbFactory.GetProvider(command);

            DbFactory.BuildCommandText(command, provider, commandType, TableName, cmdPart1, cmdPart2, cmdPart3, cmdPart4,autNumberField);

			#endregion
	
		}


        /// <summary>
        /// Generates command parameters. 
        /// For some command types the command text can be changed during parameter generating.
        /// </summary>
        /// <param name="command">Command object.</param>
        /// <param name="parameters">Array of values for the command parameters.</param>
        /// <param name="indexes">Array of parameter indexes.</param>
        /// <param name="commandType"><see cref="DBCommandType"/> enumeration value</param>
        internal static void SetParameters(IDbCommand command, DataParameter[] parameters, int[] indexes, DBCommandType commandType)
        {
            #region InsertUpdate parts declaration
            string cmdPart1 = "";
            string cmdPart2 = "";
            string cmdPart3 = "";
            string cmdPart4 = "";
            string autNumberField = "";
            int sqlParamIndex = 0;

            #endregion
  
            #region parameters
            if (parameters != null)
            {
                for (int paramIndex = 0; paramIndex < parameters.Length; ++paramIndex)
                {
                    indexes[paramIndex] = -1;
                    DataParameter paramInfo = parameters[paramIndex];

                    // create command parameter
                    IDbDataParameter sqlParameter = command.CreateParameter();
                    DataParameter.Copy(paramInfo, sqlParameter);

                    DalParamType paramCustType = paramInfo.ParamType;

                    // set default values
                    string paramName = paramInfo.ParameterName;
 
                    // parameter direction
                    if (paramCustType == DalParamType.SPReturnValue)
                    {
                        sqlParameter.Direction = ParameterDirection.ReturnValue;
                        sqlParameter.DbType = DbType.Int32;
                    }
                    else
                    {
                        sqlParameter.Direction = paramInfo.Direction;
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
                        Type reftype = GetRefType(paramInfo.Value.GetType());
                        if (reftype == null)
                        {
                            throw new DalException("Identity parameter must be ByRef parameter");
                        }
                        sqlParameter.DbType = DbType.Int32;

                    }

                    if (commandType == DBCommandType.InsertOrUpdate)
                    {
                        string fieldName = "[" + paramName + "]";
                        string cmdparamName = "@" + paramName;

                        if (paramCustType != DalParamType.Identity)
                        {
                            cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName);
                            cmdPart2 = AddWithDelim(cmdPart2, ", ", cmdparamName);
                            cmdPart3 = AddWithDelim(cmdPart1, ", ", fieldName + "=" + cmdparamName);

                        }
                        if (paramCustType == DalParamType.Key)
                        {
                            cmdPart4 = AddWithDelim(cmdPart4, " and ", fieldName + "=" + cmdparamName);
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
                            cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName);
                            cmdPart2 = AddWithDelim(cmdPart2, ", ", cmdparamName);
                        }
                    }

                    else if (commandType == DBCommandType.Update)
                    {
                        string fieldName = "[" + paramName + "]";
                        string cmdparamName = "@" + paramName;

                        if ((paramCustType == DalParamType.Key) || (paramCustType == DalParamType.Identity))
                        {
                            cmdPart2 = AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
                        }
                        else if (paramCustType != DalParamType.Identity)
                        {
                            cmdPart1 = AddWithDelim(cmdPart1, " and ", fieldName + "=" + cmdparamName);
                        }
                    }
                    else if (commandType == DBCommandType.Delete)
                    {
                        string fieldName = "[" + paramName + "]";
                        string cmdparamName = "@" + paramName;
                        cmdPart1 = "";
                        if ((paramCustType == DalParamType.Key) || (paramCustType == DalParamType.Identity))
                        {
                            cmdPart2 = AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
                        }
                    }

                    #endregion

                    // set parameter name
                    sqlParameter.ParameterName = paramInfo.ParameterNameFixed;// "@" + paramName;
                    sqlParameter.Value = paramInfo.ValueFixed;

                   
                    // add parameter to the command object
                    command.Parameters.Add(sqlParameter);
                    indexes[paramIndex] = sqlParamIndex;
                    sqlParamIndex++;
                }
            }
 
            #endregion

            #region CommandBuilder
            
            string TableName = command.CommandText;
            DBProvider provider = DbFactory.GetProvider(command);

            DbFactory.BuildCommandText(command,provider, commandType, TableName, cmdPart1, cmdPart2,cmdPart3,cmdPart4, autNumberField);
            
            #endregion

        }

        internal static IDbCommand CreateCommand(IDbConnection connection, DBCommandType commandType, string commandText, int commandTimeOut, DataParameter[] parameters, out int[] indexes)
        {
            // create command object
            IDbCommand command = connection.CreateCommand();
            command.Connection = connection;
            if (commandTimeOut > 0)
            {
                command.CommandTimeout = commandTimeOut;
            }

            // set command text
            command.CommandText = commandText;
            //command.CommandType = CommandType.Text;

            // set command type
            switch (commandType)
            {
                case DBCommandType.Lookup:
                case DBCommandType.Insert:
                case DBCommandType.Update:
                case DBCommandType.Delete:
                case DBCommandType.InsertOrUpdate:
                case DBCommandType.Text:
                    command.CommandType = CommandType.Text; break;
                case DBCommandType.StoredProcedure:
                    command.CommandType = CommandType.StoredProcedure; break;
                default:
                    command.CommandType = CommandType.Text; break;
            }

            // define command parameters.
            // In this step command text can be changed.
            indexes = null;
            if (parameters != null)
            {
                indexes = new int[parameters.Length];
                SetParameters(command, parameters, indexes, commandType);
            }
 
            return command;
        }

        #endregion

        #region return value

        internal static T ReturnValue<T>(object result)
        {

            if (result == null || result == System.DBNull.Value)
            {
                if (typeof(T).IsValueType)
                    return ActivatorUtil.CreateInstance<T>();
                return default(T);
            }

            Type ttype = typeof(T);
            if (ttype == result.GetType())
                return (T)result;

            return GenericTypes.Convert<T>(result);

        }


        /// <summary>
        /// Return Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="result"></param>
        /// <param name="retIfNull"></param>
        /// <returns></returns>
        internal static T ReturnValue<T>(DBCommandType commandType, object result, T retIfNull)
        {
            Type type = typeof(T);

            // adjust null result
            if (result == null || result == System.DBNull.Value)
            {
                return retIfNull;
            }

            if (commandType == DBCommandType.Lookup)
            {
                return GenericTypes.Convert<T>(result, retIfNull);
            }
            if (type == result.GetType())
            {
                return (T)result;
            }

            return retIfNull;
        }


        /// <summary>
        /// Return Value
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="result"></param>
        /// <param name="retType"></param>
        /// <param name="retIfNull"></param>
        /// <returns></returns>
        internal static object ReturnValue(DBCommandType commandType, object result, Type retType, object retIfNull)
        {

            Type defaultType=retIfNull==null?retType: retIfNull.GetType();

            if(defaultType!=retType)
            {
                return result;
            }

            // adjust null result
            if (result == null || result == System.DBNull.Value)
            {
                result = retIfNull;
                if (defaultType == typeof(DateTime))
                {
                    result = new DateTime((int)retIfNull);
                }
                return result;
            }

            if (commandType == DBCommandType.Lookup)
            {
                if (result == null)
                {
                    return null;
                }
                
                if (retType == typeof(int))
                {
                    return GenericTypes.Convert<int>(result, (defaultType == typeof(int)) ? (int)retIfNull : 0);
                }
                if (retType == typeof(decimal))
                {
                    return GenericTypes.Convert<decimal>(result, (defaultType == typeof(decimal)) ? (decimal)retIfNull : 0m);
                }
                if (retType == typeof(float))
                {
                    return GenericTypes.Convert<float>(result, (defaultType == typeof(float)) ? (float)retIfNull : 0f);
                }
                if (retType == typeof(double))
                {
                    return GenericTypes.Convert<double>(result, (defaultType == typeof(double)) ? (double)retIfNull : 0L);
                }
                if (retType == typeof(bool))
                {
                    return GenericTypes.Convert<bool>(result, (defaultType == typeof(bool)) ? (bool)retIfNull : false);
                }
                if (retType == typeof(DateTime))
                {
                    return GenericTypes.Convert<DateTime>(result, (defaultType == typeof(DateTime)) ? (DateTime)retIfNull : DateTime.Now);
                }
                else //if (retType == typeof(string))
                {
                    return result.ToString();
                }
            }
            return result;
        }

        /// <summary>
		/// Concats two strings with a delimiter.
		/// </summary>
		/// <param name="s1">string 1</param>
		/// <param name="delim">delimiter</param>
		/// <param name="s2">string 2</param>
		/// <returns></returns>
		internal static string AddWithDelim(string s1, string delim, string s2)
		{
			if(s1 == "") return s2;
			else return s1 + delim + s2;
		}


 
        #endregion

        #region run command

        /// <summary>
		/// Execute Command
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="retType"></param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
		internal static object RunCommand(IDbCommand cmd, Type retType, bool autoCloseConnection)
		{
			return RunCommand(cmd, retType, autoCloseConnection, MissingSchemaAction.Add);
		}

             /// <summary>
		/// Executes a command object according to the return type.
		/// </summary>
		/// <param name="cmd">The command object.</param>
		/// <param name="retType">Return type</param>
		/// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
		/// <param name="missingSchemaAction">Determines <see cref="MissingSchemaAction"/> type value in case of filling a datasets.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal static object RunCommand(IDbCommand cmd, Type retType, bool autoCloseConnection, MissingSchemaAction missingSchemaAction)
        {
            return RunCommand(cmd, retType, autoCloseConnection, missingSchemaAction,false);
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
        private static object RunCommand(IDbCommand cmd, Type retType, bool autoCloseConnection, MissingSchemaAction missingSchemaAction, bool isScalar)
        {
            try
            {
                object result = null;

                //System.Diagnostics.Trace.Write(String.Format("RunCommand Name: {0} ", cmd.CommandText));

                lock (m_lock)
                {
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }
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
                        result = dt.EntityList(retType);
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
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cmd">The command object.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="missingSchemaAction">Determines <see cref="MissingSchemaAction"/> type value in case of filling a datasets.</param>
        /// <param name="isScalar">Determines if the command is a lookup function.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal static TResult RunCommand<TItem, TResult>(IDbCommand cmd, bool autoCloseConnection, MissingSchemaAction missingSchemaAction, bool isScalar)
        {
            Type retType = typeof(TResult);

            try
            {

                object result = null;

                //System.Diagnostics.Trace.Write(String.Format("RunCommand Name: {0} ", cmd.CommandText));

                lock (m_lock)
                {
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }
                }

                if (isScalar)
                {
                    return (TResult)cmd.ExecuteScalar();
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
                        result = dt.Rows[0].ToEntity<TResult>();
                }
                else if (typeof(IEntityItem[]).IsAssignableFrom(retType))
                {
                    DataTable dt = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                        result = dt.EntityList<TItem>().ToArray();
                }
                else if (AdapterFactory.IsAssignableOfList(retType, typeof(IEntityItem)))//, false))
                {
                    DataTable dt = AdapterFactory.ExecuteDataTable(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        result = dt.EntityList<TItem>();
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

                return (TResult)result;
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
        /// <param name="cmd">The command object.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="missingSchemaAction">Determines <see cref="MissingSchemaAction"/> type value in case of filling a datasets.</param>
        /// <param name="tableMapping"></param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        private static object RunCommandDs(IDbCommand cmd, bool autoCloseConnection, MissingSchemaAction missingSchemaAction, string[] tableMapping)
        {
            try
            {
                lock (m_lock)
                {
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }
                }

                return AdapterFactory.ExecuteDataSet(cmd, null, missingSchemaAction == MissingSchemaAction.AddWithKey, false, tableMapping);
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
                if (autoCloseConnection)
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
        internal static T RunCommand<T>(IDbCommand cmd, bool autoCloseConnection, MissingSchemaAction missingSchemaAction, bool isScalar)
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

        /// <summary>
        /// Executes a command object according to the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd">The command object.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="missingSchemaAction">Determines <see cref="MissingSchemaAction"/> type value in case of filling a datasets.</param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal static T RunCommand<T>(IDbCommand cmd, bool autoCloseConnection, MissingSchemaAction missingSchemaAction)
        {
            Type retType = typeof(T);
            try
            {

                object result = RunCommand(cmd, retType, autoCloseConnection, missingSchemaAction, false);

                return InternalCmd.ReturnValue<T>(result);
            }
            catch (Exception dbex)
            {
                throw new DalException(dbex.Message);
            }
        }
    
        #endregion

        #region run command none query

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmd">The command object.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <returns>return the number of rows affected.</returns>
        internal static int RunCommandNonQuery(IDbCommand cmd, bool autoCloseConnection)
        {
            try
            {
                lock (m_lock)
                {
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }
                }
                return cmd.ExecuteNonQuery();
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
                if (autoCloseConnection)
                {
                    if (cmd.Connection.State == ConnectionState.Open)
                        cmd.Connection.Close();
                }
            }
        }

        #endregion

        #region run command scalar

        /// <summary>
        /// Executes a command Scalar according to the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd">The command object.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="returnIfNull"></param>
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        internal static T RunCommandScalar<T>(IDbCommand cmd, bool autoCloseConnection, T returnIfNull)
        {
            Type retType = typeof(T);
            try
            {
                object result = null;

                lock (m_lock)
                {
                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }
                }
                result = cmd.ExecuteScalar();
                return GenericTypes.Convert<T>(result, returnIfNull);//default(T));
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
        /// Executes a command Scalar according to the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="parameters"></param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="commandTimeOut"></param>
        /// <returns></returns>
        internal static T RunCommandScalar<T>(IDbConnection connection, string commandText, T returnIfNull, DataParameter[] parameters, bool autoCloseConnection, int commandTimeOut)
        {
            int[] indexes;

            IDbCommand command = CreateCommand(connection, DBCommandType.Lookup, commandText, commandTimeOut, parameters, out indexes);

            return RunCommandScalar<T>(command, autoCloseConnection, returnIfNull);
        }

        #endregion

        #region run async command

        /// <summary>
        /// RunAsyncCommand, asynchronously execute
        /// the specified command against the connection. 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="interval"></param>
        /// <returns>complete Affected rows</returns>
        internal static int RunAsyncCommand(SqlCommand cmd, bool autoCloseConnection, int interval)
        {
            // Given command text and connection string, asynchronously execute
            // the specified command against the connection. For this example,
            // the code displays an indicator as it is working, verifying the 
            // asynchronous behavior. 
            
            using (cmd.Connection)//SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    if (cmd.Connection.State == ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }

                    int result = 0;

                    IAsyncResult aresult = cmd.BeginExecuteNonQuery();

                    while (!aresult.IsCompleted)
                    {
                        System.Threading.Thread.Sleep(interval);
                    }
                    result = cmd.EndExecuteNonQuery(aresult);
                    return result;
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
                finally
                {
                    if (autoCloseConnection)
                    {
                        cmd.Connection.Close();
                    }
                }
            }
        }

		#endregion

		#region Functions
		/// <summary>
		/// Returns type from reference type.
		/// </summary>
		/// <param name="type">Reference type value.</param>
		/// <returns></returns>
		internal static Type GetRefType(Type type)
		{
			Type reftype = null;

			string typeName = type.FullName;
			if(typeName.EndsWith("&"))
			{
				reftype = Type.GetType(typeName.Substring(0, typeName.Length-1));
			}

			return reftype;
		}

		/// <summary>
		/// Compares parameter value with a value that must be treated as DBNull.
		/// </summary>
		/// <param name="AsNull">The value that must be treated as DBNull</param>
		/// <param name="ParamValue">The parameter value.</param>
		/// <param name="ParamType">Type of the parameter value.</param>
		/// <returns></returns>
		internal static bool CompareAsNullValues(object AsNull, object ParamValue, Type ParamType)
		{
			bool b = false;
			if(AsNull.ToString() == DbFieldAttribute.NullValueToken) return false;
			if(AsNull == null) return false;
			
			if(ParamType == typeof(DateTime))
			{
				DateTime d = new DateTime((int)AsNull);
				b = (d == (DateTime)ParamValue);
			}
			else if(ParamType == typeof(byte) || ParamType == typeof(int) || ParamType == typeof(long))
			{
				long v = Convert.ToInt64(AsNull);
				b = v == Convert.ToInt64(ParamValue);
			}
			else if(ParamType == typeof(float) || ParamType == typeof(double))
			{
				double v = Convert.ToDouble(AsNull);
				b = v == Convert.ToDouble(ParamValue);
			}
			else if(AsNull.Equals(ParamValue))
			{
				b = true;
			}
			return b;
		}
		#endregion

	}
}



