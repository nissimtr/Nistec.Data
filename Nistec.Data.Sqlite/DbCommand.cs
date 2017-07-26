using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace Nistec.Data.Sqlite
{
    public class DbCommand : Nistec.Data.Entities.DbContextCommand, IDisposable
    {
        #region Ctor
        /// <summary>
        /// ctor
        /// </summary>
        public DbCommand() : base() { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cnn"></param>
        public DbCommand(IDbConnection cnn)
            : base(cnn)
        {
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbCommand(string connectionKey)
            : base(connectionKey)
        {
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbCommand(string connectionString, DBProvider provider)
            : base(connectionString, DBProvider.SQLite)
        {
        }

        #endregion

        #region Dispose

        //private bool disposed = false;

        //protected void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            if (_Command != null)
        //            {
        //                _Command.Dispose();
        //                _Command = null;
        //            }
        //            ConnectionName = null;
        //            ConnectionString = null;
        //            Database = null;
        //        }
        //        disposed = true;
        //    }
        //}

        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // take this object off the finalization queue     
            GC.SuppressFinalize(this);
        }

        ~DbCommand()
        {
            Dispose(false);
        }

        #endregion

        #region override

        protected override IDbConnection CreateConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
        protected override DBProvider GetProvider(IDbConnection cnn)
        {
            return DBProvider.SQLite;
        }
        #endregion


    }
}
