using Nistec.Data.Entities;
using Nistec.Data.Persistance;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Nistec.Data.Sqlite
{

  

    public class PersistentBinary<T> : PersistentDictionary<T, PersistBinaryItem> 
    {

        #region ctor

        public PersistentBinary(DbLiteSettings settings)
            : base(settings)
        {
            
        }

        public PersistentBinary(string name)
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
                          body BLOB,
                          name TEXT,
                          timestamp DATETIME DEFAULT CURRENT_TIMESTAMP     
                        ) WITHOUT ROWID;";
        const string sqlinsert = "insert into {0} (key, body, name) values (@key, @body, @name)";
        const string sqldelete = "delete from {0} where key=@key";
        const string sqlupdate = "update {0} set body=@body, timestamp=CURRENT_TIMESTAMP where key=@key";
        const string sqlinsertOrIgnore = "insert or ignore into {0}(key, body, name) values(@key, @body, @name)";
        const string sqlinsertOrReplace = "insert or replace into {0}(key, body, name) values(@key, @body, @name)";
        const string sqlselect = "select {1} from {0} where key=@key";
        const string sqlselectall = "select {1} from {0}";

        protected override string DbCreateCommand()
        {
            return string.Format(sqlcreate, Name);
        }
        protected override string DbAddCommand()
        {
            return string.Format(sqlinsert, Name);
        }

        protected override string DbDeleteCommand()
        {

            return string.Format(sqldelete, Name);
        }

        protected override string DbUpdateCommand()
        {
            return string.Format(sqlupdate, Name);
        }

        protected override string DbUpsertCommand()
        {
            return string.Format(sqlinsertOrReplace, Name);
        }

        protected override string DbSelectCommand(string select, string where)
        {
            if(where==null)
                return string.Format(sqlselectall, Name, select);

            return string.Format(sqlselect, Name, select, where);
        }

        protected override string DbLookupCommand()
        {
            return string.Format(sqlselect, Name, "body");
        }

        protected override string DbUpdateStateCommand()
        {
            return string.Format(sqlupdate, Name);
        }

        protected override object GetDataValue(T item)
        {
            return BinarySerializer.SerializeToBytes(item);
            //return value;
        }
        protected override string GetItemKey(PersistBinaryItem value)
        {
            return value.key;
        }

        protected override T DecompressValue(PersistBinaryItem value)
        {
            return BinarySerializer.Deserialize<T>(value.body);
        }

        protected override PersistBinaryItem ToPersistItem(string key, T item)
        {
            return new PersistBinaryItem()
            {
                body = BinarySerializer.SerializeToBytes(item),
                key = key,
                name = this.Name,
                timestamp = DateTime.Now
            };
        }
        protected override T FromPersistItem(PersistBinaryItem value)
        {
            return BinarySerializer.Deserialize<T>(value.body);
        }


        #endregion

        #region Commands


        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="item">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int AddOrUpdate(string key, T item)
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
                var value = ToPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpsertCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText,DataParameter.Get<SQLiteParameter>("key", key,"body", value.body, "name", value.name) , (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = item;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }
                        break;
                    case CommitMode.OnMemory:
                        {
                            dictionary.AddOrUpdate(key, item, (oldkey, oldValue) =>
                            {
                                return item;
                            });
                            var cmdText = DbUpsertCommand();
                            ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                            res = 1;
                            iscommited = true;
                        }
                        break;
                    default:
                        dictionary.AddOrUpdate(key, item, (oldkey, oldValue) =>
                        {
                            return item;
                        });
                        
                        res = 1;
                        iscommited = true;
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("AddOrUpdate", null, item);
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
        /// <param name="item">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int Update(string key, T item)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                var value = ToPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name), (result) =>
                            {
                                if (result > 0)
                                {
                                    dictionary[key] = item;
                                    //trans.Commit();
                                    iscommited = true;
                                }
                                return iscommited;
                            });
                        }

                        break;
                    case CommitMode.OnMemory:
                        {
                            dictionary[key] = item;
                            var cmdText = DbUpdateCommand();
                            ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                            res = 1;
                            iscommited = true;
                        }
                        break;
                    default:
                        dictionary[key] = item;
                        res = 1;
                        iscommited = true;
                        break;
                }
                if (iscommited)
                {
                    OnItemChanged("Update", null, item);
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
        /// <param name="item">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public override bool TryAdd(string key, T item)
        {

            bool iscommited = false;

            try
            {

                var value = ToPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbAddCommand();

                            //db.ExecuteNonQueryTrans(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name), (result) =>
                            //{

                            //    if (result > 0)
                            //    {
                            //        if (dictionary.TryAdd(key, item))
                            //        {
                            //            //trans.Commit();
                            //            iscommited = true;
                            //        }
                            //    }
                            //    return iscommited;
                            //});


                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name), (result) =>
                            {
                                if (result > 0)
                                {
                                    if (dictionary.TryAdd(key, item))
                                    {
                                        //trans.Commit();
                                        iscommited = true;
                                    }
                                }
                                return iscommited;
                            });
                        }
                        break;
                    case CommitMode.OnMemory:
                        {
                            if (dictionary.TryAdd(key, item))
                            {
                                var cmdText = DbAddCommand();
                                //var res = ExecuteAsync(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                                ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", value.body, "name", value.name));
                                iscommited = true;
                            }
                        }
                        break;
                    default:
                        iscommited = dictionary.TryAdd(key, item);
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryAdd", key, item);
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
        public override bool TryRemove(string key, out T item)
        {

            bool iscommited = false;
            T outval = item = default(T);
            try
            {
                //var value = GetPersistItem(key, item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbDeleteCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key), (result) =>
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
                    case CommitMode.OnMemory:
                        {
                            if (dictionary.TryRemove(key, out outval))
                            {
                                var cmdText = DbDeleteCommand();
                                ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key));
                                iscommited = true;
                            }
                        }
                        break;
                    default:
                        iscommited = dictionary.TryRemove(key, out outval);
                        break;
                }

                item = outval;

                if (iscommited)
                {
                    OnItemChanged("TryRemove", key, item);
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
        public override bool TryUpdate(string key, T newItem, T comparisonValue)
        {

            bool iscommited = false;

            try
            {
                var newValue = ToPersistItem(key, newItem);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbUpdateCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", newValue.body, "name", newValue.name), (result) =>
                             {
                                 if (result > 0)
                                 {
                                     if (dictionary.TryUpdate(key, newItem, comparisonValue))
                                     {
                                        //trans.Commit();
                                        iscommited = true;
                                     }
                                 }
                                 return iscommited;
                             });
                        }
                        break;
                    case CommitMode.OnMemory:
                        {
                            if (dictionary.TryUpdate(key, newItem, comparisonValue))
                            {
                                var cmdText = DbUpdateCommand();
                                ExecuteTask(cmdText, DataParameter.Get<SQLiteParameter>("key", key, "body", newValue.body, "name", newValue.name));
                                iscommited = true;
                            }
                        }
                        break;
                    default:
                        iscommited = dictionary.TryUpdate(key, newItem, comparisonValue);
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryUpdate", key, newItem);
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

        //public override void LoadDb()
        //{
        //    IList<PersistBinaryItem> list;

        //    using (TransactionScope tran = new TransactionScope())
        //    {
        //        try
        //        {
        //            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //            {

        //                var sql = "select * from " + Name;
        //                list = db.Query<PersistBinaryItem>(sql, null);
        //                if (list != null && list.Count > 0)
        //                {
        //                    foreach (var entry in list)
        //                    {
        //                        var val = BinarySerializer.Deserialize<T>((byte[])entry.body);

        //                        //var o = BinarySerializer.Deserialize((byte[])entry.Value);

        //                        //BinaryStreamer streamer = new BinaryStreamer(new NetStream((byte[])entry.Value), null);

        //                        dictionary[entry.key] = val;// GenericTypes.Convert<T>(val);
        //                    }
        //                }

        //            }

        //            tran.Complete();
        //        }
        //        catch (Exception ex)
        //        {
        //            string err = ex.Message;
        //        }
        //    }
        //}

    }
}
