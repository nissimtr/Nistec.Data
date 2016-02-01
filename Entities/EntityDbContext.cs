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
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Nistec.Data.Entities
{
    /// <summary>
    /// Represent db context for entities.
    /// </summary>
    public class EntityDbContext 
    {

        #region static

        /// <summary>
        /// Get EntityDbContext using current instance of  <see cref="DbContext"/>
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static EntityDbContext Get<Dbc>(string mappingName, EntitySourceType sourceType) where Dbc : IDbContext
        {
            IDbContext db = DbContext.Get<Dbc>();
            return new EntityDbContext();//db, mappingName, sourceType);
        }

        /// <summary>
        /// Create EntityDbContext using new instance of  <see cref="DbContext"/>
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static EntityDbContext Get<Dbc>(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys) where Dbc : IDbContext
        {
            IDbContext db = DbContext.Create<Dbc>();
            return new EntityDbContext();//db, entityName, mappingName, sourceType, keys);
        }

        /// <summary>
        /// Create EntityDbContext using new instance of  <see cref="DbContext"/>
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static EntityDbContext Get<Dbc>(string mappingName, EntityKeys keys) where Dbc : IDbContext
        {
            IDbContext db = DbContext.Create<Dbc>();
            return new EntityDbContext();//db, mappingName, mappingName, EntitySourceType.Table, keys);
        }
        /// <summary>
        /// Create EntityDbContext using entity object and culture.
        /// </summary>
        /// <param name="ientity"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static EntityDbContext Get(object ientity, CultureInfo culture)
        {
            EntityDbContext db = null;

            EntityAttribute[] attributes = ientity.GetType().GetCustomAttributes<EntityAttribute>();
            if (attributes == null || attributes.Length == 0)
                return db;
            var attribute = attributes[0];
            db = new EntityDbContext(attribute);
            db.EntityCulture = culture;
            return db;
        }

        public static EntityDbContext Get<Dbe>(CultureInfo culture) where Dbe : IEntity
        {
            EntityDbContext db = null;

            EntityAttribute[] attributes = typeof(Dbe).GetCustomAttributes<EntityAttribute>();
            if (attributes == null || attributes.Length == 0)
                return db;
            var attribute = attributes[0];
            db = new EntityDbContext(attribute);
            db.EntityCulture = culture;
            return db;
        }


        #endregion

        #region ctor
        /// <summary>
        /// Crate a new instance of <see cref="EntityDbContext"/>
        /// </summary>
        public EntityDbContext()
        {
            SourceType = EntitySourceType.Table;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbContext"/>
        /// </summary>
        /// <param name="attr"></param>
        public EntityDbContext(EntityAttribute attr)
            : this()
        {
            EntityName = attr.EntityName;
            MappingName = attr.MappingName;
            ConnectionKey = attr.ConnectionKey;
            SourceType = attr.EntitySourceType;
            Keys = attr.EntityKey;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbContext"/>
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="connectionKey"></param>
        public EntityDbContext(string mappingName,string connectionKey)
            : this()
        {
            EntityName = mappingName;
            MappingName = mappingName;
            ConnectionKey = connectionKey;
            Keys = null;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbContext"/>
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="connectionKey"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        public EntityDbContext(string entityName, string mappingName, string connectionKey,EntitySourceType sourceType, EntityKeys keys)
        {
            EntityName = entityName;
            MappingName = mappingName;
            ConnectionKey = connectionKey;
            SourceType = sourceType;
            Keys = keys.ToArray();
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbContext"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        public EntityDbContext(IDbContext db, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys)
        {
            EntityName = entityName;
            MappingName = mappingName;
            _Db = db;
            ConnectionKey = db.ConnectionName;
            SourceType = sourceType;
            Keys = keys.ToArray();
        }
         /// <summary>
        ///  Crate a new instance of <see cref="EntityDbContext"/>
         /// </summary>
         /// <param name="db"></param>
         /// <param name="mappingName"></param>
         /// <param name="keys"></param>
        public EntityDbContext(IDbContext db, string mappingName, EntityKeys keys)
            : this()
        {
             this.MappingName = mappingName;
             this.EntityName = mappingName;
             this._Db = db;
             this.ConnectionKey = db.ConnectionName;
             this.Keys = keys.ToArray();
             
         }
        #endregion

        #region properties

        IDbContext _Db;
        /// <summary>
        /// Get <see cref="IDbContext"/> Context.
        /// </summary>
        /// <returns></returns>
        public IDbContext Context()
        {
            if (_Db == null)
            {
                _Db = DbContext.Get(ConnectionKey);
            }
            return _Db;
        }
        /// <summary>
        /// Get or Set entity name.
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// Get or Set mapping name.
        /// </summary>
        public string MappingName { get; set; }
        /// <summary>
        /// Get or Set connection key.
        /// </summary>
        public string ConnectionKey { get; set; }
        /// <summary>
        /// Get or Set entity keys.
        /// </summary>
        public string[] Keys { get; set; }
        /// <summary>
        /// Get or Set <see cref="EntitySourceType"/> source type.
        /// </summary>
        public EntitySourceType SourceType { get; set; }

        internal CommandType CmdType()
        {
                if (SourceType == Entities.EntitySourceType.Procedure)
                {
                    return CommandType.StoredProcedure;
                }
                return CommandType.Text;
        }
        
        CultureInfo m_CultureInfo;

        /// <summary>
        /// Get or Set current culture
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public virtual CultureInfo EntityCulture
        {
            get
            {
                if (m_CultureInfo == null)
                    return EntityLocalizer.DefaultCulture;
                return m_CultureInfo;
            }
            set { m_CultureInfo = value; }
        }

        EntityKeys m_EntityKeys;
        /// <summary>
        /// Get <see cref="EntityKeys"/>.
        /// </summary>
        public EntityKeys EntityKeys
        {
            get
            {
                if (m_EntityKeys == null)
                {
                    m_EntityKeys = Keys == null ? new EntityKeys() : new EntityKeys(Keys);
                }
                return m_EntityKeys;
            }
            //set { m_EntityKeys = value; }
        }

        /// <summary>
        /// Get indicate if entity has connection properties
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool HasConnection
        {
            get
            {
                if ((!string.IsNullOrEmpty(ConnectionKey) || (_Db != null && _Db.HasConnection)) 
                    && !string.IsNullOrEmpty(MappingName))
                {
                    return true;
                }
                return false;
            }
        }
        #endregion

        internal IDbConnection DbConnection()
        {
            ValidateContext();
            return _Db.Connection();
        }

        internal string DbConnectionName()
        {
            ValidateContext();
            return _Db.ConnectionName;
        }
        internal DBProvider DbProvider()
        {
            ValidateContext();
            return _Db.Provider;
        }
  
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

        #region Command formater

        internal string GetCommandText(int top)
        {
            if (string.IsNullOrEmpty(MappingName))
            {
                throw new EntityException("Invalid MappingName");
            }

            return SqlFormatter.SelectString(top, "*", MappingName, null);
        }
         internal string GetCommandText(bool addWhere)
        {
            if (string.IsNullOrEmpty(MappingName))
            {
                throw new EntityException("Invalid MappingName");
            }
            string where = "";
            if (addWhere)
            {
                if (Keys == null)
                {
                    throw new EntityException("Invalid Entity Keys");
                }
                where = SqlFormatter.CommandWhere(EntityKeys.ToArray(), false);
            }
            return SqlFormatter.SelectString("*", MappingName, where);
        }
        internal object[] EntityKeysToParam(object[] keys)
        {
            if (string.IsNullOrEmpty(MappingName))
            {
                throw new Exception("Invalid MappingName");
            }
            if (keys == null)
            {
                return null;
            }
            if (this.Keys == null)
            {
                return null;
            }
            int count = EntityKeys.Count;
            List<object> prm = new List<object>();
            for (int i = 0; i < count; i++)
            {
                prm.Add(EntityKeys[i]);
                prm.Add(keys[i]);
            }

            return prm.ToArray();
        }

   
        internal string GetCommandText(object[] keyValueParameters)
        {
            if (string.IsNullOrEmpty(MappingName))
            {
                throw new EntityException("Invalid MappingName");
            }
            string where = "";
            if (keyValueParameters!=null)
            {
                where = SqlFormatter.CommandWhere(keyValueParameters);
            }
            return SqlFormatter.SelectString("*", MappingName, where);
        }
        public T QueryEntity<T>(params object[] keys)
        {
            ValidateContext();
            try
            {
                var cmd = Context();
                object[] keyValueParameters = EntityKeysToParam(keys);
                if (SourceType == EntitySourceType.Procedure)
                {
                    return cmd.ExecuteSingle<T>(MappingName, keyValueParameters);
                }
                else
                {
                    string commandText = GetCommandText(keyValueParameters);
                    return cmd.QuerySingle<T>(commandText, keyValueParameters);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }
        public T QuerySingle<T>(params object[] keyValueParameters)
        {
            ValidateContext();
            try
            {
                var cmd = Context();
                if (SourceType == EntitySourceType.Procedure)
                {
                    return cmd.ExecuteSingle<T>(MappingName, keyValueParameters);
                }
                else
                {
                    string commandText = GetCommandText(keyValueParameters);
                    return cmd.QuerySingle<T>(commandText, keyValueParameters);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }

        public IList<T> Query<T>(params object[] keyValueParameters)
        {
            ValidateContext();
            try
            {
                var cmd = Context();
                if (SourceType == EntitySourceType.Procedure)
                {
                    return cmd.ExecuteQuery<T>(MappingName, keyValueParameters);
                }
                else
                {
                    string commandText = GetCommandText(keyValueParameters);
                    return cmd.Query<T>(commandText, keyValueParameters);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }

        internal T DoCommand<T>(DataFilter filter)
        {
            ValidateContext();
            try
            {
                var context = Context();
                string commandText = filter == null ? SqlFormatter.SelectString(MappingName) : filter.Select(MappingName);
                IDbDataParameter[] parameters = filter == null ? null : filter.Parameters;
                using (var cmd = context.DbCmd())
                {
                    return cmd.ExecuteCommand<T>(commandText, parameters, CmdType());
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }
        public T DoCommand<T>(string commandText, IDbDataParameter[] parameters, CommandType cmdType, int commandTimeout = 0)
        {
            ValidateContext();
            try
            {
                var context = Context();
                using (var cmd = context.DbCmd())
                {
                    return cmd.ExecuteCommand<T>(commandText, parameters, cmdType, commandTimeout, true);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }
        public TResult DoCommand<TItem, TResult>(string commandText, IDbDataParameter[] parameters, CommandType cmdType, int commandTimeout = 0)
        {
            ValidateContext();
            try
            {
                var context = Context();
                using (var cmd = context.DbCmd())
                {
                    return cmd.ExecuteCommand<TItem, TResult>(commandText, parameters, cmdType, commandTimeout, true);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }
        public int ExecuteNonQuery(string commandText, IDbDataParameter[] parameters, CommandType cmdType, int commandTimeout = 0)
        {
            ValidateContext();
            try
            {
                var context = Context();
                using (var cmd = context.DbCmd())
                {
                    return cmd.ExecuteNonQuery(commandText, parameters, cmdType, commandTimeout);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }

        public DataTable ExecuteDataTable()
        {
            var context = Context();
            using (var cmd = context.DbCmd())
            {
                return cmd.ExecuteDataTable(MappingName, true);
            }
        }
        internal DataTable ExecuteDataTable(string sql)
        {
           var context = Context();
           using (var cmd = context.DbCmd())
           {
               return cmd.ExecuteDataTable(EntityName, sql, true);
           }

        }
        internal DataTable ExecuteDataTable(int top)
        {
            DataTable dt = null;
            ValidateContext();
            try
            {
                var context = Context();
                using (var cmd = context.DbCmd())
                {
                    string commandText = GetCommandText(top);
                    DataParameter[] prm = null;
                    dt = cmd.ExecuteCommand<DataTable>(commandText, prm, CmdType(), 0, true);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new EntityException(ex.Message);
            }
        }

        /// <summary>
        /// Validate if entity has connection properties
        /// </summary>
        /// <exception cref="EntityException"></exception>
        public void ValidateContext()
        {
            if (string.IsNullOrEmpty(ConnectionKey) && string.IsNullOrEmpty(MappingName))
            {
                throw new EntityException("Invalid MappingName or ConnectionContext");
            }
            if (_Db == null)
            {
                _Db = DbContext.Get(ConnectionKey);
            }

        }
        #endregion

    }
}
