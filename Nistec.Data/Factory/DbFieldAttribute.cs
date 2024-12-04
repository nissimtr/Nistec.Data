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
namespace Nistec.Data.Factory
{


	/// <summary>
	/// This attribute defines properties of method's parameters
	/// </summary>
	[ AttributeUsage(AttributeTargets.Parameter) ]
	public class DbFieldAttribute : Attribute
	{
		/// <summary>
		///  Null Value Return
		/// </summary>
		public const string NullValueToken = "";//"NullValue";

		#region Private members
		private string m_name = "";
        private const DbType ParamTypeNotDefinedValue = (DbType)1000000;
        private DbType m_sqlDbType = ParamTypeNotDefinedValue;
		private int m_size = 0;
		private byte m_precision = 0;
		private byte m_scale = 0;
		private object m_AsNull = NullValueToken;
		private DalParamType m_parameterType = DalParamType.Default;
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class
		/// </summary>
		public DbFieldAttribute() {}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
		public DbFieldAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class
		/// </summary>
		/// <param name="size">Is a value of <see cref="Size"/> property</param>
		public DbFieldAttribute(int size)
		{
			Size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
		public DbFieldAttribute(DalParamType parameterType)
		{
			ParameterType = parameterType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
		/// <param name="size">Is a value of <see cref="Size"/> property</param>
		public DbFieldAttribute(DalParamType parameterType, int size)
		{
			ParameterType = parameterType;
			Size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
		public DbFieldAttribute(DalParamType parameterType, string name)
		{
			ParameterType = parameterType;
			m_name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
		/// <param name="size">Is a value of <see cref="Size"/> property</param>
		public DbFieldAttribute(string name, int size)
		{
			Name = name;
			Size = size;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DbFieldAttribute"/> class with the specified
        /// <see cref="Name"/>, <see cref="DbType"/>, <see cref="Size"/>, <see cref="Precision"/>, 
        /// <see cref="Scale"/>, <see cref="AsNull"/> and <see cref="ParameterType"/> values
        /// </summary>
        /// <param name="sqlDbType">Is a value of <see cref="DbType"/> property</param>
        /// <param name="precision">Is a value of <see cref="Precision"/> property</param>
        /// <param name="scale">Is a value of <see cref="Scale"/> property</param>
        public DbFieldAttribute(DbType sqlDbType, byte precision, byte scale)
        {
            SqlDbType = sqlDbType;
            Precision = precision;
            Scale = scale;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DbFieldAttribute"/> class with the specified
		/// <see cref="Name"/>, <see cref="DbType"/>, <see cref="Size"/>, <see cref="Precision"/>, 
		/// <see cref="Scale"/>, <see cref="AsNull"/> and <see cref="ParameterType"/> values
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
		/// <param name="sqlDbType">Is a value of <see cref="DbType"/> property</param>
		/// <param name="size">Is a value of <see cref="Size"/> property</param>
		/// <param name="precision">Is a value of <see cref="Precision"/> property</param>
		/// <param name="scale">Is a value of <see cref="Scale"/> property</param>
		/// <param name="asNull">Is a value of <see cref="AsNull"/> property</param>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        public DbFieldAttribute(string name, DbType sqlDbType, int size, byte precision, byte scale, object asNull, DalParamType parameterType)
		{
			Name = name;
			SqlDbType = sqlDbType;
			Size = size;
			Precision = precision;
			Scale = scale;
			AsNull = asNull;
			ParameterType = parameterType;
		}

		/// <summary>
		/// An attribute builder method
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
		public static CustomAttributeBuilder GetAttributeBuilder(DbFieldAttribute attr)
		{
			string name = attr.m_name;
			Type[] arrParamTypes = new Type[] {typeof(string), typeof(DbType), typeof(int), typeof(byte), typeof(byte), typeof(object), typeof(DalParamType)};
			object[] arrParamValues = new object[] {name, attr.m_sqlDbType, attr.m_size, attr.m_precision, attr.m_scale, attr.m_AsNull, attr.m_parameterType};
			ConstructorInfo ctor = typeof(DbFieldAttribute).GetConstructor(arrParamTypes);
			return new CustomAttributeBuilder(ctor, arrParamValues);
		}
		#endregion

		#region Properties

		/// <summary>
		/// Sql parameter name. If this property is not defined 
		/// then a method parameter name is used.
		/// </summary>
		public string Name
		{
			get { return m_name == null ? string.Empty : m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// Sql parameter size. 
		/// It is strongly recomended to define this property for string parameters
		/// so that they could be trimmed to the size specified.
		/// </summary>
		public int Size
		{
			get { return m_size; }
			set { m_size = value; }
		}

		/// <summary>
		/// Sql parameter precision. It has not sense for non-numeric parameters.
		/// </summary>
		public byte Precision
		{
			get { return m_precision; }
			set { m_precision = value; }
		}

		/// <summary>
		/// Sql parameter scale. It has not sense for non-numeric parameters.
		/// </summary>
		public byte Scale
		{
			get { return m_scale; }
			set { m_scale = value; }
		}

		/// <summary>
		/// This parameter contains a value that will be interpreted as null. 
		/// This is usefull if you want to pass a null to a value type parameter.
		/// </summary>
		public object AsNull
		{
			get { return m_AsNull; }
			set { m_AsNull = value; }
		}

		/// <summary>
		/// Parameter Type
		/// </summary>
		public DalParamType ParameterType
		{
			get { return m_parameterType; }
			set { m_parameterType = value; }
		}

		/// <summary>
		/// Sql parameter type. 
		/// If not defined then method parameter type is converted to <see cref="DbType"/> type
		/// </summary>
        public DbType SqlDbType
		{
			get 
			{ 
				return m_sqlDbType; 
			}

			set 
			{ 
				m_sqlDbType = value; 
			}
		}

		/// <summary>
		/// Is Name Defined
		/// </summary>
        public bool IsNameDefined
		{
			get { return m_name != null && m_name.Length != 0; }
		}

		/// <summary>
		/// Is Size Defined
		/// </summary>
        public bool IsSizeDefined
		{
			get { return m_size > 0; }
		}

		/// <summary>
		/// Is Type Defined
		/// </summary>
        public bool IsTypeDefined
		{
			get { return m_sqlDbType != ParamTypeNotDefinedValue; }
		}

		/// <summary>
		/// Is Scale Defined
		/// </summary>
		internal bool IsScaleDefined
		{
			get { return m_scale > 0; }
		}

		/// <summary>
		/// Is Precision Defined
		/// </summary>
		internal bool IsPrecisionDefined
		{
			get { return m_precision > 0; }
		}
		#endregion

	}

	

}

