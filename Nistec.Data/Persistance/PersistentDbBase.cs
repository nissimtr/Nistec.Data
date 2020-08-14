using Nistec.Data.Entities;
using Nistec.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Nistec.Data.Persistance
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
    public abstract class PersistentDbBase<T, PI, TP> 
        where PI : IPersistItem
        where TP : IDbDataParameter
    {


        #region properties


        protected readonly ConcurrentDictionary<string, T> dictionary;

        public const DBProvider DbProvider = DBProvider.SqlServer;// DbLite.ProviderName;
        protected CommitMode _CommitMode;
        protected bool _EnableTasker;

        public PersistentDbSettings Settings { get; protected set; }
        //public string ConnectionString { get; protected set; }
        public string TableName { get; protected set; }
        public string JournalName { get; protected set; }

        //public bool EnableCompress { get; protected set; }
        //public int CompressLevel { get; protected set; }
        ///// <summary>
        ///// Get value indicating if the config file exists
        ///// </summary>
        //public bool FileExists
        //{
        //    get
        //    {
        //        return File.Exists(Settings.DbFilename);
        //    }
        //}

        #endregion

        #region events

        bool ignorEvent = false;
        //public event ConfigChangedHandler ItemChanged;
        //public event EventHandler ConfigFileChanged;
        public event GenericEventHandler<string> ErrorOcurred;
        public event EventHandler Initilaized;
        public event EventHandler BeginLoading;
        public event GenericEventHandler<string, int> LoadCompleted;
        //public event GenericEventHandler<string, T> ItemLoaded;
        public event GenericEventHandler<string, string, T> ItemChanged;
        public event EventHandler ClearCompleted;

        public Action<T> ItemLoaded { get; set; }

        protected virtual void OnInitilaized(EventArgs e)
        {
            if (Initilaized != null)
            {
                Initilaized(this, e);
            }
        }
        protected virtual void OnBeginLoading()
        {
            if (BeginLoading != null)
            {
                BeginLoading(this, EventArgs.Empty);
            }
        }
        protected virtual void OnClearCompleted()
        {
            if (ClearCompleted != null)
            {
                ClearCompleted(this, EventArgs.Empty);
            }
        }
        protected virtual void OnLoadCompleted(string message, int count)
        {
            if (LoadCompleted != null)
            {
                LoadCompleted(this, new GenericEventArgs<string, int>(message, count));
            }
        }
        protected virtual void OnLoadCompleted(GenericEventArgs<string, int> e)
        {
            if (LoadCompleted != null)
            {
                LoadCompleted(this, e);
            }
        }

        protected virtual void OnItemLoaded(T value)
        {
            if (ItemLoaded != null)
                ItemLoaded(value);
            //OnItemLoaded(new GenericEventArgs<string, T>(key, value));
        }

        //protected virtual void OnItemLoaded(GenericEventArgs<string, T> e)
        //{
        //    if (ItemLoaded != null)
        //        ItemLoaded(this, e);
        //}

        protected virtual void OnErrorOcurred(string action, string message)
        {
            if (ErrorOcurred != null)
            {
                OnErrorOcurred(new GenericEventArgs<string>("ErrorOcurred in: " + action + ", Messgae: " + message));
            }
        }
        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
            {
                ErrorOcurred(this, e);
            }
        }

        protected virtual void OnItemChanged(string action, string key, T value)
        {
            if (!ignorEvent)
                OnItemChanged(new GenericEventArgs<string, string, T>(action, key, value));
        }

        protected virtual void OnItemChanged(GenericEventArgs<string, string, T> e)
        {
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

        
        #endregion

        #region ctor
        /// <summary>
        /// PersistentDictionary ctor with a specefied filename
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        /// <param name="tableName"></param>
        public PersistentDbBase(string connectionString, DBProvider provider, string tableName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("PersistentDictionary.connectionString");
            }
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = new PersistentDbSettings(connectionString, provider, tableName);
            _CommitMode = Settings.CommitMode;
            TableName = Settings.TableName;
            JournalName = Settings.JournalName;
            Init();
        }

      
        /// <summary>
        /// XConfig ctor with default filename CallingAssembly '.mconfig' in current direcory
        /// </summary>
        /// <param name="settings"></param>
        public PersistentDbBase(PersistentDbSettings settings)
        {
            dictionary = new ConcurrentDictionary<string, T>();
            this.Settings = settings;
            TableName = Settings.TableName;
            JournalName = Settings.JournalName;
            _CommitMode = Settings.CommitMode;
            Init();
            //if (loadPersistItems)
            //    LoadDb();
            //else
            //    Clear();
            OnInitilaized(EventArgs.Empty);
        }
        #endregion

        protected virtual void Init()
        {
            //string filename = null;
            try
            {
                //if (this.Settings.InMemory)
                //{
                //    //filename = PersistentSettings.InMemoryFilename;
                //    throw new Exception("PersistentDictionary do not supported in memory db");
                //}
                //else
                //{
                //    //string filename = this.Settings.DbFilename;

                //    //DbLiteUtil.CreateFolder(Settings.DbPath);
                //    //DbLiteUtil.CreateFile(filename);
                //    //DbLiteUtil.ValidateConnection(filename, true);
                //}
                //ConnectionString = Settings.ConnectionString;//.GetConnectionString();
                //EnableCompress = Settings.EnableCompress;
                //CompressLevel = Settings.ValidCompressLevel;

                using (var db = Settings.Connect())
                {
                    var sql = DbCreateCommand();
                    db.ExecuteCommandNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Initilaized Error: " + ex.Message));
                throw ex;
            }

        }

        public virtual void LoadDb()
        {
            IList<PI> list;
            OnBeginLoading();
            using (TransactionScope tran = new TransactionScope())
            {
                try
                {
                    using (var db = Settings.Connect())
                    {
                        var sql = "select * from " + TableName;
                        list = db.Query<PI>(sql, null);
                        if (list != null && list.Count > 0)
                        {
                            foreach (var entry in list)
                            {
                                var val = FromPersistItem(entry);
                                if (val != null)
                                {
                                    dictionary[entry.key] = val;
                                    OnItemLoaded(val);
                                    //if (onTake != null)
                                    //    onTake(val);
                                }
                            }
                        }

                    }

                    tran.Complete();
                    OnLoadCompleted(this.TableName, this.Count);
                }
                catch (Exception ex)
                {
                    OnErrorOcurred("LoadDb", ex.Message);
                }
            }
        }


        //string sqlCreateTable = "create table demo_score (name varchar(20), score int)";

        protected abstract string DbCreateCommand();// DbLite db);
        protected abstract string DbUpsertCommand();// string key, T value);
        protected abstract string DbAddCommand();//string key, T value);
        protected abstract string DbAddJournalCommand();
        protected abstract string DbUpdateCommand();//string key, T value);
        protected abstract string DbDeleteCommand();//string key);
        protected abstract string DbSelectCommand(string select, string where);
        protected abstract string DbLookupCommand();//string key);
        protected abstract string DbUpdateStateCommand();//string key, int state);
        protected abstract string GetItemKey(PI value);
        //protected abstract TType GetDataValue<TType>(T item);
        protected abstract byte[] GetDataBinary(T item);
        
        protected abstract T DecompressValue(PI value);
        protected abstract PI ToPersistItem(string key, string name, T item);
        protected abstract T FromPersistItem(PI value);
        //protected abstract string DbFetchCommand();
        //protected abstract string DbFetchCommitCommand();
        //protected abstract string DbFetchCountCommand();


        #region Commands

        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="name">The section name to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual int AddOrUpdate(string key, string name, T value)
        {
            int res = 0;

            //dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
            //{
            //    isExists = true;
            //    return value;
            //});

            bool iscommited = false;

            try
            {
                var body = GetDataBinary(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbUpsertCommand();//key, value);
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        dictionary.AddOrUpdate(key, value, (oldkey, oldValue) =>
                        {
                            return value;
                        });
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistentDbTask()
                            {
                                DdProvider=DbProvider,
                                CommandText = DbUpsertCommand(),//key, value),
                                CommandType = "DbUpsert",
                                ConnectionString = Settings.ConnectionString,
                                Parameters = DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("AddOrUpdate", null, default(T));
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("AddOrUpdate", ex.Message);
            }

            return res;
        }

        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual int Update(string key, T value)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                var body = GetDataBinary(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbUpdateCommand();//key, value);
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }

                        break;
                    default:
                        dictionary[key] = value;
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistentDbTask()
                            {
                                DdProvider = DbProvider,
                                CommandText = DbUpdateCommand(),//key, value),
                                CommandType = "DbUpdate",
                                ConnectionString = Settings.ConnectionString,
                                Parameters = DataParameter.GetDbParam<TP>("key", key, "body", body)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, default(T));
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("Update", ex.Message);
            }

            return res;
        }

        /// <summary>
        /// Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="name">The section name to be added or whose value should be updated</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary.
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public virtual bool TryAdd(string key, string name, T value)
        {

            bool iscommited = false;

            try
            {
                var body = GetDataBinary(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbAddCommand();//key, value);
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryAdd(key, value))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryAdd(key, value))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistentDbTask()
                                {
                                    DdProvider = DbProvider,
                                    CommandText = DbAddCommand(),//(key, value),
                                    CommandType = "DbAdd",
                                    ConnectionString = Settings.ConnectionString,
                                    Parameters = DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name)
                                };
                                task.ExecuteTask(_EnableTasker);
                            }
                            iscommited = true;
                        }
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryAdd", key, value);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryAdd", ex.Message);
            }
            return iscommited;

        }

        /// <summary>
        /// Summary:
        ///     Attempts to remove and return the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the element to remove and return.
        ///
        ///   value:
        ///     When this method returns, value contains the object removed from the System.Collections.Concurrent.ConcurrentDictionary
        ///     or the default value of if the operation failed.
        ///
        /// Returns:
        ///     true if an object was removed successfully; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is a null reference (Nothing in Visual Basic).
        /// </summary>
        public virtual bool TryRemove(string key, out T value)
        {

            bool iscommited = false;
            T outval = value = default(T);
            try
            {

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbDeleteCommand();//key);
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryRemove(key, out outval))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryRemove(key, out outval))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistentDbTask()
                                {
                                    DdProvider = DbProvider,
                                    CommandText = DbDeleteCommand(),//key),
                                    CommandType = "DbDelete",
                                    ConnectionString = Settings.ConnectionString,
                                    Parameters = DataParameter.GetDbParam<TP>("key", key)
                                };
                                task.ExecuteTask(_EnableTasker);
                            }
                            iscommited = true;
                        }
                        break;
                }

                value = outval;

                if (iscommited)
                {
                    OnItemChanged("TryRemove", key, value);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("TryRemove", ex.Message);
            }
            return iscommited;
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
        public virtual bool TryUpdate(string key, T newValue, T comparisonValue)
        {

            bool iscommited = false;

            try
            {
                var body = GetDataBinary(newValue);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbUpdateCommand();//key, newValue);
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryUpdate(key, newValue, comparisonValue))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    default:

                        if (dictionary.TryUpdate(key, newValue, comparisonValue))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistentDbTask()
                                {
                                    CommandText = DbUpdateCommand(),//key, newValue),
                                    CommandType = "DbUpdate",
                                    ConnectionString = Settings.ConnectionString,
                                    Parameters = DataParameter.GetDbParam<TP>("key", key, "body", body)
                                };
                                task.ExecuteTask(_EnableTasker);
                            }
                            iscommited = true;
                        }
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryUpdate", key, newValue);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryUpdate", ex.Message);
            }
            return iscommited;

        }


        #endregion
 

        #region ConcurrentDictionary<string,T>


        /// <summary>
        /// Gets a value that indicates whether the System.Collections.Concurrent.ConcurrentDictionary is empty.
        /// Returns:
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary
        ///     is empty; otherwise, false.
        /// </summary>

        public bool IsEmpty { get { return dictionary.IsEmpty; } }



        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
        ///    if the key does not already exist.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="name">The section name to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public virtual void Add(string key, string name, T value)
        {

            TryAdd(key, name, value);

        }


        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <returns></returns>
        public virtual bool Remove(string key)
        {
            T value = default(T);
            return TryRemove(key, out value);

        }


        //
        // Summary:
        //     Removes all keys and values from the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>.

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            string cmdText = "delete from [" + TableName + "]";
            bool iscommited = false;

            try
            {
                //Insert create script here.
                using (var db = Settings.Connect())
                {
                    db.ExecuteTransCommandNonQuery(cmdText, (result) =>
                    {
                        if (result > 0)
                        {
                            dictionary.Clear();
                            //trans.Commit();
                            iscommited = true;
                        }
                        return iscommited;
                    });
                }
                if (iscommited)
                {
                    OnClearCompleted(); //OnItemChanged("Clear", null, default(T));
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("Clear", ex.Message);
            }

        }


        /// <summary>
        /// Determines whether the System.Collections.Concurrent.ConcurrentDictionary
        ///     contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the System.Collections.Concurrent.ConcurrentDictionary</param>
        /// <returns>
        ///     true if the System.Collections.Concurrent.ConcurrentDictionary
        ///     contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is a null reference</exception>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }


        /// <summary>
        /// Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
        ///     if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="name">The section name to be added or whose value should be updated</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key
        ///     if the key is already in the dictionary, or the new value if the key was
        ///     not in the dictionary.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"> key is a null reference.</exception>
        /// <exception cref="System.OverflowException"> The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public T GetOrAdd(string key, string name, T value)
        {
            T val;
            if (dictionary.TryGetValue(key, out val))
            {
                return val;
            }
            if (TryAdd(key, name,value))
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
        /// Copies the key and value pairs stored in the System.Collections.Concurrent.ConcurrentDictionary
        ///     to a new array.
        /// </summary>
        /// <returns>
        ///  A new array containing a snapshot of key and value pairs copied from the
        ///     System.Collections.Concurrent.ConcurrentDictionary
        /// </returns>
        public KeyValuePair<string, T>[] ToArray()
        {
            return dictionary.ToArray();
        }



        /// <summary>
        /// Summary:
        ///     Attempts to get the value associated with the specified key from the System.Collections.Concurrent.ConcurrentDictionary.
        ///
        /// Parameters:
        ///   key:
        ///     The key of the value to get.
        ///
        ///   value:
        ///     When this method returns, value contains the object from the System.Collections.Concurrent.ConcurrentDictionary
        ///     with the specified key or the default value of , if the operation failed.
        ///
        /// Returns:
        ///     true if the key was found in the System.Collections.Concurrent.ConcurrentDictionary
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

        #endregion

        #region IDictionary implemenation

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the keys of
        ///     the System.Collections.Generic.IDictionary.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the keys of the object
        ///     that implements System.Collections.Generic.IDictionary.
        /// </summary>
        public ICollection<string> Keys { get { return dictionary.Keys; } }

        /// <summary>
        /// Summary:
        ///     Gets an System.Collections.Generic.ICollection<T> containing the values in
        ///     the System.Collections.Generic.IDictionary.
        ///
        /// Returns:
        ///     An System.Collections.Generic.ICollection<T> containing the values in the
        ///     object that implements System.Collections.Generic.IDictionary
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
        ///     The property is set and the System.Collections.Generic.IDictionary
        ///     is read-only.
        /// </summary>
        public T this[string key, string name]
        {
            get { return dictionary[key]; }
            set
            {

                AddOrUpdate(key, name,value);
            }
        }


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
        public bool IsReadOnly
        {
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
        public void Add(string name,KeyValuePair<string, T> item)
        {
            Add(item.Key, name, item.Value);
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
            if (TryGetValue(item.Key, out val))
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
            array = dictionary.ToArray();//.CopyTo(array,arrayIndex);
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

        #region select query

        public T SelectValue(string key)
        {
            //T value-default(T);
            var sql = DbLookupCommand();
            using (var db = Settings.Connect())
            {
                var res = db.QuerySingle<PI>(sql, null);

                //if (EnableCompress)
                //{
                //    return DecompressValue(res);
                //}
                return FromPersistItem(res);
            }
        }

        public IEnumerable<IPersistEntity> QueryDictionaryItems(string name)
        {

            List<IPersistEntity> list = new List<IPersistEntity>();
            try
            {
                if (dictionary != null && dictionary.Count > 0)
                {
                    foreach (var g in this.dictionary)
                    {
                        list.Add(new PersistItem() { body = g.Value, key = g.Key, name = name, timestamp = DateTime.Now });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        public IList<PersistItem> QueryItems(string select, string where, params object[] keyValueParameters)
        {
            using (var db = Settings.Connect())
            {
                var sql = DbSelectCommand(select, where);
                var list = db.Query<PersistItem>(sql, keyValueParameters);
                return list;
            }
        }

        public IList<T> Query(string select, string where, params object[] keyValueParameters)
        {
            IList<T> list = new List<T>();
            using (var db = Settings.Connect())
            {
                var sql = DbSelectCommand(select, where);
                var res = db.Query<PI>(sql, keyValueParameters);
                //if (EnableCompress)
                //{
                //    for (int i = 0; i < res.Count; i++)
                //    {
                //        var item = DecompressValue(res[i]);
                //        list.Add(item);
                //        //res[i] = item;
                //    }
                //}
                //else
                //{
                //    for (int i = 0; i < res.Count; i++)
                //    {
                //        var item = FromPersistItem(res[i]);
                //        list.Add(item);
                //        //res[i] = item;
                //    }
                //}
                for (int i = 0; i < res.Count; i++)
                {
                    var item = FromPersistItem(res[i]);
                    list.Add(item);
                    //res[i] = item;
                }
                return list;
            }
        }

        public T QuerySingle(string select, string where, params object[] keyValueParameters)
        {
            using (var db = Settings.Connect())
            {
                var sql = DbSelectCommand(select, where);
                var res = db.QuerySingle<PI>(sql, keyValueParameters);
                //if (EnableCompress)
                //{
                //    return DecompressValue(res);
                //}
                return FromPersistItem(res);
            }
        }

        //public TVal QuerySingle<TVal>(string select, string where, TVal returnIfNull, params object[] keyValueParameters)
        //{
        //    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //    {
        //        var sql = DbSelectCommand(select, where);
        //        var res = db.QueryScalar<TVal>(sql, returnIfNull, keyValueParameters);

        //        return res;
        //    }
        //}


        #endregion

        #region Compression

        public void ReloadOrClearPersist(bool loadPersistItems)
        {
            if (loadPersistItems)
            {
                dictionary.Clear();
                LoadDb();
            }
            else
                Clear();
        }

        public void Load(IDictionary<string, T> dict, string name)
        {
            ignorEvent = true;
            try
            {

                foreach (KeyValuePair<string, T> entry in dict)
                {
                    AddOrUpdate(entry.Key,name, entry.Value);
                }
            }
            finally
            {
                ignorEvent = false;
            }
        }

        /// <summary>
        /// Compress data to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="level">from 1-9, 1 is fast p is bettr</param>
        /// <returns></returns>
        public byte[] Compress(string value, int level)
        {
            return Nistec.Generic.NetZipp.Compress(value);
        }

        public string Decompress(byte[] b)
        {
            return Nistec.Generic.NetZipp.Decompress(b);
        }

        /// <summary>
        /// Compress data to byte array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="level">from 1-9, 1 is fast p is bettr</param>
        /// <returns></returns>
        public string Zip(string value, int level)
        {
            return Nistec.Generic.NetZipp.Zip(value);
        }

        public string UnZip(string compressed)
        {
            return Nistec.Generic.NetZipp.UnZip(compressed);
        }
        #endregion

        #region Async

        public Task ExecuteTask(string cmdText, IDbDataParameter[] parameters)
        {
            return Task.Factory.StartNew<int>(() => Execute(cmdText, parameters));
        }

        public int ExecuteAsync(string cmdText, IDbDataParameter[] parameters)
        {
            var result = Task.Factory.StartNew<int>(() => Execute(cmdText, parameters));
            return (result == null) ? 0 : result.Result;
        }

        public int Execute(string cmdText, IDbDataParameter[] parameters)
        {
            using (var db = Settings.Connect())
            {
                return db.ExecuteCommandNonQuery(cmdText, parameters);
            }
        }
        #endregion
    }
}
