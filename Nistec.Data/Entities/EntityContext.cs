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
using System.Collections; 
using System.Reflection;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Nistec.Data.Factory;
using System.IO;
using Nistec.Runtime;
using Nistec.IO;
using Nistec.Generic;
using Nistec.Serialization;

namespace Nistec.Data.Entities
{

    public class EntityContext<Dbc, T>
        where Dbc : IDbContext
        where T : IEntityItem
    {

        public static IList<T> GetEntityList(params object[] keyvalueParameters)
        {
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityList<T>(keyvalueParameters);
            }
            //return DbContext.EntityList<Dbc, T>(keyvalueParameters);
        }
        public static IList<T> ExecEntityList(params object[] keyvalueParameters)
        {
            using (var Db = DbContext.Get<Dbc>())
            {
                return Db.EntityProcList<T>(keyvalueParameters);
            }
        }

        #region ctor
        public EntityContext()
        {

        }
        public EntityContext(T entity)
        {
            Current = entity;
        }
        public EntityContext(params object[] keyvalueParameters)
        {
            if (EntityItem.IsEntityItem<T>(keyvalueParameters))
                Set(Get((T)keyvalueParameters[0]));
            else
                Current = Get(keyvalueParameters);
        }
        #endregion

        #region settings
        public void Set(System.Collections.Specialized.NameValueCollection form)
        {
            Current = EntityContext.Create<T>(form);
        }
        public void Set(T entity)
        {
            Current = entity;
        }

        public void SetParams(params object[] keyvalueParameters)
        {
            Current = Get(keyvalueParameters);
        }
        
        //public virtual void Validate(UpdateCommandType commandType = UpdateCommandType.Update)
        //{
        //    EntityValidator.Validate(Current);
        //}
        public virtual void Validate(ProcedureType commandType = ProcedureType.Update)
        {
            EntityValidator.Validate(Current);
        }
        public virtual EntityValidator Validate(string title, string lang, object[] args = null)
        {
            return EntityValidator.ValidateEntity(Current, title, lang, args);
        }

        protected bool EnableCache
        {
            get;
            set;
        }
        protected string CacheKey
        {
            get;
            set;
        }
        public T Current
        {
            get;
            protected set;// { return GenericTypes.Cast<T>(this);}
        }
        public string EntityName
        {
            get { return EntityMappingAttribute.Name<T>(); }
        }
        public string MappingName
        {
            get { return EntityMappingAttribute.Mapping<T>(); }
        }
        //protected object[] GetKeyFields(bool useOrder = false)
        //{
        //    return EntityPropertyBuilder.GetEntityKeyValueParameters<T>(Current, useOrder);
        //}
        protected object[] GetKeyValueFields(bool useKeyTypesOnly)
        {
            return EntityPropertyBuilder.GetEntityKeyValueByOrder(Current, useKeyTypesOnly);
        }
        #endregion

        #region Get Lists

        public virtual IList<T> ExecList(params object[] keyvalueParameters)
        {
            using (var Db = DbContext.Get<Dbc>())
            {
                return Db.EntityProcList<T>(keyvalueParameters);
            }
        }

        public virtual string ExecListJson(params object[] keyvalueParameters)
        {
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.GetList);
            if (proc == null)
            {
                throw new InvalidOperationException("Invalid get list by StoredProcedure!");
            }
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.ExecuteJson(proc, keyvalueParameters);
            }
        }
        public virtual IList<T> ViewList(params object[] keyvalueParameters)
        {
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityList<T>(keyvalueParameters);
            }
        }

        public virtual string ViewListJson(params object[] keyvalueParameters)
        {
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityListJson<T>(keyvalueParameters);
            }
        }

        public virtual IList<T> ExecOrViewList(params object[] keyvalueParameters)
        {
            //int ttl = 3;
            //string key = DbContextCache.GetKey<TaskComment>(Settings.ProjectName, EntityCacheGroups.Task, 0, userId);
            //return DbContextCache.EntityList<DbTeam, TaskComment>(key, ttl, new object[] { "Task_Id", taskId });

            //var list = TryFromCach(keyvalueParameters);

            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.GetList);
            if (proc != null)
            {
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    return Db.ExecuteList<T>(proc, keyvalueParameters);
                }
            }
            else
            {
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    return Db.EntityList<T>(keyvalueParameters);
                }
            }
        }
        public virtual string ExecOrViewListJson(params object[] keyvalueParameters)
        {
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.GetList);
            if (proc != null)
            {
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    return Db.ExecuteJson(proc, keyvalueParameters);
                }
            }
            else
            {
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    return Db.EntityListJson<T>(keyvalueParameters);
                }
            }
        }

        #endregion

        #region Get

        public virtual T ExecOrGet(params object[] keyvalueParameters)
        {
            //int ttl = 3;
            //string key = DbContextCache.GetKey<TaskComment>(Settings.ProjectName, EntityCacheGroups.Task, 0, userId);
            //return DbContextCache.EntityList<DbTeam, TaskComment>(key, ttl, new object[] { "Task_Id", taskId });

            //var list = TryFromCach(keyvalueParameters);

            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.GetRecord);
            if (proc != null)
            {
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    return Db.ExecuteSingle<T>(proc, keyvalueParameters);
                }
            }
            else
            {
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    return Db.EntityGet<T>(keyvalueParameters);
                }
            }
        }
        public virtual T ExecGet(params object[] keyvalueParameters)
        {
            //int ttl = 3;
            //string key = DbContextCache.GetKey<TaskComment>(Settings.ProjectName, EntityCacheGroups.Task, 0, userId);
            //return DbContextCache.EntityList<DbTeam, TaskComment>(key, ttl, new object[] { "Task_Id", taskId });

            using (var Db = DbContext.Get<Dbc>())
            {
                return Db.EntityProcGet<T>(keyvalueParameters);
            }
        }

        public T Get(params object[] keyvalueParameters)
        {
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Current = Db.EntityGet<T>(keyvalueParameters);
            }
        }

        public string GetJson(params object[] keyvalueParameters)
        {
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityGetJson<T>(keyvalueParameters);
            }
        }
        public V GetScalar<V>(string field, V defaultValue, params object[] keyvalueParameters)
        {
            var sql=SqlFormatter.CreateCommandText(1,field,MappingName,keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.QueryScalar<V>(sql, defaultValue, keyvalueParameters);
            }
        }
        public static V GetScalar<V>(string field, string mappingName, V defaultValue, params object[] keyvalueParameters)
        {
            var sql = SqlFormatter.CreateCommandText(1,field, mappingName, keyvalueParameters);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.QueryScalar<V>(sql, defaultValue, keyvalueParameters);
            }
        }
        #endregion

        #region Save
        public int Upsert(GenericEntity entity)
        {
            //Validate(UpdateCommandType.Update);
            int res = entity.Upsert<Dbc>();
            OnChanged(ProcedureType.Upsert);
            return res;
        }
        /// <summary>
        /// Save entity changes to update or insert if not exists.(EntitySaveChanges)
        /// </summary>
        /// <returns></returns>
        public EntityCommandResult SaveChanges(bool InsertIfNotExists = true)
        {
            Validate(ProcedureType.Upsert);
            EntityCommandResult res = null;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.EntitySaveChanges<T>(Current, InsertIfNotExists);
            }
            OnChanged(ProcedureType.Upsert);
            return res ?? EntityCommandResult.Empty;
        }
        public EntityCommandResult Save()
        {
            Validate(ProcedureType.Upsert);
            EntityCommandResult res = null;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.EntitySave<T>(Current);
            }
            OnChanged(ProcedureType.Upsert);
            return res ?? EntityCommandResult.Empty;
        }

        public EntityCommandResult Insert()
        {
            Validate(ProcedureType.Insert);
            EntityCommandResult res = null;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.EntityInsert<T>(Current);
            }
            OnChanged(ProcedureType.Insert);
            return res ?? EntityCommandResult.Empty;
        }

        //public EntityCommandResult Upsert(UpsertType commandType = UpsertType.Upsert, ReturnValueType returnType = ReturnValueType.ReturnValue, params object[] keyvalueParameters)
        //{
        //    switch (commandType)
        //    {
        //        case UpsertType.Insert:
        //            if (returnType == ReturnValueType.ReturnValue)
        //                return Insert(keyvalueParameters);
        //            else
        //                return InsertOutput(keyvalueParameters);
        //        case UpsertType.Update:
        //            if (returnType == ReturnValueType.ReturnValue)
        //                return Update(keyvalueParameters);
        //            else
        //                return UpdateOutput(keyvalueParameters);
        //        case UpsertType.Upsert:
        //            if (returnType == ReturnValueType.ReturnValue)
        //                return Upsert(keyvalueParameters);
        //            else
        //                return UpsertOutput(keyvalueParameters);
        //        default:
        //            return null;
        //    }
        //}

        public EntityCommandResult Upsert(UpsertType commandType = UpsertType.Upsert, ReturnValueType returnType = ReturnValueType.ReturnValue, params object[] keyvalueParameters)
        {
            ProcedureType updateCommandType = (ProcedureType)(int)commandType;

            EntityCommandResult res = null;
            var proc = EntityMappingAttribute.Proc<T>(updateCommandType);
            if (proc != null)
            {
                if (keyvalueParameters == null || keyvalueParameters.Length==0)
                {
                    Validate(updateCommandType);
                    keyvalueParameters = GetKeyValueFields(false);
                }

                if (returnType == ReturnValueType.OutputParameters)
                {
                    res = ExecuteOutput(ProcedureType.Upsert, proc, keyvalueParameters);
                }
                else
                {
                    //if(Current==null)
                    //{
                    //    throw new Exception("Current entity not set");
                    //}
                    var result = ExecuteReturnValue(ProcedureType.Upsert, proc, -1, keyvalueParameters);
                    var p = DataProperties.GetEntityProperty(typeof(T), EntityPropertyType.Identity);
                    string entityField = (p == null) ? "ReturnValue" : p.Attribute.Column;
                    res = new EntityCommandResult(1, result, entityField);
                }
            }
            else
            {
                Validate(updateCommandType);
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntitySaveChanges<T>(Current, commandType== UpsertType.Upsert);
                }
            }
            OnChanged(updateCommandType);

            return res ?? EntityCommandResult.Empty;
        }
       
        #endregion

        #region Exec proc  virtual
        public virtual int ExecuteNonQuery(ProcedureType procType,string procName, params object[] nameValueParameters)
        {
            //Validate(UpdateCommandType.Upsert);
            int res = 0;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.ExecuteNonQuery(procName, nameValueParameters);
            }
            OnChanged(procType);
            return res;
        }
        public virtual int ExecuteReturnValue(ProcedureType procType, string procName, int returnIfNull, params object[] nameValueParameters)
        {
            //Validate(UpdateCommandType.Upsert);
            int res = 0;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.ExecuteReturnValue(procName, returnIfNull, nameValueParameters);
            }
            OnChanged(procType);
            return res;
        }
        public virtual EntityCommandResult ExecuteOutput(ProcedureType procType, string procName, params object[] nameValueParameters)
        {
            //Validate(UpdateCommandType.Upsert);
            EntityCommandResult res = null;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.ExecuteOutput(procName, nameValueParameters);
            }
            OnChanged(procType);
            return res;
        }
        public virtual T ExecuteSingle(string procName, params object[] nameValueParameters)
        {
            T res = default(T);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.ExecuteSingle<T>(procName, nameValueParameters);
            }
            return res;
        }
        public virtual IList<T> ExecuteList(string procName, params object[] nameValueParameters)
        {
            IList<T> res = null;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.ExecuteList<T>(procName, nameValueParameters);
            }
            return res;
        }
        #endregion

        #region protected
        protected EntityCommandResult UpsertReturnValue(params object[] keyvalueParameters)
        {

            EntityCommandResult res = null;
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.Upsert);
            if (proc != null)
            {
                if (keyvalueParameters == null || keyvalueParameters.Length == 0)
                {
                    Validate(ProcedureType.Upsert);
                    keyvalueParameters = GetKeyValueFields(false);
                }
                var result = ExecuteReturnValue(ProcedureType.Upsert, proc, -1, keyvalueParameters);
                var p = DataProperties.GetEntityProperty(Current.GetType(), EntityPropertyType.Identity);
                string entityField = (p == null) ? "ReturnValue" : p.Attribute.Column;
                res = new EntityCommandResult(1, result, entityField);
            }
            else
            {
                Validate(ProcedureType.Upsert);
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntitySaveChanges<T>(Current,true);
                }
            }
            OnChanged(ProcedureType.Upsert);
            return res ?? EntityCommandResult.Empty;
        }
        protected EntityCommandResult UpsertOutput(params object[] keyvalueParameters)
        {

            EntityCommandResult res = null;
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.Upsert);
            if (proc != null)
            {
                if (keyvalueParameters == null || keyvalueParameters.Length == 0)
                {
                    Validate(ProcedureType.Upsert);
                    keyvalueParameters = GetKeyValueFields(false);
                }
                res = ExecuteOutput(ProcedureType.Upsert, proc, keyvalueParameters);
            }
            else
            {
                Validate(ProcedureType.Upsert);
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntitySaveChanges<T>(Current, true);
                }
            }
            OnChanged(ProcedureType.Upsert);
            return res ?? EntityCommandResult.Empty;
        }
        protected EntityCommandResult InsertOutput(params object[] keyvalueParameters)
        {
            
            EntityCommandResult res = null;
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.Insert);
            if (proc != null)
            {
                if (keyvalueParameters == null || keyvalueParameters.Length == 0)
                {
                    Validate(ProcedureType.Insert);
                    keyvalueParameters = GetKeyValueFields(false);
                }
                res = ExecuteOutput(ProcedureType.Insert, proc, keyvalueParameters);
            }
            else
            {
                Validate(ProcedureType.Insert);
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntityInsert<T>(Current);
                }
            }
            OnChanged(ProcedureType.Insert);
            return res ?? EntityCommandResult.Empty;
        }
        protected EntityCommandResult InsertReturnValue(params object[] keyvalueParameters)
        {
           
            EntityCommandResult res = null;
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.Insert);
            if (proc != null)
            {
                if (keyvalueParameters == null || keyvalueParameters.Length == 0)
                {
                    Validate(ProcedureType.Insert);
                    keyvalueParameters = GetKeyValueFields(false);
                }
                var result = ExecuteReturnValue(ProcedureType.Insert, proc, -1, keyvalueParameters);
                var p = DataProperties.GetEntityProperty(Current.GetType(), EntityPropertyType.Identity);
                string entityField = (p == null) ? "ReturnValue" : p.Attribute.Column;
                res = new EntityCommandResult(1, result, entityField);
            }
            else
            {
                Validate(ProcedureType.Insert);
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntityInsert<T>(Current);
                }
            }
            OnChanged(ProcedureType.Insert);
            return res ?? EntityCommandResult.Empty;
        }
        protected EntityCommandResult UpdateOutput(params object[] keyvalueParameters)
        {
            
            EntityCommandResult res = null;
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.Update);
            if (proc != null)
            {
                if (keyvalueParameters == null || keyvalueParameters.Length == 0)
                {
                    Validate(ProcedureType.Update);
                    keyvalueParameters = GetKeyValueFields(false);
                }
                res = ExecuteOutput(ProcedureType.Update, proc, keyvalueParameters);
            }
            else
            {
                Validate(ProcedureType.Update);
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntitySaveChanges<T>(Current,false);
                }
            }
            OnChanged(ProcedureType.Update);
            return res ?? EntityCommandResult.Empty;
        }
        protected EntityCommandResult UpdateReturnValue(params object[] keyvalueParameters)
        {
            
            EntityCommandResult res = null;
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.Update);
            if (proc != null)
            {
                if (keyvalueParameters == null || keyvalueParameters.Length == 0)
                {
                    Validate(ProcedureType.Update);
                    keyvalueParameters = GetKeyValueFields(false);
                }
                var result = ExecuteReturnValue(ProcedureType.Update, proc, -1, keyvalueParameters);
                //var p = DataProperties.GetEntityProperty(Current.GetType(), EntityPropertyType.Identity);
                res = new EntityCommandResult(1, result, "ReturnValue");
            }
            else
            {
                Validate(ProcedureType.Update);
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntitySaveChanges<T>(Current,false);
                }
            }
            OnChanged(ProcedureType.Insert);
            return res ?? EntityCommandResult.Empty;
        }

        protected int DoCommandNoneQuery(string commandText, ProcedureType procedureType, params object[] keyvalueParameters)
        {

            int affectedRecord = 0;
            using (var Db = DbContext.Get<Dbc>())
            {
                affectedRecord = Db.ExecuteCommand(commandText, CommandType.Text, keyvalueParameters);
            }

            OnChanged(procedureType);
            return affectedRecord;
        }
        protected Result DoCommand<Result>(string commandText, ProcedureType procedureType, CommandType commandType, params object[] keyvalueParameters)
        {

            Result result = default(Result);
            using (var Db = DbContext.Get<Dbc>())
            {
                result = Db.ExecuteCommand<Result>(commandText, commandType, keyvalueParameters);
            }
            if (!(procedureType == ProcedureType.GetList || procedureType == ProcedureType.GetRecord))
                OnChanged(procedureType);
            return result;
        }

        #endregion

        //public int Update()
        //{
        //    Validate(UpdateCommandType.Update);
        //    int res = 0;
        //    using (IDbContext Db = DbContext.Create<Dbc>())
        //    {
        //        res = Db.EntityUpdate<T>(Current);
        //    }
        //    OnChaned(UpdateCommandType.Update);
        //    return res;
        //}

        //public int Delete()
        //{
        //    Validate(UpdateCommandType.Delete);
        //    var keyvalueParameters = GetKeyValueFields(true);

        //    int res = 0;
        //    var proc = EntityMappingAttribute.Proc<T>(UpdateCommandType.Delete);
        //    if (proc != null)
        //        res = ExecuteNonQuery(proc, keyvalueParameters);
        //    else
        //    {
        //        using (IDbContext Db = DbContext.Create<Dbc>())
        //        {
        //            res = Db.EntityDelete<T>(keyvalueParameters);
        //        }
        //    }
        //    OnChanged(UpdateCommandType.Delete);
        //    return res;
        //}

        public int DeleteReturnValue(int returnIfNull, params object[] keyvalueParameters)
        {
            if (keyvalueParameters == null || keyvalueParameters.Length == 0)
            {
                Validate(ProcedureType.Delete);
                keyvalueParameters = GetKeyValueFields(true);
            }

            var procName = EntityMappingAttribute.Proc<T>(ProcedureType.Delete);
            if(procName==null)
            {
                throw new EntityException("Invalid Entity ProcName");
            }
            int res = returnIfNull;
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                res = Db.ExecuteReturnValue(procName, returnIfNull, keyvalueParameters);
            }
            OnChanged(ProcedureType.Delete);
            return res;
        }

        public int Delete(params object[] keyvalueParameters)
        {
            if (keyvalueParameters == null || keyvalueParameters.Length == 0)
            {
                Validate(ProcedureType.Delete);
                keyvalueParameters = GetKeyValueFields(true);
            }

            int res = 0;
            var proc = EntityMappingAttribute.Proc<T>(ProcedureType.Delete);
            if (proc != null)
                res = ExecuteNonQuery(ProcedureType.Delete, proc, keyvalueParameters);
            else
            {
                using (IDbContext Db = DbContext.Create<Dbc>())
                {
                    res = Db.EntityDelete<T>(keyvalueParameters);
                }
            }
            OnChanged(ProcedureType.Delete);
            return res;
        }
 
        //protected virtual IList<T> TryFromCach(object[] keyvalueParameters)
        //{
        //    return null;
        //}
        protected virtual void OnChanged(ProcedureType commandType)
        {

        }
    }

    /// <summary>
    /// Represent Entity Context generic class 
	/// </summary>
    [Serializable]
    public class EntityContext<T> : EntityContext, IEntity<T>
    {
        #region ctor

        /// <summary>
        /// Initialize a new instance of EntityContext with specified <see cref="EntityDbContext"/>
        /// </summary>
        internal EntityContext(EntityDbContext db, T instance)
            : base(db)
        {
            m_EntityDbOwner = false;
            _instance = instance;
            _Data = EntityPropertyBuilder.CreateGenericRecord(instance, true);//.BuildEntityContext(instance, false);
            isEmpty = _Data == null || _Data.IsEmpty;
        }

       
        /// <summary>
        /// Initialize a new instance of <see cref="EntityContext"/> using array of entity keys
        /// </summary>
        public EntityContext()
            : base()
        {
            _instance = ActivatorUtil.CreateInstance<T>();
        }
              

        /// <summary>
        /// Initialize a new instance of <see cref="EntityContext"/> using array of entity keys
        /// </summary>
        /// <param name="keys"></param>
        //[Obsolete("Use KeySet Insted")]
        public EntityContext(params object[] keys)
            : base()
        {
            if (keys == null)
            {
                throw new ArgumentNullException("EntityContext<>.keys");
            }
            GenericRecord gv = new GenericRecord();
            _instance = ActivatorUtil.CreateInstance<T>();

            if (HasConnection())
            {
                _Data = EntityDb.QueryEntity<GenericRecord>(keys);
                EntityPropertyBuilder.SetEntityContext(_instance, _Data);
                isEmpty = _Data == null || _Data.IsEmpty;
            }
        }

 
        /// <summary>
        /// Initialize a new instance of <see cref="EntityContext"/> using <see cref="GenericEntity"/>.
        /// </summary>
        /// <param name="ge"></param>
        public EntityContext(GenericEntity ge)
            : base()
        {
            if (ge == null)
            {
                throw new ArgumentNullException("EntityContext<>.ge");
            }
            
            Set(ge.Record);

        }

        /// <summary>
        /// Initialize a new instance of <see cref="EntityContext"/> using entity instance
        /// </summary>
        /// <param name="instance"></param>
        public EntityContext(T instance)
            : base()
        {
            _instance = instance;
            _Data = EntityPropertyBuilder.CreateGenericRecord(instance,true);//.BuildEntityContext(instance, false);

            isEmpty = _Data == null || _Data.IsEmpty;

        }

  
        public EntityContext(Stream stream, IBinaryStreamer streamer)
            : base()
        {
            if (stream == null)
            {
                throw new ArgumentNullException("EntityContext<>.stream");
            }
            _instance = ActivatorUtil.CreateInstance<T>();
            EntityRead(stream, streamer);
        }

        #endregion

        #region Set
        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/>
        /// </summary>
        public void SetChanged()
        {
            if (_instance == null)
            {
                throw new EntityException("The current entity instance is null, SetInstance first."); 
            }

            _Data = EntityPropertyBuilder.CreateGenericRecord(_instance,true);//, false);
            isEmpty = _Data == null || _Data.IsEmpty;
        }

        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/>
        /// </summary>
        /// <param name="instance"></param>
        public void Set(T instance)
        {
            _instance = instance;
            if (_instance == null)
            {
                throw new EntityException("The current entity instance is null.");
            }
            if (_Data == null)
            {
                _Data = EntityPropertyBuilder.CreateGenericRecord(_instance, true);
            }
            isEmpty = _Data == null || _Data.IsEmpty;
        }

        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="setChanges"></param>
        internal void SetChanged(T instance, bool setChanges)
        {
            _instance = instance;
            if (_instance == null)
            {
                throw new EntityException("The current entity instance is null, SetInstance first.");
            }
            if (setChanges)
            {
                _Data = EntityPropertyBuilder.CreateGenericRecord(_instance, true);//, false);
            }
            isEmpty = _Data == null || _Data.IsEmpty;
        }

    
        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/> using EntityMap and EntityKeys attributes.
        /// </summary>
        /// <param name="keyValueParameters">keys Parameters.</param>
        public void SetByParam(params object[] keyValueParameters)
        {
            if (HasConnection())
            {
                _Data = EntityDb.QuerySingle<GenericRecord>(keyValueParameters);
                EntityPropertyBuilder.SetEntityContext(_instance, _Data);
                isEmpty = _Data == null || _Data.IsEmpty;
            }
        }

        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/> using EntityMap and EntityKeys attributes.
        /// </summary>
        /// <param name="keys">keys Parameters.</param>
        public void SetEntity(params object[] keys)
        {
            if (HasConnection())
            {
                _Data = EntityDb.QueryEntity<GenericRecord>(keys);
                EntityPropertyBuilder.SetEntityContext(_instance, _Data);
                isEmpty = _Data == null || _Data.IsEmpty;
            }
        }

 
        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/> using <see cref="DataFilter"/> filter.
        /// </summary>
        /// <param name="filter"></param>
        public void Set(DataFilter filter)
        {
            if (HasConnection())
            {
                _Data = EntityDb.DoCommand<GenericRecord>(filter);
                EntityPropertyBuilder.SetEntityContext(_instance, _Data);
                isEmpty = _Data == null || _Data.IsEmpty;
            }
        }

        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/> using command with a <see cref="DataParameter"/> key Value Parameters.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">Array of key value.</param>
        /// <param name="cmdType">Sql command.</param>
         public void Set(string cmdText, IDbDataParameter[] parameters, CommandType cmdType)
        {
            //Init(cmdText, keyValueParameters);

            if (HasConnection())
            {
                _Data = EntityDb.DoCommand<GenericRecord>(cmdText, parameters, cmdType);
                EntityPropertyBuilder.SetEntityContext(_instance, _Data);
                isEmpty = _Data == null || _Data.IsEmpty;
            }
        }

         /// <summary>
         /// Set the current instance of <see cref="EntityContext"/> using <see cref="GenericRecord"/> source.
         /// </summary>
         /// <param name="gr"></param>
         public void Set(GenericRecord gr)//rcd
         {
             if (gr == null)
             {
                 throw new ArgumentNullException("EntityContext<>.Set.gr");
             }
             _Data = gr;
             if (_instance == null)
             {
                 _instance = ActivatorUtil.CreateInstance<T>();
             }

             isEmpty = _Data == null || _Data.IsEmpty;
             if (_instance != null && isEmpty == false)
             {
                 EntityPropertyBuilder.SetEntityContext(_instance, _Data);
             }

             //EntityPropertyBuilder.SetEntityContext(_instance, _Data);
             //isEmpty = _Data == null || _Data.IsEmpty;

         }

        #endregion

        #region Create

  
        /// <summary>
        ///  Set the current instance of <see cref="EntityContext"/> using EntityMap and <see cref="DataFilter"/> filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public T Create(DataFilter filter)
        {
            Set(filter);
            return _instance;
        }

 
        #endregion

        #region SaveChanges

        public override int SaveChanges()
        {
            return SaveChanges(UpdateCommandType.Update);
        }

        /// <summary>
        /// Save all Changes to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public virtual int SaveChanges(T instance, UpdateCommandType cmdType = UpdateCommandType.Update)
        {
            SetChanged(instance,false);
            return SaveChanges(cmdType);
        }

        /// <summary>
        /// Save all Changes by <see cref="UpdateCommandType"/> specific command to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        /// <exception cref="EntityException"></exception>
        /// <exception cref="DalException"></exception>
        public override int   SaveChanges(UpdateCommandType commandType)
        {
            if (_Data == null && commandType == UpdateCommandType.Insert)
            {
                _Data = EntityPropertyBuilder.CreateGenericRecord(_instance, true);//.BuildEntityContext(_instance, false);
                isEmpty = _Data == null || _Data.IsEmpty;
            }
            ValidateUpdate();

            if (commandType == UpdateCommandType.Delete)
            {
                return EntityCommandBuilder.DeleteCommand(_instance, EntityDb);
            }

            _FieldsChanges = new EntityFieldsChanges(this, _instance, commandType,true);

            var result = _FieldsChanges.SaveChanges();
            int res = EntityCommandResult.GetAffectedRecords(result);
            if (res > 0)
            {
                _Data = _FieldsChanges.Data;
            }
            _FieldsChanges = null;
            return res;
        }

  
         #endregion

        #region Entity properties

        T _instance;
        /// <summary>
        /// Get the current entity
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public T Entity
        {
            get { return _instance; }
        }

 
        #endregion

        #region IEntityDictionary
        [EntityProperty(EntityPropertyType.NA)]
       public override Type EntityType
        {
            get { return typeof(T); }
        }

        public override void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            streamer.WriteValue(Entity);

        }

        public override void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            T entity = ((BinaryStreamer)streamer).ReadAnyEntity<T>();


            //T entity = streamer.ReadValue<T>();
            if (entity == null)
            {
                return;
                //TODO:
                //throw new Exception("EntityContext could not read stream to entity");
            }

            Set(entity);
        }

        public void EntityConvert(Stream stream)
        {
            IBinaryStreamer streamer = new BinaryStreamer(stream);
            object o= streamer.ReadValue();
            if (o == null)
            {
                return;
            }
            Type typ = o.GetType();
            if (typ == typeof(GenericEntity))
            {
                Set(((GenericEntity)o).Record);
            }
            else if (typ == typeof(GenericRecord))
            {
                Set((GenericRecord)o);
            }
            else if (typ == typeof(GenericRecord))
            {
                Set((GenericRecord)o);
            }
            else if (typ == typeof(T))
            {
                Set((T)o);
            }
            else if (SerializeTools.IsAssignableFromDictionary(typ))
            {
                Set(new GenericRecord((IDictionary)o));
            }
        }
       
        #endregion

        #region List
        

        /// <summary>
        /// Create Entity collection using Entity Keys
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IList<T> EntityList()
        {
            if (EntityDb.HasConnection)
            {
                return EntityDb.Query<T>(null);
            }

            return null;
        }

        /// <summary>
        /// Create Entity collection using Entity Keys
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IList<T> EntityList(params object[] keyValueParameters)
        {
            if (EntityDb.HasConnection)
            {
                //DataTable dt = EntityDb.DoCommand<DataTable>(keys);
                //return dt.EntityList<T>();

                return EntityDb.Query<T>(keyValueParameters);
            }

            return null;
        }

    
        /// <summary>
        /// Create Entity collection using <see cref="DataFilter"/> filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public List<T> EntityList(DataFilter filter)
        {
            if (EntityDb.HasConnection)
            {
                DataTable dt = EntityDb.DoCommand<DataTable>(filter);
                return dt.EntityList<T>();
            }

            return null;
        }
  

        #endregion

        #region Json List
    
        /// <summary>
        /// Create Entity collection as json using <see cref="DataFilter"/> filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public JsonResults EntityListToJson(DataFilter filter)
        {
            if (EntityDb.HasConnection)
            {
                return EntityDb.DoCommand<JsonResults>(filter);
            }
            return null;
        }
 
        #endregion
   
        #region override

        protected override void EntityBind()
        {
            
        }

        protected override void SetEntityContext()
        {
            if (_Data != null && _Data.Count > 0)
            {
                EntityPropertyBuilder.SetEntityContext(_instance, _Data);
                isEmpty = false;
            }
        }

        /// <summary>
        /// Create new instance off <see cref="EntityProperties"/>
        /// </summary>
        internal protected override void CreateEntityAttributes()
        {
            if (m_ControlAttributes == null)
            {
                m_ControlAttributes = new EntityProperties(_instance, EntityDb);
            }
        }

        /// <summary>
        /// Create new instance of <see cref="EntityFieldsChanges"/>
        /// </summary>
        /// <param name="initChanges"></param>
        /// <returns></returns>
        protected override EntityFieldsChanges CreateFieldsChanges(bool initChanges)
        {
            return new EntityFieldsChanges(this, _instance, UpdateCommandType.Update, initChanges);
        }

        /// <summary>
        /// Get ActiveProperties that implement <see cref="EntityPropertyAttribute"/>
        /// </summary>
        /// <returns></returns>
        protected override PropertyInfo[] ActiveProperties()
        {
            return EntityExtension.GetEntityProperties(_instance, true, false);
        }
        #endregion
    }

  	/// <summary>
    /// Represent Entity class 
	/// </summary>
    [Serializable]
    public abstract class EntityContext : IEntity, IEntityFields, IEntityDictionary, IDisposable
    {
        #region static <T>

  
        /// <summary>
        /// Get Entity using <see cref="EntityAttribute"/> attribute and <see cref="DataFilter"/> filter. 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CustomAttributeFormatException"></exception>
        public static T Get<T>(EntityAttribute attribute, DataFilter filter) where T : IEntityItem
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("CreateEntity.attribute");
            }
            var db= attribute.Get();
            if (db == null)
            {
                throw new CustomAttributeFormatException("CreateEntity.attribute.EntityDbContext");
            }
            return db.DoCommand<T>(filter);
        }

        /// <summary>
        /// Get Entity using <see cref="EntityAttribute"/> attribute and keys. 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CustomAttributeFormatException"></exception>
        [Obsolete("Use KeySet insted")]
        public static T Get<T>(EntityAttribute attribute, object[] keys) where T : IEntityItem
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("CreateEntity.attribute");
            }
            var db = attribute.Get();
            if (db == null)
            {
                throw new CustomAttributeFormatException("CreateEntity.attribute.EntityDbContext");
            }
            return db.ToEntity<T>(keys);
        }

        /// <summary>
        /// Get Entity using <see cref="EntityAttribute"/> attribute and keys. 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CustomAttributeFormatException"></exception>
        public static T Get<T>(EntityAttribute attribute, KeySet keys) where T : IEntityItem
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("CreateEntity.attribute");
            }
            var db = attribute.Get();
            if (db == null)
            {
                throw new CustomAttributeFormatException("CreateEntity.attribute.EntityDbContext");
            }
            return db.ToEntity<T>(keys);
        }

        /// <summary>
        /// Create Entity from <see cref="GenericRecord"/>
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Get<T>(GenericRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("CreateEntity.record");
            }
            using (EntityContext<T> context = new EntityContext<T>())
            {
                context.Set(record);
                return context.Entity;
            }
        }

      
        public static T Create<T>(System.Collections.Specialized.NameValueCollection form)
        {
            return EntityExtension.Create<T>(form);

            //T instance = ActivatorUtil.CreateInstance<T>();

            //var props = DataProperties.GetEntityProperties(typeof(T), true);
            //foreach (var pa in props)
            //{
            //    PropertyInfo property = pa.Property;
            //    EntityPropertyAttribute attr = pa.Attribute;

            //    if (!property.CanRead)
            //    {
            //        continue;
            //    }

            //    if (attr != null)
            //    {
            //        if (attr.ParameterType == EntityPropertyType.NA)
            //        {
            //            continue;
            //        }
            //        if (attr.ParameterType == EntityPropertyType.View)
            //        {
            //            continue;
            //        }
            //        if (property.CanWrite)
            //        {
            //            string field = attr.GetColumn(property.Name);
            //             //if (attr.ParameterType == EntityPropertyType.Optional)
            //             //{
            //             //    Console.WriteLine("Optional");
            //             //}
            //            object value = form[field];
            //            if (value == null)
            //            {
            //                if (attr.ParameterType == EntityPropertyType.Optional)
            //                    continue;
            //                value = attr.AsNull;
            //            }
            //            property.SetValue(instance, Types.ChangeType(value, property.PropertyType), null);
            //        }
            //    }
            //}


            //return instance;
        }

        #endregion
    
        #region Dispose

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (m_EntityDbOwner)
                    {
                        if (m_EntityDb != null)
                        {
                            m_EntityDb.Dispose();
                            m_EntityDb = null;
                        }
                    }
                    m_ControlAttributes = null;
                    _Data = null;
                }

                //DisposeInner(disposing);
                //dispose unmanaged resources here.
                disposed = true;
            }
        }

      
        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // take this object off the finalization queue     
            GC.SuppressFinalize(this);
        }

        ~EntityContext()
        {
            Dispose(false);
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize a new instance of EntityContext with specified <see cref="EntityDbContext"/>
        /// </summary>
        internal EntityContext(EntityDbContext db)
        {
            EntityDb = db;

            EntityBind();

        }
        
        /// <summary>
        /// ctor for serialization
        /// </summary>
        public EntityContext()
        {
            //GenericRecord gv = new GenericRecord();

            EntityDb = EntityDbContext.Get(this, EntityLocalizer.DefaultCulture);

            EntityBind();

        }

        /// <summary>
        /// Initialize a new instance of EntityContext with specified culture
        /// </summary>
        /// <param name="culture"></param>
        protected EntityContext(CultureInfo culture)
        {
            GenericRecord gv = new GenericRecord();

            EntityDb = EntityDbContext.Get(this, culture);

            EntityBind();

        }

        /// <summary>
        /// Initialize a new instance of ActiveEntity using array of entity keys
        /// </summary>
        /// <param name="keys"></param>
        protected EntityContext(params object[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("EntityContext.keys");
            }
            GenericRecord gv = new GenericRecord();

            EntityDb = EntityDbContext.Get(this, EntityLocalizer.DefaultCulture);

            EntityBind();

            if (HasConnection())
            {
                _Data = EntityDb.QueryEntity<GenericRecord>(keys);
                EntityPropertyBuilder.SetEntityContext(this, _Data);
                isEmpty = _Data == null || _Data.IsEmpty;
            }
        }

        /// <summary>
        /// Initialize a new instance of ActiveEntity with specified DataRow
        /// </summary>
        /// <param name="dr"></param>
        protected EntityContext(DataRow dr)
        {
            if (dr == null)
            {
                throw new ArgumentNullException("EntityContext.dr");
            }

            EntityDb = EntityDbContext.Get(this, EntityLocalizer.DefaultCulture);

            EntityBind();

            Init(dr);
        }

        /// <summary>
        /// Initialize a new instance of ActiveEntity with specified IDictionary
        /// </summary>
        /// <param name="data"></param>
        protected EntityContext(IDictionary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("EntityContext.data");
            }
            _Data.Load(data);
            EntityDb = EntityDbContext.Get(this, EntityLocalizer.DefaultCulture);
            EntityBind();

            if (_Data != null && _Data.Count > 0)
            {
                EntityPropertyBuilder.SetEntityContext(this, _Data);
                isEmpty = false;
            }
        }


        /// <summary>
        /// Bind to EntityDbContext, Occured on constrauctor
        /// </summary>
        protected abstract void EntityBind();


        internal bool isEmpty = true;

        [EntityProperty(EntityPropertyType.NA)]
        public bool IsEmpty
        {
            get { return isEmpty; }
        }

        #endregion

        #region Init


        /// <summary>
        /// Init entity using command with a <see cref="DataFilter"/> key Value Parameters.
        /// </summary>
        /// <param name="filter">Array of key value.</param>
        protected void Init(DataFilter filter)
        {
            string cmdText = SqlFormatter.SelectString("*", EntityDb.MappingName, filter.Filter);
            InitRow(cmdText, filter.Parameters, CommandType.Text);
        }

        /// <summary>
        /// Init entity using command and <see cref="IDbDataParameter"/> Parameters.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cmdType">Specifies how a command string is interpreted.</param>
        protected void Init(string cmdText, IDbDataParameter[] parameters, CommandType cmdType)
        {
            InitRow(cmdText, parameters, cmdType);
        }

        private void InitRow(string cmdText, IDbDataParameter[] parameters, CommandType cmdType)
        {
            DataRow row = EntityDb.DoCommand<DataRow>(cmdText, parameters, cmdType, 0);
            Init(row);
        }


        private void Init(DataRow row)
        {
            _Data = new GenericRecord(row);//rcd

            SetEntityContext();
        }

        protected virtual void SetEntityContext()
        {
            if (_Data != null && _Data.Count > 0)
            {
                EntityPropertyBuilder.SetEntityContext(this, _Data);
                isEmpty = false;
            }
        }

        #endregion

        #region EntityDbContext

        internal GenericRecord _Data;

        /// <summary>
        /// Get or Set properties values as IDictionary
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public GenericRecord EntityRecord
        {
            get { return _Data; }
            set
            {
                _Data = value;
                SetEntityContext();
            }
        }

        /// <summary>
        /// Init EntityDbContext using current instance of <see cref="DbContext"/>
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        protected void SetDb<Dbc>(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys) where Dbc : IDbContext
        {
            m_EntityDb = EntityDbContext.Get<Dbc>(entityName, mappingName, sourceType, keys);
        }

        /// <summary>
        /// Set the current of <see cref="EntityDbContext"/> using current instance of <see cref="DbContext"/>
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="entityKeys"></param>
        public void SetDb<Dbc>(string mappingName, EntitySourceType sourceType, EntityKeys entityKeys) where Dbc : IDbContext
        {
            m_EntityDb = EntityDbContext.Get<Dbc>(mappingName, mappingName, sourceType, entityKeys);
        }

        /// <summary>
        /// Set the current of <see cref="EntityDbContext"/> using current instance of <see cref="DbContext"/>
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="mappingName"></param>
        /// <param name="entityKeys"></param>
        public void SetDb<Dbc>(string mappingName, EntityKeys entityKeys) where Dbc : IDbContext
        {
            m_EntityDb = EntityDbContext.Get<Dbc>(mappingName, mappingName, EntitySourceType.Table, entityKeys);

        }
        
        internal protected bool m_EntityDbOwner = true;

        private EntityDbContext m_EntityDb;
        /// <summary>
        /// Get EntityDbContext
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public EntityDbContext EntityDb
        {
            get
            {
                if (m_EntityDb == null)
                {
                    return new EntityDbContext();
                }
                return m_EntityDb;
            }
            set
            {
                m_EntityDb = value;
            }
        }

        /// <summary>
        /// Validate if entity has connection properties
        /// </summary>
        /// <exception cref="EntityException"></exception>
        protected void ValidateEntityDb()
        {
            if (m_EntityDb == null)
            {
                throw new EntityException("Invalid MappingName or ConnectionContext");
            }
            m_EntityDb.ValidateContext();
        }

        /// <summary>
        /// Get indicate if entity has connection properties
        /// </summary>
        protected bool HasConnection()
        {
            if (m_EntityDb == null)
            {
                return false;
            }
            return m_EntityDb.HasConnection;
        }

 
        #endregion

        #region IEntityDictionary

        public IDictionary EntityDictionary()
        {
            return _Data == null ? null : _Data.EntityDictionary(); 
        }
        [EntityProperty(EntityPropertyType.NA)]
        public virtual Type EntityType
        {
            get { return typeof(EntityContext); }
        }
       
        public virtual void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
             EntityRecord.EntityWrite(stream, streamer);
        }

        public virtual void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
             EntityRecord.EntityRead(stream,streamer);
        }

   
        #endregion

        #region Control attributes

        /// <summary>
        /// Create new instance off <see cref="EntityProperties"/>
        /// </summary>
        internal protected virtual void CreateEntityAttributes()
        {
            if (m_ControlAttributes == null)
            {
                m_ControlAttributes = new EntityProperties(this, EntityDb);// EntityLangManager());
            }
        }

        internal EntityProperties m_ControlAttributes;
        /// <summary>
        /// Get EntityProperties
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public EntityProperties EntityProperties
        {
            get
            {
                CreateEntityAttributes();
                return m_ControlAttributes;
            }
        }


        #endregion

        #region public methods

        /// <summary>
        /// Get ActiveProperties that implement <see cref="EntityPropertyAttribute"/>
        /// </summary>
        /// <returns></returns>
        protected virtual PropertyInfo[] ActiveProperties()
        {
            return EntityExtension.GetEntityProperties(this, true, false);
        }

        /// <summary>
        /// Refresh
        /// </summary>
        public virtual void Refresh()
        {

        }

        #endregion

        #region ExecuteCommand
        /// <summary>
        /// Executes Command and returns T value (DataSet|DataTable|DataRow) or scalar value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <returns></returns> 
        protected T ExecuteCommand<T>(string commandText, IDbDataParameter[] parameters, CommandType commandType= CommandType.Text)
        {
            ValidateEntityDb();
            return EntityDb.DoCommand<T>(commandText, parameters, commandType);

        }

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="commandType">Specifies how a command string is interpreted.</param>
        /// <returns></returns> 
        protected int ExecuteNonQuery(string commandText, IDbDataParameter[] parameters, CommandType commandType = CommandType.Text)
        {
            ValidateEntityDb();
            return EntityDb.ExecuteNonQuery(commandText, parameters, commandType, 0);
        }

        #endregion

        #region update

        internal EntityFieldsChanges _FieldsChanges;
        /// <summary>
        /// Get Fields Changes
        /// </summary>
        /// <param name="isInsert"></param>
        /// <returns></returns>
        public EntityFieldsChanges GetFieldsChanged()
        {
            return _FieldsChanges;
        }

        internal void ValidateUpdate()
        {

            if (_Data != null && _Data.IsReadOnly)
            {
                throw new Exception("DataSource is ReadOnly");
            }

            ValidateEntityDb();
        }

        /// <summary>
        /// Save all Changes to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <returns></returns>
        public virtual int SaveChanges()
        {
            return SaveChanges(UpdateCommandType.Update);
        }

        /// <summary>
        /// Create new instance of <see cref="EntityFieldsChanges"/>
        /// </summary>
        /// <param name="initChanges"></param>
        /// <returns></returns>
        protected virtual EntityFieldsChanges CreateFieldsChanges(bool initChanges)
        {
            return new EntityFieldsChanges(this, null,UpdateCommandType.Update, initChanges);
        }

        /// <summary>
        /// Save all Changes by <see cref="UpdateCommandType"/> specific command to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        /// <exception cref="EntityException"></exception>
        /// <exception cref="DalException"></exception>
        public virtual int SaveChanges(UpdateCommandType commandType)
        {
            if (_Data == null && commandType == UpdateCommandType.Insert)
            {
                _Data = EntityPropertyBuilder.CreateGenericRecord(this,true);//.BuildEntityContext(this, false);
            }
            ValidateUpdate();

            EntityFieldsChanges fg = new EntityFieldsChanges(this, null, commandType,true);
            EntityCommandResult result = fg.SaveChanges();
            int res =EntityCommandResult.GetAffectedRecords(result);
            if (res > 0)
            {
                _Data = fg.Data;
            }
            return res;
        }

  
        #endregion

       
    }
}
