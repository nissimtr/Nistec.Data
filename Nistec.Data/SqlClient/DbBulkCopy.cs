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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Nistec.Data.Factory;
using Nistec.Data.Entities;

namespace Nistec.Data.SqlClient
{

    #region generic DbCommand

    /// <summary>
    /// Represent an object that execute <see cref="SqlBulkCopy"/>  commands using generic <see cref="IDbContext"/> .
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbBulkCopy<T> : DbBulkCopy where T : IDbContext
    {
        public DbBulkCopy()
        {
            base.Init<T>();
        }
    }
    #endregion

    /// <summary>
    /// Represent an object that execute <see cref="SqlBulkCopy"/>  commands.
    /// </summary>
    public class DbBulkCopy : DbBase, IDisposable
    {

        #region Constructor

        

        internal DbBulkCopy()
        {
            SetDefault();
        }

        /// <summary>
        /// DbCommand Constructor with AutoBase. 
        /// </summary>
        public DbBulkCopy(IAutoBase dalBase)//:base(dalBase)
        {
            SetDefault();
            base.Init(dalBase);
        }

        /// <summary>
        /// DbCommand Constructor with IDbContext. 
        /// </summary>
        public DbBulkCopy(IDbContext db)//:base(dalBase)
        {
            SetDefault();
            base.Init(db.ConnectionString, DBProvider.SqlServer,true);
        }


        /// <summary>
        /// DbCommand Constructor with connection string
        /// </summary>
        public DbBulkCopy(string connectionString)
        {
            SetDefault();
            base.Init(connectionString, DBProvider.SqlServer, true);
        }


        /// <summary>
        /// DbCommand Constructor with connection
        /// </summary>
        public DbBulkCopy(SqlConnection connection, bool autoCloseConnection)
        {
            SetDefault();
            base.Init(connection, autoCloseConnection);
        }

        /// <summary>
        /// Class destructor.
        /// </summary>
        ~DbBulkCopy()
        {
            Dispose(false);
        }

        #endregion

        #region bulk copy async


        public event SqlRowsCopiedEventHandler SqlRowsCopied;


        private void SetDefault()
        {
            BatchSize = 1000;
            NotifyAfter = 1000;
            BulkCopyTimeout = 300;
        }

        //bool uploaded;
        string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
        }

        public int BatchSize
        {
            get;
            set;
        }

        public int NotifyAfter
        {
            get;
            set;
        }

        public int BulkCopyTimeout
        {
            get;
            set;

        }

        delegate void BulkInsertDelegate(DataTable source, string destinationTableName, int batchSize, int notifyAfter, int timeout, params SqlBulkCopyColumnMapping[] mapings);

        public static SqlBulkCopyColumnMapping[] CreateMapping(DataTable dt)
        {
            List<SqlBulkCopyColumnMapping> list = new List<SqlBulkCopyColumnMapping>();

            foreach(DataColumn col in dt.Columns)
            {
                list.Add(new SqlBulkCopyColumnMapping(col.ColumnName, col.ColumnName));
            }
            return list.ToArray();
        }
        public static SqlBulkCopyColumnMapping[] CreateMapping(Dictionary<string,string> columns)
        {
            List<SqlBulkCopyColumnMapping> list = new List<SqlBulkCopyColumnMapping>();

            foreach (var col in columns)
            {
                list.Add(new SqlBulkCopyColumnMapping(col.Key, col.Value));
            }
            return list.ToArray();
        }

        public static SqlBulkCopyColumnMapping[] CreateMapping(params string[] columns)
        {
            List<SqlBulkCopyColumnMapping> list = new List<SqlBulkCopyColumnMapping>();

            foreach (var col in columns)
            {
                list.Add(new SqlBulkCopyColumnMapping(col, col));
            }
            return list.ToArray();
        }

        public void BulkInsertAsync(DataTable source, string destinationTableName)
        {

            BulkInsertDelegate d = new BulkInsertDelegate(BulkInsert);

            IAsyncResult ar = d.BeginInvoke(source, destinationTableName, BatchSize,NotifyAfter,BulkCopyTimeout, null, null,null);

            d.EndInvoke(ar);

        }

        public void BulkInsertAsync(DataTable source, string destinationTableName, int timeout)
        {
            BulkCopyTimeout = timeout;

            BulkInsertDelegate d = new BulkInsertDelegate(BulkInsert);

            IAsyncResult ar = d.BeginInvoke(source, destinationTableName, BatchSize, NotifyAfter, BulkCopyTimeout, null, null, null);

            d.EndInvoke(ar);
        }


        public void BulkInsert(DataTable source, string destinationTableName, int batchSize, int notifyAfter, int timeout, params SqlBulkCopyColumnMapping[] mapings)
        {
            BatchSize = batchSize;
            NotifyAfter = notifyAfter;
            BulkCopyTimeout = timeout;

            BulkInsert(source, destinationTableName, BatchSize, NotifyAfter, BulkCopyTimeout, null);
        }

        public void BulkInsert(DataTable source, string destinationTableName, int timeout, params SqlBulkCopyColumnMapping[] mapings)
        {
            BulkCopyTimeout = timeout;
            BulkInsert(source, destinationTableName, BatchSize, NotifyAfter, BulkCopyTimeout, null);
        }

        public void BulkInsert(DataTable source, string destinationTableName, params SqlBulkCopyColumnMapping[] mapings)
        {

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)base.Connection))
            {
                try
                {
                    base.Connection.Open();
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }
                System.Data.DataTableReader reader = new System.Data.DataTableReader(source);
                bulkCopy.DestinationTableName = destinationTableName;
                bulkCopy.BatchSize = BatchSize;
                // Set up the event handler to notify after x rows.
                bulkCopy.SqlRowsCopied +=
                    new SqlRowsCopiedEventHandler(OnSqlRowsCopied);
                bulkCopy.NotifyAfter = NotifyAfter;
                bulkCopy.BulkCopyTimeout = BulkCopyTimeout;
                if (mapings != null)
                {
                    foreach (SqlBulkCopyColumnMapping col in mapings)
                    {
                        bulkCopy.ColumnMappings.Add(col);
                    }
                }
                try
                {
                    // Write from the source to the destination.
                    bulkCopy.WriteToServer(reader);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    throw ex;
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        private void OnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            if (this.SqlRowsCopied != null)
                this.SqlRowsCopied(this, e);

        }

        #endregion

    }
}
