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
using Nistec.Data.Entities;
using System.Data.SQLite;

namespace Nistec.Data.Sqlite
{

    #region IDbContext

    /// <summary>
    /// DbContext interface
    /// </summary>
    public interface IDbContext:IDisposable
    {
        #region properties
        string ConnectionName { get; }
        string ConnectionString { get; }
        DBProvider Provider { get; }
        bool HasConnection { get; }
        ILocalizer Localization { get; }
        IDbConnection Connection { get; }
        DbCommand NewCmd();

        #endregion

        #region Query

        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        T EntityGet<T>(params object[] keyValueParameters) where T : IEntityItem;
             /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        IList<T> EntityList<T>(params object[] keyValueParameters) where T : IEntityItem;

        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        T EntityGet<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem;
      
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        IList<T> EntityList<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem;

        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int EntityInsert<T>(T entity) where T : IEntityItem;

        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        int EntityUpdate<T>( T originalEntity, T newEntity) where T : IEntityItem;
       
        /// <summary>
        /// Entity delete command by mapping name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int EntityDelete<T>(T entity) where T : IEntityItem;
        /// <summary>
        /// Entity delete command by mapping name and name value parameters.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        int EntityDelete(string mappingName, params object[] nameValueParameters);
        /// <summary>
        /// Executes StoredProcedure as NonQuery Command and returns effective records..
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string procName, params object[] nameValueParameters);

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        T ExecuteScalar<T>(string procName, T returnIfNull, params object[] nameValueParameters);
        

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        T ExecuteSingle<T>(string procName, params object[] nameValueParameters);
      

        /// <summary>
        /// Executes StoredProcedure and returns List of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IList<T> ExecuteList<T>(string procName, params object[] nameValueParameters);
       
        /// <summary>
        /// Executes Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class.
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
        #endregion

        #region command

        /// <summary>
        /// Executes sql as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        T ExecuteCommand<T>(string commandText, object[] nameValueParameters);


        /// <summary>
        /// Execute Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class or scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="addWithKey"></param>
        /// <returns></returns>
        T ExecuteCommand<T>(string commandText, IDbDataParameter[] parameters, int commandTimeout = 0, bool addWithKey = false);

        /// <summary>
        /// Execute as NonQuery Command  and returns effective records.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        int ExecuteCommandNonQuery(string commandText, IDbDataParameter[] parameters, int commandTimeout = 0);
      
        #endregion

        #region Advanced
        /// <summary>
        /// Executes CommandType.Text and returns Dictionary.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IList<Dictionary<string, object>> QueryDictionary(string commandText, params object[] nameValueParameters);
        

        /// <summary>
        /// Executes StoredProcedure and returns Dictionary.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IList<Dictionary<string, object>> ExecuteDictionary(string mappingName, params object[] nameValueParameters);
       


        /// <summary>
        /// Executes CommandType.Text and returns Json.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        string QueryJson(string commandText, params object[] nameValueParameters);
       
        /// <summary>
        /// Executes StoredProcedure and returns Json.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        string ExecuteJson(string mappingName, params object[] nameValueParameters);
       



        /// <summary>
        /// Executes CommandType.Text and returns Json.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        string QueryJsonRecord(string commandText, params object[] nameValueParameters);
       
        

        /// <summary>
        /// Executes StoredProcedure and returns Json.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        string ExecuteJsonRecord(string mappingName, params object[] nameValueParameters);
       

        /// <summary>
        /// Executes CommandType.Text and returns Dictionary.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IDictionary<string, object> QueryDictionaryRecord(string commandText, params object[] nameValueParameters);
       

        /// <summary>
        /// Executes StoredProcedure and returns Dictionary.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IDictionary<string, object> ExecuteDictionaryRecord(string mappingName, params object[] nameValueParameters);
        

        /// <summary>
        /// Executes CommandType.Text and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        DataTable QueryDataTable(string commandText, params object[] nameValueParameters);
        

        /// <summary>
        /// Executes StoredProcedure and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        DataTable ExecuteDataTable(string mappingName, params object[] nameValueParameters);
        
        /// <summary>
        /// Executes StoredProcedure and returns DataSet.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        DataSet ExecuteDataSet(string mappingName, params object[] nameValueParameters);
       
        /// <summary>
        /// Executes Command and returns IDataReader.
        /// </summary>
        /// <param name="commandText"></param>
        /// /// <param name="behavior"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        IDataReader QueryReader(string commandText, CommandBehavior behavior, params object[] nameValueParameters);
        

        #endregion

    }

    #endregion

    /// <summary>
    /// Represent DbEntities that implement <see cref="IDbContext"/>
    /// </summary>
    [Serializable]
    public class DbContext : DbContextCommand, IDbContext
    {
        /// <summary>
        /// Occured befor DbContext initilaized
        /// </summary>
        protected virtual void EntityBind() { }
        
        #region static entities

        public static T EntityGet<Dbc, T>(params object[] keyValueParameters)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = Create<Dbc>())
            {
                return db.EntityGet<T>(keyValueParameters);
            }
        }
        public static IList<T> EntityList<Dbc, T>(params object[] keyValueParameters)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = Create<Dbc>())
            {
                return db.EntityList<T>(keyValueParameters);
            }
        }

        public static IList<T> Query<Dbc, T>(string commandText, params object[] keyValueParameters)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = Create<Dbc>())
            {
                return db.Query<T>(commandText,keyValueParameters);
            }
        }
        public static T QuerySingle<Dbc, T>(string commandText, params object[] keyValueParameters)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = Create<Dbc>())
            {
                return db.QuerySingle<T>(commandText, keyValueParameters);
            }
        }

        public static T ExecuteSingle<Dbc, T>(string procName, params object[] keyValueParameters)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = Create<Dbc>())
            {
                return db.ExecuteSingle<T>(procName, keyValueParameters);
            }
        }
        public static IList<T> ExecuteList<Dbc, T>(string procName, params object[] keyValueParameters)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = Create<Dbc>())
            {
                return db.ExecuteList<T>(procName, keyValueParameters);
            }
        }
        #endregion

        #region static IDbContext


        public static void CreateFile(string dbName)
        {
            SQLiteConnection.CreateFile(dbName);//("MyDatabase.sqlite");
        }

        /// <summary>
        /// Create an instance of IDbContext
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <returns></returns>
        public static IDbContext Create<Dbc>() where Dbc : IDbContext
        {
            return DbContextAttribute.Create<Dbc>();
            //return ActivatorUtil.CreateInstance<Dbc>();
        }

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
        /*
        /// <summary>
        /// Get indicate if DbEntites IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(ConnectionString) ; }
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

        IDbCmd _Command;
        public IDbCmd Command
        {
            get
            {
                ValidateConnection();
                if(_Command==null)
                {
                    Command = DbFactory.Create(ConnectionString, Provider);
                }
                return _Command;
            }
            private set { _Command = value; }
        }
               */

        //DbContextCommand _Command;
        //public DbContextCommand Command
        //{
        //    get
        //    {
        //        ValidateConnectionSettings();
        //        if (_Command == null)
        //        {
        //            _Command = new DbContextCommand(ConnectionString, Provider);
        //        }
        //        return _Command;
        //    }
        //    private set { _Command = value; }
        //}
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
            SetConnectionInternal(connectionName, connectionString,provider, true);
        }

        private void SetConnection(string connectionKey)
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
            ConnectionString = connectionString;
            SetConnectionInternal(connectionName, connectionString,provider, true);
        }

        internal void SetConnectionInternal(string connectionKey, string connectionString,DBProvider provider, bool enableBinding)
        {
 
            if (string.IsNullOrEmpty(connectionString))
            {
                ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
                if (cnn == null)
                {
                    throw new Exception("ConnectionStringSettings configuration not found");
                }
                ConnectionString = cnn.ConnectionString;
                ConnectionName = connectionKey;
            }
            else
            {
                ConnectionName = connectionKey;
                ConnectionString = connectionString;
            }

            Provider=provider;

            if (enableBinding)
            {
                EntityBind();
            }
        }
        

        #endregion

        #region Dispose

        //private bool disposed = false;

        //protected void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            if (_Command != null)
        //            {
        //                _Command.Dispose();
        //                _Command = null;
        //            }
        //            ConnectionName = null;
        //            ConnectionString = null;
        //            Database = null;
        //        }
        //        disposed = true;
        //    }
        //}
      
        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // take this object off the finalization queue     
            GC.SuppressFinalize(this);
        }

        ~DbContext()
        {
            Dispose(false);
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


        public DbCommand NewCmd()
        {
            ValidateConnectionSettings();
            return new DbCommand(ConnectionString);//DbFactory.Create(ConnectionString, Provider);
        }

        #endregion

        #region EntityDbCache

        //EntityDbCache m_Entites;// = new Dictionary<string, EntityDbContext>();

        ///// <summary>
        ///// Get EntityDbContext Items
        ///// </summary>
        //public EntityDbCache DbEntities
        //{
        //    get
        //    {
        //        if (m_Entites == null)
        //        {
        //            m_Entites = new Nistec.Data.Entities.Cache.EntityDbCache(this);
        //        }
        //        return m_Entites;
        //    }
        //    //set { m_db = value; }
        //}

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
        internal EntityDbContext GetEntityDb<T>(string mappingName) where T : IEntityItem
        {
            EntityKeys keys = EntityPropertyBuilder.GetEntityPrimaryKey<T>();
            if (keys == null || keys.Count == 0)
                throw new EntityException("Invalid entity key which is required");
            EntityDbContext db = new EntityDbContext(this, mappingName, keys);
            db.EntityCulture = EntityLocalizer.DefaultCulture;
            return db;
        }
        #endregion

        #region Quer entity

        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public T EntityGet<T>(params object[] keyValueParameters) where T : IEntityItem
        {
            string mappingName=EntityMappingAttribute.Mapping<T>();
            //ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGet.mappingName");
            }
            if (keyValueParameters == null)
            {
                throw new CustomAttributeFormatException("EntityGet.key");
            }
            string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return QuerySingle<T>(commandText, keyValueParameters);
        }
                /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public IList<T> EntityList<T>(params object[] keyValueParameters) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityQuery.mappingName");
            }

            string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return Query<T>(commandText, keyValueParameters);
        }
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public T EntityGet<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGet.mappingName");
            }
            if (keyValueParameters == null)
            {
                throw new CustomAttributeFormatException("EntityGet.key");
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
        public IList<T> EntityList<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityQuery.mappingName");
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
        public IEnumerable<T> EntityFilterList<T>(string mappingName, DataFilter filter) where T : IEntityItem
        {
            ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGetList.mappingName");
            }
            string cmdText = filter == null ? SqlFormatter.SelectString(mappingName) : filter.Select(mappingName);
            IDbDataParameter[] parameters = filter == null ? null : filter.Parameters;
            DataTable dt = ExecuteCommand<DataTable>(cmdText,parameters);
            //DataTable dt = ExecuteCommand<DataTable>(mappingName, filter);
            if (dt == null)
                return null;
            return dt.EntityList<T>();

        }


        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int EntityInsert<T>(T entity) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityInsert.mappingName");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("EntityInsert.entity");
            }
            //T instance = ActivatorUtil.CreateInstance<T>();
            using (EntityDbContext edb = GetEntityDb(entity, mappingName))
            {
                using (EntityContext<T> context = new EntityContext<T>(edb, entity))
                {
                    //context.Set(entity);
                    return context.SaveChanges(UpdateCommandType.Insert);
                }
            }
        }
        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        public int EntityUpdate<T>(T originalEntity, T newEntity) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();
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

            using (EntityDbContext edb = GetEntityDb(originalEntity, mappingName))
            {
                using (EntityContext<T> context = new EntityContext<T>(edb, originalEntity))
                {
                    context.Set(newEntity);//, false);
                    return context.SaveChanges(UpdateCommandType.Update);
                }
            }
        }
        /// <summary>
        /// Entity delete command by mapping name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int EntityDelete<T>(T entity) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityDelete.mappingName");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("DeleteEntity.entity");
            }
            using (EntityDbContext edb = GetEntityDb(entity, mappingName))
            {
                return EntityCommandBuilder.DeleteCommand(entity, edb);
            }
        }
        /// <summary>
        /// Entity delete command by mapping name and name value parameters.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int EntityDelete(string mappingName, params object[] nameValueParameters)
        {
            ValidateConnectionSettings();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityDelete.mappingName");
            }
            if (nameValueParameters == null)
            {
                throw new ArgumentNullException("EntityDelete.nameValueParameters");
            }
            string commandText = SqlFormatter.DeleteCommand(mappingName, nameValueParameters);
            //using (IDbCmd cmd = DbCmd())
            //{
            return ExecuteNonQuery(commandText, DataParameter.GetSql(nameValueParameters));
            //}
        }
        #endregion

        #region Execute StoredProcedure

        /// <summary>
        /// Executes StoredProcedure as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string procName, params object[] nameValueParameters)
        {
            return ExecuteCommandNonQuery(procName, DataParameter.GetSql(nameValueParameters));//, CommandType.StoredProcedure);
        }
        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string procName, T returnIfNull, params object[] nameValueParameters)
        {
            return ExecuteCommandScalar<T>(procName, DataParameter.GetSql(nameValueParameters), returnIfNull, 0);
        }

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (DataSet|DataTable|DataRow) or any entity class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T ExecuteSingle<T>(string procName, params object[] nameValueParameters)
        {
            return ExecuteCommand<T>(procName, DataParameter.GetSql(nameValueParameters));
        }


        /// <summary>
        /// Executes StoredProcedure and returns List of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<T> ExecuteList<T>(string procName, params object[] nameValueParameters)
        {
            return ExecuteCommand<T, IList<T>>(procName, DataParameter.GetSql(nameValueParameters));
        }

     
        #endregion

        #region Execute Command

        /// <summary>
        /// Executes sql as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="commandType"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T ExecuteCommand<T>(string procName, object[] nameValueParameters)
        {
            if (procName == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }

            return ExecuteCommand<T>(procName, DataParameter.GetSql(nameValueParameters));
        }

        #endregion

        #region Query

        /// <summary>
        /// Executes CommandType.Text and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T QueryScalar<T>(string commandText, T returnIfNull, params object[] nameValueParameters)
        {
            return ExecuteCommandScalar<T>(commandText, DataParameter.GetSql(nameValueParameters), returnIfNull, 0);
        }

        /// <summary>
        /// Executes CommandType.Text and returns T value such as (DataSet|DataTable|DataRow) or any entity class..
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T QuerySingle<T>(string commandText, params object[] nameValueParameters)
        {
            return ExecuteCommand<T>(commandText, DataParameter.GetSql(nameValueParameters));
        }


        /// <summary>
        /// Executes CommandType.Text and returns List of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<T> Query<T>(string commandText, params object[] nameValueParameters)
        {
            return ExecuteCommand<T, IList<T>>(commandText, DataParameter.GetSql(nameValueParameters));
        }
        #endregion

        #region Advanced
        /// <summary>
        /// Executes CommandType.Text and returns Dictionary.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<Dictionary<string, object>> QueryDictionary(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteDictionary.commandText");
            }
            commandText = SqlFormatter.GetCommandText(commandText, nameValueParameters);

            DataTable dt = ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters));
            if (dt == null)
                return null;
            return dt.ToListDictionary();// DataUtil.DatatableToDictionary(dt, Pk);
        }

        /// <summary>
        /// Executes StoredProcedure and returns Dictionary.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IList<Dictionary<string, object>> ExecuteDictionary(string procName, params object[] nameValueParameters)
        {
            DataTable dt = ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters));
            if (dt == null)
                return null;
            return dt.ToListDictionary();//DataUtil.DatatableToDictionary(dt, Pk);
        }


        /// <summary>
        /// Executes CommandType.Text and returns Json.
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
            commandText = SqlFormatter.GetCommandText(commandText, nameValueParameters);

            DataTable dt = ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters));
            if (dt == null)
                return null;
            return JsonSerializer.Serialize(dt);
            //return dt.ToJson();
        }
        /// <summary>
        /// Executes StoredProcedure and returns Json.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public string ExecuteJson(string procName, params object[] nameValueParameters)
        {
            DataTable dt = ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters));
            if (dt == null)
                return null;
            //return dt.ToJson();
            return JsonSerializer.Serialize(dt);
        }



        /// <summary>
        /// Executes CommandType.Text and returns Json.
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
            commandText = SqlFormatter.GetCommandText(commandText, nameValueParameters);

            DataTable dt = ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters));
            if (dt == null || dt.Rows.Count == 0)
                return null;
            return JsonSerializer.Serialize(dt.Rows[0]);
            //return dt.ToJson();
        }

        /// <summary>
        /// Executes StoredProcedure and returns Json.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public string ExecuteJsonRecord(string procName, params object[] nameValueParameters)
        {
            DataTable dt = ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters));
            if (dt == null || dt.Rows.Count == 0)
                return null;
            //return dt.ToJson();
            return JsonSerializer.Serialize(dt.Rows[0]);
        }

        /// <summary>
        /// Executes CommandType.Text and returns Dictionary.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IDictionary<string, object> QueryDictionaryRecord(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QueryDictionaryRecord.commandText");
            }
            commandText = SqlFormatter.GetCommandText(commandText, nameValueParameters);
            DataRow dr = ExecuteCommand<DataRow>(commandText, DataParameter.GetSql(nameValueParameters));
            if (dr == null)
                return null;
            return dr.ToDictionary();
        }

        /// <summary>
        /// Executes StoredProcedure and returns Dictionary.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public IDictionary<string, object> ExecuteDictionaryRecord(string procName, params object[] nameValueParameters)
        {
            DataRow dr = ExecuteCommand<DataRow>(procName, DataParameter.GetSql(nameValueParameters));
            if (dr == null)
                return null;
            return dr.ToDictionary();
        }

        /// <summary>
        /// Executes CommandType.Text and returns DataTable.
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public DataTable QueryDataTable(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteDataTable.commandText");
            }
            commandText = SqlFormatter.GetCommandText(commandText, nameValueParameters);
            return ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters));
        }

     
        /// <summary>
        /// Executes StoredProcedure and returns DataTable.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string procName, params object[] nameValueParameters)
        {
            return ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters));
        }
        /// <summary>
        /// Executes StoredProcedure and returns DataSet.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string procName, params object[] nameValueParameters)
        {
            return ExecuteCommand<DataSet>(procName, DataParameter.GetSql(nameValueParameters));
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
            return ExecuteReader(commandText, behavior, DataParameter.GetSql(nameValueParameters));
        }

        #endregion

    }

    
}
