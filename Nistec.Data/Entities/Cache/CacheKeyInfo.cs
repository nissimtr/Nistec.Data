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
using System.Xml;
using System.Data;
using System.Runtime.Serialization;
using Nistec.Generic;

namespace Nistec.Data.Entities.Cache
{

    /// <summary>
    /// Represent the item key info for each item in cache
    /// </summary>
    [Serializable]
    public class CacheKeyInfo
    {

        public static CacheKeyInfo Get(string name, string key)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("CacheKeyInfo.name");
            }
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("CacheKeyInfo.keys");
            }
            //return new CacheKeyInfo() { ItemName = name, ItemKeys = keys };
            return new CacheKeyInfo() { ItemName = name, ItemKey = key };
        }

        public static CacheKeyInfo Get(string name, string[] keys)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("CacheKeyInfo.name");
            }
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentNullException("CacheKeyInfo.keys");
            }
            //return new CacheKeyInfo() { ItemName = name, ItemKeys = keys };
            return new CacheKeyInfo() { ItemName = name, ItemKey = keys.JoinTrim("|") };
        }

        public static CacheKeyInfo Get(string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentNullException("CacheKeyInfo.keys");
            }
            //return new CacheKeyInfo() { ItemName = "", ItemKeys = keys };
            return new CacheKeyInfo() { ItemName = "", ItemKey = keys.JoinTrim("|") };
        }

        public static CacheKeyInfo Parse(string strKeyInfo)
        {
            if (string.IsNullOrEmpty(strKeyInfo))
            {
                throw new ArgumentNullException("CacheKeyInfo.strKeyInfo");
            }
            string[] args = strKeyInfo.SplitTrim(':');
            if (args == null || args.Length < 2)
            {
                throw new ArgumentException("CacheKeyInfo.strKeyInfo is incorrect");
            }
            //return new CacheKeyInfo() { ItemName = args[0], ItemKeys = args[1].SplitTrim('|') };
            return new CacheKeyInfo() { ItemName = args[0].Trim(), ItemKey = args[1].Trim() };
        }

        #region properties

        public string ItemName
        {
            get;
            set;
        }
        public string ItemKey
        {
            get;
            set;
        }
        //public string[] ItemKeys
        //{
        //    get;
        //    set;
        //}

        public string CacheKey
        {
            get { return ItemKey; }
            //get { return ItemKeys == null ? null : string.Join("|", ItemKeys); }
        }

        public bool IsEmpty
        {
            get { return ItemKey == null || ItemKey.Length == 0; }
            //get { return ItemKeys == null || ItemKeys.Length == 0; }
        }

        #endregion

        /// <summary>
        /// Get CacheKeyInfo as string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", ItemName, CacheKey);
        }
        ///// <summary>
        ///// Split key to ItemKeys.
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public static string[] SplitKey(string key)
        //{
        //    return key.SplitTrim('|');
        //}
    }
}
