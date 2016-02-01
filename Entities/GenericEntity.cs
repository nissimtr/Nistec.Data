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
using System.Collections;
using System.Runtime.Serialization;
using System.Xml;
using System.Data;
using Nistec.Runtime;
using Nistec.Generic;
using System.IO;
using Nistec.Data.Factory;
using System.Reflection;
using Nistec.IO;
using Nistec.Xml;
using Nistec.Serialization;

namespace Nistec.Data.Entities
{
    /// <summary>
    /// Represent Generic serializable entity that implement <see cref="IGenericEntity"/>, <see cref="ISerialEntity"/> and <see cref="IEntityDictionary"/>.
    /// </summary>
    [Serializable]
    public class GenericEntity : System.Xml.Serialization.IXmlSerializable, IGenericEntity, IEntityDictionary, ISerialJson
    {
        public const string EntityName = "GenericEntity";

       
        #region xml serialization

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            WriteElement(writer, EntityName, m_data);
        }

        void WriteElement(XmlWriter xmlWriter, string elementName, IDictionary properties)
        {
            string nameSpace = Nistec.Xml.XmlFormatter.TargetNamespace;
            xmlWriter.WriteStartElement(elementName, nameSpace);

            foreach (string propName in properties.Keys)
            {
                object propValue = properties[propName];
                if (propValue == null)
                    continue;

                if (propValue is IDictionary)
                {
                    WriteElement(xmlWriter, propName, (IDictionary)propValue);
                }
                else if (propValue is XmlNode)
                {
                    xmlWriter.WriteStartElement(propName);//, m_namespace);
                    xmlWriter.WriteNode(new XmlNodeReader((XmlNode)propValue), false);
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    string propValueStr = propValue.ToString();
                    if (!string.IsNullOrEmpty(propValueStr))
                        xmlWriter.WriteElementString(propName, propValueStr);
                }
            }

            xmlWriter.WriteEndElement();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
             
            string nodeName = "";
            bool isEnd = false;

            while (reader.Read() && !isEnd)
            {

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        nodeName = reader.Name;
                       
                        break;

                    case XmlNodeType.Text:

                        
                        if (!string.IsNullOrEmpty(nodeName))
                        {
                            this.Add(nodeName, reader.Value);
                        }
                        break;

                    case XmlNodeType.EndElement:

                        if (reader.Name.Contains(EntityName))
                            isEnd = true;
                       
                        nodeName = string.Empty;
                        break;

                    default:

                        break;
                }
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }


        public string SerializeToXml(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
                entityName = GenericEntity.EntityName;
            XmlFormatter formatter = new XmlFormatter(entityName, Xml.XmlFormatter.TargetNamespace, "utf-8");
            return formatter.DictionaryToXmlString(m_data);
        }

        public static GenericEntity DeserializeFromXml(string xmlString, string entityName)
        {
            GenericEntity gr = new GenericEntity();

            XmlFormatter.WriteXmlToDictionary(xmlString, gr.Record);

            return gr;
        }

        #endregion

        #region members

        GenericRecord m_data;
        bool m_allowNew = false;
        #endregion

        #region ctor

        public GenericEntity()
        {
            m_allowNew = true;
            m_data = new GenericRecord();
        }
     

        public GenericEntity(Stream stream, IBinaryStreamer streamer)
        {
            m_data = new GenericRecord(stream, streamer);
            m_allowNew = false;
        }

        public GenericEntity(SerializeInfo info)
        {
            m_data = new GenericRecord(info);
            m_allowNew = false;
        }

        public GenericEntity(DataRow dr)
        {
            ValidateData();
            DataUtil.LoadDictionaryEntityFromDataRow(m_data , dr);
            m_allowNew = true;
        }

        public GenericEntity(DataRow dr, string[] columns)
        {
            ValidateData(); 
            DataUtil.LoadDictionaryEntityFromDataRow(m_data, dr, columns);
            m_allowNew = true;
        }

        public GenericEntity(DataRow dr, string[] columns, string[] fieldsKey)
        {
           ValidateData();
            DataUtil.LoadDictionaryEntityFromDataRow(m_data, dr, columns);
            if (fieldsKey != null && fieldsKey.Length > 0)
                this._PrimaryKey = KeySet.BuildKeys(m_data, fieldsKey);
            m_allowNew = true;
        }

        public GenericEntity(IDictionary dr)
        {
            ValidateData();
            DataUtil.CopyIDictionary(m_data, dr);
            m_allowNew = false;
        }

        public GenericEntity(Dictionary<string, object> dr)
        {
            m_data = GenericTypes.Cast<GenericRecord>(dr);
            ValidateData();
            m_allowNew = false;
        }

        void ValidateData()
        {
            if (m_data == null)
            {
                m_data = new GenericRecord();
            }
        }
        #endregion

        #region static

        /// <summary>
        /// Initialize a new instance of GenericEntity
        /// </summary>
        /// <param name="db"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static GenericEntity Create(EntityDbContext db, params object[] keys)
        {
            DataRow row = db.QueryEntity<DataRow>(keys);
            return new GenericEntity(row);
        }


        /// <summary>
        /// Initialize a new instance of GenericEntity
        /// </summary>
        /// <param name="db"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static GenericEntity Create(EntityDbContext db, IDbDataParameter[] parameters)
        {
            string cmdText = SqlFormatter.SelectString("*", db.MappingName, SqlFormatter.ParametersString(parameters));

            DataRow row = db.DoCommand<DataRow>(cmdText, parameters, CommandType.Text, 0); ;
         
            return new GenericEntity(row);

        }

        /// <summary>
        /// Initialize a new instance of GenericEntity
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cmdType">Specifies how a command string is interpreted.</param>
        /// <returns></returns>
        public static GenericEntity Create<Dbc>(string cmdText, IDbDataParameter[] parameters, CommandType cmdType) where Dbc : IDbContext
        {
            IDbContext db = DbContext.Get<Dbc>();

            DataRow row = null;
            using (IDbCmd cmd = db.DbCmd())
            {
                row = cmd.ExecuteCommand<DataRow>(cmdText, parameters, cmdType, 0, true);
            }
            return new GenericEntity(row);
        }


        public static GenericEntity Create(DataRow dr, string[] fields)
        {
            if (dr == null)
                return null;
            DataTable dt = dr.Table;
            if (dt == null)
                return null;
            GenericEntity gv = new GenericEntity();

            foreach (string field in fields)
            {
                gv.m_data[field] = GenericTypes.NZ(dr[field], null);
            }
            return gv;
        }

        /// <summary>
        /// Create Array of <see cref="GenericEntity"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="enablePrimaryKey"></param>
        /// <returns></returns>
        public static GenericEntity[] CreateEntities(DataTable dt, bool enablePrimaryKey = false)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("GenericEntity.CreateEntities dt");
            }
            List<GenericEntity> values = new List<GenericEntity>();
            string[] columns = DataUtil.ColumnsFromDataTable(dt);
            if (enablePrimaryKey)
            {
                string[] fieldsKey = DataUtil.GetColumnsPrimaryKey(dt);
                if (fieldsKey == null)
                {
                    throw new Exception("GenericEntity.CreateEntities, DataTable has no PrimaryKey");
                }
                foreach (DataRow dr in dt.Rows)
                {
                    values.Add(new GenericEntity(dr, columns, fieldsKey));
                }
            }
            else
            {
                foreach (DataRow dr in dt.Rows)
                {
                    values.Add(new GenericEntity(dr, columns));
                }
            }
            return values.ToArray();
        }

        /// <summary>
        /// Create Array of <see cref="GenericEntity"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="enablePrimaryKey"></param>
        /// <returns></returns>
        internal static List<GenericEntity> CreateEntitiesList(DataTable dt, bool enablePrimaryKey = false)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("GenericEntity.CreateEntities dt");
            }
            List<GenericEntity> values = new List<GenericEntity>();
            string[] columns = DataUtil.ColumnsFromDataTable(dt);
            if (enablePrimaryKey)
            {
                string[] fieldsKey = DataUtil.GetColumnsPrimaryKey(dt);
                if (fieldsKey == null)
                {
                    throw new Exception("GenericEntity.CreateEntities, DataTable has no PrimaryKey");
                }
                foreach (DataRow dr in dt.Rows)
                {
                    values.Add(new GenericEntity(dr, columns, fieldsKey));
                }
            }
            else
            {
                foreach (DataRow dr in dt.Rows)
                {
                    values.Add(new GenericEntity(dr, columns));
                }
            }
            return values;
        }

        public static GenericEntity[] CreateEntities(DataTable dt, string[] fieldsKey)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("GenericEntity.CreateEntities dt");
            }
            if (fieldsKey == null || fieldsKey.Length == 0)
            {
                throw new ArgumentException("GenericEntity.CreateEntities, fieldsKey is not correct");
            }
            List<GenericEntity> values = new List<GenericEntity>();
            string[] columns = DataUtil.ColumnsFromDataTable(dt);
            foreach (DataRow dr in dt.Rows)
            {
                values.Add(new GenericEntity(dr, columns, fieldsKey));
            }

            return values.ToArray();
        }

        #endregion

        #region Validation

        internal void ValidateReadOnly()
        {
            if (IsReadOnly)
            {
                throw new EntityException("GenericEntity is ReadOnly");
            }
        }

        #endregion

        #region Load

        public void Load(DataRow dr)
        {
            ClearChanges();
            ValidateData();
            DataUtil.LoadDictionaryEntityFromDataRow(m_data, dr);
            m_allowNew = true;
        }

        public void Load(GenericEntity value)
        {
            ClearChanges();
            m_data = value.Record;
            ValidateData();
            m_allowNew = false;
        }

        public void Load(GenericRecord value)
        {
            ClearChanges();
            m_data = value;
            ValidateData();
            m_allowNew = false;
        }

        public void Load(IDictionary dic)
        {
            ClearChanges();
            DataUtil.CopyIDictionary(m_data, dic);
            ValidateData();
            m_allowNew = false;
        }

        public void Load(Dictionary<string, object> dic, bool useCopy=false)
        {
            ClearChanges();
            if (useCopy)
                DataUtil.CopyDictionary(m_data, dic);
            else
                m_data = GenericTypes.Cast<GenericRecord>(dic);
            ValidateData();
            m_allowNew = false;
        }
 
        #endregion

        #region Values

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <returns></returns>
        public object GetValue(string field)
        {
            object value = null;
            m_data.TryGetValue(field, out value);
            return value;
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>T</returns>
        public T GetValue<T>(string field)
        {
            object value = null;
            if (m_data.TryGetValue(field, out value))
            {
                return GenericTypes.Convert<T>(value);
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
        public T GetValue<T>(string field, T defaultValue)
        {
            object value=null;
            if (m_data.TryGetValue(field,out value))
            {
                return GenericTypes.Convert<T>(value, defaultValue);
            }
            return default(T);

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
            return m_data.TryGetValue<T>(field, out value);

        }


        /// <summary>
        /// SetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        public void SetValue<T>(string field, T value)
        {
            ValidateReadOnly();
            this[field] = value;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        public void SetValue(string field, object value)
        {
            ValidateReadOnly();
            this[field] = value;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        public void SetValue(object field, object value)
        {
            ValidateReadOnly();
            if (field == null)
                return;
            this[field.ToString()] = value;
        }
        #endregion

        #region Properties

        bool _IsReadOnly = false;
        public bool IsReadOnly
        {
            get { return _IsReadOnly; }
            internal set { _IsReadOnly = value; }
        }

        private Dictionary<string, object> iData
        {
            get
            {
                if (m_data == null)
                {
                    m_data = new GenericRecord();
                }
                return m_data;
            }
        }

          
        public void SetPrimaryKey(string[] fieldsKey)
        {
            if (fieldsKey != null && fieldsKey.Length > 0)
            {
                this._PrimaryKey = KeySet.BuildKeys(m_data, fieldsKey);
            }
        }
       
        KeySet _PrimaryKey;
        /// <summary>
        /// Get PrimaryKey as EntityKeys
        /// </summary>
        public KeySet PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
            internal set { _PrimaryKey = value; }
        }
        /// <summary>
        /// Gets the number of key/value pairs contained in the System.Collections.Hashtable.
        /// </summary>
        public int Count
        {
            get { return iData.Count; }
        }

        /// <summary>
        ///  Determines whether the System.Collections.Hashtable contains a specific key.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool ContainsKey(string field)
        {
            return m_data.ContainsKey(field);
        }
        /// <summary>
        ///  Determines whether the System.Collections.Hashtable contains a specific value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsValue(object value)
        {
            if (value == null)
                return false;
            return m_data.ContainsValue(value.ToString());
        }
        /// <summary>
        /// Adds an element with the specified key and value into the System.Collections.Hashtable.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public void Add(string field, object value)
        {
            this[field] = value;// GenericTypes.NZ(value, null);
        }

        public void Remove(string field)
        {
            AddChanges(field);
            m_data.Remove(field);
        }

        /// <summary>
        ///  Removes all elements from the System.Collections.Hashtable.
        /// </summary>
        public void Clear()
        {
            m_data.Clear();
            FieldsChanged.Clear();
        }

        /// <summary>
        /// Gets or sets the value associated with the specified field.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public object this[string field]
        {
            get { return GetValue(field); }
            set
            {
                ValidateReadOnly();
   
                if (m_data == null)
                {
                    throw new ArgumentException("Invalid Data ", field);
                }
                if (!m_data.ContainsKey(field))
                {
                    if (!m_allowNew)
                    {
                        throw new ArgumentException("Invalid fielde:" + field);
                    }
                    AddNewField(field);
                }
                else if (!CompareValues(field, value))
                {
                    AddChanges(field);
                }
                m_data[field] = value;
            }
        }
        #endregion

        #region FieldsChanged

        Dictionary<string, object> _FieldsChanged;
        const string EntityNewField = "$$EntityNewField$$";

        [EntityProperty(EntityPropertyType.NA)]
        Dictionary<string, object> FieldsChanged
        {
            get { if (_FieldsChanged == null) { _FieldsChanged = new Dictionary<string, object>(); } return _FieldsChanged; }
        }

        void AddNewField(string field)
        {
            FieldsChanged.Add(field, EntityNewField);
        }

        void AddChanges(string field)
        {
            FieldsChanged[field] = GetValue(field);
        }
        /// <summary>
        /// Clear all changes
        /// </summary>
        public void ClearChanges()
        {
            if (IsDirty)
            {
                Restor();
            }
            FieldsChanged.Clear();
        }

        /// <summary>
        /// End edit and save all changes localy
        /// </summary>
        public void CommitChanges()
        {
            if (IsDirty)
            {
                FieldsChanged.Clear();
            }
        }

        private void Restor()
        {
            ValidateData();

            foreach (KeyValuePair<string, object> entry in FieldsChanged)
            {
                if (entry.Value != null && entry.Value.ToString() == EntityNewField)
                    m_data.Remove(entry.Key);
                else
                    m_data[entry.Key] = entry.Value;
            }

            FieldsChanged.Clear();
        }

        public Dictionary<string, object> GetFieldsChanged()
        {

            Dictionary<string, object> fc = new Dictionary<string, object>();
            foreach (string k in FieldsChanged.Keys)
            {
                fc[k] = m_data[k];
            }

            return fc;
        }
        /// <summary>
        /// Get indicate if data source has changes
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool IsDirty
        {
            get { if (_FieldsChanged != null && _FieldsChanged.Count > 0)return true; return false; }
        }

        internal PropertyInfo[] GetProperties(bool fieldsChanges)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            if (fieldsChanges)
            {
                foreach (var entry in GetFieldsChanged())
                {
                    PropertyInfo info = entry.Value.GetType().GetProperty(entry.Key);
                    info.SetValue(entry.Value, entry.Value, null);
                }
            }
            else
            {
                foreach (var entry in m_data)
                {
                    PropertyInfo info = entry.Value.GetType().GetProperty(entry.Key);
                    info.SetValue(entry.Value, entry.Value, null);
                }
            }
            return list.ToArray();
        }

        #endregion

        #region SaveChanges

       

        /// <summary>
        /// Save all Changes by <see cref="UpdateCommandType"/> specific command to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        /// <exception cref="EntityException"></exception>
        /// <exception cref="DalException"></exception>
        public virtual int SaveChanges(UpdateCommandType commandType, EntityDbContext db)
        {
            ValidateReadOnly();
            
            int res = EntityFieldsChanges.SaveChanges(commandType,this,db);
           
            return res;
        }

        #endregion

        #region compare

        /// <summary>
        /// Compare Values between current field value and valueToComparee
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="valueToCompare">the T value to Compare</param>
        /// <returns></returns>
        public bool CompareValues<T>(string field, T valueToCompare) //where T : class 
        {
            T x = GetValue<T>(field);
            return EqualityComparer<T>.Default.Equals(x, valueToCompare);
        }

        /// <summary>
        /// Compare Values between current field value and valueToComparee
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="valueToCompare">the value to Compare</param>
        /// <returns></returns>
        public bool CompareValues(string field, object valueToCompare)
        {
            object x = GetValue(field);
            return EqualityComparer<object>.Default.Equals(x, valueToCompare);
        }

       
        /// <summary>
        ///  Compare  Values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected bool Compare<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        /// <summary>
        /// Compare between 2 GenericRecords
        /// </summary>
        /// <param name="gr"></param>
        /// <returns></returns>
        public bool Compare(GenericEntity gr)
        {
            foreach (KeyValuePair<string,object> entry in m_data)
            {
                bool ok = CompareValues(entry.Key.ToString(), entry.Value);
                if (!ok)
                    return false;
            }
            return true;
        }

       
        #endregion

        #region static

        public static IDictionary CreateData(DataRow dr)
        {
            if (dr == null)
                return null;
            GenericRecord gv = new GenericRecord(dr);
            if (gv == null)
                return null;
            return gv;
        }

   
        /// <summary>
        /// Create Array of <see cref="GenericEntity"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static GenericRecord[] CreateRecords(DataTable dt)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("GenericEntity.CreateRecords dt");
            }
            List<GenericRecord> values = new List<GenericRecord>();
            string[] columns = DataUtil.ColumnsFromDataTable(dt);

            foreach (DataRow dr in dt.Rows)
            {
                values.Add(new GenericRecord(dr, columns));
            }

            return values.ToArray();
        }

        /// <summary>
        /// Create Array of <see cref="GenericEntity"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        internal static List<GenericRecord> CreateRecordsList(DataTable dt)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("GenericEntity.CreateRecords dt");
            }
            List<GenericRecord> values = new List<GenericRecord>();
            string[] columns = DataUtil.ColumnsFromDataTable(dt);

            foreach (DataRow dr in dt.Rows)
            {
                values.Add(new GenericRecord(dr, columns));
            }

            return values;
        }
 

        /// <summary>
        /// Create Array of <see cref="GenericEntity"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Dictionary<string, GenericRecord> CreateEntityList(DataTable dt)
        {
            GenericEntity[] records = CreateEntities(dt, true);
            Dictionary<string, GenericRecord> values = new Dictionary<string, GenericRecord>();
            foreach (var gr in records)
            {
                if (gr.PrimaryKey != null)
                {
                    values.Add(gr.PrimaryKey.ToString(), gr.Record);
                }
            }
            return values;
        }

 
        /// <summary>
        /// Create Vertical GenericEntity (key value dictionary) by fetching keys and values from DataTable using array of fields as row key and value field
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fieldKeys"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public static GenericEntity DataTableToDictionary(DataTable dt, string[] fieldKeys, string fieldValue)
        {
            GenericEntity gv = new GenericEntity();

            if (dt == null || dt.Rows.Count == 0)
            {
                return gv;
            }
            if (fieldKeys == null || fieldKeys.Length == 0 || string.IsNullOrEmpty(fieldValue))
            {

                return gv;
            }
            int count = dt.Rows.Count;
            StringBuilder sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                foreach (string s in fieldKeys)
                {
                    sb.AppendFormat("{0}_", Types.NZ(dr[s], ""));
                }
                string key = sb.ToString().TrimEnd('_');
                gv.m_data[key] = Types.NZ(dr[fieldValue], "");
            }
            return gv;
        }

       
        #endregion

        #region public

        public string[] GetRecordKey(string[] fields)
        {
            if (fields == null || fields.Length == 0)
            {
                return null;
            }
            
            List<string> list = new List<string>();
            for (int i = 0; i < fields.Length; i++)
            {
                list.Add(Types.NZ(this[fields[i]], ""));
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get Indicate if GenericEntity is Empty
        /// </summary>
        public bool IsEmpty
        {
            get { return this.Count == 0; }
        }

        /// <summary>
        /// Convert GenericEntity to DataRow
        /// </summary>
        /// <returns></returns>
        public DataRow ToDataRow()
        {
            return DataUtil.HashtableToDataRow(m_data, EntityName);
        }

        /// <summary>
        /// Add current record to specified DataTable using fields list
        /// </summary>
        /// <param name="table"></param>
        /// <param name="enableIdentity"></param>
        public void AddTo(DataTable table,bool enableIdentity)
        {
            if (table == null || table.Columns.Count == 0)
            {
                throw new ArgumentException("AddDataRow.table is null or empty");
            }
 
            DataRow dr= table.NewRow();

            foreach (DataColumn col in table.Columns)
            {
                if (!enableIdentity && col.AutoIncrement)
                    continue;

                if (this.ContainsKey(col.ColumnName))
                    dr[col.ColumnName] = this[col.ColumnName];
                else
                {
                    if (col.AllowDBNull)
                        dr[col.ColumnName] = null;
                    else if(DataParameter.IsNumericType(col.DataType))
                        dr[col.ColumnName] = 0;
                    else if (col.DataType==typeof(Guid))
                        dr[col.ColumnName] = Guid.Empty;
                    else
                        dr[col.ColumnName] = string.Empty;
                }
            }
            table.Rows.Add(dr);
        }

        /// <summary>
        /// Display data as Vertical view, 
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public string Print(string headerName, string headerValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("{0}\t{1}", headerName, headerValue);

            foreach (KeyValuePair<string, object> entry in m_data)
            {
                sb.AppendLine();
                sb.AppendFormat("{0}: {1}", entry.Key, entry.Value);
            }
            sb.AppendLine();
            return sb.ToString();
        }
        #endregion

        #region ISerialJson

        public string EntityWrite(IJsonSerializer serializer)
        {
            return Record.EntityWrite(serializer);
        }

        public object EntityRead(string json, IJsonSerializer serializer)
        {
            return Record.EntityRead(json, serializer);
        }

        
        #endregion

        #region IEntityDictionary

        public IDictionary EntityDictionary()
        {
            return m_data;
        }

        public virtual Type EntityType()
        {
            return typeof(GenericEntity);
        }

       
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            Record.EntityWrite(stream, streamer);
        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            Record.EntityRead(stream, streamer);
        }
 
        [EntityProperty(EntityPropertyType.NA)]
        public GenericRecord Record
        {
            get { return m_data; }
        }

        #endregion

    }

}
