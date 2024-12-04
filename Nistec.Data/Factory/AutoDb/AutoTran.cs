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
#pragma warning disable CS1591
namespace Nistec.Data.Factory
{

	/// <summary>
	/// Execute Sql command with transaction
	/// </summary>
	public  class AutoTran
	{
        #region Dispose

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (command != null)
                    {
                        command.Dispose();
                        command = null;
                    }
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

        //~AutoTran()
        //{
        //    Dispose(false);
        //}

        #endregion

		#region Members
		private IDbCommand command=null;// = new SqlCommand();
		#endregion

		#region Constructor
		/// <summary>
		/// Private constructor. This is a static class and it cannot be created.
		/// </summary>
		private AutoTran() 
		{

		}

		/// <summary>
		/// Begin Transaction
		/// </summary>
		public void Begin(System.Data.IsolationLevel level)
		{
			try
			{
				if(command==null)return ;
				command.Connection.BeginTransaction(level);
			}
			catch(Exception ex)
			{
				throw new DalException(ex.Message);
			}
		}
		/// <summary>
		/// Commit Transaction
		/// </summary>
		public void Commit()
		{
			try
			{
				if(command==null)return ;
				command.Transaction.Commit();
			}
			catch(Exception ex)
			{
				throw new DalException(ex.Message);
			}
		}

		/// <summary>
		/// Rollback Transaction
		/// </summary>
		public void Rollback()
		{
			try
			{
				if(command==null)return ;
				command.Transaction.Rollback();
			}
			catch(Exception ex)
			{
				throw new DalException(ex.Message);
			}
		}


		/// <summary>
		/// public constructor
		/// </summary>
		/// <param name="connection">Connection property.</param>
		/// <param name="transaction">Transaction property.</param>
        public AutoTran(IDbConnection connection, IDbTransaction transaction)//, bool autoCloseConnection ) 
		{
			// create command object
            command =  connection.CreateCommand();
			command.Connection = connection;
			command.Transaction = transaction;
		}

		/// <summary>
		/// public constructor
		/// </summary>
		/// <param name="connection">Connection property.</param>
        public AutoTran(IDbConnection connection)//, bool autoCloseConnection ) 
		{
			// create command object
            command = connection.CreateCommand();
			command.Connection = connection;
		}

        /// <summary>
        /// public constructor
        /// </summary>
        /// <param name="dal">Connection property.</param>
        public AutoTran(AutoDb dal)//, bool autoCloseConnection ) 
        {
            // create command object
            command = dal.Connection.CreateCommand();// new SqlCommand();
            command.Connection = dal.Connection;
        }

		#endregion

		#region Methods

		/// <summary>
		/// Executes Sql commandText and returns execution result. 
		/// </summary>
		/// <param name="cmdText"></param>
		/// <param name="returnType"><see cref="DalReturnType"/> type object from which the command object is built.</param>
		/// <returns>return one of list <see cref="DalReturnType"/> type </returns>
		public  object Execute(string cmdText, DalReturnType returnType)
		{
		
			// set command text
			command.CommandText = cmdText;
			command.CommandType = CommandType.Text;
			
			// execute command
			object result = null;
            result = InternalCmd.RunCommand(command, AutoFactory.GetReturnType(returnType), false);
			return result;

		}

		#endregion

		#region TransMethods

		/// <summary>
		/// Concats two strings with a delimiter.
		/// </summary>
		/// <param name="s1">string 1</param>
		/// <param name="delim">delimiter</param>
		/// <param name="s2">string 2</param>
		/// <returns></returns>
		private string AddWithDelim(string s1, string delim, string s2)
		{
			if (s1 == "")
			{
				return s2;
			}
			return (s1 + delim + s2);
		}

		/// <summary>
		/// Executes Sql commandText and returns execution result. 
		/// </summary>
		/// <param name="cmdType">DBCommandType</param>
		/// <param name="cmdText">string command</param>
		/// <param name="returnType"><see cref="DalReturnType"/> type object from which the command object is built.</param>
		/// <returns>return one of list <see cref="DalReturnType"/> type </returns>
		public object Execute(DBCommandType cmdType, string cmdText, DalReturnType returnType)
		{
			return this.Execute(cmdType, cmdText, returnType, null);
		}

 

		/// <summary>
		/// Executes Sql commandText and returns execution result. 
		/// </summary>
		/// <param name="cmdType">DBCommandType</param>
		/// <param name="cmdText">string command</param>
		/// <param name="returnType"><see cref="DalReturnType"/> type object from which the command object is built.</param>
		/// <param name="values">values part of sql command</param>
		/// <returns>return one of list <see cref="DalReturnType"/> type </returns>
		public object Execute(DBCommandType cmdType, string cmdText, DalReturnType returnType, object[] values)
		{
			MethodInfo info1 = (MethodInfo) new StackTrace().GetFrame(1).GetMethod();
			DBCommandType type1 = cmdType;
			this.command.CommandText = cmdText;
			switch (type1)
			{
				case DBCommandType.Text:
				case DBCommandType.Insert:
				case DBCommandType.Update:
                case DBCommandType.InsertOrUpdate:
                case DBCommandType.InsertNotExists:
                    this.command.CommandType = CommandType.Text;
					break;

				case DBCommandType.StoredProcedure:
					this.command.CommandType = CommandType.StoredProcedure;
					break;

				default:
					this.command.CommandType = CommandType.Text;
					break;
			}
			object obj1 = null;
			if (values != null)
			{
				this.command.Parameters.Clear();
				int[] numArray1 = new int[values.Length];
                InternalCmd.SetParameters(this.command, info1, values, numArray1, type1);
                obj1 = InternalCmd.RunCommand(this.command, AutoFactory.GetReturnType(returnType), false);
				for (int num1 = 0; num1 < values.Length; num1++)
				{
					int num2 = numArray1[num1];
					if (num2 >= 0)
					{
						values[num1] =((IDbDataParameter) this.command.Parameters[num1]).Value;
					}
				}
				return obj1;
			}
            return InternalCmd.RunCommand(this.command, AutoFactory.GetReturnType(returnType), false);
		}

 
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private Type GetRefType(Type type)
		{
			Type reftype = null;
			string typeName = type.FullName;
			if (typeName.EndsWith("&"))
			{
				reftype = Type.GetType(typeName.Substring(0, typeName.Length - 1));
			}
			return reftype;
		}


		#endregion
	}	
}



