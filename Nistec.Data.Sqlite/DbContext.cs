using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Configuration;
using Nistec.Generic;
using System.IO;
using Nistec.Data.Entities;
using System.Reflection;
using Nistec.Serialization;

namespace Nistec.Data.Sqlite
{

    /*
            string sqlCreateTable = "create table demo_score (name varchar(20), score int)";
            string sqlInsert1 = "insert into demo_score (name, score) values ('Nissim', 9001)";
            string sqlInsert2 = "insert into demo_score (name, score) values ('Neomi', 9002)";
            string sqlInsert3 = "insert into demo_score (name, score) values ('Shani', 9003)";
            string sqlUpdate = "update demo_score set name='Shani' where score=@score";
            string sqlDelete = "delete from demo_score where score=@score";
            string sqlSelect = "select * from demo_score order by score desc";

            DataTable dt = null;

            DbLite.CreateFileFromSettings("sqlite.demo");
            using(DbLite db=new DbLite("sqlite.demo"))
            {
                db.OwnsConnection = true;
                db.ExecuteCommandNonQuery(sqlCreateTable);
                db.ExecuteCommandNonQuery(sqlInsert1);
                db.ExecuteCommandNonQuery(sqlInsert2);
                db.ExecuteCommandNonQuery(sqlInsert3);
                db.ExecuteCommandNonQuery(sqlUpdate, DbLite.GetParam("score", 9004));
                db.ExecuteCommandNonQuery(sqlDelete, DbLite.GetParam("score", 9002));

                dt=db.ExecuteDataTable("demo_score", sqlSelect);
                db.OwnsConnection = false;
            }

            Console.Write(dt.TableName);
    */
    public class DbLite : Nistec.Data.Entities.DbContextCommand, IDisposable, Nistec.Data.Entities.IDbContext
    {

        public const string ProviderName = "SQLite";

        //string _connectionString;
        //public string CreateConnectionString(string filename)
        //{
        //    if (_connectionString == null)
        //    {
        //        _connectionString = SQLiteUtil.ConnectionStringBuilder(filename);
        //    }
        //    return _connectionString;
        //}

        #region ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public DbLite():base()
        {
        }


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionKey"></param>
        public DbLite(string connectionKey)
            : base(connectionKey)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbLite(string connectionString, DBProvider provider)
            : base(connectionString, provider)
        {
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
                Provider = DBProvider.SQLite;
                ConnectionString = cnn.ConnectionString;
                ConnectionName = connectionKey;
            }
            else
            {
                ConnectionName = connectionKey;
                Provider = DBProvider.SQLite;
                ConnectionString = connectionString;
            }

            if (enableBinding)
            {
                EntityBind();
            }
        }
        /// <summary>
        /// Occured befor DbContext initilaized
        /// </summary>
        protected virtual void EntityBind() { }

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

        ~DbLite()
        {
            Dispose(false);
        }

        #endregion


        public SQLiteConnection SQLiteConnection
        {
            get { return (SQLiteConnection)Connection; }
        }


        public SQLiteTransaction CreateTransaction(SQLiteConnection cnn)
        {
            return cnn.BeginTransaction();
        }

        public int ExecuteNonQueryTrans(string cmdText, SQLiteParameter[] parameters, Func<int, bool> transAction)
        {
            int result = 0;
            using (SQLiteConnection con = new SQLiteConnection(this.ConnectionString))
            {
                con.Open();

                using (SQLiteTransaction tr = con.BeginTransaction())
                {
                    using (SQLiteCommand cmd = con.CreateCommand())
                    {
                        cmd.Parameters.AddRange(parameters);
                        cmd.Transaction = tr;
                        cmd.CommandText = cmdText;
                        result = cmd.ExecuteNonQuery();

                    }
                    bool ok = transAction.Invoke(result);
                    if (ok)
                        tr.Commit();
                }

                con.Close();
            }
            return result;
        }

        #region override

        protected override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
        protected override DBProvider GetProvider(IDbConnection cnn)
        {
            return DBProvider.SQLite;
        }

        protected override IDbCommand CreateCommand(string cmdText)
        {
            return base.CreateCommand(cmdText);
        }

        protected override IDbCommand CreateCommand(string cmdText, IDbDataParameter[] parameters)
        {
            SQLiteCommand command = new SQLiteCommand(cmdText, Connection as SQLiteConnection);
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            return command;
        }

        protected override IDbDataAdapter CreateIDataAdapter(IDbCommand cmd)
        {
                return new SQLiteDataAdapter((SQLiteCommand)cmd) as IDbDataAdapter;
        }
        #endregion

        /*

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="keyValueParameters">Sql parameters.</param>
        /// <returns></returns> 
        public int ExecuteNonQuery(string cmdText, params object[] keyValueParameters)
        {
            IDbDataParameter[] patameters = (keyValueParameters == null || keyValueParameters.Length == 0) ? null : GetParam(keyValueParameters);
            return ExecuteCommandNonQuery(cmdText, patameters, CommandType.Text);
        }
        */

        //public void ExecuteReader()
        //{

        //    //SQLiteDataReader reader = command.ExecuteReader();
        //    //while (reader.Read())
        //    //    Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);
        //}

        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static IDbDataParameter[] GetParam(params object[] keyValueParameters)
        {

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new SQLiteParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
            }

            return list.ToArray();
        }

        #region IDbContext

        public Nistec.Data.Factory.IDbCmd NewCmd()
        {
            throw new NotImplementedException();
            //ValidateConnectionSettings();
            //return DbFactory.Create(ConnectionString, Provider);
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

        //internal void SetLocalizer(IEntity instance, string resource)
        //{
        //    if (instance != null)
        //    {
        //        m_EntityLang = new DynamicEntityLocalizer(instance, resource);
        //    }
        //}



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

        #region internal
        internal EntityDbContext GetEntityDb(object instance, string mappingName)
        {
            EntityKeys keys = new EntityKeys(instance);// EntityPropertyBuilder.GetEntityPrimaryKey(instance);
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
            DataTable dt = ExecuteCommand<DataTable>(cmdText, parameters);
            if (dt == null)
                return null;
            return dt.EntityList<T>();

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

        /// <summary>
        /// Save entity to db update or insert using <see cref="GenericEntity"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int EntityUpsert(GenericEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("EntityUpsert.entity");
            }

            using (EntityDbContext edb = GetEntityDb(entity))
            {
                return entity.SaveChanges(UpdateCommandType.Upsert, edb);
            }
        }

 
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
                }
            }
            finally
            {
                this.OwnsConnection = false;
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
        public int EntityItemDelete(string mappingName, params object[] nameValueParameters)
        {
            ValidateConnectionSettings();
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityDelete.mappingName");
            }
            if (nameValueParameters == null || nameValueParameters.Length == 0)
            {
                throw new ArgumentNullException("EntityDelete.nameValueParameters");
            }
            string commandText = SqlFormatter.DeleteCommand(mappingName, nameValueParameters);
            return ExecuteCommandNonQuery(commandText, DataParameter.GetSql(nameValueParameters));
        }

        /// <summary>
        /// Entity delete command by mapping name and name value parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameValueParameters"></param>
        /// <returns></returns>
        public int EntityDelete<T>(params object[] nameValueParameters) where T : IEntityItem
        {
            ValidateConnectionSettings();
            string mappingName = EntityMappingAttribute.Mapping<T>();

            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("EntityDelete.mappingName");
            }
            if (nameValueParameters == null || nameValueParameters.Length == 0)
            {
                throw new ArgumentNullException("EntityDelete.nameValueParameters");
            }
            string commandText = SqlFormatter.DeleteCommand(mappingName, nameValueParameters);
            return ExecuteCommandNonQuery(commandText, DataParameter.GetSql(nameValueParameters));
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
        public int ExecuteCommand(string commandText, CommandType commandType, params object[] nameValueParameters)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException("ExecuteCommand.commandText");
            }

            return ExecuteCommandNonQuery(commandText, DataParameter.GetSql(nameValueParameters), commandType);
        }

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

        #endregion

    }
}
