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

namespace Nistec.Data.Entities
{
    public abstract class EntityItem : IEntityItem
    {

    }


    public abstract class EntityDbContext<T> where T : IEntityItem
    {
        public abstract IDbContext Db { get; }
        public abstract string MappingName { get; }
        public abstract string Title { get; }
        public abstract string Lang { get;}

        public T Get(params object[] keys)
        {
            return Db.QueryEntity<T>(MappingName, keys);
        }

        public IEnumerable<T> GetList(params object[] keys)
        {
            return Db.QueryEntityList<T>(MappingName, keys);
        }

        public int Update(T current,T newEntity) //where T : IEntityItem
        {
            //T current = GenericTypes.Cast<T>(this);
            var validation = EntityValidator.ValidateEntity(newEntity, Title, Lang);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            return Db.EntityUpdate<T>(MappingName, current, newEntity);
        }
        public int Insert(T newEntity) //where T : IEntityItem
        {
            //T current = GenericTypes.Cast<T>(this);
            var validation = EntityValidator.ValidateEntity(newEntity, Title, Lang);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            return Db.EntityInsert<T>(MappingName, newEntity);
        }
        public int Delete(T current) //where T : IEntityItem
        {
            return Db.EntityDelete<T>(MappingName, current);
        }
    }

    public abstract class EntityItem<Dbc> where Dbc : IDbContext
    {
        public abstract string MappingName();// { get; }
        public abstract EntityValidator Validate(UpdateCommandType commandType = UpdateCommandType.Update);

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
            T current = GenericTypes.Cast<T>(this);
            var validation = Validate(UpdateCommandType.Update);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            IDbContext Db = DbContext.Get<Dbc>();
            return Db.EntityUpdate<T>(MappingName(), current, newEntity);
        }
        public int DoInsert<T>() where T : IEntityItem
        {
            T current = GenericTypes.Cast<T>(this);
            var validation = Validate(UpdateCommandType.Insert);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            IDbContext Db = DbContext.Get<Dbc>();
            return Db.EntityInsert<T>(MappingName(), current);
        }
        public int DoDelete<T>() where T : IEntityItem
        {
            T current = GenericTypes.Cast<T>(this);
            var validation = Validate(UpdateCommandType.Delete);
            if (!validation.IsValid)
            {
                throw new Exception("EntityValidator eror: " + validation.Result);
            }
            IDbContext Db = DbContext.Get<Dbc>();
            return Db.EntityDelete<T>(MappingName(), current);
        }
    }


}
