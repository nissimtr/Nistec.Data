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
using System.Data.OleDb;

using Debug = System.Diagnostics.Debug;
using StackTrace = System.Diagnostics.StackTrace;
using Nistec.Data.Entities;
using Nistec.Data.Factory;

namespace Nistec.Data.OleDb
{
    #region generic DbCommand

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
    /// This class contains method RunCommand to create and execute Sql commands
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

    public class DbCommand : Nistec.Data.Factory.CommandFactory, IDisposable//,IDalCommand
    {

        #region Constructor
        ///// <summary>
        ///// public constructor. 
        ///// </summary>
        internal DbCommand()
        {
            //Nistec.Net.DalNet.NetFram("Data.Sql", "CTL");
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
        // : base(connectionString, DBProvider.OleDb) 
        {
            base.Init(connectionString, DBProvider.OleDb, true);
        }


        /// <summary>
        /// DbCommand Constructor with connection
        /// </summary>
        public DbCommand(OleDbConnection connection, bool autoCloseConnection)
        //    : base(connection, autoCloseConnection)
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
        internal override string BuildCommandText(IDbCommand command, DBCommandType commandType, string TableName, string cmdPart1, string cmdPart2, string cmdPart3,string cmdPart4, string autNumberField)
        {
            return BuildCommandTextInternal(command, commandType, TableName, cmdPart1, cmdPart2,cmdPart3, cmdPart4, autNumberField);
        }
        /// <summary>
        /// Create Adapter as <see cref="IDbAdapter"/>
        /// </summary>
        /// <returns></returns>
        protected override IDbAdapter CreateAdapter()
        {
            IDbAdapter adapter = new DbAdapter(m_connection as OleDbConnection);
            return adapter;
        }


        internal static string BuildCommandTextInternal(IDbCommand command, DBCommandType commandType, string TableName, string cmdPart1, string cmdPart2, string cmdPart3, string cmdPart4, string autNumberField)
        {
            string cmdString = null;
            string CommandText = "";

            if (commandType == DBCommandType.InsertOrUpdate)
            {
                cmdString = SqlFormatter.InsertOrUpdateString(TableName, cmdPart1, cmdPart2,cmdPart3,cmdPart4);
                CommandText = cmdString;
                command.CommandText = CommandText;
            }
            else if (commandType == DBCommandType.InsertNotExists)
            {
                cmdString = SqlFormatter.InsertNotExistsString(TableName, cmdPart1, cmdPart2, cmdPart4);
                CommandText = cmdString;
                command.CommandText = CommandText;
            }
            else if (commandType == DBCommandType.Insert)
            {

                cmdString = SqlFormatter.InsertString(TableName, cmdPart1, cmdPart2);// String.Format(" INSERT INTO [{0}]({1}) VALUES({2}) ", TableName, cmdPart1, cmdPart2);
                CommandText = cmdString;
                command.CommandText = CommandText;
            }

            else if (commandType == DBCommandType.Update)
            {
                if (cmdPart2 == "")
                {
                    throw new DalException("No Identity or Autoincrement field is defined.");
                }

                cmdString = SqlFormatter.UpdateString(TableName, cmdPart1, cmdPart2);// String.Format(" UPDATE [{0}] SET {1} WHERE {2} ", TableName, cmdPart1, cmdPart2);
                CommandText = cmdString;
                command.CommandText = CommandText;

            }
            else if (commandType == DBCommandType.Delete)
            {
                if (cmdPart2 == "")
                {
                    throw new DalException("No Identity or Autoincrement field is defined.");
                }

                cmdString = SqlFormatter.DeleteString(TableName, cmdPart2);// String.Format(" DELETE FROM {0} where {1} ", TableName, cmdPart2);

                CommandText = cmdString;
                command.CommandText = CommandText;
            }



            return command.CommandText;

        }

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

    }
}



