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

namespace Nistec.Data.Entities.Cache
{
    public class DbCache<T> : Dictionary<string, T>
    {

    }

    public class EntityDbCache : DbCache<EntityDbContext>
    {
        IDbContext context;

        public EntityDbCache(IDbContext context)
        {
            this.context = context;
        }


        /// <summary>
        /// Get or Create <see cref="EntityDbContext"/>
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="entityKeys"></param>
        /// <param name="enableCache"></param>
        /// <returns></returns>
        public EntityDbContext Get(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys entityKeys, bool enableCache=true)
        {
            EntityDbContext db = null;
            if (enableCache)
            {
                if (this.TryGetValue(entityName, out db))
                {
                    return db;
                }
                db = new EntityDbContext(this.context, entityName, mappingName, sourceType, entityKeys);
                this[entityName] = db;
                return db;
            }
            return new EntityDbContext(this.context, entityName, mappingName, sourceType, entityKeys); ;
        }

        /// <summary>
        /// Get or Create <see cref="EntityDbContext"/> using EntitySourceType.Table
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="entityKeys"></param>
        /// <param name="enableCache"></param>
        /// <returns></returns>
        public EntityDbContext Get(string mappingName, EntityKeys entityKeys, bool enableCache = true)
        {
            EntityDbContext db = null;
            if (enableCache)
            {
                if (this.TryGetValue(mappingName, out db))
                {
                    return db;
                }
                db = new EntityDbContext(this.context, mappingName, mappingName, EntitySourceType.Table, entityKeys);
                this[mappingName] = db;
                return db;
            }
            return new EntityDbContext(this.context, mappingName, mappingName, EntitySourceType.Table, entityKeys); ;
        }

        /// <summary>
        /// Set Entity using <see cref="EntityAttribute"/>
        /// </summary>
        /// <typeparam name="Dbe"></typeparam>
        public void Set<Dbe>() where Dbe : IEntity
        {
            EntityDbContext Db = EntityDbContext.Get<Dbe>(EntityLocalizer.DefaultCulture);
            if (Db == null)
            {
                throw new EntityException("Could not create EntityDbContext, Entity attributes is incorrect");
            }
            Set(Db);
        }

        public void Set(IEntity entity)//, string tableName, string mappingName)
        {
            Set(entity.EntityDb);
        }

        public void Set(EntityDbContext entity)
        {
            this[entity.EntityName] = entity;
        }

        /// <summary>
        /// Set EntityDbContext
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourseType"></param>
        /// <param name="keys"></param>
        public void Set(string tableName, string mappingName, EntitySourceType sourseType, EntityKeys keys)
        {
            this[tableName] = new EntityDbContext(this.context, tableName, mappingName, sourseType, keys);
        }

        /// <summary>
        /// Set EntityDbContext with EntitySourceType.Table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        public void Set(string tableName, string mappingName, EntityKeys keys)
        {
            this[tableName] = new EntityDbContext(this.context, tableName, mappingName, EntitySourceType.Table, keys);
        }

        public void RemoveEntity(EntityDbContext entity, bool dispose)
        {
            if (this.ContainsKey(entity.EntityName))
            {
                this.Remove(entity.EntityName);
                if (entity != null && dispose)
                {
                    entity.Dispose();
                }
            }
        }

    }

    
}
