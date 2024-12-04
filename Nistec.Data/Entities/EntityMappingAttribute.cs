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
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{


    /// <summary>
    /// This attribute defines properties of DbContext Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityMappingAttribute : Attribute
    {
        public const int DefaultCacheTtlMinutes = 3;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMappingAttribute"/> class
        /// </summary>
        public EntityMappingAttribute()
        {
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="EntityMappingAttribute"/> class
        ///// </summary>
        ///// <param name="mappingName"></param>
        ///// <param name="viewName"></param>
        ///// <param name="connectionName"></param>
        //public EntityMappingAttribute(string mappingName, string viewName, string connectionName)
        //{
        //    m_MappingName = mappingName;
        //    m_ViewName = viewName;
        //    m_ConnectionKey = connectionName;
        //}
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMappingAttribute"/> class
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="viewName"></param>
        /// <param name="connectionName"></param>
        /// <param name="entityName"></param>
        /// <param name="lang"></param>
        public EntityMappingAttribute(string mappingName, string viewName, string connectionName, string entityName, string lang)
        {
            m_MappingName = mappingName;
            m_ViewName = viewName;
            m_ConnectionKey = connectionName;
            m_EntityName = entityName;
            Lang = lang;
            EnableCache = false;
            CacheTtl = DefaultCacheTtlMinutes;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMappingAttribute"/> class with default cache ttl
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="viewName"></param>
        /// <param name="entityName"></param>
        /// <param name="cacheTtl"></param>
        public EntityMappingAttribute(string mappingName, string viewName, string entityName, int cacheTtl = DefaultCacheTtlMinutes)
        {
            m_MappingName = mappingName;
            m_ViewName = viewName;
            m_EntityName = entityName;
            //Lang = lang;
            EnableCache = cacheTtl > 0;
            CacheTtl = cacheTtl;
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="EntityMappingAttribute"/> class
        ///// </summary>
        ///// <param name="mappingName"></param>
        ///// <param name="viewName"></param>
        //public EntityMappingAttribute(string mappingName, string viewName)
        //{
        //    m_MappingName = mappingName;
        //    m_ViewName = viewName;
        //    m_EntityName = mappingName;
        //    EnableCache = false;
        //    CacheTtl = DefaultCacheTtlMinutes;
        //}
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMappingAttribute"/> class
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="viewName"></param>
        /// <param name="cacheTtl"></param>
        public EntityMappingAttribute(string mappingName, string viewName, int cacheTtl = DefaultCacheTtlMinutes)
        {
            m_MappingName = mappingName;
            m_ViewName = viewName;
            m_EntityName = mappingName;
            EnableCache = cacheTtl > 0;
            CacheTtl = cacheTtl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMappingAttribute"/> class
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="cacheTtl"></param>
        public EntityMappingAttribute(string mappingName, int cacheTtl = DefaultCacheTtlMinutes)
        {
            m_MappingName = mappingName;
            m_EntityName = mappingName;
            EnableCache = cacheTtl > 0;
            CacheTtl = cacheTtl;
        }

        #endregion

        #region Properties

        private string m_ConnectionKey;
        private string m_MappingName;
        private string m_ViewName;
        private string m_EntityName;
        
        public int CacheTtl
        {
            get;
            set;
        }
        public bool EnableCache
        {
            get;
            set;
        }
        public DateTime CacheTimeout
        {
            get { return DateTime.Now.AddMinutes(CacheTtl); }
        }
       

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
        public string MappingName
        {
            get { return m_MappingName; }
            set { m_MappingName = value; }
        }
        /// <summary>
        /// Parameter ViewName represent the DB entity (View).
        /// </summary>
        public string ViewName
        {
            get 
            {
                return m_ViewName ?? m_MappingName; 
            }
            set { m_ViewName = value; }
        }

        /// <summary>
        /// Get or Set EntityName
        /// </summary>
        public string EntityName
        {
            get { return m_EntityName ?? m_MappingName; }
            set { m_EntityName = value; }
        }
        /// <summary>
        /// Get or Set EntityName
        /// </summary>
        public string Lang
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set Entity Primary Fields
        /// </summary>
        public string PrimaryFields
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set get procedure
        /// </summary>
        public string ProcGet
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set delete procedure
        /// </summary>
        public string ProcDelete
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set update procedure
        /// </summary>
        public string ProcUpdate
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set insert procedure
        /// </summary>
        public string ProcInsert
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set insert or update procedure
        /// </summary>
        public string ProcUpsert
        {
            get;
            set;
        }
        /// <summary>
        /// Get or Set list view procedure
        /// </summary>
        public string ProcListView
        {
            get;
            set;
        }
        /// <summary>
        /// Is MappingName Defined
        /// </summary>
        public bool IsViewNameDefined
        {
            get { return !string.IsNullOrEmpty(m_ViewName); }
        }

        /// <summary>
        /// Is MappingName Defined
        /// </summary>
        public bool IsMappingNameDefined
        {
            get { return !string.IsNullOrEmpty(m_MappingName); }
        }
        /// <summary>
        /// Is ConnectionKey Defined
        /// </summary>
        public bool IsConnectionKeyDefined
        {
            get { return !string.IsNullOrEmpty(m_ConnectionKey); }
        }


        /// <summary>
        /// Is Property has valid Definition
        /// </summary>
        public bool IsValid
        {
            get { return IsMappingNameDefined; }
        }

        #endregion

        public DbContext Create()
        {
            DbContext db = null;
            if (IsConnectionKeyDefined)
                db = new DbContext(ConnectionKey);
            return db;
        }
        public static EntityMappingAttribute Get<T>()
        {
            IEnumerable<EntityMappingAttribute> attributes = typeof(T).GetCustomAttributes<EntityMappingAttribute>();
            if (attributes == null || attributes.Count() == 0)
                return null;
            return attributes.FirstOrDefault();//[0];
            //foreach (var attribute in attributes)
            //{
            //    return attribute;
            //}
            //return null;
        }

        public static string[] Primary<T>(bool useEntityKeysBuilder=false) where T:IEntityItem
        {
            EntityMappingAttribute attribute = Get<T>();
            if (attribute == null)
            {
                return useEntityKeysBuilder ? EntityPropertyBuilder.GetEntityPrimaryFields<T>().ToArray() : null;
            }
            var pfields = attribute.PrimaryFields;

            return string.IsNullOrWhiteSpace(pfields) ? (useEntityKeysBuilder ? EntityPropertyBuilder.GetEntityPrimaryFields<T>().ToArray() : null) : pfields.SplitTrim(',');
        }

        //public static string[] Primary<T>()
        //{
        //    EntityMappingAttribute attribute = Get<T>();
        //    if (attribute == null)
        //    {
        //        EntityKeys.BuildKeys<T>().ToArray();
        //    }
        //    return attribute.PrimaryFields.SplitTrim(',');
        //}

        public static string Name<T>()
        {
            EntityMappingAttribute attribute = Get<T>();
            if (attribute == null)
                return null;
            return attribute.EntityName;
        }
        public static string Mapping<T>()
        {
            EntityMappingAttribute attribute = Get<T>();
            if (attribute == null)
                       return null;
            return attribute.MappingName;
        }
        public static string View<T>()
        {
            EntityMappingAttribute attribute = Get<T>();
            if (attribute == null)
                return null;
            return attribute.ViewName;
        }
        public static string Proc<T>(ProcedureType cmdType)
        {
            EntityMappingAttribute attribute = Get<T>();
            if (attribute == null)
                return null;
            switch (cmdType)
            {
                case ProcedureType.Delete:
                    return attribute.ProcDelete;
                case ProcedureType.Insert:
                    return attribute.ProcInsert;
                case ProcedureType.Update:
                    return attribute.ProcUpdate;
                case ProcedureType.Upsert:
                    return attribute.ProcUpsert;
                case ProcedureType.GetList:
                    return attribute.ProcListView;
                case ProcedureType.GetRecord:
                    return attribute.ProcGet;
                default:
                    return null;
            }
        }

        public static EntityMappingAttribute Get(IEntityItem instance)
        {
            IEnumerable<EntityMappingAttribute> attributes = instance.GetType().GetCustomAttributes<EntityMappingAttribute>();
            if (attributes == null || attributes.Count() == 0)
                return null;
            return attributes.FirstOrDefault();//[0];
            //foreach (var attribute in attributes)
            //{
            //    return attribute;
            //}
            //return null;
        }
        //public static string GetMapping(IEntityItem instance)
        //{
        //    EntityMappingAttribute[] attributes = instance.GetType().GetCustomAttributes<EntityMappingAttribute>();
        //    if (attributes == null || attributes.Length == 0)
        //        return null;
        //    foreach (var attribute in attributes)
        //    {
        //        //DbContext db = attribute.Create();
        //        if (attribute.IsMappingNameDefined)
        //            return attribute.MappingName;
        //    }
        //    return null;
        //}


    }
}

