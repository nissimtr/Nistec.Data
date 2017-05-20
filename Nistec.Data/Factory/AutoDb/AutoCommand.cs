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
using Debug = System.Diagnostics.Debug;
using StackTrace = System.Diagnostics.StackTrace;
using System.Data.OleDb;
using Nistec.Data;
using Nistec.Data.Entities;

namespace Nistec.Data.Factory
{
	#region summary
	/// <summary>
	/// This class contains static method <see cref="Execute"/> to create and execute Sql commands
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

    public static class AutoCommand
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
        /// <returns>return one of list <see cref="DalReturnType"/> type </returns>
        public static object Execute(IDbConnection connection, IDbTransaction transaction, MethodInfo method, object[] values, bool autoCloseConnection)
        {
            if (method == null)
            {
                // this is done because this method can be called explicitly from code.
                method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            }
            return InternalCmd.ExecuteCmd(connection, transaction, method, values, autoCloseConnection, 0);

        }

        #endregion

	}
}



