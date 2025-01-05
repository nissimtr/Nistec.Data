using Nistec.Data.Entities;
using Nistec.Data.Persistance;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Nistec.Data.Sqlite
{



    public class PersistentBag : PersistentBase<BagItem, BagItem>
    {

        #region ctor

        public PersistentBag(DbLiteSettings settings)
            : base(settings)
        {
            
        }

        public PersistentBag(string name)
            : base(new DbLiteSettings() { Name=name })
        {

        }

        #endregion

        #region override
        
        //const string sqlcreate = @"CREATE TABLE bookmarks(
        //    users_id INTEGER,
        //    lessoninfo_id INTEGER,
        //    UNIQUE(users_id, lessoninfo_id)
        //);";

        const string sqlcreate = @"CREATE TABLE IF NOT EXISTS {0} (
                          key TEXT PRIMARY KEY,
                          body TEXT,
                          state INTEGER DEFAULT 0,
                          timestamp DATETIME DEFAULT CURRENT_TIMESTAMP     
                        ) WITHOUT ROWID;";
        /*
        const string sqlinsert = "insert into {0} (key, body) values (@key, @body)";
        const string sqldelete = "delete from {0} where key=@key";
        const string sqlupdate = "update {0} set body=@body, timestamp=CURRENT_TIMESTAMP where key=@key";
        const string sqlinsertOrIgnore = "insert or IGNORE into {0}(key, body) values(@key, @body)";
        const string sqlinsertOrReplace = "insert or REPLACE into {0}(key, body) values(@key, @body)";
        const string sqlselect = "select {1} from {0} where key=@key";

        const string sqlupdatestate = "update {0} set state=@state,timestamp=CURRENT_TIMESTAMP where key=@key";
        */
        protected override string DbCreateCommand()
        {
            return string.Format(sqlcreate, Name);
        }
        //protected override string DbAddCommand(bool isTrans = false)
        //{
        //    if (isTrans)
        //        return string.Format(sqlinsertTrans, Name);
        //    return string.Format(sqlinsert, Name);
        //}
        //protected override string DbAddIgnoreCommand(bool isTrans = false)
        //{
        //    if (isTrans)
        //        return string.Format(sqlinsertOrIgnoreTrans, Name);
        //    return string.Format(sqlinsertOrIgnore, Name);
        //}
        //protected override string DbAddReplaceCommand(bool isTrans = false)
        //{
        //    if (isTrans)
        //        return string.Format(sqlinsertOrReplaceTrans, Name);
        //    return string.Format(sqlinsertOrReplace, Name);
        //}

        //protected override string DbDeleteCommand()
        //{
        //    return string.Format(sqldelete, Name);
        //}

        //protected override string DbUpdateCommand()
        //{
        //    return string.Format(sqlupdate, Name);
        //}

        //protected override string DbUpsertCommand()
        //{
        //    return string.Format(sqlinsertOrReplace, Name);
        //}

        //protected override string DbSelectCommand(string select, string where)
        //{
        //    return string.Format(sqlselect, Name, select, where);
        //}

        //protected override string DbLookupCommand()
        //{
        //    return string.Format(sqlselect, Name, "body");
        //}

        //protected override string DbUpdateStateCommand()
        //{
        //    return string.Format(sqlupdatestate, Name);
        //}

        protected override object GetDataValue(BagItem value)
        {
            string body = value.body;
            if (EnableCompress)
            {
                body = Zip(value.body, CompressLevel);
            }
            return body;
        }

        protected override string GetItemKey(BagItem value)
        {
            return value.key;
        }

        protected override BagItem DecompressValue(BagItem value)
        {
            value.body = UnZip(value.body);
            return value;
        }


        protected override BagItem ToPersistItem(string key, BagItem value)
        {
            return value;
        }
        protected override BagItem FromPersistItem(BagItem value)
        {
            value.body = UnZip(value.body);
            return value;
        }
        #endregion

        #region Commands

        /// <summary>
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public int UpdateState(string key, int state)
        {
            bool iscommited = false;
            int res = 0;
            BagItem val = null;
            try
            {
                var cmdText = DbUpdateStateCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "state", state), (result) =>
                            {
                                if (result > 0)
                                {
                                    if( dictionary.TryGetValue(key, out val))
                                    {
                                        val.state = state;
                                        dictionary[key] = val;
                                    }
                                    //if (dictionary.ContainsKey(key))
                                    //    dictionary[key].State = state;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }

                        break;
                    default:
                        //if (dictionary.ContainsKey(key))
                        //    dictionary[key].State = state;
                        if (dictionary.TryGetValue(key, out val))
                        {
                            val.state = state;
                            dictionary[key] = val;
                        }
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistentTask()
                            {
                                DdProvider = DBProvider.SQLite,
                                CommandText = cmdText,
                                CommandType = "DbUpdateState",
                                ConnectionString = ConnectionString,
                                Parameters = DataParameter.Get<SQLiteParameter>("key", key, "state", state)
                            };
                            task.ExecuteTask(_EnableTasker);
                        }
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, val);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("Update", ex.Message);
            }

            return res;
        }

        /*
         
        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int AddOrUpdate(string key, BagItem value)
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

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpsertCommand(key, value);
                            res = db.ExecuteTransCommandNonQuery(cmdText, (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    trans.Commit();
                                    iscommited = true;
                                }
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
                            var task = new PersistanceTask()
                            {
                                CommandText = DbUpsertCommand(key, value),
                                CommandType = "DbUpsert",
                                ConnectionString = ConnectionString
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
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///    if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int Update(string key, BagItem value)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand(key, value);
                            res = db.ExecuteTransCommandNonQuery(cmdText, (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = value;
                                    trans.Commit();
                                    iscommited = true;
                                }
                            });
                        }

                        break;
                    default:
                        dictionary[key] = value;
                        if (_CommitMode == CommitMode.OnMemory)
                        {
                            var task = new PersistanceTask()
                            {
                                CommandText = DbUpdateCommand(key, value),
                                CommandType = "DbUpdate",
                                ConnectionString = ConnectionString
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
        public override bool TryAdd(string key, BagItem value)
        {

            bool iscommited = false;

            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbAddCommand(key, value);
                            db.ExecuteTransCommandNonQuery(cmdText, (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryAdd(key, value))
                                    {
                                        trans.Commit();
                                        iscommited = true;
                                    }
                                }
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryAdd(key, value))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistanceTask()
                                {
                                    CommandText = DbAddCommand(key, value),
                                    CommandType = "DbAdd",
                                    ConnectionString = ConnectionString
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
        public override bool TryRemove(string key, out BagItem value)
        {

            bool iscommited = false;
            T outval = value = default(T);
            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbDeleteCommand(key);
                            db.ExecuteTransCommandNonQuery(cmdText, (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryRemove(key, out outval))
                                    {
                                        trans.Commit();
                                        iscommited = true;
                                    }
                                }
                            });
                        }
                        break;
                    default:
                        if (dictionary.TryRemove(key, out outval))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistanceTask()
                                {
                                    CommandText = DbDeleteCommand(key),
                                    CommandType = "DbDelete",
                                    ConnectionString = ConnectionString
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
        public override bool TryUpdate(string key, BagItem newValue, BagItem comparisonValue)
        {

            bool iscommited = false;

            try
            {
                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand(key, newValue);
                            db.ExecuteTransCommandNonQuery(cmdText, (result, trans) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryUpdate(key, newValue, comparisonValue))
                                    {
                                        trans.Commit();
                                        iscommited = true;
                                    }
                                }
                            });
                        }
                        break;
                    default:

                        if (dictionary.TryUpdate(key, newValue, comparisonValue))
                        {
                            if (_CommitMode == CommitMode.OnMemory)
                            {
                                var task = new PersistanceTask()
                                {
                                    CommandText = DbUpdateCommand(key, newValue),
                                    CommandType = "DbUpdate",
                                    ConnectionString = ConnectionString
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

        */


        #endregion

    }
}
