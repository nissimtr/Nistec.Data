using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Configuration;
using Nistec.Generic;
using System.IO;

namespace Nistec.Data.Sqlite
{

    /*
            string sqlCreateTable = "create table demo_score (name varchar(20), score int)";
            string sqlInsert1 = "insert into demo_score (name, score) values ('Nissim', 9001)";
            string sqlInsert2 = "insert into demo_score (name, score) values ('Neomi', 9002)";
            string sqlInsert3 = "insert into demo_score (name, score) values ('Shani', 9003)";
            string sqlUpdate = "update demo_score set name='Shani' where score=@score";
            string sqlDelete = "delete from demo_score where score=@score";
            string sqlSelect = "select * from demo_score order by score desc";

            DataTable dt = null;

            DbLite.CreateFileFromSettings("sqlite.demo");
            using(DbLite db=new DbLite("sqlite.demo"))
            {
                db.OwnsConnection = true;
                db.ExecuteCommandNonQuery(sqlCreateTable);
                db.ExecuteCommandNonQuery(sqlInsert1);
                db.ExecuteCommandNonQuery(sqlInsert2);
                db.ExecuteCommandNonQuery(sqlInsert3);
                db.ExecuteCommandNonQuery(sqlUpdate, DbLite.GetParam("score", 9004));
                db.ExecuteCommandNonQuery(sqlDelete, DbLite.GetParam("score", 9002));

                dt=db.ExecuteDataTable("demo_score", sqlSelect);
                db.OwnsConnection = false;
            }

            Console.Write(dt.TableName);
    */
    public class DbLite: Nistec.Data.Entities.DbContextCommand, IDisposable
    {
        public static void CreateFolder(string folder)
        {
            if (Directory.Exists(folder))
                return;
            Directory.CreateDirectory(folder);
        }

        public static void CreateFile(string filename)
        {
            if (File.Exists(filename))
                return;
            SQLiteConnection.CreateFile(filename);//("MyDatabase.sqlite");
        }
        public static void CreateFileFromSettings(string dbName)
        {
            string filename = NetConfig.AppSettings[dbName];
            if (filename == null || filename == "")
            {
                throw new Exception("ConnectionStringSettings configuration not found");
            }
            if (File.Exists(filename))
                return;
            SQLiteConnection.CreateFile(filename);//("MyDatabase.sqlite");
        }
          
         

        #region ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public DbLite():base()
        {
        }


        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionKey"></param>
        public DbLite(string connectionKey)
            : base(connectionKey)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="provider"></param>
        public DbLite(string connectionString, DBProvider provider)
            : base(connectionString, provider)
        {
        }

        internal void SetConnectionInternal(string connectionKey, string connectionString, DBProvider provider, bool enableBinding)
        {

            if (string.IsNullOrEmpty(connectionString))
            {
                ConnectionStringSettings cnn = NetConfig.ConnectionContext(connectionKey);
                if (cnn == null)
                {
                    throw new Exception("ConnectionStringSettings configuration not found");
                }
                Provider = DBProvider.SQLite;
                ConnectionString = cnn.ConnectionString;
                ConnectionName = connectionKey;
            }
            else
            {
                ConnectionName = connectionKey;
                Provider = DBProvider.SQLite;
                ConnectionString = connectionString;
            }

            if (enableBinding)
            {
                EntityBind();
            }
        }
        /// <summary>
        /// Occured befor DbContext initilaized
        /// </summary>
        protected virtual void EntityBind() { }

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

        ~DbLite()
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

        protected override IDbCommand CreateCommand(string cmdText)
        {
            return base.CreateCommand(cmdText);
        }

        protected override IDbCommand CreateCommand(string cmdText, IDbDataParameter[] parameters)
        {
            SQLiteCommand command = new SQLiteCommand(cmdText, Connection as SQLiteConnection);
            if (parameters != null)
                command.Parameters.AddRange(parameters);
            return command;
        }

        protected override IDbDataAdapter CreateIDataAdapter(IDbCommand cmd)
        {
                return new SQLiteDataAdapter((SQLiteCommand)cmd) as IDbDataAdapter;
        }
        #endregion

        /// <summary>
        /// Executes a command NonQuery and returns the number of rows affected.
        /// </summary>
        /// <param name="cmdText">Sql command.</param>
        /// <param name="keyValueParameters">Sql parameters.</param>
        /// <returns></returns> 
        public int ExecuteNonQuery(string cmdText, params object[] keyValueParameters)
        {
            IDbDataParameter[] patameters = (keyValueParameters == null || keyValueParameters.Length == 0) ? null : GetParam(keyValueParameters);
            return ExecuteCommandNonQuery(cmdText, patameters, CommandType.Text);
        }

        //public void ExecuteReader()
        //{

        //    //SQLiteDataReader reader = command.ExecuteReader();
        //    //while (reader.Read())
        //    //    Console.WriteLine("Name: " + reader["name"] + "\tScore: " + reader["score"]);
        //}

        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static IDbDataParameter[] GetParam(params object[] keyValueParameters)
        {

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new SQLiteParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
            }

            return list.ToArray();
        }

    }
}
