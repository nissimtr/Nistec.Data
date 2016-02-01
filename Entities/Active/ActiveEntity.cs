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
using System.Reflection;
using System.Data;

using System.Text;
using System.Collections.Generic;
using System.Globalization;
using Nistec.Data.Factory;
using System.IO;
using Nistec.IO;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.Serialization;

namespace Nistec.Data.Entities
{


    /// <summary>
    /// Represents ActiveEntity which implements <see cref="IDataEntity"/> 
    /// </summary>
    [Serializable]
    public class ActiveEntity : EntityBase, IDataEntity, IActiveEntity, IEntityDictionary
    {
        #region members
        //rcd
        GenericRecord _Data;

        protected override void DisposeInner(bool disposing)
        {
            base.DisposeInner(disposing);
            if (disposing)
            {
                if (_FieldsChanged != null)
                {
                    _FieldsChanged.Clear();
                    _FieldsChanged = null;
                }
                if (_Data != null)
                {
                     _Data = null;
                }
            }
        }

        #endregion

        #region Ctor

       
        /// <summary>
        /// Initialize a new instance of ActiveEntity
        /// You can bind this entity to DB by using Entity attribute or by EntityBind method
        /// </summary>
        public ActiveEntity()
        {
            _Data = new GenericRecord();//rcd

            m_EntityMode = EntityPropertyBuilder.BuildEntityDb(this, EntityLocalizer.DefaultCulture);

            EntityBind();

            if (HasConnection() && m_EntityMode == EntityMode.Reflection)
            {
                EntityPropertyBuilder.SetEntityContext(this, _Data);
            }
        }

        /// <summary>
        /// Initialize a new instance of ActiveEntity with specified culture
        /// You can bind this entity to DB by using Entity attribute or by EntityBind method
        /// </summary>
        /// <param name="culture"></param>
        protected ActiveEntity(CultureInfo culture)
        {
            _Data = new GenericRecord();//rcd

            m_EntityMode = EntityPropertyBuilder.BuildEntityDb(this,culture);

            EntityBind();

            if (HasConnection() && m_EntityMode == EntityMode.Reflection)
            {
                EntityPropertyBuilder.SetEntityContext(this, _Data);
            }
        }
        
        /// <summary>
        /// Initialize a new instance of ActiveEntity using array of entity keys
        /// You can bind this entity to DB by using Entity attribute or by EntityBind method
        /// </summary>
        /// <param name="keys"></param>
        protected ActiveEntity(params object[] keys)
        {
            EntityMode mode = EntityPropertyBuilder.BuildEntityDb(this, EntityLocalizer.DefaultCulture);
            InitEntity(mode, keys);
        }

        /// <summary>
        /// Initialize a new instance of ActiveEntity with specified EntityMode
        /// You can bind this entity to DB by using Entity attribute or by EntityBind method
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="keys"></param>
        protected ActiveEntity(EntityMode mode, params object[] keys)
        {
            InitEntity(mode, keys);
        }

      
        /// <summary>
        /// Initialize a new instance of ActiveEntity with specified DataRow
        /// Usefull for dynamic entity
        /// </summary>
        /// <param name="dr"></param>
        internal protected ActiveEntity(DataRow dr)
        {
            _Data = new GenericRecord(dr);//rcd

            m_EntityMode = EntityPropertyBuilder.BuildEntityDb(this, EntityLocalizer.DefaultCulture);

            EntityBind();

            if (HasConnection() && m_EntityMode == EntityMode.Reflection)
            {
                EntityPropertyBuilder.SetEntityContext(this, _Data);
            }
        }


        /// <summary>
        /// Initialize a new instance of ActiveEntity 
        /// Create Vertical GenericRecord (key value dictionary) by fetching keys and values from DataTable using array of fields as row key and value field
        /// Usefull for Vertical combination of data and also for dynamic entity
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fieldKeys"></param>
        /// <param name="fieldValue"></param>
        protected ActiveEntity(DataTable dt, string[] fieldKeys, string fieldValue)
        {
            m_EntityMode = EntityPropertyBuilder.BuildEntityDb(this, EntityLocalizer.DefaultCulture);

            if (_Data == null)
            {
                _Data = GenericEntity.DataTableToDictionary(dt, fieldKeys, fieldValue).Record;//rcd
            }

            EntityBind();

            if (HasConnection() && m_EntityMode == EntityMode.Reflection)
            {
                EntityPropertyBuilder.SetEntityContext(this, _Data);
            }
        }


        /// <summary>
        /// Initialize a new instance of ActiveEntity and create array of entities with specified DataTable
        /// usefull to create array of entities and also for dynamic entity
        /// </summary>
        /// <param name="dt"></param>
        protected ActiveEntity(DataTable dt)
        {
            EntityDataSource = GenericDataTable.Convert(dt);// new GenericData(dt);
            m_EntityMode = EntityMode.Multi;
            EntityBind();
        }

        /// <summary>
        /// Create a new instance of EntityDb.
        /// </summary>
        /// <typeparam name="Dbc"></typeparam>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static EntityDbContext CreateEntityDb<Dbc>(string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys) where Dbc : IDbContext
        {
            IDbContext dbc = DbContext.Get<Dbc>();
            return new EntityDbContext(dbc, entityName, mappingName, sourceType, keys);
        }

        #endregion

        #region Init

        /// <summary>
        /// Bind to EntityDbContext, Occured on constrauctor
        /// </summary>
        protected virtual void EntityBind()
        {

        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="keys"></param>
        internal void InitEntity(EntityMode mode, params object[] keys)
        {
            m_EntityMode = mode;

            _Data = new GenericRecord();

            EntityBind();

            EntityDb.ValidateContext();

            switch (mode)
            {
                case EntityMode.Multi:
                    EntityDataSource = GenericDataTable.Convert(EntityDb.QueryEntity<DataTable>(keys));
                    break;
                case EntityMode.Reflection:
                    Init(EntityDb.QueryEntity<DataRow>(keys));
                    EntityPropertyBuilder.SetEntityContext(this, _Data);
                    break;
                case EntityMode.Generic:
                    Init(EntityDb.QueryEntity<DataRow>(keys));
                    break;
            }
        }

 

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="row"></param>
        protected void Init(DataRow row)
        {
            _Data = new GenericRecord(row);//rcd
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="row"></param>
        /// <param name="readOnly"></param>
        protected void Init(DataRow row, bool readOnly)
        {
            _Data = new GenericRecord(row);//rcd

        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="isMulti"></param>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="parameters">SqlParameter array key value.</param>
        /// <param name="cmdType">Specifies how a command string is interpreted.</param>
        protected void Init(bool isMulti, string cmdText, IDbDataParameter[] parameters, CommandType cmdType)
        {
            if (isMulti)
            {
                InitDataSource(cmdText, parameters, cmdType);
            }
            else
            {
                InitRow(cmdText, parameters, cmdType);
            }

            //DataRow row = null;
            //using (IDbCmd cmd = EntityDb.DbCmd())
            //{
            //    row = cmd.ExecuteCommand<DataRow>(cmdText, cmdType, 0, parameters, true);
            //}
            //Init(row);
        }

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="isMulti"></param>
        /// <param name="parameters">SqlParameter array key value.</param>
        protected void Init(bool isMulti, IDbDataParameter[] parameters)
        {
            string cmdText = null;
            CommandType cmdType = EntityDb.CmdType();

            if (cmdType == CommandType.StoredProcedure)
                cmdText = EntityDb.MappingName;
            else
                cmdText = SqlFormatter.SelectString("*", EntityDb.MappingName, SqlFormatter.ParametersString(parameters));

            if (isMulti)
            {
                InitDataSource(cmdText, parameters, cmdType);
            }
            else
            {
                InitRow(cmdText, parameters, cmdType);
            }
        }

        private void InitRow(string cmdText, IDbDataParameter[] parameters, CommandType cmdType)
        {
            DataRow row = EntityDb.DoCommand<DataRow>(cmdText, parameters, cmdType, 0);
          
            Init(row);
        }

       
        #endregion

        #region IEntityDictionary
        
        public IDictionary EntityDictionary()
        {
            return _Data==null ?null: _Data.EntityDictionary(); 
        }

       
        public Type EntityType()
        {
            return typeof(ActiveEntity); 
        }


        public virtual void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            EntityRecord.EntityWrite(stream, streamer);
        }

        public virtual void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            EntityRecord.EntityRead(stream, streamer);
        }
     
        #endregion

        #region properties
      
        [EntityProperty(EntityPropertyType.NA)]
        public override bool IsEmpty
        {
            get { return _Data == null || _Data.Count == 0; }
        }

      

        #endregion

        #region Values
        //rcd
        /// <summary>
        /// Get or Set properties values as IDictionary
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public override GenericRecord EntityRecord
        {
            get { return _Data; }
            set { _Data = (GenericRecord)value; }
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>T</returns>
        public override T GetValue<T>(string field)
        {
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Data ", field);
            }
            if (!_Data.ContainsKey(field))
                return GenericTypes.Default<T>();// default(T);
            return _Data.GetValue<T>(field);
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue">if null or error return defaultValue will return</param>
        /// <returns>if null or error return defaultValue</returns>
        public override T GetValue<T>(string field, T defaultValue)
        {
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Data ", field);
            }
            if (!_Data.ContainsKey(field))
                return GenericTypes.Default<T>();//default(T);
            return _Data.GetValue<T>(field, defaultValue);
        }

        /// <summary>
        ///  Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">>The key whose value to get.</param>
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
        protected override bool TryGetValue<T>(string field, out T value)
        {
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Data ", field);
            }
            return _Data.TryGetValue<T>(field, out value);
        }

        /// <summary>
        /// Set Value in specified field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        protected override void SetValue<T>(string field, T value)
        {
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Data ", field);
            }
            if (!_Data.ContainsKey(field))
            {
                AddNewField(field);
            }
            else if (!_Data.CompareValues<T>(field, value))
            {
                AddChanges(field);
            }
            _Data.SetValue<T>(field, value);
        }

        /// <summary>
        /// Set Value in specified field
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        protected override void SetValue(string field, object value)
        {
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Data ", field);
            }
            if (!_Data.ContainsKey(field))
            {
                AddNewField(field);
            }
            else if (!_Data.CompareValues(field, value))
            {
                AddChanges(field);
            }
            _Data.SetValue(field, value);
        }

        #endregion

        #region override

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);
        }

        protected override void OnPositionChanged(EventArgs e)
        {
            base.OnPositionChanged(e);

            DataRow dr = _DataSource.Rows[Position];

            Init(dr);

        }


        #endregion

        #region Generic Table

        /// <summary>
        /// Load a new entity that contains the specified primary key values.
        /// </summary>
        public void Load(params object[] keys)
        {
            Init(EntityDb.QueryEntity<DataRow>(keys));
        }

     

        /// <summary>
        /// Find and load entity record from DataSource that contains the specified primary key values.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>true if found else false</returns>
        public bool GoTo(params object[] keys)
        {
            if (Count <= 0)
                return false;
            if (!_DataSource.HasPrimaryKey)
                return false;
            return _DataSource.GoTo(keys);

  
        }

        /// <summary>
        /// Get Current item by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected IEntity Get(int index)
        {
            Position = index;
            return this;
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

        
        void ValidateData()
        {
            if (_Data == null)
            {
                _Data = new GenericRecord();//rcd
            }
        }

        void AddNewField(string field)
        {
            FieldsChanged.Add(field, EntityNewField);
        }

        void AddChanges(string field)
        {
            FieldsChanged[field] = _Data[field];
        }
        /// <summary>
        /// Clear all changes
        /// </summary>
        public void ClearChanges()
        {
            if (IsDirty())
            {
                Restor();
                //FieldsChanged.Clear();
            }
        }

        /// <summary>
        /// End edit and save all changes localy
        /// </summary>
        internal void CommitChanges()
        {
            if (IsDirty())
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
                    _Data.Remove(entry.Key);
                else
                    _Data[entry.Key] = entry.Value;
            }

            FieldsChanged.Clear();
        }

        public Dictionary<string, object> GetFieldsChanged()
        {
            Dictionary<string, object> fc = new Dictionary<string, object>();
            foreach (string k in FieldsChanged.Keys)
            {
                fc[k] = _Data[k];
            }

            return fc;
        }

        /// <summary>
        /// Get indicate if data source has changes
        /// </summary>
        //[EntityProperty(EntityPropertyType.NA)]
        public bool IsDirty()
        {
           if (_FieldsChanged != null && _FieldsChanged.Count > 0)return true; return false; 
        }

        #endregion

        #region update

        

        /// <summary>
        /// Save all Changes to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            //if (m_IsDynamic)
            //    return -1;
            //ValidateUpdate();
            return SaveChanges(UpdateCommandType.Update);
        }

        /// <summary>
        /// Save all Changes by <see cref="UpdateCommandType"/> specific command to DB and return number of AffectedRecords
        /// If not <see cref="IsDirty"/> which mean no changed has been made return 0
        /// </summary>
        /// <param name="commandType"></param>
        /// <returns></returns>
        /// <exception cref="EntityException"></exception>
        /// <exception cref="DalException"></exception>
        protected int SaveChanges(UpdateCommandType commandType)
        {
            if (m_EntityMode != EntityMode.Generic)
            {
                throw new EntityException("SaveChanges not supported in current EntityMode.");
            }
            ValidateUpdate();

            if (!IsDirty())
                return 0;
            EntityDbContext db = EntityDb;
            EntityCommandResult res = null;
            using (EntityCommandBuilder ac = new EntityCommandBuilder(this, db.DbConnection(), db.MappingName))
            {
                res = ac.ExecuteCommand(commandType);
            }
            if (res == null)
            {
                throw new EntityException("SaveChanges was not succeed.");
            }
            if (commandType == UpdateCommandType.Insert)
            {
                foreach (KeyValuePair<string, object> p in res.OutputValues)
                {
                    SetValue(p.Key, p.Value);
                }
            }
            CommitChanges();
            return res.AffectedRecords;
        }

        /// <summary>
        /// Set entity values by Executing a Query with commandText argument.
        /// Return number of AffectedRecords
        /// </summary>
        /// <param name="commandText">StoredProcedure name</param>
        /// <returns></returns>
        /// <exception cref="EntityException"></exception>
        /// <exception cref="DalException"></exception>
        protected int ExecuteQuery(string commandText)
        {
            if (m_EntityMode != EntityMode.Generic)
            {
                throw new EntityException("ExecuteQuery not supported in current EntityMode.");
            }
            EntityDbContext db = EntityDb;
            EntityCommandResult res = null;
            using (EntityCommandBuilder ac = new EntityCommandBuilder(this, db.DbConnection(), commandText))
            {
                res = ac.ExecuteQuery(commandText);
            }
            if (res == null)
            {
                throw new EntityException("ExecuteQuery was not succeed.");
            }
            foreach (KeyValuePair<string, object> p in res.OutputValues)
            {
                SetValue(p.Key, p.Value);
            }
            CommitChanges();
            return res.AffectedRecords;
        }
        #endregion

    }

}
