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
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable CS1591

namespace Nistec.Data.Advanced
{
    internal static class AutoDataCache
    {
        public static readonly AutoCache<string, object> DW = new AutoCache<string, object>(60);
   }

    /// <summary>
    /// Used to create a cache object of type Value with a string key.
    /// </summary>
    /// <typeparam name="Value">Type of object to hold in the cache.</typeparam>
    internal class AutoCache<Value> : AutoCache<string, Value>
    {
        internal AutoCache(int timeout)
            : base(timeout)
        {
        }
    }

    /// <summary>
    /// A cache object used to store key value pairs until a timeout has expired.
    /// </summary>
    /// <typeparam name="Key">Type of the key for the class.</typeparam>
    /// <typeparam name="Value">Type of the value for the class.</typeparam>
    internal class AutoCache<Key, Value>
    {
        #region Fields

        private DateTime _nextServiceTime = DateTime.MinValue;
        private readonly Dictionary<Key, Value> _internalCache;
        private readonly Dictionary<Key, DateTime> _lastAccessed;

        // The last time this process serviced the cache file.
        private readonly int _timeout;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs the cache clearing key value pairs after the timeout period 
        /// specified in minutes.
        /// </summary>
        /// <param name="timeout">Number of minutes to hold items in the cache for.</param>
        internal AutoCache(int timeout)
        {
            _internalCache = new Dictionary<Key, Value>();
            _lastAccessed = new Dictionary<Key, DateTime>();
            _timeout = timeout;
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        /// <param name="key">Key to be removed.</param>
        protected internal void Remove(Key key)
        {
            if (_internalCache.ContainsKey(key))
                _internalCache.Remove(key);
        }

        /// <summary>
        /// Returns the value associated with the key.
        /// </summary>
        /// <param name="key">Key of the value being requested.</param>
        /// <returns>Value or null if not found.</returns>
        protected internal Value this[Key key]
        {
            get
            {
                Value value;
                lock (this)
                {
                    if (_internalCache.TryGetValue(key, out value))
                        _lastAccessed[key] = DateTime.UtcNow;
                }
                CheckIfServiceRequired();
                return value;
            }
            set
            {
                lock (this)
                {
                    if (Contains(key))
                    {
                        _internalCache[key] = value;
                    }
                    else
                    {
                        _internalCache.Add(key, value);
                        _lastAccessed[key] = DateTime.UtcNow;
                    }
                }
            }
        }

        /// <summary>
        /// If the key exists in the cache then provide the value in the
        /// value parameter.
        /// </summary>
        /// <param name="key">Key of the value to be retrieved.</param>
        /// <param name="value">Set to the associated value if found.</param>
        /// <returns>True if the key was found in the list, otherwise false.</returns>
        protected internal bool GetTryParse(Key key, out Value value)
        {
            bool result = false;
            if (key != null)
            {
                lock (this)
                {
                    result = _internalCache.TryGetValue(key, out value);
                    if (result)
                        _lastAccessed[key] = DateTime.UtcNow;
                }
            }
            else
            {
                value = default(Value);
            }
            CheckIfServiceRequired();
            return result;
        }

        /// <summary>
        /// Determines if the key is available in the cache.
        /// </summary>
        /// <param name="key">Key to be checked.</param>
        /// <returns>True if the key is found, otherwise false.</returns>
        protected internal bool Contains(Key key)
        {
            bool result = false;
            if (key != null)
            {
                lock (this)
                {
                    result = _internalCache.ContainsKey(key);
                    if (result)
                        _lastAccessed[key] = DateTime.UtcNow;
                }
            }
            return result;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// If the time has passed the point another check of the cache is needed 
        /// start a thread to check the cache.
        /// </summary>
        private void CheckIfServiceRequired()
        {
            if (_nextServiceTime >= DateTime.UtcNow || _internalCache.Count <= 0) return;
            
            // Set the next service time to a date far in the future
            // to prevent another thread being started.
            _nextServiceTime = DateTime.MaxValue;
            Task.Factory.StartNew(() => ServiceCache(DateTime.UtcNow.AddMinutes(-_timeout)));
        }

        /// <summary>
        /// The main method of the thread to service the cache. Checks for old items
        /// and removes them.
        /// </summary>
        /// <param name="purgeDate">The date before which items should be removed.</param>
        private void ServiceCache(object purgeDate)
        {
            Queue<Key> purgeKeys = new Queue<Key>();

            // Obtain a list of the keys to be purged.
            lock (this)
            {

                foreach (Key key in _lastAccessed.Keys)
                {
                    if (_lastAccessed[key] < (DateTime) purgeDate)
                        purgeKeys.Enqueue(key);
                }
            }

            // Remove the keys from the lists.
            if (purgeKeys.Count > 0)
            {
                while (purgeKeys.Count > 0)
                {
                    Key key = purgeKeys.Dequeue();
                    if (key != null)
                    {
                        lock (this)
                        {
                            if (_lastAccessed[key] < (DateTime) purgeDate)
                            {
                                _lastAccessed.Remove(key);
                                _internalCache.Remove(key);
                            }
                        }
                    }
                }
            }

            // Set the next service time to one minute from now.
            _nextServiceTime = DateTime.UtcNow.AddMinutes(1);
        }

        #endregion
    }
}