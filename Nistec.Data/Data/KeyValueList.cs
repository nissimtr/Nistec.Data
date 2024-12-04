using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data
{
    [Serializable]
    public class KeyValueResult //: List<KeyValuePair<string,object>>
    {
        //public virtual void Add(string key, object value)
        //{
        //    if (key == null)
        //    {
        //        throw new ArgumentNullException("KeyValueList.Add key");
        //    }

        //    this.Add(new KeyValuePair<string, object>(key, value));
        //}
        public object Id { get; set; }
        public object TotalRows { get; set; }
        //public object TotalValue { get; set; }
        public KeyValueList Result { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

    }
    //[Serializable]
    //public class JsonRawResult //: List<KeyValuePair<string,object>>
    //{
    //    //public virtual void Add(string key, object value)
    //    //{
    //    //    if (key == null)
    //    //    {
    //    //        throw new ArgumentNullException("KeyValueList.Add key");
    //    //    }

    //    //    this.Add(new KeyValuePair<string, object>(key, value));
    //    //}

    //    public JsonRawResult(DataTable dt, bool totalRows, bool totalValue)
    //    {
    //        Rows = JsonSerializer.Serialize(dt);
    //        TotalRows = dt.Rows[0]["TotalRows"];
    //        TotalValue = dt.Rows[0]["TotalValue"];
    //    }


    //    public object Id { get; set; }
    //    public object TotalRows { get; set; }
    //    public object TotalValue { get; set; }
    //    [RawJson]
    //    public string Rows { get; set; }

    //}

    //public static class KeyValueListExtension
    //{

    //    public static KeyValueResult GetResult(this KeyValueList list, bool totalRows, bool totalValue)
    //    {
    //        if (list == null || list.Count <= 0)
    //            return null;
    //        return new KeyValueResult() { Rows = list, TotalRows = totalRows ? list[0]["TotalRows"] : null, TotalValue = totalValue ? list[0]["TotalValue"] : null };
    //    }

    //    //public static KeyValueResult GetResult(this KeyValueList list, params string[] keys)
    //    //{
    //    //    var record = new KeyValueResult();
    //    //    if (list == null || list.Count <= 0)
    //    //        return record;

    //    //    foreach (var key in keys)
    //    //    {
    //    //        if (key != null)
    //    //            record.Add(key, list[0][key]);
    //    //    }
    //    //    record.Add("Rows", list);

    //    //    return record;
    //    //}

    //    //public static KeyValueRecord GetResultTotals(this KeyValueList list, bool TotalRows, bool TotalValue)
    //    //{
    //    //    return GetResult(list, TotalRows ? "TotalRows" : null, TotalValue ? "TotalValue" : null);
    //    //}

    //    //public static KeyValueRecord GetResult(this KeyValueList list, params string[] keys)
    //    //{
    //    //    var record = new KeyValueRecord();
    //    //    if (list == null || list.Count <= 0)
    //    //        return record;

    //    //    foreach (var key in keys)
    //    //    {
    //    //        if (key != null)
    //    //            record.Add(key, list[0][key]);
    //    //    }
    //    //    record.Add("Rows", list);

    //    //    return record;
    //    //}

    //    //public static KeyValueResult GetResult(this KeyValueList list, int id)
    //    //{
    //    //    if (list == null || list.Count <= 0)
    //    //        return new KeyValueResult();

    //    //    return new KeyValueResult()
    //    //    {
    //    //        Id=id,
    //    //        TotalRows = list[0]["TotalRows"],
    //    //        TotalValue = list[0]["TotalValue"],
    //    //        Rows = list
    //    //    };
    //    //}
    //}

    /// <summary>
    /// List of KeyValuePair{string, object}
    /// </summary>
    [Serializable]
    public class KeyValueList : List<KeyValueRecord>
    {
 
        #region ctor

        public KeyValueList()
        {
        }
 
        public KeyValueList(KeyValueRecord[] keyValueArray)
        {
            this.AddRange(keyValueArray);
        }

        public void Load(List<KeyValueRecord> keyValueList)
        {
            this.Clear();
            this.AddRange(keyValueList.ToArray());
        }

        #endregion

        public static KeyValueList CreateList(DataTable table, bool allowNull = true)
        {

            if (table == null)// || values.Rows.Count == 0)
            {
                throw new ArgumentNullException("KeyValueList.CreateList.values");
            }

            KeyValueList list = new KeyValueList();
            var cols = table.Columns;
            int length = table.Columns.Count;
            foreach (DataRow row in table.Rows)
            {
                var item = KeyValueRecord.CreateItem(row, cols);
                if (item == null)// (item.Value == null && allowNull == false)
                    continue;
                list.Add(item);
            }

            return list;
        }

    }

    /// <summary>
    /// List of KeyValuePair{string, object}
    /// </summary>
    [Serializable]
    public class KeyValueRecord : List<KeyValuePair<string, object>>, ISerialJson
    {

        #region collection methods

        void Set(KeyValuePair<string, object> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentNullException("KeyValueList.Add item");
            }
            var index = (this.Count == 0) ? -1 : IndexOf(item.Key);

            if (index < 0)
                base.Add(item);
            else
                base[index] = item;
        }

        public new void Add(KeyValuePair<string, object> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentNullException("KeyValueList.Add item");
            }
            base.Add(item);
        }

        public virtual void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("KeyValueList.Add key");
            }

            this.Add(new KeyValuePair<string, object>(key, value));
        }

        public virtual void RemoveItem(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("KeyValueList.RemoveItem key");
            }
            var item = GetItem(key);
            this.Remove(item);
        }

        public virtual KeyValuePair<string, object> GetItem(string key)
        {
            return this.Where(p => p.Key == key).FirstOrDefault();
        }
        [Serialization.NoSerialize]
        public new KeyValuePair<string, object> this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this.Insert(index, value);
            }
        }
        [Serialization.NoSerialize]
        public object this[string key]
        {
            get
            {
                return this.Where(p => p.Key == key).Select(p => p.Value).FirstOrDefault();
            }
            set
            {
                this.Set(new KeyValuePair<string, object>(key, value));
            }
        }

        public bool Contains(string key)
        {
            return this.Exists(p => p.Key == key);
        }

        public int IndexOf(string key)
        {
            return this.FindIndex(p => p.Key == key);
            //var item = this.Select((n, i) => new { Value = n, Index = i }).FirstOrDefault();
            //return item == null ? -1 : item.Index;
        }

        #endregion

        #region ctor

        public KeyValueRecord()
        {
        }
        public KeyValueRecord(List<KeyValuePair<string, object>> keyValueList)
        {
            Load(keyValueList);
        }

        public KeyValueRecord(KeyValuePair<string, object>[] keyValueArray)
        {
            this.AddRange(keyValueArray);
        }

        //public KeyValueList(byte[] bytes)
        //{
        //    EnableDuplicate = false;
        //    using (MemoryStream ms = new MemoryStream(bytes))
        //    {
        //        EntityRead(ms, null);
        //    }
        //}

        public KeyValueRecord(object[] keyValue)
        {
            Load(ParseQuery(keyValue));
        }

        //public virtual void Prepare(DataRow dr)
        //{
        //    this.ToKeyValue(dr);
        //}

        /// <summary>
        /// Get this as sorted see IOrderedEnumerable<KeyValuePair./>
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, object>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }

        public void Load(List<KeyValuePair<string, object>> keyValueList)
        {
            this.Clear();
            this.AddRange(keyValueList.ToArray());
        }

        #endregion

        #region static
        //internal static KeyValueList CreateList(params object[] keyValue)
        //{
        //    KeyValueList query = new KeyValueList(ParseQuery(keyValue));

        //    return query;
        //}

        internal static KeyValueRecord ParseQuery(params object[] keyValue)
        {
            KeyValueRecord pair = new KeyValueRecord();
            if (keyValue == null)
                return pair;
            int count = keyValue.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                object o = keyValue[i];
                if (o != null)
                {
                    pair.Add(new KeyValuePair<string, object>(keyValue[i].ToString(), keyValue[++i]));
                }
            }
            return pair;
        }
        #endregion

        #region properties

        ///// <summary>
        ///// Get or Set if enable duplicate keys, Default is (false)
        ///// </summary>
        //public bool EnableDuplicate
        //{
        //    get; set;
        //}

        public object Get(string key)
        {
            return this[key];
        }

        public object Get<T>(string key, T defaultValue)
        {
            return GenericTypes.Convert<T>(this[key], defaultValue);
        }

        public Type GetKeyType()
        {
            return typeof(string);
        }

        public Type GetValueType()
        {
            return typeof(object);
        }

        #endregion

        #region  IEntityFormatter
        /*
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            ((BinaryStreamer)streamer).WriteGenericKeyValue<T>(this);


            //streamer.WriteValue(this);
            streamer.Flush();

        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            //var o =(KeyValueList<T>) streamer.ReadValue();
            var o = ((BinaryStreamer)streamer).ReadGenericKeyValue<T>();

            //BinaryStreamer reader = new BinaryStreamer(stream);
            //var o = reader.ReadKeyValue<T>();
            Load(o);
        }
        */

        #endregion

        #region ISerialJson

        public string ToJson(bool pretty = false)
        {
            return EntityWrite(new JsonSerializer(JsonSerializerMode.Write, null), pretty);
        }

        public string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);

            foreach (var entry in this)
            {
                serializer.WriteToken(entry.Key, entry.Value);
            }
            return serializer.WriteOutput(pretty);
        }

        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);

            Dictionary<string, object> d = new Dictionary<string, object>();
            serializer.ParseTo(d, json);

            //var dic = serializer.ParseToDictionary(json);

            AddRange(d.ToArray());

            return this;
        }


        #endregion

        #region converter

        //public IDictionary Dictionary()
        //{
        //    var dict = this
        //       .Select(item => new { Key = item.Key, Value = item.Value })
        //       .Distinct()
        //       .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
        //    return dict;
        //}

        public IDictionary<string, object> ToDictionary()
        {
            var dict = this
               .Select(item => new { Key = item.Key, Value = item.Value })
               .Distinct()
               .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            return dict;
        }

        public IEnumerable<string> Keys()
        {
            return this.Select(item => item.Key);
        }

        public IEnumerable<object> Values()
        {
            return this.Select(item => item.Value);
        }

        public List<KeyValuePair<string, object>> ToList()
        {
            return this as List<KeyValuePair<string, object>>;
        }

        #endregion

        public static KeyValueRecord CreateItem(DataRow row)
        {
            DataTable table = row.Table;
            return CreateItem(row, table.Columns);
        }
        public static KeyValueRecord CreateItem(DataRow row, DataColumnCollection cols)
        {
            if (row == null || row.ItemArray.Length < 2)
            {
                throw new ArgumentException("KeyValueItem.Create DataRow is  incorrect");
            }
            //int length = values.ItemArray.Length;

            //return new KeyValuePair<string, object>(string.Format("{0}", values[0]), values[1]);

            //DataTable table = row.Table;

            //DataColumnCollection cols = table.Columns;
            KeyValueRecord record = new KeyValueRecord();
            foreach (DataColumn column in cols)
            {
                record.Add(column.ColumnName, row[column]);
            }
            return record;
        }
    }
}
