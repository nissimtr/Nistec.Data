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

using System.Runtime.InteropServices;
//using Nistec.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.Runtime;
using System.IO;
using Nistec.IO;
using Nistec.Xml;
using Nistec.Serialization;

namespace Nistec.Data.Entities
{
    

    /// <summary>
    /// Represent Generic Data Values
    /// </summary>
    [Serializable, Serialize]
    public class GenericData : ISerialEntity, IDisposable
    {
        public const string EntityName = "EntityData";

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
        public static GenericData DeserializeDataSource(string serialaized, FormatterType format)
        {
            if (format == FormatterType.Xml)
                return GenericData.DeserializeFromXml(serialaized, null);
            else if (format == FormatterType.GZip)
                return (GenericData)NetSerializer.DeserializeFromBase64(NetZipp.UnZip(serialaized));
            else //if (format == SerializationFormat.Binary)
                return (GenericData)NetSerializer.DeserializeFromBase64(serialaized);
        }

        public string SerializeToXml(string entityName)
        {
            this.m_data.TableName = entityName ?? EntityName;
            return XSerializer.Serialize(m_data);
        }

        public static GenericData DeserializeFromXml(string xmlString, string entityName)
        {
            DataTable dt = XSerializer.Deserialize<DataTable>(xmlString);
            dt.TableName=entityName?? EntityName;
            return new GenericData(dt);
        }
        #endregion

        #region Dispose

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (m_data != null)
                    {
                        m_data.Dispose();
                        m_data = null;
                    }
                }
                //dispose unmanaged resources here.
                disposed = true;
            }
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

        ~GenericData()
        {
            Dispose(false);
        }

        #endregion

        #region members
 
        DataTable m_data;
        int index;
        bool m_readOnly;
        bool m_dirty;
        #endregion

        #region ctor

        public GenericData()
        {
            index = 0;
            m_readOnly = false;
            m_dirty = false;
        }

        public GenericData(DataTable dt)
        {
            index = 0;
            m_readOnly = false;
            m_dirty = false;
            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }
            Copy(dt);
        }
        public GenericData(DataTable dt, EntityKeys keys)
        {
            index = 0;
            m_readOnly = false;
            m_dirty = false;
            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }
            Copy(dt);
            SetPrimaryKeys(keys);
        }

        #endregion

        #region events

        public event EventHandler IndexChanged;

        protected virtual void OnIndexChanged(EventArgs e)
        {
            if (IndexChanged != null)
                IndexChanged(this, e);
        }

        public event EventHandler DataSourceChanged;

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
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

        #endregion

        #region Convert To

        /// <summary>
        /// Get values
        /// </summary>
        /// <returns></returns>
        public GenericRecord[] ToValues()
        {
            DataTable dt = m_data;
            List<GenericRecord> values = new List<GenericRecord>();
            foreach (DataRow dr in dt.Rows)
            {
                values.Add(new GenericRecord(dr));
            }
            return values.ToArray();
        }

        #endregion

        #region data methods
 
        public void Clear()
        {
            m_data.Clear();
            index = 0;
        }

        public void Copy(DataTable dt)
        {
            if (ReadOnly)
            {
                throw new Exception("Read only object");
            }
            index = 0;
            m_data = dt.Copy();
            m_data.TableName = EntityName;
        }

        public void AcceptChanges()
        {
            m_data.AcceptChanges();
            OnAcceptChanges(EventArgs.Empty);
        }

        public void RejectChanges()
        {
            m_data.RejectChanges();
            OnRejectChanges(EventArgs.Empty);
        }

        /// <summary>
        /// Select DataRows array from DataSource by filter
        /// </summary>
        /// <returns></returns>
        public DataRow[] Select()
        {
            if (IsEmpty)
                return null;
            return m_data.Select();
        }

        /// <summary>
        /// Select DataRows array from DataSource by filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DataRow[] Select(string filter)
        {
            if (IsEmpty)
                return null;
            if (string.IsNullOrEmpty(filter))
                return m_data.Select();
            return m_data.Select(filter);
        }
        /// <summary>
        /// Select DataRows array from DataSource by filter and sort fields
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public DataRow[] Select(string filter, string sort)
        {
            if (IsEmpty)
                return null;
            return m_data.Select(filter, sort);
        }
        #endregion

        #region rows methods

        /// <summary>
        /// Copies a System.Data.DataRow into a System.Data.DataTable, preserving any
        /// property settings, as well as original and current values.
        /// </summary>
        /// <param name="row">The System.Data.DataRow to be imported.</param>
        public void ImportRow(DataRow row)
        {
            if (m_data == null)
            {
                throw new Exception("Invalid data source");
            }

            ValidateReadOnly();

            if (row != null)
            {

                m_data.ImportRow(row);
                if (row.HasErrors)
                {
                    row.RowError = row.RowError;
                    DataColumn[] columnsInError = row.GetColumnsInError();
                    for (int i = 0; i < columnsInError.Length; i++)
                    {
                        DataColumn column = row.Table.Columns[columnsInError[i].ColumnName];
                        row.SetColumnError(column, row.GetColumnError(columnsInError[i]));
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new System.Data.DataRow with the same schema as the ActiveTable.
        /// </summary>
        public DataRow NewRow()
        {
            if (m_data == null)
            {
                throw new Exception("Invalid data source");
            }
 
            ValidateReadOnly();

            return m_data.NewRow();
        }

        /// <summary>
        /// Creates a row using specified values and adds it to the ActiveTable RowCollection.
        /// </summary>
        /// <param name="values"></param>
        public void AddRow(params object[] values)
        {
            if (m_data == null)
            {
                throw new Exception("Invalid data source");
            }
            
            ValidateReadOnly();

            m_data.Rows.Add(values);

            this.OnDataSourceChanged(EventArgs.Empty);
        }
        /// <summary>
        /// Adds the specified System.Data.DataRow to the ActiveTable
        /// </summary>
        /// <param name="row"></param>
        public void AddRow(DataRow row)
        {
            if (m_data == null)
            {
                throw new Exception("Invalid data source");
            }
            ValidateReadOnly();

            m_data.Rows.Add(row);
            this.OnDataSourceChanged(EventArgs.Empty);
        }
        /// <summary>
        /// Clears the collection of all rows.
        /// </summary>
        public void ClearRows()
        {
            if (m_data == null)
            {
                throw new Exception("Invalid data source");
            }
            ValidateReadOnly();

            m_data.Rows.Clear();
            this.OnDataSourceChanged(EventArgs.Empty);
        }
        /// <summary>
        /// Removes the row at the specified index from the collection.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveRow(int index)
        {
            if (m_data == null)
            {
                throw new Exception("Invalid data source");
            }
            ValidateReadOnly();


            m_data.Rows.RemoveAt(index);

            this.OnDataSourceChanged(EventArgs.Empty);
        }

        internal DataRow GetRowItem(int index)
        {
            DataRowCollection rows = Rows;
            if (rows == null)
            {
                throw new DataException("No rows were found");
            }
            if (index < 0 || index >= rows.Count)
            {
                throw new IndexOutOfRangeException("The given row index is out of range");
            }
            return rows[index];
        }

        internal T GetRowItem<T>(int index, string field)
        {
            DataRow rows = GetRowItem(index);
            return GenericTypes.Convert<T>(rows[field]);
        }

        public DataRow GetRow()
        {
            return GetRowItem(Index);// Rows[Index];
        }
        #endregion

        #region row edit methods

        /// <summary>
        /// Canceles the current edit on the row.
        /// </summary>
        public void CancelEdit()
        {
            if (!ValidCurrent(false)) return;
            m_data.Rows[Index].CancelEdit();
        }

        /// <summary>
        /// Starts an edit operation on a System.Data.DataRow object.
        /// </summary>
        /// <exception cref="System.Data.InRowChangingEventException">The method was called inside the System.Data.DataTable.RowChanging event.</exception>
        /// <exception cref="System.Data.DeletedRowInaccessibleException">The method was called upon a deleted row.</exception>
        public void BeginEdit()
        {
            if (!ValidCurrent(true)) return;
            m_data.Rows[Index].BeginEdit();
        }
        /// <summary>
        /// Clears the errors for the row. This includes the System.Data.DataRow.RowError
        /// and errors set with System.Data.DataRow.SetColumnError(System.Int32,System.String).
        /// </summary>
        public void ClearErrors()
        {
            if (!ValidCurrent(false)) return;
            m_data.Rows[Index].ClearErrors();
        }
        /// <summary>
        /// Deletes the System.Data.DataRow.
        /// </summary>
        ///<exception cref="System.Data.DeletedRowInaccessibleException">The System.Data.DataRow has already been deleted.</exception>
        public void Delete()
        {
            if (!ValidCurrent(true)) return;
            m_data.Rows[Index].Delete();
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
            if (!ValidCurrent(true)) return;
            m_data.Rows[Index].EndEdit();
        }
      
        #endregion

        #region row property

        /// <summary>
        /// Gets a value that indicates whether there are errors in a row.
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public virtual bool HasErrors
        {
            get
            {
                if (!ValidCurrent(false)) return false;
                return m_data.Rows[Index].HasErrors;
            }
        }
        /// <summary>
        ///  Gets the current state of the row with regard to its relationship to the
        ///     System.Data.DataRowCollection.
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public virtual DataRowState RowState
        {
            get
            {
                if (!ValidCurrent(false)) return DataRowState.Unchanged;
                return m_data.Rows[Index].RowState;
            }
        }

        /// <summary>
        /// Gets or sets the custom error description for a row.
        /// </summary>
        [EntityProperty(EntityPropertyType.NA), NoSerialize]
        public virtual string RowError
        {
            get
            {
                if (!ValidCurrent(false)) return "";
                return m_data.Rows[Index].RowError;
            }
            set
            {
                if (!ValidCurrent(false)) return;
                m_data.Rows[Index].RowError = value;

            }
        }

        #endregion

        
        #region validatating

        /// <summary>
        /// Validate current index
        /// </summary>
        /// <param name="validateEdit"></param>
        /// <returns></returns>
        internal bool ValidCurrent(bool validateEdit)
        {
            if (IsEmpty)
            {
                return false;
            }
            if (validateEdit && ReadOnly)
            {
                return false;
            }
            return (index >= 0 && index < Count);
        }

  
        /// <summary>
        /// Validate if edit is ReadOnly
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        internal void ValidateReadOnly()
        {
            if (ReadOnly)
            {
                throw new ArgumentException("Data is ReadOnly");
            }
        }
        /// <summary>
        /// Validate if edit is enabled
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        internal void ValidateEdit()
        {
            ValidateCurrent();
            if (ReadOnly)
            {
                throw new ArgumentException("Data is ReadOnly");
            }
        }
        /// <summary>
        /// Validate data and index position
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        internal void ValidateCurrent()
        {
            if (m_data == null)
            {
                throw new ArgumentException("Invalid Data source");
            }
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
                return this.Data.PrimaryKey != null && this.Data.PrimaryKey.Length>0;
            }
        }

        /// <summary>
        /// Get <see cref="EntityKeys"/>
        /// </summary>
        public EntityKeys PrimaryKeys
        {
            get
            {
                return new EntityKeys(this.Data.PrimaryKey);
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
            keys.SetPrimaryKeys(this.Data);
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
        /// Get indicate if is it ReadOnly
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool ReadOnly
        {
            get { return m_readOnly; }
            internal set
            {
                if (m_readOnly != value)
                {
                    m_readOnly = value;
                }
                if (m_readOnly)
                {
                    Copy(m_data);
                }
            }
        }

        /// <summary>
        /// Get Data source (When is ReadOnly return a Copy)
        /// </summary>
        [Serialize]
        public DataTable Data
        {
            get 
            {
                if (m_data == null)
                {
                    m_data = new DataTable(EntityName);
                }
                return m_data; 
            }
            private set { m_data = value; }
        }

        /// <summary>
        /// Get Data Row Collection
        /// </summary>
        internal DataRowCollection Rows
        {
            get { return IsEmpty ? null : m_data.Rows; }
        }
        /// <summary>
        /// Get Data Column Collection
        /// </summary>
        internal DataColumnCollection Columns
        {
            get { return IsEmpty ? null : m_data.Columns; }
        }
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
                return m_data.Rows.Count;
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
                if (!ValidCurrent(false))
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
        [EntityProperty(EntityPropertyType.NA),NoSerialize]
        public object this[int columnIndex]
        {
            get
            {
                if (!ValidCurrent(false))
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
                if (!ValidCurrent(false)) return null;
                return Rows[Index].ItemArray;
            }
            set
            {
                if (!ValidCurrent(true)) return;
                Rows[Index].ItemArray = value;
            }
        }

        /// <summary>
        /// Get the value indicating that data source IsEmpty
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool IsEmpty
        {
            get { return m_data == null || m_data.Rows == null || m_data.Rows.Count == 0; }
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
                    throw new EntityException("Generic table Is empty");
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
            ValidateReadOnly();
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
            ValidateReadOnly();
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
            ValidateReadOnly();
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
        public static GenericData Create(EntityDbContext db, params object[] keys)
        {
            DataTable dt = db.QueryEntity<DataTable>(keys);
            return new GenericData(dt);
        }

  
        /// <summary>
        /// Initialize a new instance of GenericData
        /// </summary>
        /// <param name="db"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <returns></returns>
        public static GenericData Create(EntityDbContext db, IDbDataParameter[] parameters)
        {
            string cmdText = SqlFormatter.SelectString("*", db.MappingName, SqlFormatter.ParametersString(parameters));

            DataTable dt = db.DoCommand<DataTable>(cmdText, parameters, CommandType.Text, 0);
            return new GenericData(dt);

        }

        /// <summary>
        /// Initialize a new instance of GenericData
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cmdType">Specifies how a command string is interpreted.</param>
        /// <returns></returns>
        public static GenericData Create<Dbc>(string cmdText, IDbDataParameter[] parameters, CommandType cmdType) where Dbc : IDbContext
        {
            using (IDbContext db = DbContext.Create<Dbc>())
            {
                DataTable dt = null;
                //using (IDbCmd cmd = db.DbCmd())
                //{
                    dt = db.ExecuteCommand<DataTable>(cmdText, parameters, cmdType, 0, true);
                //}
                return new GenericData(dt);
            }
        }

        #endregion

        #region IEntityFormatter


        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            streamer.WriteValue(m_data);
        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            DataTable dt = (DataTable)streamer.ReadValue();

            if (dt != null)
            {
                m_data = dt;
            }
        }
   
        #endregion


    }
    
}
