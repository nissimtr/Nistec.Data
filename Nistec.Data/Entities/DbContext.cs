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
    public interface IDbContext:IDisposable
    {
        #region properties
        string ConnectionName { get; }
        string ConnectionString { get; }
        DBProvider Provider { get; }
        bool HasConnection { get; }
        ILocalizer Localization { get; }
        IDbConnection Connection { get; }

        /// <summary>
        /// Get indicate if the connection is valid
        /// </summary>
        bool IsValidConnection { get; }

        //IDbCmd Command { get; }

        IDbCmd NewCmd();

        //IDbCmd DbCmd();
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
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        string EntityGetJson<T>(params object[] keyValueParameters) where T : IEntityItem;
        /// <summary>
        /// Get Entity as <see cref="JsonResults"/> using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns><see cref="JsonResults"/></returns>
        JsonResults EntityJsonResult<T>(params object[] keyValueParameters) where T : IEntityItem;
        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        IList<T> EntityList<T>(params object[] keyValueParameters) where T : IEntityItem;
        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        string EntityListJson<T>(params object[] keyValueParameters) where T : IEntityItem;
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        T EntityItemGet<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem;
      
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        IList<T> EntityItemList<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem;
      
        /// <summary>
        /// Save entity to db update or insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityCommandResult EntitySave<T>(T entity) where T : IEntityItem;
        
        /// <summary>
        /// Save entity changes to update or insert if not exists.(EntitySaveChanges)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityCommandResult EntitySaveChanges<T>(T entity, bool InsertIfNotExists=true) where T : IEntityItem;
        
        /// <summary>
        /// Save entity to db update or insert.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int EntityUpsert(GenericEntity entity);
       

        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityCommandResult EntityInsert<T>(T entity) where T : IEntityItem;

        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        EntityCommandResult EntityUpdate<T>(T originalEntity, T newEntity) where T : IEntityItem;

         /// <summary>
        /// Update entity changes by getting the original from db and save changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityCommandResult EntityUpdate<T>(T entity) where T : IEntityItem;
        
        ///// <summary>
        ///// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="newEntity"></param>
        ///// <param name="keyValueParameters"></param>
        ///// <returns></returns>
        //int EntityUpdate<T>(T newEntity, object[] keyValueParameters) where T : IEntityItem;

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
        int EntityItemDelete(string mappingName, object[] nameValueParameters);
               /// <summary>
        /// Entity delete command by mapping name and name value parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        int EntityDelete<T>(params object[] nameValueParameters) where T : IEntityItem;

                /// <summary>
        /// Executes a command NonQuery and returns <see cref="EntityCommandResult"/> OutptResults and the number of rows affected.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="keyValueDirectionParameters"></param>
        /// <returns><see cref="EntityCommandResult"/></returns> 
        EntityCommandResult ExecuteOutput(string procName, params object[] keyValueDirectionParameters);

        /// <summary>
        /// Executes StoredProcedure as NonQuery and returns the RetrunValue.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns><see cref="int"/></returns> 
        int ExecuteReturnValue(string procName, int returnIfNull, params object[] nameValueParameters);


        /// <summary>
        /// Executes StoredProcedure as NonQuery Command and returns effective records..
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string procName, params object[] nameValueParameters);

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (String|Number|DateTime) or any primitive type.
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
        /// Executes Command and returns T value such as (String|Number|DateTime) or any primitive type.
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
        T ExecuteCommand<T>(string commandText, CommandType commandType, object[] nameValueParameters);

        ///// <summary>
        ///// Executes sql as NonQuery Command  and returns effective records..
        ///// </summary>
        ///// <param name="commandText"></param>
        ///// <param name="commandType"></param>
        ///// <param name="nameValueParameters"></param>
        ///// <returns></returns>
        //int ExecuteCommand(string commandText, CommandType commandType, params object[] nameValueParameters);

        /// <summary>
        /// Execute Command and returns T value such as (DataSet|DataTable|DataRow) or any entity class or scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="addWithKey"></param>
        /// <returns></returns>
        T ExecuteCommand<T>(string commandText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeout = 0, bool addWithKey = false);

        /// <summary>
        /// Execute as NonQuery Command  and returns effective records.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        int ExecuteCommandNonQuery(string commandText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeout = 0);
      
         /// <summary>
        /// Executes a command NonQuery and returns <see cref="EntityCommandResult"/> OutptResults and the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <param name="commandTimeOut">Set the command time out, default =0</param>
        /// <returns></returns> 
        EntityCommandResult ExecuteCommandOutput(string cmdText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text, int commandTimeOut = 0);
        
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
        /// Executes CommandType.Text and returns <see cref="JsonResults"/>.
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        JsonResults QueryJsonResults(string commandText, params object[] nameValueParameters);

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

        //public static int EntitySave<Dbc, T>(T entity, UpdateCommandType commandType, object[] keyvalueParameters)
        //    where Dbc : IDbContext
        //    where T : IEntityItem
        //{

        //    using (var db = DbContext.Create<Dbc>())
        //    {
        //        if (commandType == UpdateCommandType.Delete)
        //        {
        //            if (entity != null)
        //                return db.EntityDelete<T>(entity);
        //            else
        //                return db.EntityDelete<T>(keyvalueParameters);
        //        }
        //        EntityValidator.Validate<T>(entity);
                
        //        if (commandType == UpdateCommandType.Insert)
        //            return db.EntityInsert<T>(entity);
                
        //        if (commandType == UpdateCommandType.Update)
        //            return db.EntityUpdate<T>(entity, keyvalueParameters);
        //    }

        //    return 0;
        //}

        public static int EntityUpsert<Dbc, T>(GenericEntity entity)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = DbContext.Create<Dbc>())
            {
                return db.EntityUpsert(entity);
            }
        }

        //public static int EntitySave<Dbc, T>(T entity)
        //    where Dbc : IDbContext
        //    where T : IEntityItem
        //{

        //   //var parameters=  EntityPropertyBuilder.GetEntityDbParameters(entity);

        //   var parameters = EntityPropertyBuilder.GetEntityKeyValueParameters(entity);

        //    using (var db = DbContext.Create<Dbc>())
        //    {
        //        var item = EntityGet<Dbc, T>(parameters);
        //        if(item==null)
        //        {
        //            return db.EntityInsert<T>(entity);
        //        }
        //        return db.EntityUpdate<T>(item,entity);//, parameters);
        //    }
        //    //return 0;
        //}

        public static EntityCommandResult EntitySave<Dbc, T>(T entity)
            where Dbc : IDbContext
            where T : IEntityItem
        {
            using (var db = DbContext.Create<Dbc>())
            {
                return db.EntitySave<T>(entity);
            }
        }

        public static EntityCommandResult EntityInsert<Dbc, T>(T entity)
            where Dbc : IDbContext
            where T : IEntityItem
        {

            EntityValidator.Validate<T>(entity);

            using (var db = DbContext.Create<Dbc>())
            {
               return db.EntityInsert<T>(entity);
            }
        }

        public static int EntityDelete<Dbc, T>(params object[] keyvalueParameters)
            where Dbc : IDbContext
            where T : IEntityItem
        {

            using (var db = DbContext.Create<Dbc>())
            {
                return db.EntityDelete<T>(keyvalueParameters);
            }

        }
        public static V Lookup<Dbc, V>(string field, string mappingName, V defaultValue, params object[] keyvalueParameters)
            where Dbc : IDbContext
        {
            var sql = SqlFormatter.CreateCommandText(1,field, mappingName, keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.QueryScalar<V>(sql, defaultValue, keyvalueParameters);
            }
        }
        public static string Lookup<Dbc>(string field, string mappingName, string defaultValue, params object[] keyvalueParameters)
            where Dbc : IDbContext
        {
            var sql = SqlFormatter.CreateCommandText(1, field, mappingName, keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.QueryScalar<string>(sql, defaultValue, keyvalueParameters);
            }
        }
        #endregion

        #region static IDbContext
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
        /// Create an instance of IDbContext
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <returns></returns>
        public static IDbContext Create(string connectionName)
        {
            return new DbContext(connectionName);
        }

        ///// <summary>
        ///// Get an instance of IDbContext using connection name.
        ///// </summary>
        ///// <typeparam name="Dbc"></typeparam>
        ///// <returns></returns>
        //public static IDbContext Get(string connectionName)
        //{
        //    IDbContext db = null;
        //    if (!Hash.TryGetValue(connectionName, out db))
        //    {
        //        db = new DbContext(connectionName);
        //        if (db.IsValidConnection)
        //        {
        //            Hash[connectionName] = db;
        //        }
        //    }
        //    return db;
        //}


        ///// <summary>
        ///// Get an instance of (Dbc) IDbContext 
        ///// </summary>
        ///// <typeparam name="Dbc"></typeparam>
        ///// <returns></returns>
        //public static IDbContext Get<Dbc>() where Dbc : IDbContext
        //{
        //    IDbContext db = null;
        //    string name = typeof(Dbc).Name;
        //    if (!Hash.TryGetValue(name, out db))
        //    {
        //        db = Create<Dbc>();
        //        if (db.IsValidConnection)
        //        {
        //            Hash[name] = db;
        //        }
        //    }
        //    return db;
        //}

        //static Dictionary<string, IDbContext> m_hash;

        //private static Dictionary<string, IDbContext> Hash
        //{
        //    get
        //    {
        //        if (m_hash == null)
        //        {
        //            m_hash = new Dictionary<string, IDbContext>();
        //        }
        //        return m_hash;
        //    }
        //}
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
            : base(connectionKey)
        {
            EntityBind();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbContext(string connectionString, DBProvider provider)
            : base(connectionString, provider)
        {
            //SetConnectionInternal(connectionName, connectionString, provider, true);
            EntityBind();
        }

        protected void SetConnection(string connectionName, string connectionString, DBProvider provider)
        {
            ConnectionName = connectionName;
            Provider = provider;
            ConnectionString = connectionString;
            SetConnectionInternal(connectionName, connectionString, provider, true);
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

            //Command = DbFactory.Create(ConnectionString, Provider);

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

        ///// <summary>
        ///// Validate if entity has connection properties
        ///// </summary>
        ///// <exception cref="EntityException"></exception>
        //public void ValidateConnectionSettings()
        //{
        //    if (string.IsNullOrEmpty(ConnectionString))
        //    {
        //        throw new EntityException("Invalid ConnectionName");
        //    }
        //}

        public IDbCmd NewCmd()
        {
            ValidateConnectionSettings();
            return DbFactory.Create(ConnectionString, Provider);
        }

        //internal IDbCmd DbCmd()
        //{
        //    ValidateConnection();
        //    return DbFactory.Create(ConnectionString, Provider);
        //}

        ///// <summary>
        ///// Get Connection
        ///// </summary>
        ///// <returns></returns>
        //private IDbConnection Connection()
        //{
        //    return DbCmd().Connection;
        //}
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
        internal EntityDbContext GetEntityDb<T>(string mappingName) where T : IEntityItem
        {
            EntityKeys keys = EntityPropertyBuilder.GetEntityPrimaryKey<T>();
            if (keys == null || keys.Count == 0)
                throw new EntityException("Invalid entity key which is required");
            EntityDbContext db = new EntityDbContext(this, mappingName, keys);
            db.EntityCulture = EntityLocalizer.DefaultCulture;
            return db;
        }
        internal EntityDbContext GetEntityDb(GenericEntity entity) //where T : IEntityItem
        {
            string mappingName = entity.MappingName;// EntityMappingAttribute.Mapping<T>();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityUpsert.mappingName");
            }
            EntityDbContext db = new EntityDbContext(this, mappingName, entity.PrimaryKey);
            db.EntityCulture = EntityLocalizer.DefaultCulture;
            return db;
        }
        #endregion

        #region Query entity

        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public T EntityGet<T>(params object[] keyValueParameters) where T : IEntityItem
        {
            string mappingName=EntityMappingAttribute.View<T>();
            //ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGet.mappingName");
            }
            if (keyValueParameters == null)
            {
                throw new CustomAttributeFormatException("EntityGet.keyValueParameters");
            }
            string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return QuerySingle<T>(commandText, keyValueParameters);
        }

        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public string EntityGetJson<T>(params object[] keyValueParameters) where T : IEntityItem
        {
            string mappingName = EntityMappingAttribute.View<T>();
            //ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGet.mappingName");
            }
            if (keyValueParameters == null)
            {
                throw new CustomAttributeFormatException("EntityGet.keyValueParameters");
            }
            //string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return QueryJsonRecord(mappingName, keyValueParameters);
        }
        /// <summary>
        /// Get Entity as <see cref="JsonResults"/> using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns><see cref="JsonResults"/></returns>
        public JsonResults EntityJsonResult<T>(params object[] keyValueParameters) where T : IEntityItem
        {
            string mappingName = EntityMappingAttribute.View<T>();
            //ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGet.mappingName");
            }
            if (keyValueParameters == null)
            {
                throw new CustomAttributeFormatException("EntityGet.keyValueParameters");
            }
            //string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return QueryJsonResults(mappingName, keyValueParameters);
        }
        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public IList<T> EntityList<T>(params object[] keyValueParameters) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.View<T>();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityQuery.mappingName");
            }

            string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return Query<T>(commandText, keyValueParameters);
        }
        /// <summary>
        /// Get Entity using <see cref="EntityMappingAttribute"/> mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public string EntityListJson<T>(params object[] keyValueParameters) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.View<T>();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityQuery.mappingName");
            }

            //string commandText = SqlFormatter.GetCommandText(mappingName, keyValueParameters);
            return QueryJson(mappingName, keyValueParameters);
        }
        /// <summary>
        /// Get Entity using <see cref="EntityContext"/> with mapping name and keys filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public T EntityItemGet<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            if (mappingName == null)
            {
                throw new ArgumentNullException("EntityGet.mappingName");
            }
            if (keyValueParameters == null)
            {
                throw new CustomAttributeFormatException("EntityGet.keyValueParameters");
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
        public IList<T> EntityItemList<T>(string mappingName, params object[] keyValueParameters) where T : IEntityItem
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

            //using(EntityDbContext edb = GetEntityDb<T>(mappingName))
            //{
            //    return edb.EntityList<T>(filter);
            //}
            //T instance = ActivatorUtil.CreateInstance<T>();//Activator.CreateInstance<T>();
            //EntityDbContext edb = GetEntityDb(instance, mappingName);
            //if (filter == null)
            //    return edb.EntityList<T>();
            //return edb.EntityList<T>(filter);
        }
        
        /// <summary>
        /// Save entity changes to update or insert if not exists.(EntitySaveChanges)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="InsertIfNotExists"></param>
        /// <returns></returns>
        public EntityCommandResult EntitySaveChanges<T>(T entity, bool InsertIfNotExists = true) where T : IEntityItem
        {
            EntityCommandResult res = null;
            if (entity == null)
            {
                throw new ArgumentNullException("EntityUpsert.entity");
            }

            var parameters = EntityPropertyBuilder.GetEntityKeyValueParameters(entity);
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentNullException("EntityUpsert.EntityKey");
            }

            try
            {
                this.OwnsConnection = true;
                var item = EntityGet<T>(parameters);

                if (item == null)
                {
                    if (InsertIfNotExists)
                        res = EntityInsert<T>(entity);
                }
                else
                {
                    res = EntityUpdate<T>(item, entity);//, parameters);
                }
                return res ?? EntityCommandResult.Empty;
            }
            finally
            {
                this.OwnsConnection = false;
            }
        }
        
        /// <summary>
        /// Save all entity fields to db update or insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityCommandResult EntitySave<T>(T entity) where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("EntitySave.entity");
            }

            string mappingName = EntityMappingAttribute.Mapping<T>();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityUpsert.mappingName");
            }
            var parameters = EntityPropertyBuilder.GetEntityKeyValueParameters(entity);
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentNullException("EntitySave.EntityKey");
            }
            var fieldsValues = EntityPropertyBuilder.EntityToDictionary(entity);

            using (EntityCommandBuilder ac = new EntityCommandBuilder(entity, this, mappingName, fieldsValues))
            {
                return ac.ExecuteCommand(UpdateCommandType.Upsert);
            }
        }
        /// <summary>
        /// Save entity to db update or insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int EntityUpsert<T>(GenericEntity entity) where T : IEntityItem
        {
            if (entity == null)
            {
                throw new ArgumentNullException("EntityUpsert.entity");
            }
            string mappingName = EntityMappingAttribute.Mapping<T>();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityUpsert.mappingName");
            }
 
            using (EntityDbContext edb = GetEntityDb(entity, mappingName))
            {
                return entity.SaveChanges<T>(UpdateCommandType.Upsert, edb);
            }
        }

        public int EntityUpsert(GenericEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("EntityUpsert.entity");
            }
            //string mappingName = entity.MappingName;
            //if (string.IsNullOrEmpty(mappingName))
            //{
            //    throw new ArgumentNullException("EntityUpsert.mappingName");
            //}

            using (EntityDbContext edb = GetEntityDb(entity))
            {
                return entity.SaveChanges(UpdateCommandType.Upsert, edb);
            }
        }

        ///// <summary>
        ///// Save entity to db update or insert using <see cref="GenericEntity"/> entity .
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="keyValueParameters"></param>
        ///// <returns></returns>
        //public int EntityUpsert<T>(params object[] keyValueParameters) where T : IEntityItem
        //{
        //    if (keyValueParameters == null)
        //    {
        //        throw new ArgumentNullException("EntityUpsert.keyValueParameters");
        //    }
        //    //string mappingName = EntityMappingAttribute.Mapping<T>();
        //    //if (string.IsNullOrEmpty(mappingName))
        //    //{
        //    //    throw new ArgumentNullException("EntityUpsert.mappingName");
        //    //}
        //    //var parameters = EntityPropertyBuilder.GetEntityKeyValueParameters(entity);
        //    //if (parameters == null || parameters.Length == 0)
        //    //{
        //    //    throw new ArgumentNullException("EntitySave.EntityKey");
        //    //}
        //    //using (EntityCommandBuilder ac = new EntityCommandBuilder(entity, Connection, mappingName))
        //    //{
        //    //    return ac.ExecCommand(UpdateCommandType.Upsert);
        //    //}

        //    GenericEntity entity = GenericEntity.Create<T>(keyValueParameters);

        //    using (EntityDbContext edb = GetEntityDb<T>(entity))
        //    {
        //        return entity.SaveChanges<T>(UpdateCommandType.Upsert, edb);
        //       //return EntityFieldsChanges.SaveChanges(UpdateCommandType.Upsert,  entity, edb);

        //        //EntityFieldsChanges changes = new EntityFieldsChanges(edb, entity, parameters);
        //        //return changes.SaveChanges();
        //    }
        //}

        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityCommandResult EntityInsert<T>(T entity) where T : IEntityItem
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
            var fieldsChanges = EntityPropertyBuilder.EntityToDictionary(entity);

            using (EntityCommandBuilder ac = new EntityCommandBuilder(entity, this, mappingName, fieldsChanges))
            {
                return ac.ExecuteCommand(UpdateCommandType.Insert);
            }

            //using (EntityDbContext edb = GetEntityDb(entity, mappingName))
            //{
            //    EntityFieldsChanges changes = new EntityFieldsChanges(edb, entity);
            //    return changes.SaveChanges();
            //}

            //T instance = ActivatorUtil.CreateInstance<T>();
            //using (EntityDbContext edb = GetEntityDb(entity, mappingName))
            //{

            //    using (EntityCommandBuilder ac = new EntityCommandBuilder(entity, Connection, mappingName))
            //    {
            //        return ac.ExecuteCommand(UpdateCommandType.Insert);
            //    }

            //    using (EntityContext<T> context = new EntityContext<T>(edb, entity))
            //    {
            //        //context.Set(entity);
            //        return context.SaveChanges(UpdateCommandType.Insert);
            //    }
            //}
        }
        /// <summary>
        /// Update entity changes from original to new one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalEntity"></param>
        /// <param name="newEntity"></param>
        /// <returns></returns>
        public EntityCommandResult EntityUpdate<T>(T originalEntity, T newEntity) where T : IEntityItem
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
                EntityFieldsChanges changes = new EntityFieldsChanges(edb, originalEntity, newEntity);
                return changes.SaveChanges();
                //using (EntityContext<T> context = new EntityContext<T>(edb, originalEntity))
                //{
                //    context.Set(newEntity);//, false);
                //    return context.SaveChanges(UpdateCommandType.Update);
                //}
            }
        }
        /// <summary>
        /// Update entity changes by getting the original from db and save changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityCommandResult EntityUpdate<T>(T entity) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityUpdate.mappingName");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("EntityUpdate.entity");
            }

            var parameters = EntityPropertyBuilder.GetEntityKeyValueParameters(entity);
            if (parameters == null || parameters.Length == 0)
            {
                throw new ArgumentNullException("EntityUpsert.EntityKey");
            }
            try
            {
                this.OwnsConnection = true;
                var originalEntity = EntityGet<T>(parameters);

                using (EntityDbContext edb = GetEntityDb(originalEntity, mappingName))
                {
                    EntityFieldsChanges changes = new EntityFieldsChanges(edb, originalEntity, entity);
                    return changes.SaveChanges();
                    //using (EntityContext<T> context = new EntityContext<T>(edb, originalEntity))
                    //{
                    //    context.Set(newEntity);//, false);
                    //    return context.SaveChanges(UpdateCommandType.Update);
                    //}
                }
            }
            finally
            {
                this.OwnsConnection = false;
            }
        }

  

/*
        /// <summary>
        /// Set entity using <see cref="UpdateCommandType"/> commandType such as update,insert,delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newEntity"></param>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public int EntityUpdate<T>(T newEntity, object[] keyValueParameters) where T : IEntityItem
        {
            //ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityUpdate.mappingName");
            }

            if (newEntity == null)
            {
                throw new ArgumentNullException("EntityUpdate.newEntity");
            }
            int res = 0;
            try
            {
                this.OwnsConnection = true;
                using (EntityDbContext edb = GetEntityDb(newEntity, mappingName))
                {
                    var originalEntity = edb.QuerySingle<T>(keyValueParameters);

                    EntityFieldsChanges changes = new EntityFieldsChanges(edb, originalEntity, newEntity);
                    res= changes.SaveChanges();

                    //using (EntityContext<T> context = new EntityContext<T>(edb, originalEntity))
                    //{
                    //    context.Set(newEntity);//, false);
                    //    res = context.SaveChanges(UpdateCommandType.Update);
                    //}
                    this.OwnsConnection = false;
                    return res;
                }
            }
            finally
            {
                this.OwnsConnection = false;
            }
        }
*/
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
        public int EntityItemDelete(string mappingName, object[] nameValueParameters)
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
            return ExecuteCommandNonQuery(commandText, DataParameter.GetSql(nameValueParameters));
            //}
        }

        /// <summary>
        /// Entity delete command by mapping name and name value parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int EntityDelete<T>(params object[] nameValueParameters)where T : IEntityItem
        {
            ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();

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
            return ExecuteCommandNonQuery(commandText, DataParameter.GetSql(nameValueParameters));
            //}
        }
        

        #endregion

        #region Execute StoredProcedure

        /// <summary>
        /// Executes a command NonQuery and returns <see cref="EntityCommandResult"/> OutptResults and the number of rows affected.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns><see cref="EntityCommandResult"/></returns> 
        public EntityCommandResult ExecuteOutput(string procName, params object[] keyValueDirectionParameters)
        {
            return ExecuteCommandOutput(procName, DataParameter.GetSqlWithDirection(keyValueDirectionParameters), CommandType.StoredProcedure);
        }

        /// <summary>
        /// Executes StoredProcedure as NonQuery and returns the RetrunValue.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns><see cref="int"/></returns> 
        public int ExecuteReturnValue(string procName, int returnIfNull, params object[] nameValueParameters)
        {
            return ExecuteCommandReturnValue(procName, DataParameter.GetSqlWithReturnValue(nameValueParameters), returnIfNull);
        }


        /// <summary>
        /// Executes StoredProcedure as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string procName, params object[] nameValueParameters)
        {
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteNonQuery.commandText");
            //}
            return ExecuteCommandNonQuery(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
        }
        /// <summary>
        /// Executes StoredProcedure and returns T value such as (String|Number|DateTime) or any primitive type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string procName, T returnIfNull, params object[] nameValueParameters)
        {
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteScalar.commandText");
            //}
            return ExecuteCommandScalar<T>(procName, DataParameter.GetSql(nameValueParameters), returnIfNull, CommandType.StoredProcedure, 0);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteSingle.commandText");
            //}
            return ExecuteCommand<T>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteQuery.commandText");
            //}
            return ExecuteCommand<T, IList<T>>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
        }

     
        #endregion

        #region Execute Command

       /// <summary>
       /// Executes sql as NonQuery Command  and returns T value.
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="commandText"></param>
       /// <param name="commandType"></param>
       /// <param name="nameValueParameters"></param>
       /// <returns></returns>
        public T ExecuteCommand<T>(string commandText, CommandType commandType, object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }

            return ExecuteCommand<T>(commandText, DataParameter.GetSql(nameValueParameters), commandType);
        }

       
        /// <summary>
        /// Executes sql as NonQuery Command  and returns effective records..
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int ExecuteCommand(string commandText, CommandType commandType , params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }

            return ExecuteCommandNonQuery(commandText, DataParameter.GetSql(nameValueParameters), commandType);

            //using (IDbCmd cmd = DbCmd())
            //{
            //return ExecuteNonQuery(commandText, DataParameter.GetSql(nameValueParameters), commandType);
            //}
        }
        /*
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
           //using (IDbCmd cmd = DbCmd())
           //{
           return Command.ExecuteCommand<T>(commandText, parameters, commandType, commandTimeout);
           //}
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
           //using (IDbCmd cmd = DbCmd())
           //{
           return Command.ExecuteNonQuery(commandText, parameters, commandType, commandTimeout);
          // }
       }
       */
        #endregion

        #region Query

        /// <summary>
        /// Executes StoredProcedure and returns T value such as (String|Number|DateTime) or any primitive type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="returnIfNull"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public T QueryScalar<T>(string commandText, T returnIfNull, params object[] nameValueParameters)
        {
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("QueryScalar.commandText");
            //}
            return ExecuteCommandScalar<T>(commandText, DataParameter.GetSql(nameValueParameters), returnIfNull, CommandType.Text, 0);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("QuerySingle.commandText");
            //}
            return ExecuteCommand<T>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("Query.commandText");
            //}
            return ExecuteCommand<T, IList<T>>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
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

            DataTable dt = ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteDictionary.commandText");
            //}
            DataTable dt = ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
            if (dt == null)
                return null;
            return dt.ToListDictionary();//DataUtil.DatatableToDictionary(dt, Pk);
        }

        /// <summary>
        /// Executes CommandType.Text and returns <see cref="JsonResults"/>.
        /// </summary>
        /// <param name="commandText">Command text or mapping name</param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public JsonResults QueryJsonResults(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QueryJsonResults.commandText");
            }
            commandText = SqlFormatter.GetCommandText(commandText, nameValueParameters);

            return ExecuteCommand<JsonResults>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
         }

        /// <summary>
        /// Executes CommandType.Text and returns Json.
        /// </summary>
        /// <param name="commandText">Command text or mapping name</param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public string QueryJson(string commandText, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("QueryJson.commandText");
            }
            commandText = SqlFormatter.GetCommandText(commandText, nameValueParameters);

            DataTable dt = ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteJson.commandText");
            //}
            DataTable dt = ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
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

            DataTable dt = ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("QueryJsonRecord.commandText");
            //}
            DataTable dt = ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
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
            DataRow dr = ExecuteCommand<DataRow>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
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
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("QueryDictionaryRecord.commandText");
            //}
            DataRow dr = ExecuteCommand<DataRow>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
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
            return ExecuteCommand<DataTable>(commandText, DataParameter.GetSql(nameValueParameters), CommandType.Text);
        }

     
        /// <summary>
        /// Executes StoredProcedure and returns DataTable.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string procName, params object[] nameValueParameters)
        {
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteDataTable.commandText");
            //}
            return ExecuteCommand<DataTable>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
        }
        /// <summary>
        /// Executes StoredProcedure and returns DataSet.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string procName, params object[] nameValueParameters)
        {
            //if (commandText == null)
            //{
            //    throw new ArgumentNullException("ExecuteDataSet.commandText");
            //}

            return ExecuteCommand<DataSet>(procName, DataParameter.GetSql(nameValueParameters), CommandType.StoredProcedure);
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
