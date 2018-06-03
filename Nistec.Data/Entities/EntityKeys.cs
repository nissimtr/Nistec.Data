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

        public static string GetPrimaryKey(this NameValueArgs keyValueArgs)
        {

            if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }

            int count = keyValueArgs.Count;
            int i = 0;
            object[] values = new object[count];
            foreach (var entry in keyValueArgs)
            {
                values[i] = entry.Value;
                i++;
            }

            return KeySet.FormatPrimaryKey(values);
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

        public EntityKeys(IEntity entity) 
        {
            this.AddRange(EntityPropertyBuilder.GetEntityPrimaryFields(entity));
        }

        public EntityKeys(object obj)
        {

            var props = DataProperties.GetEntityProperties(obj.GetType());

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;

                        this.Add(key);
                    }
                }
            }
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

            foreach(DataColumn col in dt.Columns)
            {
                if(this.IndexOf(col.ColumnName)>=0)
                {
                    columns.Add(col);
                }
            }


            //for (int i = 0; i < this.Count; i++)
            //{
            //    columns.Add(dt.Columns[this[i]]);
            //}
         
            dt.PrimaryKey = columns.ToArray();
        }

        /// <summary>
        /// Get Keys as string 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //if (Count == 0)
            //{
            //    return "";
            //}
            //return KeySet.FormatPrimaryKey(this.ToArray());

            //return ToString(KeySet.Separator);

            return string.Join(",", this);
        }

        //public string ToString(string separator)
        //{
        //    if (Count == 0)
        //    {
        //        return "";
        //    }
        //    return string.Join(separator, this);
        //}

        //public string CreateEntityPrimaryKey(object instance, bool sorted = false)
        //{
        //    IEnumerable<object> values = AttributeProvider.GetPropertiesValues(instance, this.ToArray(), sorted);
        //    return KeySet.FormatPrimaryKey(values.ToArray());
        //}

        //public string CreateEntityPrimaryKey(IDictionary<string, object> record, bool sorted = false)
        //{
        //    IEnumerable<object> values = (sorted) ?
        //        from p in record.Where(p => this.Contains(p.Key)).OrderBy(p => p.Key)
        //        select p.Value
        //        :
        //        from p in record.Where(p => this.Contains(p.Key))
        //        select p.Value;
        //    return KeySet.FormatPrimaryKey(values.ToArray());
        //}


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

        public static EntityKeys Get<T>()
        {
            return Get(typeof(T));
        }

        public static string[] ColumnsToArray(DataColumn[] columns)
        {
            List<string> keys = new List<string>();
            foreach (DataColumn col in columns)
            {
                    keys.Add(col.ColumnName);
            }
            return keys.ToArray();
        }

        public static EntityKeys Get(Type type)
        {

            var k=EntityPropertyBuilder.GetEntityPrimaryFields(type);
            EntityKeys keys = new EntityKeys(k.ToArray());
            return keys;

            //PropertyInfo[] properties = type.GetProperties();

            //EntityKeys keys = new EntityKeys();

            //foreach (PropertyInfo property in properties)
            //{

            //    EntityPropertyAttribute attr =
            //    Attribute.GetCustomAttribute(property, typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;
            //    if (attr == null)
            //    {
            //        continue;
            //    }
            //    EntityPropertyType attrType = attr.ParameterType;

            //    if (attrType == EntityPropertyType.Identity || attrType == EntityPropertyType.Key)
            //    {
            //        keys.Add(property.Name);
            //    }
            //}

            //return keys;
        }


        //public static string FormatPrimaryKey<T>(IEntity entity, IEnumerable<string> keys, bool sortedByFields = false)where T:IEntityItem
        //{
        //    IEnumerable<object> values = EntityPropertyBuilder.SelectEntityValues<T>(entity, keys, sortedByFields);
        //    return KeySet.FormatPrimaryKey(KeySet.Separator, values.ToArray());
        //}

        //public static string FormatPrimaryKey<T>(IEntityItem entity) where T : IEntityItem
        //{
        //    IEnumerable<object> values = EntityPropertyBuilder.SelectEntityValues<T>(entity);
        //    return KeySet.FormatPrimaryKey(KeySet.Separator, values.ToArray());
        //}

        //public static string GetEntityPrimaryKey<T>(IEntityItem entity) where T : IEntityItem
        //{

        //    var fields = EntityMappingAttribute.Primary<T>(false);

        //    if (fields == null)
        //    {
        //        var values = (from PropertyAttributeInfo<EntityPropertyAttribute> p in DataProperties.GetEntityProperties(typeof(T))
        //                         let key = p.Attribute.IsColumnDefined ? p.Attribute.Column : p.Property.Name
        //                         let val = p.Property.GetValue(entity, null)
        //                         where p.Attribute.ParameterType == EntityPropertyType.Key || p.Attribute.ParameterType == EntityPropertyType.Identity
        //                         orderby p.Attribute.Order  //orderby key
        //                         select val).ToArray();
        //        return KeySet.FormatPrimaryKey(values);
        //    }
        //    else
        //    {
        //        var keyvalues = (from PropertyAttributeInfo<EntityPropertyAttribute> p in DataProperties.GetEntityProperties(typeof(T))
        //                         let key = p.Attribute.IsColumnDefined ? p.Attribute.Column : p.Property.Name
        //                         let val = p.Property.GetValue(entity, null)
        //                         where fields.Contains(key) && p.Attribute.ParameterType == EntityPropertyType.Key || p.Attribute.ParameterType == EntityPropertyType.Identity
        //                         select new { key, val }).ToDictionary(c => c.key, c => c.val);


        //        List<object> values = new List<object>();
        //        for (int i = 0; i < fields.Length; i++)
        //        {
        //            values.Add(keyvalues[fields[i]]);
        //        }
        //        return KeySet.FormatPrimaryKey(values.ToArray());
        //    }

        //}

        #endregion

        #region KeyFields

        KeySet _FieldsKey;
        internal KeySet FieldsKey
        {
            get
            {
                if (_FieldsKey == null)
                {
                    _FieldsKey = new KeySet();// this.ToArray());
                    _FieldsKey.AddKeys(this.ToArray());
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

        public string GetPrimaryKey(string[] keyValueArgs)
        {

            if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }
            int length = this.Count;
            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            if ((length * 2) < count)
            {
                throw new ArgumentOutOfRangeException("values parameter Not match to fieldsKey range");
            }

            object[] values = new object[length];

            for (int i = 0; i < count; i++)
            {
                string key = keyValueArgs[i];
                int index = this.IndexOf(key);
                ++i;
                if (index >= 0)
                {
                    values[index] = keyValueArgs[i];
                }
            }

            return KeySet.FormatPrimaryKey(values);
        }

        public string GetPrimaryKey(NameValueArgs keyValueArgs)
        {

            if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }
            int length = this.Count;

            int count = keyValueArgs.Count;
            if (length < count)
            {
                throw new ArgumentOutOfRangeException("values parameter Not match to fieldsKey range");
            }

            object[] values = new object[length];
            foreach (var entry in keyValueArgs)
            {
                int index = this.IndexOf(entry.Key);
                if (index >= 0)
                {
                    values[index] = entry.Value;
                }
            }

            return KeySet.FormatPrimaryKey(values);
        }

        public string GetPrimaryKey(string queryString)
        {

            if (queryString == null)
            {
                throw new ArgumentNullException("queryString");
            }

            NameValueArgs nv = NameValueArgs.ParseQueryString(queryString);

            return GetPrimaryKey(nv);
        }

    }

}
