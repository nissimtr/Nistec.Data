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
using Nistec.Generic;

using Debug = System.Diagnostics.Debug;

namespace Nistec.Data.Entities
{
    #region EntityMode

    public enum EntityMode
    {
        NA,
        Generic,
        Reflection,
        Multi,
        Config
    }

    public enum EntitySourceType
    {
        Table,
        View,
        Procedure
    }

    #endregion

    /// <summary>
    /// This attribute defines properties of <see cref="IEntity"/> field Attribute
	/// </summary>
	[ AttributeUsage(AttributeTargets.Class) ]
	public class EntityAttribute : Attribute
	{
        
		#region Private members
        private EntityMode m_EntityMode = EntityMode.NA;
        private EntitySourceType m_EntitySourceType = EntitySourceType.Table;
        private string m_Connection = "";
        private string m_MappingName = "";
        private string m_EntityName = "";
        private string[] m_Key;
        private string m_LangResources;
        #endregion

		#region Constructors

    
		/// <summary>
        /// Initializes a new instance of the <see cref="EntityAttribute"/> class
		/// </summary>
		public EntityAttribute() 
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAttribute"/> class from <see cref="EntityDbContext"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="langResources"></param>
        public EntityAttribute(EntityDbContext db, string langResources=null)
        {
            if (db != null)
            {
                ConnectionKey = db.ConnectionKey;
                EntityKey = db.EntityKeys.ToArray();
                EntityName = db.EntityName;
                EntitySourceType = db.SourceType;
                LangResources = langResources;
                MappingName = db.MappingName;
                Mode = EntityMode.Generic;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAttribute"/> class
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="connectionKey"></param>
        /// <param name="mode"></param>
        /// <param name="sourceType"></param>
        /// <param name="key">Array as comma separated string of EntityKeys</param>
        /// <param name="langResource"></param>
        public EntityAttribute(string entityName, string mappingName, string connectionKey, EntityMode mode, EntitySourceType sourceType, string key, string langResource)//, CommandType  cmdType)
        {
            m_EntityName = entityName;
            m_MappingName = mappingName;
            m_Connection = connectionKey;
            m_EntityMode = mode;
            m_EntitySourceType = sourceType;
            if (key != null)
            {
                m_Key = key.SplitTrim(',', ';'); 
            }
            m_LangResources = langResource;
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="EntityAttribute"/> class
		/// </summary>
         /// <param name="entityName"></param>
         /// <param name="mappingName"></param>
        /// <param name="connectionKey"></param>
        /// <param name="mode"></param>
        /// <param name="key">Array as comma separated string of EntityKeys </param>
         public EntityAttribute(string entityName, string mappingName, string connectionKey, EntityMode mode, string key)
		{
            m_EntityName = entityName;
            m_MappingName = mappingName;
            m_Connection = connectionKey;
            m_EntityMode = mode;
            if (key != null)
            {
                m_Key = key.SplitTrim(',', ';'); ;
            }
		}

        /// <summary>
        /// Initializes a new instance of entity property
        /// </summary>
         /// <param name="entityName"></param>
         /// <param name="mappingName"></param>
         public EntityAttribute(string entityName, string mappingName)
        {
            m_EntityName = entityName;
            m_MappingName = mappingName;
            m_Connection = "";
            m_EntityMode = EntityMode.NA;
        }

       /// <summary>
       /// Initializes a new instance of entity property
       /// </summary>
       /// <param name="mappingName"></param>
       public EntityAttribute(string mappingName)
       {
           m_EntityName = mappingName;
           m_MappingName = mappingName;
           m_Connection = "";
           m_EntityMode = EntityMode.NA;
       }

       /// <summary>
       /// Initializes a new instance of entity property;
       /// </summary>
       /// <param name="entityName"></param>
       /// <param name="mode"></param>
       public EntityAttribute(string entityName, EntityMode mode)
       {
           m_EntityName = entityName;
           m_EntityMode = mode;
           if (mode == EntityMode.Config)
           {
               var settings = Nistec.Data.Entities.Config.EntityConfig.GetConfig();
               if (settings == null)
               {
                   throw new CustomAttributeFormatException("EntityConfig not found!");
               }
               var attr = settings.EntitySettings[entityName];
               if (attr == null)
               {
                   throw new CustomAttributeFormatException("EntitySettings.entityName not found: "  + entityName);
               }
               m_MappingName = attr.MappingName;
               m_EntityName = attr.EntityName;
               m_Connection = attr.ConnectionKey;
               m_EntitySourceType = GenericTypes.ConvertEnum<EntitySourceType>(attr.SourceType, EntitySourceType.Table); 

               m_Key = EntityKeys.Get(attr.EntityKey).ToArray();

           }
       }
        /// <summary>
       /// Initializes a new instance of entity property;
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="EntityConfigKey"></param>
        /// <param name="sourceType"></param>
        /// <param name="key"></param>
       public EntityAttribute(string entityName, string mappingName, string EntityConfigKey, EntitySourceType sourceType, string key)
       {
           m_EntityName = entityName;
           m_EntityMode = EntityMode.Config;
           var settings = Nistec.Data.Entities.Config.EntityConfig.GetConfig();
           if (settings == null)
           {
               throw new CustomAttributeFormatException("EntityConfig not found!");
           }
           var attr = settings.EntitySettings[EntityConfigKey];
           if (attr == null)
           {
               throw new CustomAttributeFormatException("EntitySettings.entityName not found: " + EntityConfigKey);
           }
           m_MappingName = mappingName;
           m_EntityName = entityName;
           m_Connection = attr.ConnectionKey;
           m_EntitySourceType = sourceType;
           //m_EntitySourceType = GenericTypes.ConvertEnum<EntitySourceType>(attr.SourceType, EntitySourceType.Table);
           if (key != null)
           {
               m_Key = key.SplitTrim(',', ';');
           }

       }

		/// <summary>
		/// An attribute builder method
		/// </summary>
		/// <param name="attr"></param>
		/// <returns></returns>
        public static CustomAttributeBuilder GetAttributeBuilder(EntityAttribute attr)
		{
            Type[] arrParamTypes = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(EntityMode), typeof(EntitySourceType), typeof(string[]), typeof(string) };
            object[] arrParamValues = new object[] { attr.EntityName, attr.MappingName, attr.ConnectionKey, attr.Mode, attr.EntitySourceType, attr.EntityKey, attr.LangResources };
            ConstructorInfo ctor = typeof(EntityAttribute).GetConstructor(arrParamTypes);
			return new CustomAttributeBuilder(ctor, arrParamValues);
		}
		#endregion

		#region Properties

        /// <summary>
        /// Property EntityKey. 
        /// </summary>
        /// <example>{"ID","Category"}</example>
        public string[] EntityKey
        {
            get { return m_Key; }
            set { m_Key = value; }
        }
      
		/// <summary>
        /// Property EntityName. If this property is not defined 
        /// then a method property MappingName is used.
		/// </summary>
        public string EntityName
		{
            get { return m_EntityName == null ? string.Empty : m_EntityName; }
            set { m_EntityName = value; }
		}

        /// <summary>
        /// Parameter MappingName represent the DB entity (Table|View).
        /// </summary>
        public string MappingName
        {
            get { return m_MappingName == null ? string.Empty : m_MappingName; }
            set { m_MappingName = value; }
        }


        /// <summary>
        /// Parameter Connection. usefull for connecetion to DB
        /// </summary>
        public string ConnectionKey
        {
            get { return m_Connection; }
            set { m_Connection = value; }
        }

        /// <summary>
        /// Parameter LangResources. usefull for multi lang
        /// </summary>
        public string LangResources
        {
            get { return m_LangResources; }
            set { m_LangResources = value; }
        }


        /// <summary>
        /// Parameter EntityMode represent the entity mode.
        /// </summary>
        public EntityMode Mode
        {
            get { return m_EntityMode; }
            set { m_EntityMode = value; }
        }

        /// <summary>
        /// Parameter EntitySourceType represent the entity source type.
        /// </summary>
        public EntitySourceType EntitySourceType
        {
            get { return m_EntitySourceType; }
            set { m_EntitySourceType = value; }
        }

        /// <summary>
        /// Is ResourceManager Defined
        /// </summary>
        public bool IsLangResourcesDefined
        {
            get { return !string.IsNullOrEmpty(m_LangResources); }
        }
		/// <summary>
        /// Is EntityName Defined
		/// </summary>
        public bool IsEntityNameDefined
		{
            get { return !string.IsNullOrEmpty(m_EntityName); }
		}
        /// <summary>
        /// Is MappingName Defined
        /// </summary>
        public bool IsMappingNameDefined
        {
            get { return !string.IsNullOrEmpty(m_MappingName);}
        }
        /// <summary>
        /// Is Connection Defined
        /// </summary>
        public bool IsConnectionDefined
        {
            get { return !string.IsNullOrEmpty(m_Connection);}
        }

        /// <summary>
        /// Is EntityMode Defined
        /// </summary>
        public bool IsModeDefined
        {
            get { return m_EntityMode != EntityMode.NA; }
        }

        /// <summary>
        /// Is Property has valid Definition
        /// </summary>
        public bool IsValid
        {
            get { return IsMappingNameDefined; }
        }

		#endregion

        internal static EntityAttribute EnsureSettings(EntityAttribute attribute)
        {
            if (attribute.Mode == EntityMode.Config)
            {
                var settings = Nistec.Data.Entities.Config.EntityConfig.GetConfig();
                if (settings == null)
                {
                    return attribute;
                }
                var attr = settings.EntitySettings[attribute.EntityName];
                if (attr == null)
                {
                    return attribute;
                }
                attribute.MappingName = attr.MappingName;
                attribute.EntityName = attr.EntityName;
                attribute.ConnectionKey = attr.ConnectionKey;
                attribute.EntitySourceType = GenericTypes.ConvertEnum<EntitySourceType>(attr.SourceType, EntitySourceType.Table); 

                attribute.EntityKey = EntityKeys.Get(attr.EntityKey).ToArray();

            }
            return attribute;

        }

        public static EntityAttribute FromConfig(string entityName)
        {
            return new EntityAttribute(entityName,  EntityMode.Config);
        }

        internal EntityDbContext Get()
        {
            return new EntityDbContext(this);
        }

	}


}

