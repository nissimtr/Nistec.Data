using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Data.Sqlite
{
    public class DbLiteUtil
    {
        public static string ConnectionStringBuilder(string databaseFilePath)
        {
            SQLiteConnectionStringBuilder conString = new SQLiteConnectionStringBuilder();
            conString.DataSource = databaseFilePath;
            conString.DefaultTimeout = 5000;
            conString.SyncMode = SynchronizationModes.Off;
            conString.JournalMode = SQLiteJournalModeEnum.Memory;
            conString.PageSize = 65536;
            conString.CacheSize = 16777216;
            conString.FailIfMissing = false;
            conString.ReadOnly = false;
            conString.Add("auto_vacuum",1);
            return conString.ConnectionString;
        }
        public static SQLiteConnection CreateConnection(string ConnectionString)
        {
            return new SQLiteConnection(ConnectionString);
        }

        #region static util

        public static void CreateFolder(string folder)
        {
            if (Directory.Exists(folder))
                return;
            Directory.CreateDirectory(folder);
        }

        public static void CreateFile(string filename)
        {
            if (filename == null || filename == "")
            {
                throw new ArgumentNullException("SQLiteConnection.filename");
            }
            if (File.Exists(filename))
                return;
            SQLiteConnection.CreateFile(filename);//("MyDatabase.sqlite");
        }

       
        public const string ProviderName = "SQLite";

        public static string GetConnectionString(string filename)
        {
            return DbLiteUtil.ConnectionStringBuilder(filename);
        }

        //public static string GetConnectionString(string filename)
        //{
        //    return "Data Source=" + filename + ";Version=3;";
        //}

        public static bool ValidateConnection(string filename, bool enableException = false)
        {

            if (filename == null || filename == "")
            {
                throw new ArgumentNullException("SQLiteConnection.filename");
            }

            if (!File.Exists(filename))
                return false;
            try
            {

                using (SQLiteConnection connection = new SQLiteConnection(GetConnectionString(filename)))
                {
                    var cnn = connection.OpenAndReturn();

                    if (cnn != null && cnn.State == ConnectionState.Open)
                    {
                        cnn.Close();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                if (enableException)
                    throw ex;
            }
            return false;
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

        #endregion
    }
}
