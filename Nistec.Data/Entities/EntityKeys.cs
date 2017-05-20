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
using System.Data;
using System.Reflection;
using System.Diagnostics;
using Nistec.Data;
using System.Collections;
using System.Collections.Specialized;
using Nistec.Generic;

namespace Nistec.Data.Entities
{

    /// <summary>
    /// Represent EntityKeys collection
    /// </summary>
    [Serializable]
    public static class EntityKeysExtension
    {
       
        public static DataFilter ToDataFilter(this KeySet key)
        {
            if (!key.HasValues())
            {
                throw new Exception("EntityKeys not conatains values!!");
            }
            return new DataFilter(key);
        }

        public static IDbDataParameter[] ToDataParameters(this KeySet key)
        {
            if (!key.HasValues())
            {
                throw new Exception("EntityKeys not conatains values!!");
            }
            return DataParameter.CreateParameters(key);
        }

    }

    /// <summary>
    /// Represent EntityKeys collection
    /// </summary>
    [Serializable]
    public class EntityKeys : List<string>
    {
        #region ctor
        public EntityKeys()
        {

        }
        public EntityKeys(params string[] keys)
        {
            this.AddRange(keys);
        }

        public EntityKeys(DataColumn[] columnKeys)
        {
            string[] keys = (from property in columnKeys
                             select property.ColumnName).ToArray();
            this.AddRange(keys);
        }
        #endregion

        #region methods

     
        /// <summary>
        /// Set DataTable primary key 
        /// </summary>
        /// <param name="dt"></param>
        public void SetPrimaryKeys(DataTable dt)
        {
            if (Count == 0)
            {
                return;
            }
            List<DataColumn> columns = new List<DataColumn>();
            for (int i = 0; i < this.Count; i++)
            {
                columns.Add(dt.Columns[this[i]]);
            }
         
            dt.PrimaryKey = columns.ToArray();
        }

        /// <summary>
        /// Get Keys as string 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Count == 0)
            {
                return "";
            }
            return KeySet.FormatPrimaryKey(this.ToArray()); ;
        }

        public string ToString(string separator)
        {
            if (Count == 0)
            {
                return "";
            }
            return string.Join(separator, this);
        }

        public string CreateEntityPrimaryKey(object instance, bool sorted = false)
        {
            IEnumerable<object> values = AttributeProvider.GetPropertiesValues(instance, this.ToArray(), sorted);
            return KeySet.FormatPrimaryKey(values.ToArray());
        }

        public string CreateEntityPrimaryKey(IDictionary<string, object> record, bool sorted = false)
        {
            IEnumerable<object> values = (sorted) ?
                from p in record.Where(p => this.Contains(p.Key)).OrderBy(p => p.Key)
                select p.Value
                :
                from p in record.Where(p => this.Contains(p.Key))
                select p.Value;
            return KeySet.FormatPrimaryKey(values.ToArray());
        }

    
        #endregion

        #region static BuildKeys

        public static EntityKeys Get(params string[] keys)
        {
            return new EntityKeys(keys);
        }

        public static EntityKeys Get(object[] keys)
        {
            string[] arr = ((IEnumerable)keys).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();
            return EntityKeys.Get(arr);
        }

        public static EntityKeys BuildKeys<T>()
        {
            Type type = typeof(T);
            return BuildKeys(type);
        }

        public static EntityKeys BuildKeys(Type type)
        {

            PropertyInfo[] properties = type.GetProperties();

            EntityKeys keys = new EntityKeys();

            foreach (PropertyInfo property in properties)
            {

                EntityPropertyAttribute attr =
                Attribute.GetCustomAttribute(property, typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;
                if (attr == null)
                {
                    continue;
                }
                EntityPropertyType attrType = attr.ParameterType;

                if (attrType == EntityPropertyType.Identity || attrType == EntityPropertyType.Key)
                {
                    keys.Add(property.Name);
                }
            }

            return keys;
        }
        #endregion

        #region KeyFields

        KeySet _FieldsKey;
        internal KeySet FieldsKey
        {
            get
            {
                if (_FieldsKey == null)
                {
                    _FieldsKey = new KeySet(this.ToArray());
                 }
                return _FieldsKey;
            }
        }

    
        internal void CreateByKeySet()
        {
            this.Clear();
            this.AddRange(FieldsKey.Keys.ToArray());
        }

        public bool HasValues()
        {
            if (_FieldsKey == null)
                return false;
            return _FieldsKey.HasValues();
        }

        #endregion

        #region static format

        public IEnumerable<string> Sorted()
        {
            return this.OrderBy(k => k);
        }


        #endregion
    }

}
