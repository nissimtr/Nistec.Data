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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data.Entities.Cache
{

    /// <summary>
    /// EntityItem Bag
    /// </summary>
    /// <typeparam name="Dbe"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class EntityItemsBag<Dbe,T> : EntityItemsBag<T>  
        where T : IEntityItem
        where Dbe:IDbContext
    {

        public EntityItemsBag()
        {
            Load<Dbe>(null);
        }

        public EntityItemsBag(params object[] keyValueParameters)
        {
            Load<Dbe>(keyValueParameters);
        }

    }

        /// <summary>
        /// EntityItem Bag
        /// </summary>
        /// <typeparam name="T"></typeparam>
    public class EntityItemsBag<T> where T : IEntityItem
    {

        ConcurrentDictionary<string, T> Bag = new ConcurrentDictionary<string, T>();


        public int Count
        {
            get { return Bag.Count; }
        }
        public ICollection<string> Keys
        {
            get { return Bag.Keys; }
        }
        public ICollection<T> Values
        {
            get { return Bag.Values; }
        }

        /// <summary>
        /// Load Entity items from DB.
        /// </summary>
        /// <typeparam name="Dbe"></typeparam>
        public void Load<Dbe>() where Dbe : IDbContext
        {
            Load<Dbe>(null);
        }

        /// <summary>
        /// Load Entity items from DB.
        /// </summary>
        /// <param name="keyValueParameters">Optional parameters</param>
        /// <typeparam name="Dbe"></typeparam>
        public void Load<Dbe>(params object[] keyValueParameters) where Dbe : IDbContext
        {
            string mappingName = EntityMappingAttribute.Name<T>();
            using (var db = DbContext.Create<Dbe>())
            {
                var list = db.EntityItemList<T>(mappingName, keyValueParameters);
                foreach (var item in list)
                {
                    var key = item.EntityPrimaryKey<T>();
                    Bag[key] = item;
                }

                //Cache[entity.EntityName<T>()] = entity;
            }
        }

        public T Get(string key)
        {
            T o = default(T);
            Bag.TryGetValue(key, out o);
            return o;
        }

        public T Get(EntityKeys key)
        {
            T o = default(T);
            Bag.TryGetValue(key.ToString(), out o);
            return o;
        }

        public bool Remove(string key)
        {
            T o = default(T);
            return Bag.TryRemove(key, out o);
        }

        public void Clear(string key)
        {
            Bag.Clear();
        }

        public T[] ToArray()
        {
            return Bag.Values.ToArray();
        }

        public T SelectFirst(Func<T, bool> query)
        {
            return ToArray().Where(query).FirstOrDefault();
        }
        public IEnumerable<T> Select(Func<T, bool> query)
        {
            return ToArray().Where(query);
        }
    }
}
