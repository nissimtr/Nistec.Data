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

    public class PersistentSqlCover<T> : PersistentDbBinary<T, SqlParameter>
    {
        #region ctor

        public PersistentSqlCover(PersistentDbSettings settings)
            : base(settings)
        {

        }

        public PersistentSqlCover(string name, string connectionString)
            : base(new PersistentDbSettings() { Name = name , DbProvider= DBProvider.SqlServer, ConnectionString=connectionString})
        {

        }

        #endregion


        #region Commands
        /// <summary>
        ///    Adds a key/value pair to the System.Collections.Concurrent.ConcurrentDictionary
        ///     if the key does not already exist, or updates a key/value pair in the System.Collections.Concurrent.ConcurrentDictionary
        ///     if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The value to be added or updated for an absent key</param>
        /// <returns></returns>
        public override int AddOrUpdate(string key, T value)
        {
            int res = 0;

            bool iscommited = false;

            try
            {
                var body = GetDataValue(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbUpsertCommand();//key, value);
                            res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body));
                            iscommited = true;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = DbUpsertCommand(),//key, value),
                            CommandType = "DbUpsert",
                            ConnectionString = ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key, "body", body)
                        };
                        task.ExecuteTask(_EnableTasker);
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
        public override int Update(string key, T value)
        {
            bool iscommited = false;
            int res = 0;

            try
            {
                var body = GetDataValue(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbUpdateCommand();//key, value);
                            res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body));
                            iscommited = true;
                        }

                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = DbUpdateCommand(),//key, value),
                            CommandType = "DbUpdate",
                            ConnectionString = ConnectionString,
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
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        ///     in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the System.Collections.Concurrent.ConcurrentDictionary.
        ///     successfully. If the key already exists, this method returns false.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="System.OverflowException">The dictionary already contains the maximum number of elements, System.Int32.MaxValue.</exception>
        public override bool TryAdd(string key, T value)
        {

            bool iscommited = false;
            try
            {
                var body = GetDataValue(value);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DBProvider.SQLite))
                        {
                            var cmdText = DbAddCommand();//key, value);
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body));
                            iscommited = res > 0;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = DbAddCommand(),//(key, value),
                            CommandType = "DbAdd",
                            ConnectionString = ConnectionString,
                            Parameters = DataParameter.GetDbParam<SqlParameter>("key", key, "body", body)
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

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbDeleteCommand();//key);
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key));
                            iscommited = res > 0;
                        }
                        break;
                    default:
                        var task = new PersistentDbTask()
                        {
                            DdProvider = DbProvider,
                            CommandText = DbDeleteCommand(),//key),
                            CommandType = "DbDelete",
                            ConnectionString = ConnectionString,
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
                var body = GetDataValue(newValue);

                switch (_CommitMode)
                {
                    case CommitMode.OnDisk:
                        using (var db = new DbContext(ConnectionString, DbProvider))
                        {
                            var cmdText = DbUpdateCommand();//key, newValue);
                            int res = db.ExecuteCommandNonQuery(cmdText, DataParameter.GetDbParam<SqlParameter>("key", key, "body", body));
                            iscommited = res > 0;

                        }
                        break;
                    default:

                        var task = new PersistentDbTask()
                        {
                            CommandText = DbUpdateCommand(),//key, newValue),
                            CommandType = "DbUpdate",
                            ConnectionString = ConnectionString,
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

        public virtual bool TryFetch(out IPersistBinaryItem item)
        {

            bool iscommited = false;
            item = null;

            try
            {

                bool WaitForCommit = (_CommitMode == CommitMode.OnDisk) ? true : false;
                using (var db = new DbContext(ConnectionString, DbProvider))
                {
                    var cmdText = DbFetchCommand();//key);
                    item = db.ExecuteCommand<PersistBinaryItem>(cmdText, DataParameter.GetDbParam<SqlParameter>("Name", Name, "WaitForCommit", WaitForCommit), CommandType.StoredProcedure);
                    iscommited = item != null;
                }

            }
            catch (Exception ex)
            {
                OnErrorOcurred("TryFetch", ex.Message);
            }
            return iscommited;
        }

        public virtual bool TryFetchBulk(int Limit, out IEnumerable<IPersistBinaryItem> items)
        {

            bool iscommited = false;
            items = null;

            try
            {
                bool WaitForCommit = (_CommitMode == CommitMode.OnDisk) ? true : false;

                using (var db = new DbContext(ConnectionString, DbProvider))
                {
                    var cmdText = DbFetchCommand();//key);
                    items = db.ExecuteCommand<PersistBinaryItem, List<PersistBinaryItem>>(cmdText, DataParameter.GetDbParam<SqlParameter>("Name", Name, "Limit", Limit, "WaitForCommit", WaitForCommit), CommandType.StoredProcedure);

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
                using (var db = new DbContext(ConnectionString, DbProvider))
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

        #endregion
    }


}
