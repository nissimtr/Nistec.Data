using Nistec.Data.Entities;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Nistec.Data.Persistance
{

    public class PersistentSqlBinary<T> : PersistentDbBinary<T, SqlParameter>
    {
        #region ctor

        public PersistentSqlBinary(PersistentDbSettings settings)
            : base(settings)
        {

        }

        //public PersistentSqlBinary(string name)
        //    : base(new PersistentDbSettings() { Name = name })
        //{

        //}

        #endregion
    }

    public class PersistentDbBinary<T,TP> : PersistentDbBase<T, PersistBinaryItem, TP>   where TP : IDbDataParameter
    {

        #region ctor

        public PersistentDbBinary(PersistentDbSettings settings)
            : base(settings)
        {

        }

        //public PersistentDbBinary(string name)
        //    : base(new PersistentDbSettings() { Name = name })
        //{

        //}

        #endregion

        #region override

        const string sqlcreate = @"if not exists (select * from sysobjects where name='{0}' and xtype='U')
                        create table {0} (
                        	[key] [varchar](50) NOT NULL PRIMARY KEY CLUSTERED ,
	                        [body] [varbinary](max) NULL,
	                        [name] [varchar](50) NOT NULL,
	                        [timestamp] [datetime] NOT NULL DEFAULT (GETDATE())
                        );";
        const string sqlinsert = "insert into {0} (key, body, name) values (@key, @body, @name)";
        const string sqldelete = "delete from {0} where key=@key";
        const string sqlupdate = "update {0} set body=@body, timestamp=getdate() where key=@key";
        const string sqlinsertOrReplace = "if not exists(select 1 from {0} where key=@key) begin insert into {0} (key, body, name) values (@key, @body, @name) end else begin update {0} set body=@body, timestamp=getdate() where key=@key end";
        const string sqlinsertOrIgnore = "if not exists(select 1 from {0} where key=@key) begin insert into {0} (key, body, name) values (@key, @body, @name) end";
        //const string sqlinsertOrIgnore = "insert or ignore into {0}(key, body, name) values(@key, @body, @name)";
        //const string sqlinsertOrReplace = "insert or replace into {0}(key, body, name) values(@key, @body, @name)";
        const string sqlselect = "select {1} from {0} where key=@key";
        const string sqlselectall = "select {1} from {0}";

       
        protected override string DbCreateCommand()
        {
            return string.Format(sqlcreate, TableName);
        }
        protected override string DbAddCommand()
        {
            return string.Format(sqlinsert, TableName);
        }

        protected override string DbDeleteCommand()
        {

            return string.Format(sqldelete, TableName);
        }

        protected override string DbUpdateCommand()
        {
            return string.Format(sqlupdate, TableName);
        }

        protected override string DbUpsertCommand()
        {
            return string.Format(sqlinsertOrReplace, TableName);
        }

        protected override string DbSelectCommand(string select, string where)
        {
            if (where == null)
                return string.Format(sqlselectall, TableName, select);

            return string.Format(sqlselect, TableName, select, where);
        }

        protected override string DbLookupCommand()
        {
            return string.Format(sqlselect, TableName, "body");
        }

        protected override string DbUpdateStateCommand()
        {
            return string.Format(sqlupdate, TableName);
        }
        
        //protected override object GetDataValue(T item)
        //{
        //    return BinarySerializer.SerializeToBytes(item);
        //}

        protected override byte[] GetDataBinary(T item)
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

        protected override PersistBinaryItem ToPersistItem(string key,string name, T item)
        {
            return new PersistBinaryItem()
            {
                body = BinarySerializer.SerializeToBytes(item),
                key = key,
                name = name,
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
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="name">The section name to be added or whose value should be updated</param>
        /// <param name="item">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int AddOrUpdate(string key, string name, T item)
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
                var body = GetDataBinary(item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbUpsertCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name), (result) =>
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
                            ExecuteTask(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name));
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
        ///    updates a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
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
                var body = GetDataBinary(item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbUpdateCommand();
                            res = db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body), (result) =>
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
                            ExecuteTask(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body));
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
        /// Attempts to add the specified key and value to the System.Collections.Concurrent.ConcurrentDictionary
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="name">The section name to be added or whose value should be updated</param>
        /// <param name="item">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public override bool TryAdd(string key, string name, T item)
        {

            bool iscommited = false;

            try
            {

                var body = GetDataBinary(item);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbAddCommand();

                            //db.ExecuteNonQueryTrans(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", value.body, "name", value.name), (result) =>
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


                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name), (result) =>
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
                                ExecuteTask(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", body, "name", name));
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
        ///     Attempts to remove and return the value with the specified key from the System.Collections.Concurrent.ConcurrentDictionary
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
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbDeleteCommand();
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
                    case CommitMode.OnMemory:
                        {
                            if (dictionary.TryRemove(key, out outval))
                            {
                                var cmdText = DbDeleteCommand();
                                ExecuteTask(cmdText, DataParameter.GetDbParam<TP>("key", key));
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
                var newValue = GetDataBinary(newItem);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            var cmdText = DbUpdateCommand();
                            db.ExecuteTransCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", newValue), (result) =>
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
                                ExecuteTask(cmdText, DataParameter.GetDbParam<TP>("key", key, "body", newValue, "name"));
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
        //            using (var db = Settings.Connect())
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
