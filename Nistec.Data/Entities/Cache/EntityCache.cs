﻿//licHeader
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
using System.Collections;
using System.Text;
using System.Data;
using Nistec.Generic;
#pragma warning disable CS1591
namespace Nistec.Data.Entities.Cache
{
   
     /// <summary>
    /// EntityCache
    /// </summary>
    [Serializable]
    public abstract class EntityCache<T> : Dictionary<string, T>, IEntityCache, IEntityCache<T> where T : IEntityItem
    {

        public const int DefaultInitialCapacity = 100;
        public const float DefaultLoadFactor = 0.5F;

        //Hashtable m_cache;
        int m_options;
        string m_cacheName;

        List<string> m_keys;
        /// <summary>
        /// Get Keys items
        /// </summary>
        public List<string> DataKeys
        {
            get 
            { 
                if (m_keys == null)
                {
                    m_keys = new List<string>();
                }
                return m_keys; 
            }
        }

        public EntityCache()
            : this("", 1)
        {
        }
        public EntityCache(int options)
            : this("", options)
        {
        }
        public EntityCache(string cacheName, int options)
        {
            this.m_cacheName = cacheName;
            this.m_options = options;
        }

        /// <summary>
        /// InitCache 
        /// </summary>
        protected abstract void InitCache();

        protected virtual void OnError(Exception ex)
        {
            //throw ex;
        }


        /// <summary>
        /// GetKey with number of options
        /// </summary>
        /// <param name="option"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual string GetKey(int option, params string[] item)
        {
            return null;
        }

        /// <summary>
        /// Get Key default
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual string GetKey(params string[] item)
        {
            return CreateKey(string.Join("_", item));
        }



        /// <summary>
        /// Reset cache
        /// </summary>
        public void Reset()
        {
            this.Clear();
        }

        /// <summary>
        /// Refresh cache
        /// </summary>
        public void Refresh()
        {
            this.Clear();
            InitCache();
        }

        protected virtual string CreateKey(string key)
        {
            return key;
        }


        /// <summary>
        /// CreateKey
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual string CreateKey(T item) 
        {
            var key = EntityPropertyBuilder.GetEntityPrimaryKey<T>(item);

            //K key = GetKey(EntityKeys.BuildKeys<T>().ToArray());//EntityKeys.BuildKeyValues(item).ToArray());
            return key;
        }

       
        protected string CreateDataKey(DataRow dr)
        {
            int count= DataKeys.Count;
            if(count<=0)
                EntityPropertyBuilder.GetEntityPrimaryKey<T>(dr.ToEntity<T>(),DataKeys,true);
            //return CreateKey(dr.ToEntity<T>);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append( string.Format( "{0}_", dr[DataKeys[i]]));
            }

            return CreateKey(sb.ToString().TrimEnd('_')); 

        }
      
        /// <summary>
        /// CreateCacheItem 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual T CreateCacheItem(object item)
        {
            return (T)item;
        }

        /// <summary>
        /// CreateCache from data table
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void CreateCache(DataTable dt)
        {
            Reset();

            if (dt == null || dt.Rows.Count == 0)
                return;
            if (string.IsNullOrEmpty(m_cacheName))
                m_cacheName = dt.TableName;
            int count = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                string key = CreateDataKey(dr);
                this[key] = CreateCacheItem(dr);
            }
        }

        /// <summary>
        /// Create cache from IDictionary
        /// </summary>
        /// <param name="d"></param>
        protected virtual void CreateCache(IDictionary<string,T> d)
        {
            Reset();

            if (d == null || d.Count == 0)
                return;

            int count = d.Count;
            foreach (var entry in d)
            {
                this[entry.Key] = (T)entry.Value;
            }
        }

        /// <summary>
        /// Create cache from IEntity
        /// </summary>
        /// <param name="entities"></param>
        protected virtual void CreateCache(T[] entities)
        {
            Reset();

            if (entities == null || entities.Length == 0)
                return;

            int count = entities.Length;
            foreach (T entry in entities)
            {
                string key = CreateKey(entry);
                this[key] = (T)entry;
            }
        }

        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public T GetItem(params string[] key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentException("null Key item in Cache: " + m_cacheName);
            }
            if (Count <= 0)
            {
                InitCache();
            }
            string ky = null;//default(K);

            try
            {
                if (DataKeys.Count <= 1)
                {
                    ky = CreateKey(key[0]);
                    if (m_options <= 1)
                        return this[ky];
                    else
                    {
                        for (int i = 0; i < m_options; i++)
                        {
                            ky = GetKey(i, key);
                            if (this.ContainsKey(ky))
                                return this[ky];
                        }
                    }
                }
                else if (m_options <= 1)
                {
                    ky = GetKey(key);
                    return this[ky];
                }
                else
                {
                    for (int i = 0; i < m_options; i++)
                    {
                        ky = GetKey(i, key);
                        if (this.ContainsKey(ky))
                            return this[ky];
                    }
                }

                throw new ArgumentException("Invalid item in Cache: " + m_cacheName + " for key: " + ky.ToString());
                //return null;
            }
            catch (ArgumentException mex)
            {
                throw mex;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid item in Cache: " + m_cacheName + " for key: " + ky.ToString());
            }

        }


        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="key"></param>
        /// <returns>return null or empty if not exists</returns>
        public T FindItem(params string[] key)
        {
            if (key == null || key.Length == 0)
            {
                return default(T);
            }
            if (Count <= 0)
            {
                InitCache();
            }
            string ky = null;

            if (DataKeys.Count <= 1)
            {
                ky = CreateKey(key[0]);
                if (this.ContainsKey(ky))
                    return this[ky];
            }
            else if (m_options <= 1)
            {
                ky = GetKey(key);
                return this[ky];
            }
            else
            {
                for (int i = 0; i < m_options; i++)
                {
                    ky = GetKey(i, key);
                    if (this.ContainsKey(ky))
                        return this[ky];

                }
            }
            return default(T);
        }

        
    }


}
