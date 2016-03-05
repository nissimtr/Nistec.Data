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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using Nistec.Data.Factory;
using System.Configuration;
using Nistec.Generic;
using Nistec.Serialization;
using Nistec.Runtime;
using Nistec.Data.Entities.Cache;

namespace Nistec.Data.Entities
{

    #region IDbContext

    /// <summary>
    /// DbContext interface
    /// </summary>
    public interface IDbContext
    {

        string ConnectionName { get; }
        string ConnectionString { get; }
        DBProvider Provider { get; }
        bool HasConnection { get; }
        ILocalizer Localization { get; }
        
        IDbCmd DbCmd();
        IDbConnection Connection();

        #region Query

 
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        T QueryEntity<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem;
      
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        IList<T> QueryEntityList<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem;

        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        int EntityInsert<T>(string mappingName, T entity) where T : IEntityItem;

        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="originalEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        int EntityUpdate<T>(string mappingName, T originalEntity, T newEntity) where T : IEntityItem;
       
        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        int EntityDelete<T>(string mappingName, T entity) where T : IEntityItem;
        

        /// <summary>
        /// Executes StoredProcedure as NonQuery Command and returns effective records..
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string commandText, params object[] nameValueParameters);

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        T ExecuteScalar<T>(string commandText, T returnIfNull, params object[] nameValueParameters);
        

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        T ExecuteSingle<T>(string commandText, params object[] nameValueParameters);
      

        /// <summary>
        /// Executes StoredProcedure and returns List of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IList<T> ExecuteQuery<T>(string commandText, params object[] nameValueParameters);
       

        /// <summary>
        /// Executes Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        T QueryScalar<T>(string commandText, T returnIfNull, params object[] nameValueParameters);
        
        /// <summary>
        /// Executes Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        T QuerySingle<T>(string commandText, params object[] nameValueParameters);

        /// <summary>
        /// Executes Command and returns List of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IList<T> Query<T>(string commandText, params object[] nameValueParameters);

                /// <summary>
        /// Executes sql as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        int ExecuteCommand(string commandText, params object[] nameValueParameters);
       
       /// <summary>
       /// Execute Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class or scalar.
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="commandText"></param>
       /// <param name="parameters"></param>
       /// <param name="commandType"></param>
       /// <param name="commandTimeout"></param>
       /// <returns></returns>
        T ExecuteCommand<T>(string commandText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeout = 0);

        /// <summary>
        /// Execute as NonQuery Command  and returns effective records.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string commandText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeout = 0);
      
        #endregion

    }

    #endregion

    /// <summary>
    /// Represent DbEntities that implement <see cref="IDbContext"/>
    /// </summary>
    [Serializable]
    public class DbContext : IDbContext
    {
        /// <summary>
        /// Occured befor DbContext initilaized
        /// </summary>
        protected virtual void EntityBind() { }
        
        #region static IDbContext


        /// <summary>
        /// Get an instance of IDbContext using connection name.
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <returns></returns>
        public static IDbContext Get(string connectionName)
        {
            IDbContext db = null;
            if (!Hash.TryGetValue(connectionName, out db))
            {
                db = new DbContext(connectionName);
                Hash[connectionName] = db;
            }
            return db;
        }

        /// <summary>
        /// Create an instance of IDbContext
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <returns></returns>
        public static IDbContext Create<Dbc>() where Dbc : IDbContext
        {
            return ActivatorUtil.CreateInstance<Dbc>();//Activator.CreateInstance<Dbc>();
        }

        /// <summary>
        /// Get an instance of (Dbc) IDbContext 
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <returns></returns>
        public static IDbContext Get<Dbc>() where Dbc : IDbContext
        {
            IDbContext db = null;
            string name = typeof(Dbc).Name;
            if (!Hash.TryGetValue(name, out db))
            {
                db = Create<Dbc>();
                Hash[name] = db;
            }
            return db;
        }

        static Dictionary<string, IDbContext> m_hash;

        private static Dictionary<string, IDbContext> Hash
        {
            get
            {
                if (m_hash == null)
                {
                    m_hash = new Dictionary<string, IDbContext>();
                }
                return m_hash;
            }
        }
        #endregion

        #region EntityLang

        ILocalizer m_EntityLang;

        /// <summary>
        /// Get <see cref="ILocalizer"/> from <see cref="DbContext"/> which usful for multi language,
        /// if  EntityDbContext not define or DbContext not define return null
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public virtual ILocalizer Localization
        {
            get { return m_EntityLang; }
        }

        internal void SetLocalizer(string cultuer, string resource, Type type)
        {
            
            m_EntityLang = new DynamicEntityLocalizer(cultuer, resource, type);
        }

        internal void SetLocalizer(IEntity instance, string resource)
        {
            if (instance != null)
            {
                m_EntityLang = new DynamicEntityLocalizer(instance, resource);
            }
        }

       

        /// <summary>
        /// Get <see cref="ILocalizer"/> from <see cref="DbContext"/> which usful for multi language,
        /// if  EntityDbContext not define or DbContext not define return null
        /// </summary>
        /// <typeparam name="Erm"></typeparam>
        /// <returns></returns>
        protected virtual ILocalizer GetLocalizer<Erm>() where Erm : ILocalizer
        {
            return EntityLocalizer.Get<Erm>();
        }

        #endregion

        #region properties

        /// <summary>
        /// Get indicate if DbEntites IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(ConnectionString) /*|| Items.Count == 0*/; }
        }

        /// <summary>
        /// Get or Set ConnectionName
        /// </summary>
        public string ConnectionName
        {
            get;
            set;
        }

        /// <summary>
        /// Get or Set ConnectionString
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Get or Set Database
        /// </summary>
        public string Database
        {
            get;
            set;
        }

        DBProvider m_Provider = DBProvider.SqlServer;
        /// <summary>
        /// Get or Set DBProvider
        /// </summary>
        public DBProvider Provider
        {
            get { return m_Provider; }
            set { m_Provider = value; }
        }
        #endregion

        #region ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public DbContext()
        {
            DbContextAttribute.BuildDbContext(this);

            EntityBind();
        }


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionKey"></param>
        public DbContext(string connectionKey)
        {
            SetConnection(connectionKey);
            EntityBind();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbContext(string connectionName, string connectionString, DBProvider provider)
        {
            SetConnectionInternal(connectionName, connectionString, provider,true);
        }

        protected void SetConnection(string connectionKey)
        {
            ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
            if (cnn == null)
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            DBProvider dbProvider = DbFactory.GetProvider(cnn.ProviderName);
            SetConnectionInternal(cnn.Name, cnn.ConnectionString, dbProvider, false);
        }

        protected void SetConnection(string connectionName, string connectionString, DBProvider provider)
        {
            ConnectionName = connectionName;
            Provider = provider;
            ConnectionString = connectionString;
  
        }

        internal void SetConnectionInternal(string connectionKey, string connectionString, DBProvider provider, bool enableBinding)
        {
 
            if (string.IsNullOrEmpty(connectionString))
            {
                ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
                if (cnn == null)
                {
                    throw new Exception("ConnectionStringSettings configuration not found");
                }
                Provider = DbFactory.GetProvider(cnn.ProviderName);
                ConnectionString = cnn.ConnectionString;
                ConnectionName = connectionKey;
            }
            else
            {
                ConnectionName = connectionKey;
                Provider = provider;
                ConnectionString = connectionString;
            }

            if (enableBinding)
            {
                EntityBind();
            }
        }


        #endregion

        #region methods

        /// <summary>
        /// Get indicate if entity has connection properties
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool HasConnection
        {
            get
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Validate if entity has connection properties
        /// </summary>
        /// <exception cref="EntityException"></exception>
        public void ValidateConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new EntityException("Invalid ConnectionName");
            }
        }

        /// <summary>
        /// Create DbCommand object
        /// </summary>
        /// <returns></returns>
        public IDbCmd DbCmd()
        {
            ValidateConnection();
            return DbFactory.Create(ConnectionString, Provider);

        }
        /// <summary>
        /// Get Connection
        /// </summary>
        /// <returns></returns>
        public IDbConnection Connection()
        {
            return DbCmd().Connection;
        }
        #endregion

        #region EntityDbCache

        EntityDbCache m_Entites;// = new Dictionary<string, EntityDbContext>();

        /// <summary>
        /// Get EntityDbContext Items
        /// </summary>
        public EntityDbCache DbEntities
        {
            get
            {
                if (m_Entites == null)
                {
                    m_Entites = new Nistec.Data.Entities.Cache.EntityDbCache(this);
                }
                return m_Entites;
            }
            //set { m_db = value; }
        }

        #endregion

        #region internal
        internal EntityDbContext GetEntityDb(object instance, string mappingName)
        {
            EntityKeys keys = EntityPropertyBuilder.GetEntityPrimaryKey(instance);
            if (keys == null || keys.Count == 0)
                throw new EntityException("Invalid entity key which is required");
            EntityDbContext db = new EntityDbContext(this, mappingName, keys);
            db.EntityCulture = EntityLocalizer.DefaultCulture;
            return db;
        }

        #endregion

        #region Query


        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public T QueryEntity<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem
        {
            ValidateConnection();
            if (mappingName == null)
            {
                throw new ArgumentNullException("QueryEntity.mappingName");
            }
            if (keyValueParameters == null)
            {
                throw new CustomAttributeFormatException("QueryEntity.key");
            }
            string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return QuerySingle<T>(commandText, keyValueParameters);
        }
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public IList<T> QueryEntityList<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem
        {
            ValidateConnection();
            if (mappingName == null)
            {
                throw new ArgumentNullException("QueryEntityList.mappingName");
            }

            string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return Query<T>(commandText, keyValueParameters);
        }

        /// <summary>
        /// Get Entity list using <see cref="EntityContext"/> with mapping name and <see cref="DataFilter"/> filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<T> EntityGetList<T>(string mappingName, DataFilter filter) where T : IEntityItem
        {
            ValidateConnection();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGetList.mappingName");
            }
            T instance = ActivatorUtil.CreateInstance<T>();//Activator.CreateInstance<T>();
            EntityDbContext edb = GetEntityDb(instance, mappingName);
            if (filter == null)
                return edb.EntityList<T>();
            return edb.EntityList<T>(filter);
        }


        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int EntityInsert<T>(string mappingName, T entity) where T : IEntityItem
        {
            ValidateConnection();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityInsert.mappingName");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("EntityInsert.entity");
            }
            T instance = ActivatorUtil.CreateInstance<T>();
            EntityDbContext edb = GetEntityDb(instance, mappingName);
            using (EntityContext<T> context = new EntityContext<T>(edb, instance))
            {
                context.Set(entity);
                return context.SaveChanges(UpdateCommandType.Insert);
            }
        }
        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="originalEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        public int EntityUpdate<T>(string mappingName, T originalEntity, T newEntity) where T : IEntityItem
        {
            ValidateConnection();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityUpdate.mappingName");
            }
            if (originalEntity == null)
            {
                throw new ArgumentNullException("EntityUpdate.originalEntity");
            }
            if (newEntity == null)
            {
                throw new ArgumentNullException("EntityUpdate.newEntity");
            }

            EntityDbContext edb = GetEntityDb(originalEntity, mappingName);
            using (EntityContext<T> context = new EntityContext<T>(edb, originalEntity))
            {
                context.Set(newEntity);//, false);
                return context.SaveChanges(UpdateCommandType.Update);
            }
        }
        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int EntityDelete<T>(string mappingName, T entity) where T : IEntityItem
        {
            ValidateConnection();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityDelete.mappingName");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("DeleteEntity.entity");
            }
            EntityDbContext edb = GetEntityDb(entity, mappingName);
            return EntityCommandBuilder.DeleteCommand(entity, edb);
        }

        /// <summary>
        /// Executes StoredProcedure as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteNonQuery(commandText, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
        }
        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string commandText, T returnIfNull, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteScalar.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteScalar<T>(commandText, DataParameter.GetSql(nameValueParameters), returnIfNull, CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T ExecuteSingle<T>(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteSingle.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteCommand<T>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
        }


        /// <summary>
        /// Executes StoredProcedure and returns List of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<T> ExecuteQuery<T>(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteQuery.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteCommand<T, IList<T>>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
        }
        /// <summary>
        /// Executes sql as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int ExecuteCommand(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteNonQuery(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
        }
        /// <summary>
        /// Execute Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class or scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public T ExecuteCommand<T>(string commandText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeout = 0)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteCommand<T>(commandText, parameters, commandType, commandTimeout);
            }
        }
        /// <summary>
        /// Execute as NonQuery Command  and returns effective records.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeout = 0)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteNonQuery.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteNonQuery(commandText, parameters, commandType, commandTimeout);
            }
        }
        /// <summary>
        /// Executes Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T QueryScalar<T>(string commandText, T returnIfNull, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QueryScalar.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteScalar<T>(commandText, DataParameter.GetSql(nameValueParameters), returnIfNull, CommandType.Text);
            }
        }

        /// <summary>
        /// Executes Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T QuerySingle<T>(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QuerySingle.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteCommand<T>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
        }


        /// <summary>
        /// Executes Command and returns List of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<T> Query<T>(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("Query.commandText");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteCommand<T, IList<T>>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
        }

        /// <summary>
        /// Executes Command and returns Dictionary.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<Dictionary<string, object>> QueryDictionary(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("ExecuteDictionary.mappingName");
            }
            string commandText = SqlFormatter.GetCommandText(mappingName, nameValueParameters);

            DataTable dt = null;
            using (IDbCmd cmd = DbCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
            if (dt == null)
                return null;
            return dt.ToListDictionary();// DataUtil.DatatableToDictionary(dt, Pk);
        }

        /// <summary>
        /// Executes StoredProcedure and returns Dictionary.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<Dictionary<string, object>> ExecuteDictionary(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("ExecuteDictionary.mappingName");
            }
            DataTable dt = null;
            using (IDbCmd cmd = DbCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>(mappingName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
            if (dt == null)
                return null;
            return dt.ToListDictionary();//DataUtil.DatatableToDictionary(dt, Pk);
        }


        /// <summary>
        /// Executes Command and returns Json.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public string QueryJson(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QueryJson.commandText");
            }
            string cmdText = (commandText.TrimStart().ToLower().StartsWith("select")) ? commandText :
                SqlFormatter.GetCommandText(commandText, nameValueParameters);

            DataTable dt = null;
            using (IDbCmd cmd = DbCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>(cmdText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
            if (dt == null)
                return null;
            return JsonSerializer.Serialize(dt);
            //return dt.ToJson();
        }
        /// <summary>
        /// Executes StoredProcedure and returns Json.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public string ExecuteJson(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("ExecuteJson.mappingName");
            }
            DataTable dt = null;
            using (IDbCmd cmd = DbCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>(mappingName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
            if (dt == null)
                return null;
            //return dt.ToJson();
            return JsonSerializer.Serialize(dt);
        }



        /// <summary>
        /// Executes Command and returns Json.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public string QueryJsonRecord(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QueryJsonRecord.commandText");
            }
            string cmdText = (commandText.TrimStart().ToLower().StartsWith("select")) ? commandText :
                SqlFormatter.GetCommandText(commandText, nameValueParameters);

            DataTable dt = null;
            using (IDbCmd cmd = DbCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>(cmdText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
            if (dt == null || dt.Rows.Count == 0)
                return null;
            return JsonSerializer.Serialize(dt.Rows[0]);
            //return dt.ToJson();
        }

        /// <summary>
        /// Executes StoredProcedure and returns Json.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public string ExecuteJsonRecord(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("ExecuteJsonRecord.mappingName");
            }
            DataTable dt = null;
            using (IDbCmd cmd = DbCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>(mappingName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
            if (dt == null || dt.Rows.Count == 0)
                return null;
            //return dt.ToJson();
            return JsonSerializer.Serialize(dt.Rows[0]);
        }

        /// <summary>
        /// Executes Command and returns Dictionary.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IDictionary<string, object> QueryDictionaryRecord(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("QueryDictionaryRecord.mappingName");
            }
            string commandText = SqlFormatter.GetCommandText(mappingName, nameValueParameters);

            DataRow dr = null;
            using (IDbCmd cmd = DbCmd())
            {
                dr = cmd.ExecuteCommand<DataRow>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
            if (dr == null)
                return null;
            return dr.ToDictionary();
        }

        /// <summary>
        /// Executes StoredProcedure and returns Dictionary.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IDictionary<string, object> ExecuteDictionaryRecord(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("ExecuteDictionaryRecord.mappingName");
            }
            DataRow dr = null;
            using (IDbCmd cmd = DbCmd())
            {
                dr = cmd.ExecuteCommand<DataRow>(mappingName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
            if (dr == null)
                return null;
            return dr.ToDictionary();
        }

        /// <summary>
        /// Executes Command and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public DataTable QueryDataTable(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("ExecuteDataTable.mappingName");
            }
            string commandText = SqlFormatter.GetCommandText(mappingName, nameValueParameters);

            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
            }
        }

        /// <summary>
        /// Executes StoredProcedure and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string mappingName, params object[] nameValueParameters)
        {
            if (mappingName == null)
            {
                throw new ArgumentNullException("ExecuteDataTable.mappingName");
            }
            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteCommand<DataTable>(mappingName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Executes Command and returns IDataReader.
        /// </summary>
        /// <param name="commandText"></param>
        /// /// <param name="behavior"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IDataReader QueryReader(string commandText, CommandBehavior behavior, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QueryReader.commandText");
            }

            using (IDbCmd cmd = DbCmd())
            {
                return cmd.ExecuteReader(commandText, behavior, DataParameter.GetSql(nameValueParameters));
            }
        }

        #endregion

    }
}
