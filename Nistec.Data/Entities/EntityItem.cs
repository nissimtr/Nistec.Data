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
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{
    internal abstract class EntityItem : IEntityItem
    {
        public static bool IsEntityItem(object[] keyvalueParameters)
        {
            if (keyvalueParameters != null && keyvalueParameters.Length == 1)
            {
                return typeof(IEntityItem).IsAssignableFrom(keyvalueParameters[0].GetType());
            }
            return false;
        }
        public static bool IsEntityItem<T>(object[] keyvalueParameters)
        {
            if (keyvalueParameters != null && keyvalueParameters.Length == 1)
            {
                return typeof(T) == keyvalueParameters[0].GetType();
            }
            return false;
        }
    }

    public abstract class EntityItem<Dbc> : IEntityItem
        where Dbc : IDbContext
    {
        public abstract string MappingName();// { get; }
        public abstract EntityValidator Validate(UpdateCommandType commandType = UpdateCommandType.Update);

        protected T Current<T>() //where T : IEntityItem
        {
           return GenericTypes.Cast<T>(this, true); 
        }

        protected object[] GetKeyFields<T>() where T : IEntityItem
        {
            return EntityPropertyBuilder.GetEntityKeyValueParameters(Current<T>());
        }

        public int Do<T>(UpdateCommandType command, T newEntity = default(T)) where T : IEntityItem
        {
            switch (command)
            {
                case UpdateCommandType.Insert://insert
                    return DoInsert<T>();
                case UpdateCommandType.Delete://delete
                    return DoDelete<T>();
                case UpdateCommandType.Update://delete
                    if (newEntity == null)
                    {
                        throw new ArgumentNullException("DoUpdtae.newEntity");
                    }
                    return DoUpdate<T>(newEntity);
                default:
                    return 0;
            }
        }

        public int DoUpdate<T>(T newEntity) where T : IEntityItem
        {
            T current = GenericTypes.Cast<T>(this, true);
            var validation = Validate(UpdateCommandType.Update);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityUpdate<T>(current, newEntity).AffectedRecords;
            }
        }
        public int DoInsert<T>() where T : IEntityItem
        {
            T current = GenericTypes.Cast<T>(this, true);
            var validation = Validate(UpdateCommandType.Insert);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityInsert<T>(current).AffectedRecords;
            }
        }
        public int DoDelete<T>() where T : IEntityItem
        {
            T current = GenericTypes.Cast<T>(this, true);
            var validation = Validate(UpdateCommandType.Delete);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityDelete<T>(current);
            }
        }
    }


    public class EntityItemContext<Dbc,T> 
        where Dbc : IDbContext
        where T:IEntityItem
    {

        protected EntityItemContext()
        {
            
        }
        public EntityItemContext(T entity)
        {
            Current = entity;
        }
        public EntityItemContext(params object[] keyvalueParameters)
        {
            if (EntityItem.IsEntityItem<T>(keyvalueParameters))
                Set(Get((T)keyvalueParameters[0]));
            else
                Current = Get(keyvalueParameters);
        }

        public void Set(T entity)
        {
            Current = entity;
        }

        public void Set(params object[] keyvalueParameters)
        {
            Current = Get(keyvalueParameters);
        }

        public virtual void Validate(UpdateCommandType commandType = UpdateCommandType.Update)
        {
            EntityValidator.Validate(Current);
        }

        protected T Current 
        {
            get;set;// { return GenericTypes.Cast<T>(this);}
        }

        protected object[] GetKeyFields()
        {
            return EntityPropertyBuilder.GetEntityKeyValueParameters<T>(Current);
        }

        //public IList<T> GetList(string refId, int pid, int userId, int ttl)
        //{
        //    string key = DbContextCache.GetKey<MediaFile>(Settings.ProjectName, EntityCacheGroups.Task, 0, userId);
        //    return DbContextCache.EntityList<DbTeam, MediaFile>(key, ttl, "RefId", refId);
        //}

        public static IList<T> GetList(params object[] keyvalueParameters)
        {
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityList<T>(keyvalueParameters);
            }
            //return DbContext.EntityList<Dbc, T>(keyvalueParameters);
        }

        public T Get(params object[] keyvalueParameters)
        {
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityGet<T>(keyvalueParameters);
            }

            //return DbContext.EntityGet<Dbc, T>(keyvalueParameters);
        }

        public int Save()
        {
            //if (entity == null)
            //    entity = Current;
            //var validation = Validate(UpdateCommandType.Update);
            //if (!validation.IsValid)
            //{
            //    throw new Exception("EntityValidator eror: " + validation.Result);
            //}
            Validate(UpdateCommandType.Update);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntitySave<T>(Current).AffectedRecords;
            }
            //return DbContext.EntitySave<Dbc, T>(entity);
        }

        public int Insert()
        {
            
            //if (entity == null)
            //    entity = Current;
            //var validation = Validate(UpdateCommandType.Insert);
            //if (!validation.IsValid)
            //{
            //    throw new Exception("EntityValidator eror: " + validation.Result);
            //}
            Validate(UpdateCommandType.Insert);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityInsert<T>(Current).AffectedRecords;
            }
            //return DbContext.EntityInsert<Dbc, T>(entity);
        }
        //public int Delete()
        //{
        //    var keyvalueParameters = GetKeyFields();
        //    //var validation = Validate(UpdateCommandType.Update);
        //    //if (!validation.IsValid)
        //    //{
        //    //    throw new Exception("EntityValidator eror: " + validation.Result);
        //    //}
        //    Validate(UpdateCommandType.Update);
        //    using (IDbContext Db = DbContext.Create<Dbc>())
        //    {
        //        return Db.EntityDelete<T>(keyvalueParameters);
        //    }
        //    //return DbContext.EntityDelete<Dbc, T>(keyvalueParameters);
        //}
        public int Delete(params object[] keyvalueParameters)
        {
            if (keyvalueParameters == null)
                keyvalueParameters = GetKeyFields();
            //var validation = Validate(UpdateCommandType.Update);
            //if (!validation.IsValid)
            //{
            //    throw new Exception("EntityValidator eror: " + validation.Result);
            //}
            Validate(UpdateCommandType.Update);
            using (IDbContext Db = DbContext.Create<Dbc>())
            {
                return Db.EntityDelete<T>(keyvalueParameters);
            }
            //return DbContext.EntityDelete<Dbc, T>(keyvalueParameters);
        }
    }

    /*
    public abstract class EntityItemContext<T> where T : IEntityItem
    {
        
        public abstract IDbContext Db { get; }
        public abstract string MappingName { get; }
        public abstract string Title { get; }
        public abstract string Lang { get; }

        public T Get(params object[] keys)
        {
            return Db.EntityGet<T>(MappingName, keys);
        }

        public IEnumerable<T> GetList(params object[] keys)
        {
            return Db.EntityList<T>(MappingName, keys);
        }

        public int Update(T current, T newEntity) //where T : IEntityItem
        {
            //T current = GenericTypes.Cast<T>(this);
            var validation = EntityValidator.ValidateEntity(newEntity, Title, Lang);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            return Db.EntityUpdate<T>(current, newEntity);
        }
        public int Insert(T newEntity) //where T : IEntityItem
        {
            //T current = GenericTypes.Cast<T>(this);
            var validation = EntityValidator.ValidateEntity(newEntity, Title, Lang);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            return Db.EntityInsert<T>(newEntity);
        }
        public int Delete(T current) //where T : IEntityItem
        {
            return Db.EntityDelete<T>(current);
        }
    }
    */
}
