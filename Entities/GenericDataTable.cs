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
using System.Runtime.Serialization;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Xml;
using Nistec.Data.Advanced;
using System.Xml.Schema;
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.IO;
using Nistec.Xml;
using Nistec.Serialization;

namespace Nistec.Data.Entities
{
 
    /// <summary>
    /// Represent Generic Data Values
    /// </summary>
    [Serializable, EntitySerialize]
    public class GenericDataTable : DataTable, ISerialEntity,ISerialContext
    {
        public const string EntityName = "EntityDataSource";

        #region Serialization override and compress

        protected GenericDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }


        #endregion

        #region IXmlSerialaizer

        internal void WriteXmlBase(System.Xml.XmlWriter writer)
        {
            XSerializer.Serialize(this, writer);
        }

        internal void ReadXmlBase(System.Xml.XmlReader reader)
        {
            XSerializer.Deserialize<GenericDataTable>(reader);
        }

 
        #endregion

        #region Serialization

        public string Serialize(FormatterType format)
        {
            if (format == FormatterType.Xml)
                return SerializeToXml(null);
            else if (format == FormatterType.GZip)
                return NetZipp.Zip(NetSerializer.SerializeToBase64(this));
            else //if (format == SerializationFormat.Binary)
                return NetSerializer.SerializeToBase64(this);
        }
        public static GenericDataTable Deserialize(string serialaized, FormatterType format)
        {
            if (format == FormatterType.Xml)
                return GenericDataTable.DeserializeFromXml(serialaized, null);
            else if (format == FormatterType.GZip)
                return (GenericDataTable)NetSerializer.DeserializeFromBase64(NetZipp.UnZip(serialaized));
            else //if (format == SerializationFormat.Binary)
                return (GenericDataTable)NetSerializer.DeserializeFromBase64(serialaized);
        }

        public string SerializeToXml(string entityName)
        {
            this.TableName = entityName ?? EntityName;
            return XSerializer.Serialize(this);
        }

        public static GenericDataTable DeserializeFromXml(string xmlString, string entityName)
        {
            GenericDataTable dt = XSerializer.Deserialize<GenericDataTable>(xmlString);
            dt.TableName = entityName ?? EntityName;
            return dt;
        }


        #endregion

        #region members

        int index;
        bool m_dirty;
        #endregion

        #region ctor

        public static GenericDataTable Convert(DataTable dt)
        {
            return new GenericDataTable(dt);
        }

        public GenericDataTable()
        {
            index = 0;
            m_dirty = false;
        }


        /// <summary>
        /// Intiliaize a new instance of <see cref="GenericDataTable"/> with
        /// specified System.Data.DataTable and default missing schema.
        /// </summary>
        /// <param name="dt"></param>
        public GenericDataTable(DataTable dt)
            : this(dt, MissingSchemaAction.Add)
        {

        }
        /// <summary>
        /// Intiliaize a new instance of <see cref="GenericDataTable"/> with
        /// specified System.Data.DataTable and missing schema.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="missingSchemaAction"></param>
        public GenericDataTable(DataTable dt, MissingSchemaAction missingSchemaAction)
        {
            index = 0;
            m_dirty = false;
            if (dt == null)
            {
                throw new ArgumentNullException("GenericDataTable.dt");
            }
            this.Merge(dt,false,missingSchemaAction);
            this.TableName = dt.TableName;
            index = 0;
        }
        public GenericDataTable(DataTable dt, EntityKeys keys)
        {
            index = 0;
            m_dirty = false;
            if (dt == null)
            {
                throw new ArgumentNullException("GenericDataTable.dt");
            }
            this.Merge(dt,false, MissingSchemaAction.AddWithKey);
            this.TableName = dt.TableName;
            index = 0;

            SetPrimaryKeys(keys);
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_CurrentRecord != null)
            {
                _CurrentRecord = null;
            }
            _Columns=null;
        }

        #endregion

        #region events

        public event EventHandler IndexChanged;

        protected virtual void OnIndexChanged(EventArgs e)
        {
            _CurrentRecord = null;
            if (IndexChanged != null)
                IndexChanged(this, e);
        }

        public event EventHandler DataSourceChanged;

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            _CurrentRecord = null;
            if (DataSourceChanged != null)
                DataSourceChanged(this, e);
        }

        public event EventHandler DataAcceptChanges;

        public event EventHandler FieldValueChanged;

        protected virtual void OnAcceptChanges(EventArgs e)
        {
            if (DataAcceptChanges != null)
                DataAcceptChanges(this, e);
            m_dirty = false;
        }
        protected virtual void OnRejectChanges(EventArgs e)
        {
            m_dirty = false;
        }
        protected virtual void OnFieldValueChanged(EventArgs e)
        {
            if (FieldValueChanged != null)
                FieldValueChanged(this, e);
            m_dirty = true;
        }

        protected override void  OnTableCleared(DataTableClearEventArgs e)
        {
            _CurrentRecord = null;
 	        base.OnTableCleared(e);
            index=0;
        }

        protected override void OnRowDeleted(DataRowChangeEventArgs e)
        {
            _CurrentRecord = null;
            base.OnRowDeleted(e);
            index = 0;
        }
        #endregion

        #region GenericRecord
        //rcd
        GenericRecord _CurrentRecord;
        string[] _Columns;

        public GenericRecord CurrentRecord()//rcd
        {
            if (_CurrentRecord == null)
            {
                _CurrentRecord = ToRecord(Index);
            }
            
            return _CurrentRecord;
        }

        string[] GetColumns()
        {
            if (_Columns == null)
            {
                _Columns = DataUtil.ColumnsFromDataTable(this);
            }
            return _Columns;
        }

        /// <summary>
        /// Get values
        /// </summary>
        /// <returns></returns>
        public GenericRecord[] ToRecords()//rcd
        {
            DataTable dt = this;
            List<GenericRecord> values = new List<GenericRecord>();
            string[] columns = GetColumns();// DataUtil.ColumnsFromDataTable(dt);
            foreach (DataRow dr in dt.Rows)
            {
                values.Add(new GenericRecord(dr, columns));
            }
            return values.ToArray();
        }

        /// <summary>
        /// Get values as <see cref="GenericRecord"/>
        /// </summary>
        /// <returns></returns>
        public GenericRecord ToRecord(int index)//rcd
        {
            DataTable dt = this;
            string[] columns = GetColumns();// DataUtil.ColumnsFromDataTable(dt);
            if (index >= 0 && index < dt.Rows.Count)
            {
                DataRow dr = dt.Rows[index];
                return new GenericRecord(dr, columns);
            }
            return null;
        }

       

        #endregion

        #region data methods

        [EntitySerialize]
        private DataTable Data
        {
            get { return this as DataTable; }
            set { Load(value); }
        }

        public void Load(DataTable dt)
        {
            Load(dt, LoadOption.PreserveChanges);
            Index = 0;
        }
        public void Load(DataTable dt,LoadOption option)
        {
            using (DataTableReader reader = new DataTableReader(dt))
            {
                this.Load(reader, option);
            }
            Index = 0;
        }

        
        #endregion

        #region rows methods
  
        internal DataRow GetRowItem(int index)
        {
            if (Count==0)
            {
                throw new DataException("No rows were found");
            }
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException("The given row index is out of range");
            }
            return Rows[index];
        }

        internal T GetRowItem<T>(int index, string field)
        {
            DataRow rows = GetRowItem(index);
            return GenericTypes.Convert<T>(rows[field]);
        }

        public DataRow GetRow()
        {
            return GetRowItem(Index);
        }

        #endregion

        #region row edit methods

        /// <summary>
        /// Canceles the current edit on the row.
        /// </summary>
        public void CancelEdit()
        {
            if (!IsCurrentValid(false)) return;
            Rows[Index].CancelEdit();
        }

        /// <summary>
        /// Starts an edit operation on a System.Data.DataRow object.
        /// </summary>
        /// <exception cref="System.Data.InRowChangingEventException">The method was called inside the System.Data.DataTable.RowChanging event.</exception>
        /// <exception cref="System.Data.DeletedRowInaccessibleException">The method was called upon a deleted row.</exception>
        public void BeginEdit()
        {
            if (!IsCurrentValid(true)) return;
            Rows[Index].BeginEdit();
        }
        /// <summary>
        /// Clears the errors for the row. This includes the System.Data.DataRow.RowError
        /// and errors set with System.Data.DataRow.SetColumnError(System.Int32,System.String).
        /// </summary>
        public void ClearErrors()
        {
            if (!IsCurrentValid(false)) return;
            Rows[Index].ClearErrors();
        }
        /// <summary>
        /// Deletes the current System.Data.DataRow.
        /// </summary>
        ///<exception cref="System.Data.DeletedRowInaccessibleException">The System.Data.DataRow has already been deleted.</exception>
        public void Delete()
        {
            if (!IsCurrentValid(true)) return;
            Rows[Index].Delete();
        }

        /// <summary>
        /// Ends the edit occurring on the row.
        /// </summary>
        /// <exception cref=" System.Data.InRowChangingEventException">The method was called inside the System.Data.DataTable.RowChanging event.</exception>
        /// <exception cref="System.Data.ConstraintException">The edit broke a constraint.</exception>
        /// <exception cref=" System.Data.ReadOnlyException">The row belongs to the table and the edit tried to change the value of a
        ///     read-only column.</exception>
        /// <exception cref="System.Data.NoNullAllowedException">The edit tried to put a null value into a column where System.Data.DataColumn.AllowDBNull
        ///     is false.</exception>
        public void EndEdit()
        {
            if (!IsCurrentValid(true)) return;
            Rows[Index].EndEdit();
        }

        #endregion


        #region validatating

        /// <summary>
        /// Validate current index
        /// </summary>
        /// <param name="validateEdit"></param>
        /// <returns></returns>
        internal bool IsCurrentValid/*ValidCurrent*/(bool validateEdit)
        {
            if (IsEmpty)
            {
                return false;
            }
            return (index >= 0 && index < Count);
        }

         /// <summary>
        /// Validate data and index position
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        internal void ValidateCurrent()
        {
           
            if (IsEmpty)
            {
                throw new ArgumentException("Data source is empty");
            }
            if (Index == -1 || Index > Count)
            {
                throw new ArgumentException("Index out of range ", Index.ToString());
            }
        }

        #endregion

        #region find

        /// <summary>
        /// Get inducate if Data has PrimaryKey columns
        /// </summary>
        public bool HasPrimaryKey
        {
            get
            {
                return this.PrimaryKey != null && this.PrimaryKey.Length > 0;
            }
        }

        /// <summary>
        /// Get <see cref="EntityKeys"/>
        /// </summary>
        public EntityKeys PrimaryKeys
        {
            get
            {
                return new EntityKeys(this.PrimaryKey);
            }
        }

        /// <summary>
        /// Set Primary Keys
        /// </summary>
        /// <param name="keys"></param>
        public void SetPrimaryKeys(EntityKeys keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            keys.SetPrimaryKeys(this);
        }


        /// <summary>
        /// Find Record in data and return entity with found record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual T Find<T>(T entity, string columnName, object value)
        {
            int i = -1;
            if (IsEmpty)
                return GenericTypes.Default<T>();
            int ordinal = Columns[columnName].Ordinal;
            i = FindRecord(ordinal, value);
            if (i < 0)
                return GenericTypes.Default<T>();
            Index = i;
            return entity;
        }

        /// <summary>
        /// Find Record in data and return entity with found record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual T Find<T>(T entity, object[] value)
        {
            int i = -1;
            if (IsEmpty)
                return GenericTypes.Default<T>();
            DataRow dr = Rows.Find(value);
            if (dr == null)
                return GenericTypes.Default<T>();
            i = Rows.IndexOf(dr);
            if (i < 0)
                return GenericTypes.Default<T>();
            Index = i;
            return entity;
        }

        /// <summary>
        /// Find Record in data and return entity with found record
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual int Find(string columnName, object value)
        {
            if (IsEmpty)
                return -1;
            int ordinal = Columns[columnName].Ordinal;
            return FindRecord(ordinal, value);
        }

        /// <summary>
        /// Find Record by column index and field value and return the row index, if not found return -1.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int FindRecord(int column, object value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Rows[i][column].Equals(value))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index of the specified row object.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public virtual int FindRow(object[] keys)
        {
            DataRow dr = Find(keys);
            if (dr == null)
                return -1;
            return Rows.IndexOf(dr);
        }

        /// <summary>
        /// Gets the row that contains the specified primary key values.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public DataRow Find(object[] keys)
        {
            if (IsEmpty)
                return null;
            if (!HasPrimaryKey)
                return null;

            return Rows.Find(keys);
        }

        /// <summary>
        /// Gets the current row .
        /// </summary>
        /// <returns></returns>
        public DataRow GetActiveRow()
        {
            if (IsEmpty)
                return null;
            return Rows[Index];
        }

        /// <summary>
        /// Compare Record and return if it's equal
        /// </summary>
        /// <param name="index"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool CompareRecord(int index, string column, object value)
        {
            return Rows[index][column].Equals(value);
        }
        #endregion

        #region Data properties
  
        /// <summary>
        /// Get the total row count in this data source.
        /// </summary>
        /// <returns>int</returns>
        [EntityProperty(EntityPropertyType.NA)]
        public virtual int Count
        {
            get
            {
                if (IsEmpty)
                    return 0;
                return Rows.Count;
            }
        }

        /// <summary>
        /// Get or Set value by column name.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [EntityProperty(EntityPropertyType.NA), NoSerialize]
        public virtual object this[string columnName]
        {
            get
            {
                if (!IsCurrentValid(false))
                {
                    return null;// throw new ArgumentException("Invalid Data ", columnName);
                }
                return Rows[Index][columnName];
            }
            set
            {
                SetValue(columnName, value);
            }
        }

        /// <summary>
        /// Get or Set value by column index.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        [EntityProperty(EntityPropertyType.NA), NoSerialize]
        public object this[int columnIndex]
        {
            get
            {
                if (!IsCurrentValid(false))
                {
                    return null;//throw new ArgumentException("Invalid Data ", columnIndex.ToString());
                }
                return Rows[Index][columnIndex];
            }
            set
            {
                SetValue(columnIndex, value);
            }
        }

        /// <summary>
        /// Get value by row index and column name.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        [EntityProperty(EntityPropertyType.NA), NoSerialize]
        public object this[int row, string columnName]
        {
            get
            {
                return Rows[row][columnName];
            }
        }

        /// <summary>
        /// Get value by row index and column index.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        [EntityProperty(EntityPropertyType.NA), NoSerialize]
        public object this[int row, int columnIndex]
        {
            get
            {
                return Rows[row][columnIndex];
            }
        }

        /// <summary>
        /// Get or Set ItemArray
        /// </summary>
        [NoSerialize]
        public object[] ItemArray
        {
            get
            {
                if (!IsCurrentValid(false)) return null;
                return Rows[Index].ItemArray;
            }
            set
            {
                if (!IsCurrentValid(true)) return;
                Rows[Index].ItemArray = value;
            }
        }

        /// <summary>
        /// Get the value indicating that data source IsEmpty
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool IsEmpty
        {
            get { return Rows == null || Rows.Count == 0; }
        }

        /// <summary>
        /// Get indicate if data source has changes
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool IsDirty
        {
            get { return m_dirty; }
        }

        /// <summary>
        /// Get or Set DataSorce row index.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="EntityException"></exception>
        [NoSerialize]
        public int Index
        {
            get
            {
                if (IsEmpty)
                {
                    return -1;// throw new EntityException("Generic table Is empty");
                }
                return index;
            }
            set
            {
                 if (IsEmpty)
                {
                    return;// throw new EntityException("Generic table Is empty");
                }
                if (value < 0 || value >= Rows.Count)
                {
                    throw new ArgumentOutOfRangeException("Index");
                }
                index = value;
                OnIndexChanged(EventArgs.Empty);
            }
        }
        #endregion

        #region Navigation methods

        /// <summary>
        /// Go to next record, if index is out of range do nothing
        /// </summary>
        public bool Next()
        {
            return GoTo(index + 1);
        }
        /// <summary>
        /// Go to next record, if index is out of range do nothing
        /// </summary>
        public bool Prev()
        {
            return GoTo(index - 1);
        }
        /// <summary>
        /// Go To Position safety, if index is out of range do nothing
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool GoTo(int index)
        {
            if (index >= Count || index < 0)
            {
                return false; //throw new IndexOutOfRangeException("Index out of range " + index.ToString());
            }
            Index = index;

            return true;
        }

        /// <summary>
        /// Find and load entity record from DataSource that contains the specified primary key values.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>true if found else false</returns>
        public bool GoTo(object[] keys)
        {
            if (Count <= 0)
                return false;
            if (!HasPrimaryKey)
                return false;
            int i = FindRow(keys);
            return GoTo(i);
        }

        #endregion

        #region Get Values

        /// <summary>
        /// Get value from current row index and field name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns></returns>
        public T GetValue<T>(string field)
        {
            return GetRowItem<T>(Index, field);// Types.Convert<T>(Rows[Index][field]);
        }

        /// <summary>
        /// Get value from current row index and field name,if not found or error occured return default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>if null or error return defaultValue</returns>
        /// <returns>T</returns>
        public T GetValue<T>(string field, T defaultValue)
        {

            try
            {
                return GenericTypes.Convert<T>(Rows[Index][field], defaultValue);
                //return (T)Rows[Index][field];
            }
            catch
            {
                return defaultValue;
            }
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
        /// <returns></returns>
        /// <returns>
        ///     true if the object that implements System.Collections.Generic.IDictionary<TKey,TValue>
        ///     contains an element with the specified key; otherwise, false.
        ///</returns>
        ///<exception cref="System.ArgumentNullException">key is null.</exception>
        public bool TryGetValue<T>(string field, out T value)
        {

            T val = GenericTypes.Default<T>();// default(T);
            try
            {
                if (!this.Columns.Contains(field))
                {
                    value = val;
                    return false;
                }
                val = GetValue<T>(field);// (T)Rows[Index][field];
                value = val;
                return true;
            }
            catch
            {
                value = val;
                return false;
            }
        }


        /// <summary>
        /// Set Value to current row index and field name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        public void SetValue<T>(string field, T value)
        {
            DataRow dr = GetRowItem(Index);
            dr[field] = value;
            this.OnFieldValueChanged(EventArgs.Empty);

        }

        /// <summary>
        /// Set Value to current row index and field name
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        internal void SetValue(string field, object value)
        {
            DataRow dr = GetRowItem(Index);
            dr[field] = value;
            this.OnFieldValueChanged(EventArgs.Empty);

        }

        /// <summary>
        /// Set Value to current row index and column Index
        /// </summary>
        /// <param name="columnIndex">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        internal void SetValue(int columnIndex, object value)
        {
            DataRow dr = GetRowItem(Index);
            dr[columnIndex] = value;
            this.OnFieldValueChanged(EventArgs.Empty);

        }

        /// <summary>
        /// Compare Values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to Compare</param>
        /// <returns></returns>
        public bool CompareValues<T>(string field, T value) //where T : class 
        {
            T x = GetValue<T>(field);
            return EqualityComparer<T>.Default.Equals(x, value);
        }


        /// <summary>
        ///  Compare  Values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Compare<T>(T x, T y) { return EqualityComparer<T>.Default.Equals(x, y); }
        #endregion

        #region static

        /// <summary>
        /// Initialize a new instance of GenericData
        /// </summary>
        /// <param name="db"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static GenericDataTable Create(EntityDbContext db, object[] keys)
        {
            GenericDataTable dt = (GenericDataTable)db.QueryEntity<GenericDataTable>(keys);
            return dt;// new GenericData(dt);
        }
     

        /// <summary>
        /// Initialize a new instance of GenericData
        /// </summary>
        /// <param name="db"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static GenericDataTable Create(EntityDbContext db, DataFilter filter)
        {
            GenericDataTable dt = (GenericDataTable)db.DoCommand<GenericDataTable>(filter);
            return dt;
        }

        /// <summary>
        /// Initialize a new instance of GenericData
        /// </summary>
        /// <param name="db"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static GenericDataTable Create(EntityDbContext db, IDbDataParameter[] parameters)
        {
            string cmdText = SqlFormatter.SelectString("*", db.MappingName, SqlFormatter.ParametersString(parameters));

            GenericDataTable dt = db.DoCommand<GenericDataTable>(cmdText, parameters, CommandType.Text, 0);
            return dt;

        }

        /// <summary>
        /// Initialize a new instance of GenericData
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cmdType">Specifies how a command string is interpreted.</param>
        /// <returns></returns>
        public static GenericDataTable Create<Dbc>(string cmdText, IDbDataParameter[] parameters, CommandType cmdType) where Dbc : IDbContext
        {
            IDbContext db = DbContext.Get<Dbc>();

            GenericDataTable dt = null;
            using (IDbCmd cmd = db.DbCmd())
            {
                dt = cmd.ExecuteCommand<GenericDataTable>(cmdText, parameters, cmdType, 0, true);
            }
            return dt;
        }

        #endregion

        #region IEntityFormatter


        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            streamer.WriteValue(this,typeof(DataTable));
        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            DataTable dt = (DataTable)streamer.ReadValue();

            if (dt != null)
            {
                Load(dt);
            }
        }

        #endregion

        #region ISerialContext

        public void WriteContext(ISerializerContext context)
        {
            DataTable dt = new DataTable();
            dt = base.Copy();
            SerializeInfo info = new SerializeInfo();
            info.Add("Data", Data, typeof(DataTable));
            context.WriteSerializeInfo(info);
        }

        public void ReadContext(ISerializerContext context)
        {
            SerializeInfo info = context.ReadSerializeInfo();
            DataTable dt = (DataTable)info.GetValue("Data");
            Load(dt);
            
        }

        #endregion

    }
   

}
