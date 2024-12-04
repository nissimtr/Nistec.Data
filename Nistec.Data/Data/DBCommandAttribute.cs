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
using System.Reflection;
using System.Reflection.Emit;

using Debug = System.Diagnostics.Debug;
#pragma warning disable CS1591
namespace Nistec.Data
{
    	
	
	#region summary
	/// <summary>
	/// This attribute defines properties of generated methods.
	/// </summary>
	/// <remarks>
	/// If this attribute is not defined for a method to be generated then
	/// <see cref="System.Data.CommandType.StoredProcedure">CommandType.StoredProcedure</see> value is applied for underlying 
	/// <see cref="System.Data.SqlClient.SqlCommand"/> object.
	/// </remarks>
	/// <example>Sample of get DataTable by command sql
	/// <code >
	///[DBCommand("SELECT * FROM Banks")]
	///public abstract DataTable Banks();
	///
	/// Sample of run Update sql
	///[DBCommand("UPDATE [Attribute] SET [Value]=@Value WHERE [AttribID]=@AttribID")]
	///public abstract void UpdateAttribute (string Value,int AttribID);
	///
	///	Sample of run Update sql with parameters
	///[DBCommand(DBCommandType.Update, "Attribute")]
	///public abstract void AttributeUpdate
	///(
	///[Nistec.Data.OleDb.DBParameter(50)]string Value,
	///[Nistec.Data.OleDb.DBParameter]int AttribID
	///);
	/// </code>
	/// </example> 
	#endregion

	[AttributeUsage(AttributeTargets.Method) ]
	public sealed class DBCommandAttribute : Attribute
	{
		#region private members
		private DBCommandType m_commandType = DBCommandType.StoredProcedure;
		private string m_commandText = String.Empty;
		private object m_returnIfNull = null;
		private MissingSchemaAction m_missingSchemaAction = MissingSchemaAction.Add;
        private int m_timeout;
		private const string NullValueToken = "Null value";
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="DBCommandAttribute"/> class with the specified
		/// <see cref="DBCommandAttribute.CommandText"/> value
		/// </summary>
		/// <param name="commandText">Is a value of <see cref="DBCommandAttribute.CommandText"/> property</param>
		public DBCommandAttribute(string commandText) 
			: this(DBCommandType.Text, commandText, null, MissingSchemaAction.Add){}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DBCommandAttribute"/> class with the specified
		/// <see cref="CommandText"/> and <see cref="ReturnIfNull"/> values
		/// </summary>
		/// <param name="commandText">Is a value of <see cref="DBCommandAttribute.CommandText"/> property</param>
		/// <param name="returnIfNull"></param>
		public DBCommandAttribute(string commandText, object returnIfNull) 
			: this(DBCommandType.Text, commandText, returnIfNull, MissingSchemaAction.Add){}

		/// <summary>
		/// Initializes a new instance of the <see cref="DBCommandAttribute"/> class with the specified
		/// <see cref="CommandType"/> and <see cref="CommandText"/> values
		/// </summary>
		/// <param name="commandType"></param>
		/// <param name="commandText">Is a value of <see cref="DBCommandAttribute.CommandText"/> property</param>
		public DBCommandAttribute(DBCommandType commandType, string commandText) 
			: this(commandType, commandText, null, MissingSchemaAction.Add){}

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCommandAttribute"/> class with the specified
        /// <see cref="CommandType"/>, <see cref="CommandText"/>
        /// and <see cref="ReturnIfNull"/> values
        /// </summary>
        /// <param name="commandType">Is a value of <see cref="DBCommandAttribute.CommandType"/> property</param>
        /// <param name="commandText">Is a value of <see cref="DBCommandAttribute.CommandText"/> property</param>
        /// <param name="returnIfNull">Is a value of <see cref="DBCommandAttribute.ReturnIfNull"/> property</param>
        public DBCommandAttribute(DBCommandType commandType, string commandText, object returnIfNull)
            : this(commandType, commandText, returnIfNull, MissingSchemaAction.Add, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBCommandAttribute"/> class with the specified
        /// <see cref="CommandType"/>, <see cref="CommandText"/>
        /// and <see cref="ReturnIfNull"/> values
        /// </summary>
        /// <param name="commandType">Is a value of <see cref="DBCommandAttribute.CommandType"/> property</param>
        /// <param name="commandText">Is a value of <see cref="DBCommandAttribute.CommandText"/> property</param>
        /// <param name="returnIfNull">Is a value of <see cref="DBCommandAttribute.ReturnIfNull"/> property</param>
        /// <param name="missingSchemaAction">Is a value of <see cref="DBCommandAttribute.MissingSchemaAction"/> property</param>
        public DBCommandAttribute(DBCommandType commandType, string commandText, object returnIfNull, MissingSchemaAction missingSchemaAction)
            : this(commandType, commandText, returnIfNull, missingSchemaAction, 0) { }

        /// <summary>
		/// Initializes a new instance of the <see cref="DBCommandAttribute"/> class with the specified
		/// <see cref="CommandType"/>, <see cref="CommandText"/>
		/// and <see cref="ReturnIfNull"/> values
		/// </summary>
		/// <param name="commandType">Is a value of <see cref="DBCommandAttribute.CommandType"/> property</param>
		/// <param name="commandText">Is a value of <see cref="DBCommandAttribute.CommandText"/> property</param>
		/// <param name="returnIfNull">Is a value of <see cref="DBCommandAttribute.ReturnIfNull"/> property</param>
		/// <param name="missingSchemaAction">Is a value of <see cref="DBCommandAttribute.MissingSchemaAction"/> property</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        public DBCommandAttribute(DBCommandType commandType, string commandText, object returnIfNull, MissingSchemaAction missingSchemaAction, int commandTimeOut) 
		{
			m_commandType = commandType;
			m_commandText = commandText;
			m_returnIfNull = returnIfNull;
            m_timeout = commandTimeOut;
			if(m_returnIfNull is string) 
				if((string)m_returnIfNull == NullValueToken) 
					m_returnIfNull = null;

			m_missingSchemaAction = missingSchemaAction;
		}

		/// <summary>
		/// An attribute builder method
		/// </summary>
		/// <param name="attr"></param>
		/// <returns>CustomAttributeBuilder</returns>
		public static CustomAttributeBuilder GetAttributeBuilder(DBCommandAttribute attr)
		{
			string commandText = attr.m_commandText;
			object returnIfNull = attr.m_returnIfNull;
            int timeout = attr.m_timeout;
            if (timeout < 0) timeout = 0;
			if(returnIfNull == null) returnIfNull = NullValueToken;
			Type[] arrParamTypes = new Type[] {typeof(DBCommandType), typeof(string), typeof(object), typeof(MissingSchemaAction),typeof(int)};
            object[] arrParamValues = new object[] { attr.m_commandType, commandText, returnIfNull, attr.m_missingSchemaAction, timeout };
			ConstructorInfo ctor = typeof(DBCommandAttribute).GetConstructor(arrParamTypes);
			return new CustomAttributeBuilder(ctor, arrParamValues);
		}
		#endregion

		#region Properties

		/// <summary>
		/// Defines the meaning of a CommandText property.
		/// </summary>
		/// <remarks>
		/// The meaning of a CommandText property depending of a CommendType property:
		/// <list type="table">
		/// <listheader>
		/// <term>CommandType value</term><description>CommandText meaning</description>
		/// </listheader>
		/// <item><term><see cref="DBCommandType.Text"/></term><description>An SQL expression</description></item>
		/// <item><term><see cref="DBCommandType.StoredProcedure"/> (default value)</term><description>A name of a stored procedure</description></item>
		/// <item><term><see cref="DBCommandType.Insert"/></term><description>A name of a table or a view</description></item>
		/// <item><term><see cref="DBCommandType.Update"/></term><description>A name of a table or a view</description></item>
		/// </list>
		/// If this property is not defined then <see cref="DBCommandType.StoredProcedure"/> as a default value.
		/// </remarks>
		public DBCommandType CommandType
		{
			get { return m_commandType; }
			set { m_commandType = value; }
		}

		/// <summary>
		/// This is a text of a command and is interpreted 
		/// according to a value of <see cref="CommandType"/> property.
		/// </summary>
		/// <remarks>
		/// The meaning of a CommandText property depending of a CommendType property:
		/// <list type="table">
		/// <listheader>
		/// <term>CommandType value</term><description>CommandText meaning</description>
		/// </listheader>
		/// <item><term><see cref="DBCommandType.Text"/></term><description>An SQL expression</description></item>
		/// <item><term><see cref="DBCommandType.StoredProcedure"/></term><description>A name of a stored procedure</description></item>
		/// <item><term><see cref="DBCommandType.Insert"/></term><description>A name of a table or a view</description></item>
		/// <item><term><see cref="DBCommandType.Update"/></term><description>A name of a table or a view</description></item>
		/// </list>
		/// If this value is not defined then the name of the method is used as a command text.
		/// </remarks>
		public string CommandText
		{
			get { return m_commandText == null ? string.Empty : m_commandText; }
			set { m_commandText = value; }
		}

		/// <summary>
		/// A value that will be returned if the command returns null. 
		/// This property should be defined if your generated method returns 
		/// a value type value and you expect that re result of the method execution 
		/// can be null. In this case the value of this property will be returned.
		/// </summary>
		public object ReturnIfNull
		{
			get { return m_returnIfNull; }
			set { m_returnIfNull = value; }
		}

		
		/// <summary>
		/// Missing Schema Action property
		/// </summary>
		public MissingSchemaAction MissingSchemaAction
		{
			get { return m_missingSchemaAction; }
			set { m_missingSchemaAction = value; }
		}

        /// <summary>
        /// command time out, default =0
        /// </summary>
        public int Timeout
        {
            get { return m_timeout; }
            set { m_timeout = value; }
        }

		#endregion

	}
    
	

}

