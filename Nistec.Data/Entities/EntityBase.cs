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
using System.Collections; 
using System.Collections.Generic; 
using System.Reflection;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.Xml;
using Nistec.Serialization;

namespace Nistec.Data.Entities
{

    /// <summary>
    ///Represent base EntityValue.
    /// </summary>
    [XmlRoot("EntityBase", Namespace = "http://www.w3.org/2001/XMLSchema", IsNullable = false)]
    [Serializable]
    public abstract class EntityBase : /*IDataEntity,*/ IDisposable//, IXmlSerializable
    {

        #region Dispose

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (m_EntityDb != null)
                    {
                        m_EntityDb.Dispose();
                        m_EntityDb = null;
                    }
                    m_ControlAttributes = null;

                    if (_DataSource != null)
                    {
                        EventsRegistration(false);
                        _DataSource.Dispose();
                        _DataSource = null;
                    }
                }
                DisposeInner(disposing);
                //dispose unmanaged resources here.
                disposed = true;
            }
        }

        protected virtual void DisposeInner(bool disposing)
        {

        }
        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // take this object off the finalization queue     
            GC.SuppressFinalize(this);
        }

        ~EntityBase()
        {
            Dispose(false);
        }

        #endregion

        #region members and ctor

        [EntityProperty(EntityPropertyType.NA)]
        public static DateTime DefaultNullDate { get { return DateTime.MinValue; } }
        public const string DefaultNullString = "";
        public const int DefaultNullInt = 0;
        public const bool DefaultNullBool = false;

        internal EntityMode m_EntityMode = EntityMode.NA;
        bool m_IsReadOnly = false;


        #endregion

        #region events

        private void EventsRegistration(bool register)
        {
            if (register)
            {
                _DataSource.IndexChanged += new EventHandler(_DataSource_IndexChanged);
                _DataSource.FieldValueChanged += new EventHandler(_DataSource_DataChanged);
                _DataSource.DataAcceptChanges += new EventHandler(_DataSource_DataAcceptChanges);
            }
            else
            {
                _DataSource.IndexChanged -= new EventHandler(_DataSource_IndexChanged);
                _DataSource.FieldValueChanged -= new EventHandler(_DataSource_DataChanged);
                _DataSource.DataAcceptChanges -= new EventHandler(_DataSource_DataAcceptChanges);
            }

        }

        void _DataSource_DataAcceptChanges(object sender, EventArgs e)
        {
            OnAcceptChanges(e);
        }

        void _DataSource_DataChanged(object sender, EventArgs e)
        {
            OnValueChanged(e);
        }

     
        void _DataSource_IndexChanged(object sender, EventArgs e)
        {
            OnPositionChanged(e);
        }

        protected virtual void OnPositionChanged(EventArgs e)
        {

        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {

        }

        protected virtual void OnAcceptChanges(EventArgs e)
        {

        }
        protected virtual void OnValueChanged(EventArgs e)
        {

        }

        #endregion

        #region GenericTable

        internal GenericDataTable _DataSource;
        
        /// <summary>
        /// Get the value indicating that data source IsEmpty
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        internal bool IsDataSourceEmpty
        {
            get { return _DataSource == null || _DataSource.Rows.Count == 0; }
        }

        /// <summary>
        /// Get or Set Data source for mulitple items
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public GenericDataTable EntityDataSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                if (_DataSource != value)
                {
                    if (_DataSource != null)
                        EventsRegistration(false);

                    _DataSource = value;
                    if (_DataSource != null)
                    {
                        EventsRegistration(true);
                        _DataSource.Index = 0;
                        OnDataSourceChanged(EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Get items count in DataSource
        /// </summary>
        /// <returns>int</returns>
        [EntityProperty(EntityPropertyType.NA)]
        internal int Count
        {
            get
            {
                if (IsDataSourceEmpty)
                    return 0;
                return _DataSource.Count;
            }
        }

        /// <summary>
        /// Get or set the Current Index position
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [EntityProperty(EntityPropertyType.NA)]
        internal int Position
        {

            get
            {
                if (IsDataSourceEmpty)
                    return -1;
                return _DataSource.Index;
            }
            set
            {
                if (Count <= 0)
                    return;
                if (value < 0 || value >= Count)
                    return;
                EntityDataSource.Index = value;
            }
        }


        /// <summary>
        /// Go to next record
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        internal void Next()
        {
            if (IsDataSourceEmpty)
                return;
            EntityDataSource.Next();
        }

        /// <summary>
        /// Go To Position safety, if index is out of range do nothing 
        /// </summary>
        /// <param name="index"></param>
        internal void GoTo(int index)
        {
            if (EntityDataSource.GoTo(index))
            {
                OnPositionChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Create list of entities form  DataSource that implement IEntity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="EntityException"></exception>
        protected IEnumerable<T> DataSourceEntities<T>() where T : IEntity
        {
            DataTable dt = this.EntityDataSource;
            if (dt == null)
            {
                throw new Exception("DataSource is empty");
            }

            return dt.EntityList<T>();// EntityDb.GetEntities<T>(dt);
        }

        protected void InitDataSource(string cmdText, IDbDataParameter[] parameters, CommandType cmdType)
        {
            GenericDataTable table = EntityDb.DoCommand<GenericDataTable>(cmdText, parameters, cmdType, 0);
            EntityDataSource = table;// new GenericDataTable(table);
        }
        #endregion

        #region EntityDbContext

       /// <summary>
        /// Init EntityDbContext using current instance of <see cref="DbContext"/> 
       /// </summary>
       /// <typeparam name="Dbc"></typeparam>
       /// <param name="entityName"></param>
       /// <param name="mappingName"></param>
       /// <param name="sourceType"></param>
       /// <param name="keys"></param>
        protected void InitEntity<Dbc>(string entityName, string mappingName,EntitySourceType sourceType, EntityKeys keys) where Dbc : IDbContext
        {
            EntityDb = EntityDbContext.Get<Dbc>(entityName, mappingName, sourceType, keys);
        }

        /// <summary>
        /// Init Entity for db provider by connection string and entity keys
        /// </summary>
        /// <param name="connectionKey"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        protected void InitEntity(string connectionKey, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys)
        {
            EntityDb = new EntityDbContext(entityName, mappingName, connectionKey, sourceType, keys);
        }

        /// <summary>
        /// Init Entity by EntityDbContext
        /// </summary>
        /// <param name="db"></param>
        internal protected void InitEntity(EntityDbContext db)//, bool enableDataSource)
        {
            EntityDb = db;
        }

        /// <summary>
        /// Init Entity by EntityDbContext and specified filter for data source
        /// </summary>
        /// <param name="db"></param>
        /// <param name="dataSourceFilter"></param>
        internal protected void InitEntity(EntityDbContext db, DataFilter dataSourceFilter)//string dataSourceFilter)
        {
            EntityDb = db;
            if (m_EntityDb != null)
            {
                DataTable dt = m_EntityDb.DoCommand<DataTable>(dataSourceFilter);
                EntityDataSource = new GenericDataTable(dt);
 
                m_EntityMode = EntityMode.Multi;
            }
        }

        private EntityDbContext m_EntityDb;
        /// <summary>
        /// Get or Set EntityDbContext
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public EntityDbContext EntityDb
        {
            get
            {
                if (m_EntityDb == null)
                {
                    m_EntityDb = new EntityDbContext();
                }
                return m_EntityDb;
            }
            set
            {
                m_EntityDb = value;
                if (m_EntityDb != null && m_EntityMode == EntityMode.NA)
                {
                    m_EntityMode = EntityMode.Generic;
                }
            }
        }


        /// <summary>
        /// Validate if entity has connection properties
        /// </summary>
        /// <exception cref="EntityException"></exception>
        protected void ValidateEntityDb()
        {
            if (m_EntityDb == null)
            {
                throw new EntityException("Invalid MappingName or ConnectionContext");
            }
            m_EntityDb.ValidateContext();
        }

        /// <summary>
        /// Indicate if entity has connection properties
        /// </summary>
        /// <exception cref="EntityException"></exception>
        protected bool HasConnection()
        {
            if (m_EntityDb == null)
            {
                return false;
            }
           return m_EntityDb.HasConnection;
        }

        #endregion


        #region Reflection methods

        /// <summary>
        /// Get Valid Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetValidValue<T>(T defaultValue)
        {
            MethodInfo method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            string field = method.Name;
            if (field.StartsWith("get_"))
            {
                field = field.Replace("get_", "");
            }
            return GetValue<T>(field, defaultValue);
        }

        /// <summary>
        /// GetValue return the field value, 
        /// if field is null and field type is ValueType it's return defaultValue 
        /// 0 for numeric type, now for date and false for bool
        /// otherwise it's return null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>object</returns>
        protected T GetValue<T>()
        {
            MethodInfo method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            string field = method.Name;
            if (field.StartsWith("get_"))
            {
                field = field.Replace("get_", "");
            }
            return GetValue<T>(field);

        }

        internal T GetInternalValue<T>(int frame)
        {
            MethodInfo method = (MethodInfo)(new StackTrace().GetFrame(frame).GetMethod());
            string field = method.Name;
            if (field.StartsWith("get_"))
            {
                field = field.Replace("get_", "");
            }
            return GetValue<T>(field);
        }



        #endregion

        #region SetValue

        /// <summary>
        /// Set Valid Value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        protected void SetValidValue<T>(T value, T defaultValue)
        {
            MethodInfo method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            if (value == null)
                SetValue(method, defaultValue);
            else
                SetValue(method, value);
        }

        /// <summary>
        /// Set Valid Value with validation
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue">if value=null or StringEmpty and defaultValue=null it's throw ArgumentNullException</param>
        /// <param name="regexValidation">Regex Validation Pattern</param>
        /// <exception cref="ArgumentNullException">when value is null or StringEmpty and defaultValue=null</exception>
        /// <exception cref="ArgumentException">when value is not valid</exception>
        protected void SetValidValue<T>(T value, T defaultValue, string regexValidation)
        {
            MethodInfo method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            string field = method.Name;
            if (field.StartsWith("set_"))
            {
                field = field.Replace("set_", "");
            }

            if (value == null || value.ToString() == String.Empty)
            {
                if (defaultValue == null)
                    throw new ArgumentNullException(field);
                else
                    SetValue(field, defaultValue);
            }
            else if (!string.IsNullOrEmpty(regexValidation) && !Nistec.Regx.RegexValidate(regexValidation, value.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                throw new ArgumentException(field + " is not valid", field);
            }
            else
                SetValue(field, value);
        }

        /// <summary>
        /// Set Value
        /// </summary>
        /// <returns>object</returns>
        protected void SetValue<T>(T value)
        {
            MethodInfo method = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            SetValue(method, value);
        }

        private void SetValue<T>(MethodInfo method, T value)
        {
            string field = method.Name;
            if (field.StartsWith("set_"))
            {
                field = field.Replace("set_", "");
            }

            SetValue(field, value);
        }
        #endregion

        #region methods

        /// <summary>
        /// Get values
        /// </summary>
        /// <returns></returns>
        protected GenericRecord[] GetValues()
        {
            DataTable dt = EntityDb.ExecuteDataTable();
            List<GenericRecord> values = new List<GenericRecord>();
            foreach (DataRow dr in dt.Rows)
            {
                values.Add(new GenericRecord(dr));
            }
            return values.ToArray();
        }


        internal void ValidateUpdate()
        {
            if (IsEmpty)
            {
                throw new Exception("Invalid DataSource");
            }
            if (IsReadOnly)
            {
                throw new Exception("DataSource is ReadOnly");
            }
            ValidateEntityDb();
        }

        /// <summary>
        /// Save all Changes to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <returns></returns>
        public abstract int SaveChanges();

        #endregion


        #region virtual methods
  
        /// <summary>
        /// Get or Set Active Value
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        [EntityProperty(EntityPropertyType.NA)]
        public virtual object this[string field]
        {
            get { return GetValue<object>(field); }
            set { SetValue<object>(field, value); }
        }

        /// <summary>
        /// Refresh
        /// </summary>
        public virtual void Refresh()
        {

        }
        #endregion

        #region absract properties


        /// <summary>
        /// Get or Set properties values as IDictionary
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public abstract GenericRecord EntityRecord { get; set; }


        /// <summary>
        /// Get or Set indicate if is it ReadOnly
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        protected bool IsReadOnly
        {
            get { return m_IsReadOnly; }
            set { m_IsReadOnly = value; }
        }


        #endregion

        #region Generic Values

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>T</returns>
        public abstract T GetValue<T>(string field);

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>if null or error return defaultValue</returns>
        /// <returns>T</returns>
        public abstract T GetValue<T>(string field, T defaultValue);

        /// <summary>
        /// Get Indicate if entity is empty
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public abstract bool IsEmpty { get; }


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
        protected abstract bool TryGetValue<T>(string field, out T value);

        /// <summary>
        /// SetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        protected abstract void SetValue<T>(string field, T value);

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        protected abstract void SetValue(string field, object value);

        #endregion

        #region converters

        /// <summary>
        /// Get ActiveProperties that implement <see cref="EntityPropertyAttribute"/>
        /// </summary>
        /// <returns></returns>
        internal PropertyInfo[] ActiveProperties()
        {
            return EntityExtension.GetEntityProperties(this, true, false);
        }

        #endregion

        #region Control attributes
       
        
        EntityProperties m_ControlAttributes;
        /// <summary>
        /// Get EntityProperties
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public EntityProperties EntityProperties
        {
            get
            {
                if (m_ControlAttributes == null)
                {
                    m_ControlAttributes = new EntityProperties(this, EntityDb);//, EntityLangManager());
                }
                return m_ControlAttributes;
            }
        }

 
        #endregion

        #region Serialization

        public const string XmlPrefix = "en";
        public const string DefultNamespace = "http://www.w3.org/2001/XMLSchema";
        private bool enableDataSourceSerialization = true;

       
        public string Serialize(FormatterType format, bool enableDataSource=true)
        {
            enableDataSourceSerialization = enableDataSource;
            try
            {
                if (format == FormatterType.Xml)
                {
                    string ser = XSerializer.Serialize(this, DefultNamespace);
                    return EntityDataExtension.ResolveRootElelment(ser, this.GetType());
                }
                else if (format == FormatterType.GZip)
                    return NetZipp.Zip(NetSerializer.SerializeToBase64(this));
                else //if (format == SerializationFormat.Binary)
                    return NetSerializer.SerializeToBase64(this);
            }
            catch (Exception ex)
            {
                throw new EntityException("SerializeEntiry error: ex" + ex.Message, ex.InnerException);
            }
            finally
            {
                enableDataSourceSerialization = true;
            }
        }

        public static T Deserialize<T>(string serialaized, FormatterType format)where T:IDataEntity
        {
            return EntityDataExtension.DeserializeEntity<T>(serialaized, format);
        }
   
         #endregion


    }

}
