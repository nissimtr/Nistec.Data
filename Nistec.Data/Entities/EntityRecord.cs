//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Data Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
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
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data;
using Nistec.IO;

namespace Nistec.Data.Entities
{

    public static class EntityRecordExtension
    {
        public static T Cast<T>(this EntityRecord de)
        {
            T entity = ActivatorUtil.CreateInstance<T>();
            PropertyInfo[] p = entity.GetType().GetProperties(true);

            if (entity == null)
            {
                return default(T);
            }
            if (p == null)
            {
                return default(T);
            }
            else
            {
                foreach (var entry in de.Properties)
                {
                    PropertyInfo pi = p.Where(pr => pr.Name == entry.Key).FirstOrDefault();
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(entity, entry.Value, null);
                    }
                }

                return entity;
            }
        }
    }


    /// <summary>
    ///  The class represent dynamic entity
    /// </summary>
    public class EntityRecord : ISerialEntity, ISerialJson
    {
        Dictionary<string, object> properties
            = new Dictionary<string, object>();

        #region static
        public static EntityRecord Get(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("EntityRecord.source");
            }

            Type sourceType = source.GetType();

            if (sourceType == typeof(EntityRecord))
            {
                return (EntityRecord)source;
            }
            else if (sourceType == typeof(Dictionary<string, object>))
            {
                return new EntityRecord((Dictionary<string, object>)source);
            }
            else
            {
                return new EntityRecord(source);
            }
        }


        public static EntityRecord Parse(string json)
        {
            var dic = (IDictionary<string, object>)JsonSerializer.Deserialize(json, typeof(Dictionary<string, object>));
            EntityRecord record = new EntityRecord(dic);
            return record;
        }

        public static List<EntityRecord> ParseList(DataTable dt)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }
            List<EntityRecord> dic = new List<EntityRecord>();
            foreach (DataRow dr in dt.Rows)
            {
                EntityRecord row = new EntityRecord();
                foreach (DataColumn col in dt.Columns)
                {
                    row[col.ColumnName] = dr[col];
                }
                dic.Add(row);
            }
            return dic;
        }

        public static EntityRecord ParseKeyValue(params object[] keyValueParameters)
        {
            if (keyValueParameters == null)
                return null;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            EntityRecord record = new EntityRecord();
            for (int i = 0; i < count; i++)
            {
                record[keyValueParameters[i].ToString()] = keyValueParameters[++i];
            }

            return record;
        }

        public static EntityRecord ParseKeyValue(System.Collections.Specialized.NameValueCollection keyValue)
        {

            EntityRecord record = new EntityRecord();
            for (int i = 0; i < keyValue.Count; i++)
            {

                record[keyValue[i]] = keyValue[++i];
            }

            return record;
        }

        #endregion

        #region properties

        internal Dictionary<string, object> Properties
        {
            get { return properties; }
        }
        /// <summary>
        /// Get the number of elements
        /// </summary>
        public int Count
        {
            get
            {
                return properties.Count;
            }
        }


        [NoSerialize]
        public object this[string key]
        {
            get
            {
                object val;
                properties.TryGetValue(key, out val);
                return val;
            }
            set
            {
                properties[key] = value;
            }
        }


        #endregion
 
        #region ctor
        public EntityRecord()
        {

        }

        public EntityRecord(object source, string name = "", bool allowReadOnly = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("EntityRecord.source");
            }
            SerializeTools.MapToDictionary(properties, source, name, allowReadOnly);
        }

        public EntityRecord(Dictionary<string, object> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("EntityRecord.source");
            }
            properties = source;
        }
        public EntityRecord(EntityRecord source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("EntityRecord.source");
            }
            properties = source.properties;
        }
        public EntityRecord(DataRow source, string[] columns=null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("EntityRecord.source");
            }
            Load(source, columns);
        }
        public EntityRecord(DataTable source, string[] columns = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("EntityRecord.source");
            }
            Load(source, columns);
        }
        #endregion

        #region Values

        public bool Contains(string field, object value)
        {
            return properties.Contains(new KeyValuePair<string, object>(field, value));
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <returns></returns>
        public object Get(string field)
        {
            object value = null;
            properties.TryGetValue(field, out value);
            return value;
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>T</returns>
        public T Get<T>(string field)
        {
            object value = null;
            if (properties.TryGetValue(field, out value))
            {
                return (T)GenericTypes.Convert<T>(value);
            }
            return default(T);
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>if null or error return defaultValue</returns>
        /// <returns>T</returns>
        public T Get<T>(string field, T defaultValue)
        {
            object value = null;
            if (properties.TryGetValue(field, out value))
            {
                return GenericTypes.Convert<T>(value/*base[field]*/, defaultValue);
            }
            return defaultValue;
        }


        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">The key whose value to get.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if
        ///     the key is found; otherwise, the default value for the type of the value
        ///     parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        ///     true if the object that implements System.Collections.Generic.IDictionary
        ///     contains an element with the specified key; otherwise, false.
        ///</returns>
        ///<exception cref="System.ArgumentNullException">key is null.</exception>
        public bool TryGetValue<T>(string field, out T value)
        {

            object ovalue = null;
            if (properties.TryGetValue(field, out ovalue))
            {
                try
                {
                    value = (T)GenericTypes.Convert<T>(ovalue);
                    return true;
                }
                catch { }
            }
            value = default(T);
            return false;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="field">The key whose value to get.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if
        ///     the key is found; otherwise, the default value for the type of the value
        ///     parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        ///     true if the object that implements System.Collections.Generic.IDictionary
        ///     contains an element with the specified key; otherwise, false.
        ///</returns>
        ///<exception cref="System.ArgumentNullException">key is null.</exception>
        public new bool TryGetValue(string field, out object value)
        {
            return properties.TryGetValue(field, out value);
        }

        /// <summary>
        ///  Gets the value associated with the specified key.
        /// </summary>
        /// <param name="field">The key whose value to get.</param>
        /// <param name="type"></param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if
        ///     the key is found; otherwise, the default value for the type of the value
        ///     parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        ///     true if the object that implements System.Collections.Generic.IDictionary
        ///     contains an element with the specified key; otherwise, false.
        ///</returns>
        ///<exception cref="System.ArgumentNullException">key is null.</exception>
        public bool TryGetValue(string field, Type type, out object value)
        {

            object val = null;
            if (properties.TryGetValue(field, out val))
            {
                value = GenericTypes.Convert(val, type);
                return true;
            }
            value = GenericTypes.Default(type);
            return false;
        }


        /// <summary>
        /// SetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        public void Set<T>(string field, T value)
        {
            properties[field] = value;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        public void Set(string field, object value)
        {
            properties[field] = value;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        public void Set(object field, object value)
        {
            if (field == null)
                return;
            properties[field.ToString()] = value;
        }
        #endregion

        #region IEntityDictionary

        public string ToJson(bool pretty = false)
        {
            var json = JsonSerializer.Serialize(this.properties, pretty);
            return json;
        }

        public IDictionary<string, object> ToDictionary()
        {
            return properties.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        #endregion

        #region  ISerialEntity

        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            bool rwSerialType = true;

            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            else
                rwSerialType = false;

            ((BinaryStreamer)streamer).WriteDictionaryAsEntity(this.properties, rwSerialType, rwSerialType);
        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            bool rwSerialType = true;

            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            else
                rwSerialType = false;

            this.properties = new Dictionary<string, object>();
            ((BinaryStreamer)streamer).TryReadEntityToDictionary(this.properties, rwSerialType, rwSerialType);

        }


        #endregion

        #region ISerialJson

        public string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);
            if (pretty)
            {
                var json = serializer.Write(properties);
                return JsonSerializer.Print(json);
            }
            return serializer.Write(properties);
        }

        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);
            properties = serializer.Read<Dictionary<string, object>>(json);

            return this;
        }

        #endregion

        #region Loaders

        void Load(DataRow dr, string[] columns)
        {

            if (dr == null)
            {
                throw new ArgumentNullException("LoadDictionaryFromDataRow.dr");
            }
            if(columns==null || columns.Length==0)
            {
                DataTable dt = dr.Table;
                if (dt == null)
                    return;
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string colName = dt.Columns[i].ColumnName;
                    properties[colName] = dr[colName];
                }

            }
            else
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    string colName = columns[i];
                    properties[colName] = dr[colName];
                }
            }
            if (dr == null || columns == null)
                return;
        }

        void Load (DataTable dt, string[] columns)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }

            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                IDictionary<string, object> row = new Dictionary<string, object>();

                if (columns == null || columns.Length == 0)
                {
                    foreach (DataColumn col in dt.Columns)
                    {
                        row[col.ColumnName] = dr[col];
                    }
                    
                }
                else
                {
                    for (int ci = 0; ci < columns.Length; ci++)
                    {
                        string colName = columns[ci];
                        row[colName] = dr[colName];
                    }
                    properties[i.ToString()] = row;
                }
            }
        }

        #endregion
    }

}