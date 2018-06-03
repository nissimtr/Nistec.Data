//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
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
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using Nistec.Generic;
using Nistec.Xml;
using Nistec.Runtime;
using System.Collections.Concurrent;
using Nistec.Data.Entities;
using System.Transactions;
using Nistec.Config;

namespace Nistec.Data.Sqlite
{
   
    /// <summary>
    /// Represent a Config file as Dictionary key-value
    /// <example>
    /// <sppSttings>
    /// <myname value='nissim' />
    /// <mycompany value='mcontrol' />
    /// </sppSttings>
    /// </example>
    /// </summary>
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class PersistentDictionaryIndex<T> //:IDictionary<string,T>
    {

        ConcurrentDictionary<string,T> dictionary;
        ConcurrentDictionary<string, long> index;
        //XDictionaryettings settings;
        //string connectionString;
        //string dbName;
        public const string DbProvider = DbLite.ProviderName;

        #region properties

        public XDictionarySettings Settings { get; protected set; }
        public string ConnectionString { get; protected set; }
        public string Name { get; protected set; }

        /// <summary>
        /// Get value indicating if the config file exists
        /// </summary>
        public bool FileExists
        {
            get
            {
                return File.Exists(Settings.DbFilename);
            }
        }

        

       // /// <summary>
       // ///     Gets a collection containing the keys in the System.Collections.Generic.Dictionary{TKey,TValue}.
       // ///
       // /// Returns:
       // ///     An System.Collections.Generic.ICollection{TKey} containing the keys in the
       // ///     System.Collections.Generic.Dictionary{TKey,TValue}.
       // /// </summary>
       // public ICollection<string> Keys  
       //{
       //    get { return dictionary.Keys; }
       //}

        
       // /// <summary>
       // ///     Gets a collection containing the values in the System.Collections.Generic.Dictionary{TKey,TValue}.
       // ///
       // /// Returns:
       // ///     An System.Collections.Generic.ICollection{TValue} containing the values in
       // ///     the System.Collections.Generic.Dictionary{TKey,TValue}.
       // /// </summary>
       // public ICollection<T> Values
       // {
       //     get { return dictionary.Values; }
       // }


        ///// <summary>
        ///// Get all items as array of KeyValuePair
        ///// </summary>
        ///// <returns></returns>
        //public KeyValuePair<string, T>[] ToArray()
        //{
        //    return dictionary.ToArray();
        //}

        #endregion

        #region events

        bool ignorEvent = false;
        //public event ConfigChangedHandler ItemChanged;
        //public event EventHandler ConfigFileChanged;
        public event GenericEventHandler<string> ErrorOcurred;
        public event EventHandler Initilaized;
        public event GenericEventHandler<string, string, T> ItemChanged;

        protected virtual void OnInitilaized(EventArgs e)
        {
            if (Initilaized != null)
            {
                Initilaized(this, e);
            }
        }

        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
            {
                ErrorOcurred(this,e);
            }
        }

        protected virtual void OnItemChanged(string action,string key, T value)
        {
            if (!ignorEvent)
                OnItemChanged(new GenericEventArgs<string, string, T>(action, key, value));
        }

        protected virtual void OnItemChanged(GenericEventArgs<string, string, T> e)
        {
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

        //protected virtual void OnItemChanged(ConfigChangedArgs e)
        //{
        //    //if (settings.AutoSave)
        //    //{
        //    //    Save();
        //    //}
        //    if (ItemChanged != null)
        //        ItemChanged(this, e);
        //}

        // protected virtual void OnConfigFileChanged(EventArgs e)
        //{
        //    FileToDictionary();

        //    if (ConfigFileChanged != null)
        //        ConfigFileChanged(this, e);
        //}

        #endregion

        #region ctor



        /// <summary>
        /// PersistentDictionary ctor with a specefied filename
        /// </summary>
        /// <param name="filename"></param>
        public PersistentDictionaryIndex(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("PersistentDictionary.filename");
            }
            dictionary = new ConcurrentDictionary<string, T>();
            index = new ConcurrentDictionary<string, long>();
            this.Settings = new XDictionarySettings()
            {
                Name = name
                //DbPath = dbpath
            };
            Name = Settings.Name;
            Init();
        }

        /// <summary>
        /// PersistentDictionary ctor with a specefied Dictionary
        /// </summary>
        /// <param name="dict"></param>
        public PersistentDictionaryIndex(string dbpath, string name, IDictionary<string, T> dict)
        {
            if (string.IsNullOrEmpty(dbpath))
            {
                throw new ArgumentNullException("PersistentDictionary.dbpath");
            }
            dictionary = new ConcurrentDictionary<string, T>();
            index = new ConcurrentDictionary<string, long>();
            this.Settings = new XDictionarySettings()
            {
                DbPath = dbpath,
                Name = name
            };
            Name = Settings.Name;
            Init();
            Load(dict);
            OnInitilaized(EventArgs.Empty);
        }

        /// <summary>
        /// XConfig ctor with default filename CallingAssembly '.mconfig' in current direcory
        /// </summary>
        public PersistentDictionaryIndex(XDictionarySettings settings)
        {
            dictionary = new ConcurrentDictionary<string, T>();
            index = new ConcurrentDictionary<string, long>();
            this.Settings = settings;
            Name = Settings.Name;
            Init();
            LoadDb();
            OnInitilaized(EventArgs.Empty);
        }
        #endregion

        private void Init()
        {
            try
            {
                string filename = this.Settings.DbFilename;
                DbLite.CreateFile(filename);
                DbLite.ValidateConnection(filename, true);
                ConnectionString = DbLite.GetConnectionString(filename);

                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                    var sql=DbCreateCommand();
                    db.ExecuteCommandNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Initilaized Error: " + ex.Message));
                throw ex;
            }

        }

        protected virtual void LoadDb()
        {
            using (TransactionScope tran = new TransactionScope())
            {
                IList<KeyValueItem> list;
                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                    var sql = "select *,rowid from " + Name;
                    list = db.Query<KeyValueItem>(sql, null);
                }
                if (list != null && list.Count > 0)
                {
                    foreach (var entry in list)
                    {
                        dictionary[entry.Key] = GenericTypes.Convert<T>(entry.Value);
                    }
                }
            }
        }

        //string sqlCreateTable = "create table demo_score (name varchar(20), score int)";

        //protected abstract string DbCreateCommand();// DbLite db);
        //protected abstract string DbUpsertCommand(string key, T value);
        //protected abstract string DbAddCommand(string key, T value);
        //protected abstract string DbUpdateCommand( string key, T value);
        //protected abstract string DbDeleteCommand( string key);
        //protected abstract string DbSelectCommand(string select, string where);
        //protected abstract string DbLookupCommand(string key);

        #region override

        //const string sqlcreate = @"CREATE TABLE bookmarks(
        //    users_id INTEGER,
        //    lessoninfo_id INTEGER,
        //    UNIQUE(users_id, lessoninfo_id)
        //);";

        const string sqlcreate = @"CREATE TABLE IF NOT EXISTS {0} (
                          key TEXT KEY,
                          body TEXT
                          )";
        const string sqlinsert = "insert into {0} (key, body) values ('{1}', '{2}')";
        const string sqldelete = "delete from {0} where rowid={1}";
        const string sqlupdate = "update {0} set body='{2}' where rowid={1}";
        const string sqlinsertOrIgnore = "insert or ignore into {0}(key, body) values('{1}', '{2}')";
        const string sqlinsertOrReplace = "insert or replace into {0}(key, body) values('{1}', '{2}')";
        const string sqlselect = "select {1} from {0} where rowid={2}";

        protected virtual string DbCreateCommand()
        {
            string sql = string.Format(sqlcreate, Name);
            return sql;
            //return db.ExecuteCommandNonQuery(sql);

        }
        protected virtual string DbAddCommand(string key, string value)
        {
            string sql = string.Format(sqlinsert, Name, key, value);
            return sql;
            //return db.ExecuteCommandNonQuery(sql);
        }

        protected virtual string DbDeleteCommand(string key)
        {

            string sql = string.Format(sqldelete, Name, key);
            return sql;
            //return db.ExecuteCommandNonQuery(sql);
        }

        protected virtual string DbUpdateCommand(string key, string value)
        {
            string sql = string.Format(sqlupdate, Name, key, value);
            return sql;
            //return db.ExecuteCommandNonQuery(sql);
        }

        protected virtual string DbUpsertCommand(string key, string value)
        {
            string sql = string.Format(sqlinsertOrReplace, Name, key, value);
            return sql;
            //return db.ExecuteCommandNonQuery(sql);
        }

        protected virtual string DbSelectCommand(string select, string where)
        {
            string sql = string.Format(sqlselect, Name, select, where);
            return sql;
            //return db.ExecuteCommandNonQuery(sql);
        }

        protected virtual string DbLookupCommand(string key)
        {
            string sql = string.Format(sqlselect, Name, "body", "key='" + key + "'");
            return sql;
            //return db.ExecuteCommandNonQuery(sql);
        }

        #endregion


        #region ConcurrentDictionary<string,T>


        /// <summary>
        /// Gets a value that indicates whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue> is empty.
        /// Returns:
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     is empty; otherwise, false.
        /// </summary>

        public bool IsEmpty { get { return dictionary.IsEmpty; } }

        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key already exists.
        //
        // Parameters:
        //   key:
        //     The key to be added or whose value should be updated
        //
        //   addValueFactory:
        //     The function used to generate a value for an absent key
        //
        //   updateValueFactory:
        //     The function used to generate a new value for an existing key based on the
        //     key's existing value
        //
        // Returns:
        //     The new value for the key. This will be either be the result of addValueFactory
        //     (if the key was absent) or the result of updateValueFactory (if the key was
        //     present).
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-addValueFactory is
        //     a null reference (Nothing in Visual Basic).-or-updateValueFactory is a null
        //     reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        //public T AddOrUpdate(string key, Func<string, T> addValueFactory, Func<string, T, T> updateValueFactory)
        //{
        //    return dictionary.AddOrUpdate(key, addValueFactory, updateValueFactory);
        //}
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key already exists.
        //
        // Parameters:
        //   key:
        //     The key to be added or whose value should be updated
        //
        //   addValue:
        //     The value to be added for an absent key
        //
        //   updateValueFactory:
        //     The function used to generate a new value for an existing key based on the
        //     key's existing value
        //
        // Returns:
        //     The new value for the key. This will be either be addValue (if the key was
        //     absent) or the result of updateValueFactory (if the key was present).
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-updateValueFactory
        //     is a null reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        //public T AddOrUpdate(string key, T addValue, Func<string, T, T> updateValueFactory)
        //{
        //    return dictionary.AddOrUpdate(key,addValue,updateValueFactory);
        //}


        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual int AddOrUpdate(string key, T value)
        {
            int res = 0;
            long rowid = 0;
            bool isExists = false;
            using (TransactionScope tran = new TransactionScope())
            {
                dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
                {
                    isExists = true;
                    return value;
                });

                isExists = index.TryGetValue(key, out rowid);

                //Insert create script here.
                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                    var cnn = db.SQLiteConnection;
                    using (var trans= cnn.BeginTransaction())
                    {
                        var sql = isExists ? DbUpdateCommand(key, value) : DbAddCommand(key, value);// DbUpsertCommand(key,value);
                        try
                        {
                            res = db.ExecuteCommandNonQuery(sql);
                        }
                        catch (Exception ex)
                        {
                            string err = ex.Message;
                            if (isExists)
                            {
                                sql = DbUpsertCommand(key, value);
                                res = db.ExecuteCommandNonQuery(sql);
                            }
                        }
                        rowid = cnn.LastInsertRowId;
                        trans.Commit();
                    }
                }
                if (res > 0)
                {
                    dictionary[key] = value;
                    index[key] = rowid;
                    //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
                    tran.Complete();

                }
            }
            if (res > 0)
                OnItemChanged("AddOrUpdate", key, value);

            return res;
        }

        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual void Add(string key, T value)
        {

            TryAdd(key,value);

            //int res = 0;
            //using (TransactionScope tran = new TransactionScope())
            //{
            //    //Insert create script here.
            //    using (var db = new DbLite(ConnectionString))
            //    {
            //        DbAdd(db, key, value);
            //    }
            //    if (res > 0)
            //    {
            //        dictionary[key] = value;

            //        //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
            //        tran.Complete();

            //        OnItemChanged("Add",key,value);
            //    }
            //}

            //return res;
        }

        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual int Update(string key, T value)
        {
            int res = 0;
            using (TransactionScope tran = new TransactionScope())
            {
                //Insert create script here.
                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                    var sql= DbUpdateCommand(key, value);
                    res= db.ExecuteCommandNonQuery(sql);
                }
                if (res > 0)
                {
                    dictionary[key] = value;

                    //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
                    tran.Complete();

                    OnItemChanged("Update",key, value);
                }
            }

            return res;
        }

        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual bool Remove(string key)
        {
            T value=default(T);
            return TryRemove(key, out value);

            //int res = 0;
            //T value=default(T);
            //using (TransactionScope tran = new TransactionScope())
            //{
            //    //Insert create script here.
            //    using (var db = new DbLite(ConnectionString))
            //    {
            //        res = DbDelete(db, key);
            //    }
            //    if (res > 0)
            //    {
            //        if(dictionary.TryRemove(key, out value))
            //        {
            //            //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
            //            tran.Complete();

            //            OnItemChanged("Remove", key, value);
            //        }
            //        else
            //        {
            //            res = -1;
            //        }
            //    }
            //}

            //return value;
        }
        

        //public T AddOrUpdate(GenericEntity entity)
        //{
        //    //var value=entity.Record.ToJson();
        //    string key = entity.PrimaryKey.ToString();
        //    int res = 0;
        //               using (TransactionScope tran = new TransactionScope())
        //    {
        //        //Insert create script here.
        //        using (var db = new DbLite(connectionString))
        //        {
        //            res = db.EntityUpsert(entity);

        //        }
        //        if (res > 0)
        //        {
        //            dictionary[key] = entity;

        //            //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
        //            tran.Complete();
        //        }
        //    }

        //    return res;
        //}


        //
        // Summary:
        //     Removes all keys and values from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            string cmdText="delete from [" + Name+"]";
            int res = 0;
            using (TransactionScope tran = new TransactionScope())
            {
                //Insert create script here.
                using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                {
                   res= db.ExecuteCommandNonQuery(cmdText);
                }
                if (res > 0)
                {
                    dictionary.Clear();
                    //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
                    tran.Complete();

                    OnItemChanged("Clear", null, default(T));
                }
            }
        }

        /// <summary>
        /// Determines whether the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.</param>
        /// <returns>
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is a null reference</exception>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }
        
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   valueFactory:
        //     The function used to generate a value for the key
        //
        // Returns:
        //     The value for the key. This will be either the existing value for the key
        //     if the key is already in the dictionary, or the new value for the key as
        //     returned by valueFactory if the key was not in the dictionary.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).-or-valueFactory is a null
        //     reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.
        //public T GetOrAdd(string key, Func<string, T> valueFactory)
        //{
        //    return dictionary.GetOrAdd(key,valueFactory);
        //}
        //
        // Summary:
        //     Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        //     if the key does not already exist.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   value:
        //     the value to be added, if the key does not already exist
        //
        // Returns:
        //     The value for the key. This will be either the existing value for the key
        //     if the key is already in the dictionary, or the new value if the key was
        //     not in the dictionary.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is a null reference (Nothing in Visual Basic).
        //
        //   System.OverflowException:
        //     The dictionary already contains the maximum number of elements, System.Int32.MaxValue.

        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key
        ///     if the key is already in the dictionary, or the new value if the key was
        ///     not in the dictionary.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"> key is a null reference.</exception>
        /// <exception cref="System.OverflowException"> The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public T GetOrAdd(string key, T value)
        {
            T val;
            if (dictionary.TryGetValue(key, out val))
            {
                return val;
            }
            if(TryAdd(key,value))
            {
                return value;
            }
            //int res = TryAdd(key, value);
            //if (res > 0)
            //{
            //    return value;
            //}
            return default(T);
            //return dictionary.GetOrAdd(key, value);
        }
      
        /// <summary>
        /// Copies the key and value pairs stored in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     to a new array.
        /// </summary>
        /// <returns>
        ///  A new array containing a snapshot of key and value pairs copied from the
        ///     System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        /// </returns>
        public KeyValuePair<string, T>[] ToArray()
        {
            return dictionary.ToArray();
        }

        
        /// <summary>
        /// Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public bool TryAdd(string key, T value)
        {

           
            using (TransactionScope tran = new TransactionScope())
            {
                int res = 0;
                if(dictionary.TryAdd(key, value))
                {
                    //Insert create script here.
                    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                    {
                        var sql=DbAddCommand(key, value);
                        res= db.ExecuteCommandNonQuery(sql);
                    }
                    if (res > 0)
                    {
                        //dictionary[key] = value;

                        //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
                        tran.Complete();

                        OnItemChanged("Add", key, value);

                        return true;
                    }

                }
                return false;
            }

            //return res;


            //int res = Add(key, value);
            //return (res > 0);
            //return dictionary.TryAdd(key, value);
        }

        /// <summary>
        /// Summary:
        ///     Attempts to get the value associated with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the value to get.
        ///
        ///   value:
        ///     When this method returns, value contains the object from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     with the specified key or the default value of , if the operation failed.
        ///
        /// Returns:
        ///     true if the key was found in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference (Nothing in Visual Basic).
        /// </summary>
        public bool TryGetValue(string key, out T value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public T SelectValue(string key)
        {
            var sql = DbLookupCommand(key);
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var res = db.QuerySingle<T>(sql, null);
                return res;
            }
        }

        public IList<T> Query(string select,string where, params object[] keyValueParameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var res = db.Query<T>(sql, keyValueParameters);
                return res;
            }
        }

        public T QuerySingle(string select, string where, params object[] keyValueParameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var res = db.QuerySingle<T>(sql, keyValueParameters);
                return res;
            }
        }

        public TVal QuerySingle<TVal>(string select, string where, TVal returnIfNull, params object[] keyValueParameters)
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                var sql = DbSelectCommand(select, where);
                var res = db.QueryScalar<TVal>(sql, returnIfNull, keyValueParameters);
                return res;
            }
        }

        /// <summary>
        /// Summary:
        ///     Attempts to remove and return the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the element to remove and return.
        ///
        ///   value:
        ///     When this method returns, value contains the object removed from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     or the default value of if the operation failed.
        ///
        /// Returns:
        ///     true if an object was removed successfully; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference (Nothing in Visual Basic).
        /// </summary>
        public bool TryRemove(string key, out T value)
        {
            
            //T value=default(T);
            using (TransactionScope tran = new TransactionScope())
            {
                int res = 0;

                if (dictionary.TryRemove(key, out value))
                {
                    //Insert create script here.
                    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                    {
                        var sql = DbDeleteCommand(key);
                        res=db.ExecuteCommandNonQuery(sql);
                    }

                    if (res > 0)
                    {
                        //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
                        tran.Complete();

                        OnItemChanged("Remove", key, value);

                        return true;
                    }
                }
                return false;
            }

            //value = Remove(key);
            //return value != null;

            //return dictionary.TryRemove(key, out value);
        }


        /// <summary>
        /// Summary:
        ///     Compares the existing value for the specified key with a specified value,
        ///     and if they are equal, updates the key with a third value.
        ///
        /// Parameters:
        ///   key:
        ///     The key whose value is compared with comparisonValue and possibly replaced.
        ///
        ///   newValue:
        ///     The value that replaces the value of the element with key if the comparison
        ///     results in equality.
        ///
        ///   comparisonValue:
        ///     The value that is compared to the value of the element with key.
        ///
        /// Returns:
        ///     true if the value with key was equal to comparisonValue and replaced with
        ///     newValue; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference.
        /// </summary>
        public bool TryUpdate(string key, T newValue, T comparisonValue)
        {
            int res=0;
            using (TransactionScope tran = new TransactionScope())
            {
                if (dictionary.TryUpdate(key, newValue, comparisonValue))
                {
                    //Insert create script here.
                    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                    {
                        var sql = DbUpdateCommand(key, newValue);
                        res=db.ExecuteCommandNonQuery(sql);
                    }
                    if (res > 0)
                    {
                        //Indicates that creating the SQLiteDatabase went succesfully, so the database can be committed.
                        tran.Complete();

                        OnItemChanged("TryUpdate", key, newValue);
                    }
                }
            }

            return res > 0;

            //return dictionary.TryUpdate(key, newValue, comparisonValue);

        }

        #endregion

        #region IDictionary implemenation

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the keys of
        ///     the System.Collections.Generic.IDictionary<TKey,TValue>.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the keys of the object
        ///     that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        /// </summary>
        public ICollection<string> Keys { get { return dictionary.Keys; } }

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the values in
        ///     the System.Collections.Generic.IDictionary<TKey,TValue>.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the values in the
        ///     object that implements System.Collections.Generic.IDictionary<TKey,TValue>.
        /// </summary>
        public ICollection<T> Values { get { return dictionary.Values; } }

        /// <summary>
        /// Summary:
        ///     Gets or sets the element with the specified key.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the element to get or set.
        ///
        /// Returns:
        ///     The element with the specified key.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is null.
        ///
        ///   System.Collections.Generic.KeyNotFoundException:
        ///     The property is retrieved and key is not found.
        ///
        ///   System.NotSupportedException:
        ///     The property is set and the System.Collections.Generic.IDictionary<TKey,TValue>
        ///     is read-only.
        /// </summary>
        public T this[string key]
        {
            get { return dictionary[key]; }
            set
            {

                AddOrUpdate(key, value);
            }
        }

        // Summary:
        //     Adds an element with the provided key and value to the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The object to use as the key of the element to add.
        //
        //   value:
        //     The object to use as the value of the element to add.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.ArgumentException:
        //     An element with the same key already exists in the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        //   System.NotSupportedException:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
        //public void Add(string key, T value)
        //{
        //    dictionary.Add(key,value);
        //}
        //
        // Summary:
        //     Determines whether the System.Collections.Generic.IDictionary<TKey,TValue>
        //     contains an element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Returns:
        //     true if the System.Collections.Generic.IDictionary<TKey,TValue> contains
        //     an element with the key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //public bool ContainsKey(string key)
        //{
        //    return dictionary.ContainsKey(key);
        //}
        //
        // Summary:
        //     Removes the element with the specified key from the System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The key of the element to remove.
        //
        // Returns:
        //     true if the element is successfully removed; otherwise, false. This method
        //     also returns false if key was not found in the original System.Collections.Generic.IDictionary<TKey,TValue>.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.NotSupportedException:
        //     The System.Collections.Generic.IDictionary<TKey,TValue> is read-only.
        //public bool Remove(string key)
        //{
        //    return dictionary.Remove(key);
        //}
        //
        // Summary:
        //     Gets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key whose value to get.
        //
        //   value:
        //     When this method returns, the value associated with the specified key, if
        //     the key is found; otherwise, the default value for the type of the value
        //     parameter. This parameter is passed uninitialized.
        //
        // Returns:
        //     true if the T that implements System.Collections.Generic.IDictionary<TKey,TValue>
        //     contains an element with the specified key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //public bool TryGetValue(string key, out T value)
        //{
        //    return dictionary.TryGetValue(key,out value);
        //}

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        // Summary:
        //     Gets the number of elements contained in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     The number of elements contained in the System.Collections.Generic.ICollection<T>.
        public int Count { get { return dictionary.Count; } }
        //
        // Summary:
        //     Gets a value indicating whether the System.Collections.Generic.ICollection<T>
        //     is read-only.
        //
        // Returns:
        //     true if the System.Collections.Generic.ICollection<T> is read-only; otherwise,
        //     false.
        public bool IsReadOnly { 
            get 
            {
                return false;// dictionary.IsReadOnly; 
            } 
        }

        // Summary:
        //     Adds an item to the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to add to the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
            //dictionary.Add(item);
        }
        //
        // Summary:
        //     Removes all items from the System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        //public void Clear()
        //{

        //}
        //
        // Summary:
        //     Determines whether the System.Collections.Generic.ICollection<T> contains
        //     a specific value.
        //
        // Parameters:
        //   item:
        //     The object to locate in the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item is found in the System.Collections.Generic.ICollection<T>; otherwise,
        //     false.
        public bool Contains(KeyValuePair<string, T> item)
        {
            T val;
            if(TryGetValue(item.Key,out val))
            {
                return item.Value.Equals(val);
            }
            return false;

            //return dictionary.ContainsKey(item.Key);
        }
        //
        // Summary:
        //     Copies the elements of the System.Collections.Generic.ICollection<T> to an
        //     System.Array, starting at a particular System.Array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional System.Array that is the destination of the elements
        //     copied from System.Collections.Generic.ICollection<T>. The System.Array must
        //     have zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.
        //
        //   System.ArgumentException:
        //     The number of elements in the source System.Collections.Generic.ICollection<T>
        //     is greater than the available space from arrayIndex to the end of the destination
        //     array.
        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            array=dictionary.ToArray();//.CopyTo(array,arrayIndex);
        }
        //
        // Summary:
        //     Removes the first occurrence of a specific object from the System.Collections.Generic.ICollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to remove from the System.Collections.Generic.ICollection<T>.
        //
        // Returns:
        //     true if item was successfully removed from the System.Collections.Generic.ICollection<T>;
        //     otherwise, false. This method also returns false if item is not found in
        //     the original System.Collections.Generic.ICollection<T>.
        //
        // Exceptions:
        //   System.NotSupportedException:
        //     The System.Collections.Generic.ICollection<T> is read-only.
        public bool Remove(KeyValuePair<string, T> item)
        {
            T val;
            return TryRemove(item.Key, out val);
           // return dictionary.Remove(item);
        }

        #endregion

        #region IEnumerable<out T> : IEnumerable
       
            // Summary:
            //     Returns an enumerator that iterates through the collection.
            //
            // Returns:
            //     A System.Collections.Generic.IEnumerator<T> that can be used to iterate through
            //     the collection.
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return dictionary.GetEnumerator();// ((IDictionary<string, T>)dictionary).GetEnumerator();
        }
        

        #endregion

        public void Load(IDictionary<string, T> dict)
        {
            ignorEvent = true;
            try
            {

                foreach (KeyValuePair<string, T> entry in dict)
                {
                    AddOrUpdate(entry.Key, entry.Value);
                }
            }
            finally
            {
                ignorEvent = false;
            }
        }

        


        #region watcher
        /*
        FileSystemWatcher watcher;

        /// <summary>
        /// Initilaize the file system watcher
        /// </summary>
        void InitWatcher()
        {
            string fpath = Path.GetDirectoryName(settings.Filename);
            string filter = Path.GetExtension(settings.Filename);

             // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = fpath;
            // Watch for changes in LastAccess and LastWrite times, and 
            //   the renaming of files or directories. 
         * 
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter ="*"+ filter;// "*.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Created += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Deleted += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Renamed += new RenamedEventHandler(WatchFile_Renamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
        }

        /// <summary>
        /// Occoured when file changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileChanged(FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                OnConfigFileChanged(EventArgs.Empty);
            }
        }
        /// <summary>
        /// Occoured when file renamed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileRenamed(RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            if (e.FullPath == settings.Filename)
            {
                OnConfigFileChanged(EventArgs.Empty);
            }
        }


        void WatchFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == settings.Filename)
            {
                if (ignorEvent == false)
                    OnFileChanged(e);
            }
        }
        void WatchFile_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath == settings.Filename || e.FullPath == settings.Filename)
            {
                OnFileRenamed(e);
            }
        }
        */
        #endregion


        /*
        /// <summary>
        /// Read Config File
        /// </summary>
        public void Read()
        {
            try
            {
                FileToDictionary();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to read Config To Dictionary, Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Save Changes 
        /// </summary>
        public void Save()
        {
            try
            {
                DictionaryToFile();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to save Dictionary To Config, Error: " + ex.Message));
            }
        }
        /// <summary>
        /// Print all item to Console
        /// </summary>
        public void PrintConfig()
        {
            Console.WriteLine("<" + settings.RootTag + ">");

            foreach (KeyValuePair<string, T> entry in dictionary)
            {
                Console.WriteLine("key={0}, value={1}, Type={2}", entry.Key , entry.Value,entry.Value==null? "String": entry.Value.GetType().ToString());
            }
            Console.WriteLine("</" + settings.RootTag + ">");

        }
   
    
        /// <summary>
        /// Init new config file from dictionary
        /// </summary>
        /// <param name="dict"></param>
        private void Init()
        {
 
            XmlBuilder builder = new XmlBuilder();
            builder.AppendXmlDeclaration();
            builder.AppendEmptyElement(settings.RootTag, 0);
            foreach (KeyValuePair<string,T> entry in dictionary)
            {
                if (entry.Value == null)
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[]{"key", entry.Key,"value", string.Empty, "type", "String"});
                }
                else
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[] { "key", entry.Key, "value", entry.Value.ToString(), "type", entry.Value.GetType().ToString() });
                }
            }

            if (settings.Encrypted)
                EncryptFile(builder.Document.OuterXml);
            else
                builder.Document.Save(settings.Filename);

            if (settings.UseFileWatcher)
                InitWatcher();
        }
        
        private void FileToDictionary()
        {

            Dictionary<string,T> dict=new Dictionary<string,T>();

            XmlDocument doc = new XmlDocument();

            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            Console.WriteLine("Load Config: " + settings.Filename);
            //XmlParser parser=new XmlParser(filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
                var attkey = node.Attributes["key"];
                if (attkey != null)
                    dict[attkey.Value] = GetValue(node);

            }
            dictionary = dict;
        }

        private object GetValue(XmlNode node)
        {
            string value = node.Attributes["value"].Value;
            string type ="string";
            XmlAttribute attrib= node.Attributes["type"];

            if (attrib == null)
            {
                return value;
            }

            type = attrib.Value;

            return Types.StringToObject(type, value);
         }

        private void ValidateConfig()
        {
            if (!FileExists && dictionary.Count > 0)
            {
                Init();
            }
        }

        private void DictionaryToFile()
        {
            ValidateConfig();

            XmlDocument doc = new XmlDocument();
            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
              
                string key=node.Attributes["key"].Value;
                object value=dictionary[key];

                if (value != null)
                {
                  node.Attributes["value"].Value =value.ToString();
                  node.Attributes["type"].Value = value.GetType().ToString();
                }
                else
                {
                    node.Attributes["value"].Value = string.Empty;
                    node.Attributes["type"].Value ="String";
                }
            }

            if (settings.Encrypted)
            {
                EncryptFile(doc.OuterXml);
            }
            else
                doc.Save(settings.Filename);
        }

       

        private string DecryptFile()
        {
            Encryption en = new Encryption(settings.Password);
            return en.DecryptFileToString(settings.Filename, true);
        }

        private bool EncryptFile(string ouput)
        {
            Encryption en = new Encryption(settings.Password);
            return en.EncryptStringToFile(ouput, settings.Filename, true);
        }
        */

    }
}
