using Nistec.Data.Entities;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data.Persistance
{

    public class PersistentSqlCover<T> : PersistentDbBinary<T, SqlParameter>
    {
        

        #region ctor

        public PersistentSqlCover(PersistentDbSettings settings)
            : base(settings)
        {

        }

        public PersistentSqlCover(string connectionString, DBProvider provider, string tableName)
            : base(new PersistentDbSettings(connectionString, provider, tableName))
        {

        }

        #endregion

        #region override

        const string sqlfetch = "spQCover_Fetch";
        const string sqlfetchcommit = "delete from {0} where key=@key";
        const string sqlfetchcount = "select count(*) from {0} where name=@name";

        protected virtual string DbFetchCommand()
        {
            return sqlfetch;
        }

        protected virtual string DbFetchCommitCommand()
        {
            return string.Format(sqlfetchcommit, TableName);
        }

        protected virtual string DbFetchCountCommand()
        {
            return string.Format(sqlfetchcount, TableName);
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

            bool iscommited = false;

            try
            {
                var value = ToPersistItem(key, name,item);
                var cmdText = DbUpsertCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", value.body, "name", value.name));
                            iscommited = true;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = cmdText,
                            CommandType = "DbUpsert",
                            ConnectionString = Settings.ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key, "body", value.body, "name", value.name)
                        };
                        task.ExecuteTask(_EnableTasker);
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
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int Update(string key, T value)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                var body = GetDataBinary(value);
                var cmdText = DbUpdateCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body));
                            iscommited = true;
                        }

                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = cmdText,
                            CommandType = "DbUpdate",
                            ConnectionString = Settings.ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key, "body", body)
                        };
                        task.ExecuteTask(_EnableTasker);
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
        public override bool TryAdd(string key, string name, T value)
        {

            bool iscommited = false;
            try
            {
                var body = GetDataBinary(value);
                var cmdText = DbAddCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body, "name", name));
                            iscommited = res > 0;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = cmdText,
                            CommandType = "DbAdd",
                            ConnectionString = Settings.ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key, "body", body, "name", name)
                        };
                        task.ExecuteTask(_EnableTasker);
                        iscommited = true;
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
        public override bool TryRemove(string key, out T value)
        {

            bool iscommited = false;
            T outval = value = default(T);
            try
            {
                var cmdText = DbDeleteCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key));
                            iscommited = res > 0;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = cmdText,
                            CommandType = "DbDelete",
                            ConnectionString = Settings.ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key)
                        };
                        task.ExecuteTask(_EnableTasker);
                        iscommited = true;
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
        public override bool TryUpdate(string key, T newValue, T comparisonValue)
        {

            bool iscommited = false;

            try
            {
                var body = GetDataBinary(newValue);
                var cmdText = DbUpdateCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body));
                            iscommited = res > 0;

                        }
                        break;
                    default:

                        var task = new PersistentDbTask()
                        {
                            CommandText = cmdText,
                            CommandType = "DbUpdate",
                            ConnectionString = Settings.ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key, "body", body)
                        };
                        task.ExecuteTask(_EnableTasker);
                        iscommited = true;
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

        #region Fetch

        public virtual bool TryFetch(string name, out IPersistBinaryItem item)
        {

            bool iscommited = false;
            item = null;

            try
            {

                bool WaitForCommit = (_CommitMode == CommitMode.OnDisk) ? true : false;
                using (var db = Settings.Connect())
                {
                    var cmdText = DbFetchCommand();//key);
                    item = db.ExecuteCommand<PersistBinaryItem>(cmdText, DataParameter.GetDbParam<SqlParameter>("Name", name, "WaitForCommit", WaitForCommit), CommandType.StoredProcedure);
                    iscommited = item != null;
                }

            }
            catch (Exception ex)
            {
                OnErrorOcurred("TryFetch", ex.Message);
            }
            return iscommited;
        }

        public virtual bool TryFetchBulk(string name, int Limit, out IEnumerable<IPersistBinaryItem> items)
        {

            bool iscommited = false;
            items = null;

            try
            {
                bool WaitForCommit = (_CommitMode == CommitMode.OnDisk) ? true : false;

                using (var db = Settings.Connect())
                {
                    var cmdText = DbFetchCommand();//key);
                    items = db.ExecuteCommand<PersistBinaryItem, List<PersistBinaryItem>>(cmdText, DataParameter.GetDbParam<SqlParameter>("Name", name, "Limit", Limit, "WaitForCommit", WaitForCommit), CommandType.StoredProcedure);

                    iscommited = items != null;
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("TryFetchBulk", ex.Message);
            }
            return iscommited;
        }

        public virtual bool TryFetchCommit(string key)
        {

            bool iscommited = false;
            try
            {
                using (var db = Settings.Connect())
                {
                    var cmdText = DbFetchCommitCommand();
                    int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key));
                    iscommited = res > 0;
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("TryFetchCommit", ex.Message);
            }
            return iscommited;
        }

        public virtual int FetchCount(string name)
        {

            try
            {
                using (var db = Settings.Connect())
                {
                    var cmdText = DbFetchCountCommand();
                    return db.ExecuteCommandScalar<int>(cmdText, DataParameter.GetDbParam<SqlParameter>("Name", name), 0);
                }
            }
            catch (Exception ex)
            {
                OnErrorOcurred("FetchCount", ex.Message);
            }
            return 0;
        }

        #endregion

        public bool TryAddJournal(string key, string name, T value)
        {

            bool iscommited = false;
            try
            {
                var body = GetDataBinary(value);
                var cmdText = DbAddJournalCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body, "name", name));
                            iscommited = res > 0;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = cmdText,
                            CommandType = "DbAddJournal",
                            ConnectionString = Settings.ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key, "body", body, "name", name)
                        };
                        task.ExecuteTask(_EnableTasker);
                        iscommited = true;
                        break;
                }

                if (iscommited)
                {
                    OnItemChanged("TryAddJournal", key, value);
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryAddJournal", ex.Message);
            }
            return iscommited;

        }

        public bool TryAddJournal(string name, IPersistBinaryItem item)
        {

            bool iscommited = false;
            try
            {
                var cmdText = DbAddJournalCommand();

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = Settings.Connect())
                        {
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", item.key, "body", item.body, "name", name));
                            iscommited = res > 0;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = cmdText,
                            CommandType = "DbAddJournal",
                            ConnectionString = Settings.ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", item.key, "body", item.body, "name", name)
                        };
                        task.ExecuteTask(_EnableTasker);
                        iscommited = true;
                        break;
                }

                //if (iscommited)
                //{
                //    OnItemChanged("TryAddJournal", item.key, value);
                //}
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                OnErrorOcurred("TryAddJournal", ex.Message);
            }
            return iscommited;

        }
        

    }


}
