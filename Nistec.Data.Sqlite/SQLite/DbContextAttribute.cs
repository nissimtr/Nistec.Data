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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nistec.Generic;

using Debug = System.Diagnostics.Debug;

namespace Nistec.Data.Sqlite
{
     
 
	/// <summary>
    /// This attribute defines properties of DbContext Attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Class) ]
    public class DbContextAttribute : Attribute
    {
      

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAttribute"/> class
		/// </summary>
		public DbContextAttribute() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAttribute"/> class
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbContextAttribute(string connectionName, string connectionString, DBProvider provider)
        {
            m_ConnectionString = connectionString;
            m_Provider = provider;
            m_ConnectionKey = connectionName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAttribute"/> class
        /// </summary>
        /// <param name="connectionKey"></param>
        public DbContextAttribute(string connectionKey)
        {
            m_ConnectionKey = connectionKey;
            //m_LangResources = resource;
        }

		#endregion

		#region Properties

        private DBProvider m_Provider = DBProvider.SqlServer;
        private string m_ConnectionKey = "";
        private string m_ConnectionString = "";
 
     
		/// <summary>
        /// Property ConnectionKey. 
		/// </summary>
        public string ConnectionKey
		{
            get { return m_ConnectionKey; }
            set { m_ConnectionKey = value; }
		}

        /// <summary>
        /// Parameter MappingName represent the DB entity (Table|View).
        /// </summary>
        public string ConnectionString
        {
            get { return m_ConnectionString; }
            set { m_ConnectionString = value; }
        }

        /// <summary>
        /// Parameter Provider represent the DB provider.
        /// </summary>
        public DBProvider Provider
        {
            get { return m_Provider; }
            set { m_Provider = value; }
        }

 
		/// <summary>
        /// Is ConnectionString Defined
		/// </summary>
        public bool IsConnectionStringDefined
		{
            get { return !string.IsNullOrEmpty(m_ConnectionString); }
		}
        /// <summary>
        /// Is ConnectionKey Defined
        /// </summary>
        public bool IsConnectionKeyDefined
        {
            get { return !string.IsNullOrEmpty(m_ConnectionKey); }
        }
        /// <summary>
        /// Is Connection Defined
        /// </summary>
        public bool IsConnectionDefined
        {
            get { return IsConnectionKeyDefined || IsConnectionStringDefined; }
        }

    
        /// <summary>
        /// Is Property has valid Definition
        /// </summary>
        public bool IsValid
        {
            get { return IsConnectionKeyDefined || IsConnectionStringDefined; }
        }

		#endregion

        public DbContext Create()
        {
            DbContext db =null;
            if (IsConnectionStringDefined)
                db = new DbContext(ConnectionKey, ConnectionString, Provider);
            else if (IsConnectionKeyDefined)
                db = new DbContext(ConnectionKey);
            return db;
        }
        public static DbContextAttribute Get<T>()
        {
            DbContextAttribute[] attributes = typeof(T).GetCustomAttributes<DbContextAttribute>();
            if (attributes == null || attributes.Length == 0)
                return null;
            return attributes[0];
        }
        public static DbContext Create<T>()
        {
            DbContextAttribute attrib = Get<T>();
            if (attrib == null)
                return null;
            return new DbContext(attrib.ConnectionKey);
        }
        public static void BuildDbContext(DbContext instance)
        {
            DbContextAttribute[] attributes = instance.GetType().GetCustomAttributes<DbContextAttribute>();
            if (attributes == null || attributes.Length==0)
                return;
            foreach (var attribute in attributes)
            {
                //DbContext db = attribute.Create();
                instance.SetConnectionInternal(attribute.ConnectionKey, attribute.ConnectionString, attribute.Provider, false);
            }
        }

	}


}

