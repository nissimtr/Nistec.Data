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
using Nistec.Generic;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Nistec.Data
{

    /// <summary>
    /// Data Extension
    /// </summary>
    public static class DataExtension
    {

        public static T Get<T>(this DataRow row, string field)
        {
            return GenericTypes.Convert<T>(row[field]);
        }

        public static T Get<T>(this DataRow row, string field, T valueIfNull)
        {
            return GenericTypes.Convert<T>(row[field], valueIfNull);
        }

        public static object GetSafe(this DataRow row, string field)
        {
            try
            {
                return row[field];
            }
            catch { return null; }
        }

    
        public static Dictionary<K, V> ToDictionary<K, V>(this DataTable dt, string keyName, string valueName)
        {

            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }
            DataRow[] drs = dt.Select(null);
            if (drs == null || drs.Length == 0)
                return null;

            var hashtable = new Dictionary<K,V>();

            foreach (DataRow dr in drs)
            {
                string hashKey = string.Format("{0}", dr[keyName]);
                var key = dr.Get<K>(keyName);
                var val = dr.Get<V>(valueName);
                hashtable[key] = val;
            }
            return hashtable;
        }

        public static IList<Dictionary<string, object>> ToListDictionary(this DataTable dt)
        {
            IList<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            string[] cols = dt.Columns.Cast<DataColumn>().Select(v => v.ColumnName).ToArray();//.Where(c => c.ColumnName != id);
            foreach (DataRow row in dt.Rows)
            {
                Dictionary<string, object> instance = new Dictionary<string, object>();
                foreach (var col in cols)
                {
                    instance[col] = row[col];
                }
                list.Add(instance);
            }
            return list;
        }

        public static string ToJson(this DataTable dt)
        {
            return JsonSerializer.ToJson(dt);
        }

        public static string ToJson(this DataRow row)
        {
            return JsonSerializer.ToJson(row);
        }

    }
}
