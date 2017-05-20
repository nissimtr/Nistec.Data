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
using System.Diagnostics;
using System.Collections.Generic;
using Nistec.Data.Factory;
using System.IO;
using Nistec.IO;
using Nistec.Serialization;
using Nistec.Generic;

namespace Nistec.Data.Entities
{

    /// <summary>
    /// Represent an entity table that contains a multiple rows.
    /// </summary>
    [Serializable]
    public class EntityTable : EntityBase, IDataEntity
    {
        #region Dispose

        protected override void DisposeInner(bool disposing)
        {
            if (m_AsyncCmd != null)
            {
                m_AsyncCmd.Dispose();
                m_AsyncCmd = null;
            }
            
            base.DisposeInner(disposing);
        }

       
        #endregion

        #region Ctor and initilaize methods
        /// <summary>
        /// ctor
        /// </summary>
        public EntityTable()
        {
            EntityPropertyBuilder.BuildEntityDb(this, EntityLocalizer.DefaultCulture);

            EntityBind();

        }

   
        /// <summary>
        /// ctor
        /// </summary>
        public EntityTable(params object[] keys)
        {
            EntityPropertyBuilder.BuildEntityDb(this, EntityLocalizer.DefaultCulture);

            EntityBind();

            Init(EntityDb.QueryEntity<DataTable>(keys));
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="table"></param>
        public EntityTable(DataTable table)
        {
            if (table == null)
            {
                throw new ArgumentException("Invalid Data Table ");
            }
            EntityPropertyBuilder.BuildEntityDb(this, EntityLocalizer.DefaultCulture);
            Init(table, false);//, false);
            EntityBind();
        }

        /// <summary>
        /// Bind to EntityDbContext, Occured on constrauctor
        /// </summary>
        protected virtual void EntityBind()
        {

        }
        /// <summary>
        /// Init data source schema
        /// </summary>
        /// <param name="table"></param>
        protected virtual void InitSchema(DataTable table)
        {
            SyncTableSchema(table);
        }
        /// <summary>
        /// Init data source
        /// </summary>
        /// <param name="table"></param>
        protected virtual void Init(DataTable table)
        {
            SyncTable(table, false);
        }

      
        /// <summary>
        /// Init data source
        /// </summary>
        /// <param name="table"></param>
        /// <param name="isCopy"></param>
        /// <param name="readOnly"></param>
        protected virtual void Init(DataTable table, bool isCopy)//, bool readOnly)
        {
            SyncTable(table, isCopy);
        }

        #endregion

        #region Entity table property
        
        
        //rcd//
        /// <summary>
        /// Get properties values as IDictionary
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public override GenericRecord EntityRecord
        {

            get { return _DataSource.CurrentRecord(); }
            set { }
        }

        #endregion

        #region IEntity GetValue

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>object</returns>
        public override T GetValue<T>(string field)
        {
            if (_DataSource == null)
            {
                throw new ArgumentException("Invalid Data source");
            }
            if (Position == -1 || Position > Count)
            {
                throw new ArgumentException("Index out of range ", Position.ToString());
            }
            return EntityDataSource.GetValue<T>(field);
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>if null or error return defaultValue</returns>
        /// <returns>T</returns>
        public override T GetValue<T>(string field, T defaultValue)
        {
            if (_DataSource == null)
            {
                throw new ArgumentException("Invalid Data source");
            }
            if (Position == -1 || Position > Count)
            {
                throw new ArgumentException("Index out of range ", Position.ToString());
            }
            return EntityDataSource.GetValue<T>(field, defaultValue);
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
        protected override bool TryGetValue<T>(string field, out T value)
        {
            if (_DataSource == null)
            {
                throw new ArgumentException("Invalid Data source");
            }
            if (Position == -1 || Position > Count)
            {
                throw new ArgumentException("Index out of range ", Position.ToString());
            }
            return EntityDataSource.TryGetValue<T>(field, out value);
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        protected override void SetValue<T>(string field, T value)
        {
            //ValidateReadOnly();

            if (_DataSource == null)
            {
                throw new ArgumentException("Invalid Data source");
            }
            if (Position == -1 || Position > Count)
            {
                throw new ArgumentException("Index out of range ", Position.ToString());
            }
            EntityDataSource.SetValue<T>(field, value);
            this.OnValueChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Set Value
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        protected override void SetValue(string field, object value)
        {
            //ValidateReadOnly();

            if (_DataSource == null)
            {
                throw new ArgumentException("Invalid Data source");
            }
            if (Position == -1 || Position > Count)
            {
                throw new ArgumentException("Index out of range ", Position.ToString());
            }
            EntityDataSource.SetValue(field, value);
            this.OnValueChanged(EventArgs.Empty);

        }
        #endregion

        #region override
                      
        /// <summary>
        /// Get the value indicating that data source IsEmpty
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public override bool IsEmpty
        {
            get { return _DataSource == null || _DataSource.Rows.Count == 0; }
        }
  
        /// <summary>
        /// Refresh
        /// </summary>
        public override void Refresh()
        {
            if (m_AsyncCmd != null)
            {
               m_AsyncCmd.AsyncExecute();
            }
        }

        /// <summary>
        /// Find and load entity record from DataSource that contains the specified primary key values.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>true if found else false</returns>
        public bool GoTo(params object[] keys)
        {
            if (_DataSource == null)
                return false;
            return _DataSource.GoTo(keys);
        }

        #endregion

        #region sync data

        static object syncRoot = new object();
 
        /// <summary>
        /// Occured when Async command Completed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAsyncCompleted(Nistec.Threading.AsyncDataResultEventArgs e)
        {
            SyncTable(e.Table, false);
        }

        /// <summary>
        /// Occured when Async command in Progress
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAsyncProgress(Nistec.Threading.AsyncProgressEventArgs e)
        {
            // _Message = e.Message;
        }

        internal void SyncTable(DataTable dt, bool isCopy)
        {
            if (dt == null)// || dt.Rows.Count == 0)
                return;
            lock (syncRoot)
            {
                if (isCopy)
                    _DataSource = GenericDataTable.Convert(dt.Copy());
                else
                    _DataSource = GenericDataTable.Convert(dt);
            }
            OnDataSourceChanged(EventArgs.Empty);
            //AcceptChanges();
        }

        internal void SyncTableSchema(DataTable dt)
        {
            if (dt == null)// || dt.Rows.Count == 0)
                return;
            lock (syncRoot)
            {
                _DataSource = GenericDataTable.Convert(dt);//.Clone();
            }
            OnDataSourceChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Occured when ReadOnly property Changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnReadOnlyChanged(EventArgs e)
        {

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
            ValidateUpdate();
      
            int res = 0;
            EntityDbContext db = EntityDb;
            string mappingName = db.MappingName;
            db.ValidateContext();
            using (IDbAdapter adapter = AdapterFactory.CreateAdapter(db.Context()))
            {
                res = adapter.UpdateChanges(_DataSource, mappingName);
            }
            EntityDataSource.AcceptChanges();
            OnAcceptChanges(EventArgs.Empty);
            return res;
        }

        /// <summary>
        /// Fill data source entity using current EntityDbContext
        /// </summary>
        public void FillEntity()
        {
            EntityDbContext db = EntityDb;
            db.ValidateContext();
            string commandText = db.GetCommandText(true);
            DataTable dt = null;
            using (IDbCmd dbCmd = DbFactory.Create(db.Context()))
            {
                dt = dbCmd.ExecuteDataTable(db.EntityName, commandText, true);
            }
            SyncTable(dt, false);
        }

        #endregion

        #region AsyncCommand

        EntityAsyncCommand m_AsyncCmd;

        /// <summary>
        /// Get EntityAsyncCommand
        /// </summary>
        public EntityAsyncCommand EntityAsyncCmd
        {
            get
            {
                if (m_AsyncCmd == null)
                {
                    m_AsyncCmd = new EntityAsyncCommand(this);
                }
                return m_AsyncCmd;
            }
        }

        #endregion
 
        #region EntityAsyncCommand

        /// <summary>
        /// Represent an async entity command
        /// </summary>
        public class EntityAsyncCommand
        {
            #region members and ctor

            EntityTable owner;

            internal EntityAsyncCommand(EntityTable entity)
            {
                owner = entity;
            }

            internal void Dispose()
            {
                if (cmd != null)
                {
                    cmd.AsyncProgress -= new Nistec.Threading.AsyncProgressEventHandler(cmd_AsyncProgress);
                    cmd.AsyncCompleted -= new Nistec.Threading.AsyncDataResultEventHandler(cmd_AsyncCompleted);
                    cmd.Dispose();
                    cmd = null;
                }
            }
            #endregion

            #region members and properties

            uint _Timeout = 100000;
            /// <summary>
            /// Get or Set the Timeout in milliseconds,the minimum is 1000 milliseconds
            /// </summary>
            [EntityProperty(EntityPropertyType.NA)]
            protected uint AsyncTimeout
            {
                get { return _Timeout; }
                set
                {
                    if (value > 1000)
                        _Timeout = value;
                }
            }
            bool _IsRunning = false;
            /// <summary>
            /// Get the value indicating that command is running
            /// </summary>
            [EntityProperty(EntityPropertyType.NA)]
            internal bool IsRunning
            {
                get { return _IsRunning; }
            }


            AsyncCommand cmd;
            static object syncRoot = new object();
            public event EventHandler CommandCompleted;

            #endregion

            #region methods
          
            private bool Wait()
            {
                int count = 0;
                while (_IsRunning)// || cmd.AsyncState == Nistec.Threading.AsyncState.Started)
                {
                    System.Threading.Thread.Sleep(10);
                    count++;
                    if (count > (_Timeout / 10))
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Execute async command and fill entity data source
            /// </summary>
            public void AsyncExecute()
            {
                AsyncExecute(10000);
            }

            /// <summary>
            /// Execute async command and fill entity data source
            /// </summary>
            /// <param name="timeout"></param>
            public void AsyncExecute(uint timeout)
            {
                AsyncTimeout = timeout;
                EntityDbContext edb = owner.EntityDb;
                IDbContext db = edb.Context();
                string commandText = owner.EntityDb.GetCommandText(true);
                if (cmd == null)
                {
                    cmd = new AsyncCommand(db.ConnectionString, db.Provider);
                    cmd.AsyncProgress += new Nistec.Threading.AsyncProgressEventHandler(cmd_AsyncProgress);
                    cmd.AsyncCompleted += new Nistec.Threading.AsyncDataResultEventHandler(cmd_AsyncCompleted);

                }
                lock (EntityTable.syncRoot)
                {
                    cmd.AsyncBeginInvoke(commandText, edb.EntityName);
                    _IsRunning = true;
                }
            }

            /// <summary>
            /// Stop executing
            /// </summary>
            public void StopExecution()
            {
                if (cmd != null)
                {
                    cmd.StopCurrentExecution();
                    _IsRunning = false;
                }
            }

            void cmd_AsyncCompleted(object sender, Nistec.Threading.AsyncDataResultEventArgs e)
            {
                _IsRunning = false;
                owner.OnAsyncCompleted(e);
                if (CommandCompleted != null)
                    CommandCompleted(this, EventArgs.Empty);
            }

            void cmd_AsyncProgress(object sender, Nistec.Threading.AsyncProgressEventArgs e)
            {
                owner.OnAsyncProgress(e);
                if (e.Level == Nistec.Threading.AsyncProgressLevel.Error)
                {
                    _IsRunning = false;
                }
            }
            #endregion
        }
        #endregion

        #region IEntityDictionary

        public IDictionary EntityDictionary()
        {
            GenericRecord gr = EntityRecord;
            if (gr == null)
                return null;
            return gr.EntityDictionary();
            //return _Data == null ? null : _Data.EntityDictionary();
        }
        [EntityProperty(EntityPropertyType.NA)]
        public Type EntityType
        {
            get { return typeof(ActiveEntity); }
        }

  
        public virtual void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            GenericRecord gr = EntityRecord;
            if (gr == null)
                return;
            gr.EntityWrite(stream, streamer);
        }

        public virtual void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            GenericRecord gr = EntityRecord;
            if (gr == null)
                return;
            EntityRecord.EntityRead(stream, streamer);
        }
   
        #endregion
    }

}
