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
using System.Linq;

using Debug = System.Diagnostics.Debug;
using Nistec.Generic;
using System.Collections.Generic;

namespace Nistec.Data.Entities
{

    /// <summary>
    /// Parameter type enumeration for <see cref="EntityPropertyAttribute.ParameterType"/> property
    /// of <see cref="EntityPropertyType"/> attribute.
    /// </summary>
    public enum EntityPropertyType
    {
        /// <summary>
        /// The parameter is defaul and has not special role.
        /// </summary>
        Default,
        /// <summary>
        /// This parameter is a part of a table key 
        /// and is applicable only 
        /// </summary>
        Key,
        /// <summary>
        /// This parameter is a part of a table autoincremental field 
        /// and is applicable only 
        /// </summary>
        Identity,
        /// <summary>
        /// This parameter is not part of a table
        /// and is applicable only 
        /// </summary>
        NA,
       /// <summary>
        /// The parameter is a part of a table but for view only.
        /// </summary>
        View,
        /// <summary>
        /// The parameter is an optional part of a table but for view only.
        /// </summary>
        Optional
    }

	/// <summary>
	/// This attribute defines properties of method's properties
	/// </summary>
	[ AttributeUsage(AttributeTargets.Property) ]
    public class EntityPropertyAttribute : Attribute, INaAttribute
	{

        public static EntityPropertyAttribute Default
        {
            get { return new EntityPropertyAttribute(); }
        }

        public static EntityPropertyAttribute Create(PropertyInfo p)
        {
            return new EntityPropertyAttribute() { m_name=p.Name }; 
        }

 
		
		/// <summary>
		///  Null Value Return
		/// </summary>
		public const string NullValueToken = "";//NullValue";
        private const DbType ParamTypeNotDefinedValue = (DbType)1000000;

		#region Private members

		private string m_name = "";
        private string m_column = "";
        private string m_caption = "";
        private string m_ResourceKey;
        private int m_order = 0;
		private DbType m_sqlDbType = ParamTypeNotDefinedValue;
        private int m_size = 0;
		private object m_asNull = NullValueToken;
        private bool m_allowNull = true;
        private EntityPropertyType m_parameterType = EntityPropertyType.Default;
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
		/// </summary>
		public EntityPropertyAttribute() {}

		/// <summary>
        /// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
        public EntityPropertyAttribute(string name)
		{
			Name = name;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
		/// </summary>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        public EntityPropertyAttribute(int size)
		{
			Size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        public EntityPropertyAttribute(EntityPropertyType parameterType)
		{
			ParameterType = parameterType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
        /// </summary>
        /// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="column">Is a value of <see cref="Column"/> property</param>
         /// <param name="order">Is a value of <see cref="Order"/> property</param>
        public EntityPropertyAttribute(EntityPropertyType parameterType, string name, string column, int order)
        {
            ParameterType = parameterType;
            Name = name;
            m_column = column;
            m_order = order;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        public EntityPropertyAttribute(EntityPropertyType parameterType, int size)
		{
			ParameterType = parameterType;
            Size = size;
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
        /// </summary>
        /// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="allowNull">Is a value of <see cref="AllowNull"/> property</param>
        public EntityPropertyAttribute(EntityPropertyType parameterType, bool allowNull)
        {
            ParameterType = parameterType;
            m_allowNull = allowNull;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
        /// </summary>
        /// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        /// <param name="allowNull">Is a value of <see cref="AllowNull"/> property</param>
        public EntityPropertyAttribute(EntityPropertyType parameterType, int size, bool allowNull)
        {
            ParameterType = parameterType;
            Size = size;
            m_allowNull = allowNull;
        }

       
		/// <summary>
		/// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
		/// </summary>
		/// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
        public EntityPropertyAttribute(EntityPropertyType parameterType, string name)
		{
			ParameterType = parameterType;
			m_name = name;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class
		/// </summary>
		/// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        public EntityPropertyAttribute(string name, int size)
		{
			Name = name;
            Size = size;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityPropertyAttribute"/> class with the specified arguments.
        /// </summary>
        /// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="allowNull">Is a value of <see cref="AllowNull"/> property</param>
        /// <param name="sqlDbType">Is a value of <see cref="DbType"/> property</param>
        /// <param name="size">Is a value of <see cref="Size"/> property</param>
        /// <param name="asNull">Is a value of <see cref="AsNull"/> property</param>
        /// <param name="parameterType">Is a value of <see cref="ParameterType"/> property</param>
        /// <param name="column">Is a value of <see cref="Column"/> property</param>
        /// <param name="caption">Is a value of <see cref="Caption"/> property</param>
        /// <param name="order">Is a value of <see cref="Order"/> property</param>
        /// <param name="resourceKey">Is a value of <see cref="ResourceKey"/> property</param>
        public EntityPropertyAttribute(string name, bool allowNull, DbType sqlDbType, int size, object asNull, EntityPropertyType parameterType, string column, string caption, int order, string resourceKey)
        {
            Name = name;
            m_order = order;
            AllowNull = allowNull;
            SqlDbType = sqlDbType;
            Size = size;
            m_asNull = asNull;
            ParameterType = parameterType;
            m_column = column;
            m_caption = caption;
            m_ResourceKey = resourceKey;
        }

        /// <summary>
        /// An attribute builder method
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static CustomAttributeBuilder GetAttributeBuilder(EntityPropertyAttribute attr)
        {
            string name = attr.m_name;
            Type[] arrParamTypes = new Type[] { typeof(string), typeof(bool), typeof(object), typeof(DbType), typeof(int), typeof(object), typeof(EntityPropertyType), typeof(string), typeof(string), typeof(int), typeof(string) };
            object[] arrParamValues = new object[] { name, attr.AllowNull, attr.SqlDbType, attr.Size, attr.AsNull, attr.ParameterType, attr.Column, attr.Caption, attr.Order, attr.ResourceKey };
            ConstructorInfo ctor = typeof(EntityPropertyAttribute).GetConstructor(arrParamTypes);
            return new CustomAttributeBuilder(ctor, arrParamValues);
        }

 
        #endregion

        #region Properties

        /// <summary>
        /// Parameter name.
		/// </summary>
		public string Name
		{
			get { return m_name == null ? string.Empty : m_name; }
			set { m_name = value; }
		}

        /// <summary>
        /// Parameter column. usefull for DB field mapping
        /// If this property is not defined 
        /// then a method parameter Name is used.
        /// </summary>
        public string Column
        {
            get { return string.IsNullOrEmpty(m_column) ? Name : m_column; }
            set { m_column = value; }
        }

        /// <summary>
        /// Parameter caption. usefull for UI
        /// If this property is not defined 
        /// then a method parameter Column is used.
        /// </summary>
        public string Caption
        {
            get { return string.IsNullOrEmpty(m_caption) ? Column : m_caption; }
            set { m_caption = value; }
        }

        /// <summary>
        /// Get or Set The order of properties (Must be greater then zero). 
        /// </summary>
        public int Order
        {
            get { return m_order; }
            set { m_order = value; }
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
		/// This parameter contains a value that will be interpreted as null. 
		/// This is usefull if you want to pass a null to a value type parameter.
		/// </summary>
		public object AsNull
		{
			get { return m_asNull; }
			set { m_asNull = value; }
		}

        

		/// <summary>
		/// Parameter Type
		/// </summary>
        public EntityPropertyType ParameterType
		{
			get { return m_parameterType; }
			set { m_parameterType = value; }
		}

        /// <summary>
        /// Get indecate if propertyType is NA
        /// </summary>
        public bool IsNA
        {
            get { return ParameterType == EntityPropertyType.NA; }
        }

        /// <summary>
        /// Get indecate if propertyType is Identity
        /// </summary>
        public bool IsIdentity
        {
            get { return ParameterType == EntityPropertyType.Identity; }
        }

        /// <summary>
        /// Get indecate if propertyType is Key or Identity
        /// </summary>
        public bool IsKeyOrIdentity
        {
            get { return ParameterType == EntityPropertyType.Identity || ParameterType == EntityPropertyType.Key; }
        }
        
        internal bool IsValidInsertParameter
        {
            get { return IsIdentity == false && IsNA == false; }
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
        /// Indicate if parameter allow null value.
        /// </summary>
        public bool AllowNull
        {
            get { return m_allowNull; }
            set { m_allowNull = value; }
        }

        /// <summary>
        /// Resource Key.
        /// </summary>
        public string ResourceKey
        {
            get { return m_ResourceKey == null ? string.Empty : m_ResourceKey; }
            set { m_ResourceKey = value; }
        }

        /// <summary>
        /// Get Caption or property name if Caption not Defined
        /// </summary>
        public string GetCaption(string propertyName)
        {
            return IsCaptionDefined ? Caption : propertyName;
        }

        /// <summary>
        /// Get Column or property name if Column not Defined
        /// </summary>
        public string GetColumn(string propertyName)
        {
            return string.IsNullOrEmpty(Column) ? propertyName : Column;
        }

        #endregion

        #region Is defined properties

        /// <summary>
        /// Is Resource Key Defined
        /// </summary>
        public bool IsResourceKeyDefined
        {
            get { return !string.IsNullOrEmpty(ResourceKey); }
        }

        /// <summary>
        /// Is Caption Defined
        /// </summary>
        public bool IsCaptionDefined
        {
            get { return m_caption != null && m_caption.Length > 0; }
        }

        /// <summary>
        /// Is Client property Defined
        /// </summary>
        public bool IsClientPropertyDefined
        {
            get { return m_name != null && m_name.Length != 0 && m_parameterType != EntityPropertyType.NA; }
        }

		/// <summary>
		/// Is Name Defined
		/// </summary>
        public bool IsNameDefined
		{
			get { return m_name != null && m_name.Length > 0; }
		}

        /// <summary>
        /// Is Source Column Defined
        /// </summary>
        public bool IsColumnDefined
        {
            get { return m_column != null && m_column.Length > 0; }
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

  		#endregion

        public object IsNull(object fieldValue)
        {
            if (Types.IsDbNull(fieldValue))
                return AsNull;
            return fieldValue;
        }
        public static string[] GetEntityKey(EntityPropertyAttribute[] attributes)
        {
            var props = from p in attributes
            where p.IsKeyOrIdentity
            select p.Column;

            return props.ToArray();
          
        }



	}

}

