using Nistec.Data.Persistance;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Data.Sqlite
{
    public class DbLiteSettings
    {
        public const string DefaultPassword = "giykse876435365&%$^#%@$#@)_(),kxa;l bttsklf12[]}{}{)(*XCJHG^%%";
        public const string InMemoryFilename = ":memory:";
        
        public const int DefaultCommandTimeout = 5000;
        public const int DefaultConnectionTimeout = 5000;

        public DbLiteSettings()
        {
            DefaultTimeout = 5000;
            //SyncMode = SynchronizationModes.Off;
            //JournalMode = SQLiteJournalModeEnum.Memory;
            //PageSize = 65536;
            //CacheSize = 16777216;
            FailIfMissing = false;
            ReadOnly = false;
            InMemory = false;

            //PRAGMA main.locking_mode = EXCLUSIVE;
            SyncMode = SynchronizationModes.Normal;//.Normal;
            JournalMode = SQLiteJournalModeEnum.Delete;
            PageSize = 65536;// 4096;
            CacheSize = 16777216;// 100000;


            Name = "xPersistent";
            AutoSave = true;
            EnableTasker = false;
            DbPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //Filename = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + ".xconfig";
            //RootTag = "xconfig";
            UseFileWatcher = true;
            CommitMode = CommitMode.OnDisk;
            EnableTasker = false;
            EnableCompress = false;
            CompressLevel = 0;
            //Encrypted = false;
            //Password = DefaultPassword;
        }

        public void SetFast()
        {
            DefaultTimeout = 5000;
            SyncMode = SynchronizationModes.Off;
            JournalMode = SQLiteJournalModeEnum.Memory;
            PageSize = 65536;
            CacheSize = 16777216;
            FailIfMissing = false;
        }

        public void Validate()
        {
            //512 to 65536

            if (PageSize < 512)
                PageSize = 1024;
            else if (PageSize > 65536)
                PageSize = 65536;

            if (CacheSize < 1000)
                CacheSize = 10000;
        }

        public string GetConnectionString()
        {
            Validate();

            SQLiteConnectionStringBuilder conString = new SQLiteConnectionStringBuilder();
            if (InMemory)
                conString.DataSource = InMemoryFilename;
            else
                conString.DataSource = DbFilename;

            conString.DefaultTimeout = DefaultTimeout;
            conString.SyncMode = SyncMode;
            conString.JournalMode = JournalMode;
            conString.PageSize = PageSize;
            conString.CacheSize = CacheSize;
            conString.FailIfMissing = FailIfMissing;
            conString.ReadOnly = ReadOnly;
            return conString.ConnectionString;

        }

        public int DefaultTimeout { get; set; }
        public SynchronizationModes SyncMode { get; set; }
        public SQLiteJournalModeEnum JournalMode { get; set; }
        public int PageSize { get; set; }
        public int CacheSize { get; set; }
        public bool FailIfMissing { get; set; }
        public bool ReadOnly { get; set; }
        public bool InMemory { get; set; }

        /// <summary>
        /// Get The config full file path.
        /// </summary>
        public string ConfigFilename { get { return Path.Combine(DbPath, ".xconfig"); } }
        /// <summary>
        /// Get or Set The db full file path.
        /// </summary>
        public string DbFilename { get { return Path.Combine(DbPath, Name + ".db"); } }


        /// <summary>
        /// Get or Set The Persistent Name.
        /// Default is 'xPersistent';
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Get or Set value indicating is XConfig will save changes to config file when each item value has changed, 
        /// Default is true;
        /// </summary>
        public bool AutoSave { get; set; }

        /// <summary>
        /// Get or Set The config file path.
        /// Default is (Current Location);
        /// </summary>
        public string DbPath { get; set; }

        /// <summary>
        /// Use event of a System.IO.FileSystemWatcher class.
        /// Default is true;
        /// </summary>
        public bool UseFileWatcher { get; set; }

        /// <summary>
        /// Use commit mode.
        /// Default is OnDisk;
        /// </summary>
        public CommitMode CommitMode { get; set; }

        /// <summary>
        /// Get or Set if enable tasker queue for CommitMode.OnMemory
        /// </summary>
        public bool EnableTasker { get; set; }

        /// <summary>
        /// Get or Set if enable compress data
        /// </summary>
        public bool EnableCompress { get; set; }
        /// <summary>
        /// Get or Set the compress level (1-9) if enable compress data
        /// </summary>
        public int CompressLevel { get; set; }
      
        public int ValidCompressLevel
        {
            get
            {
                if (CompressLevel < 1 || CompressLevel > 9)
                    return 9;
                return CompressLevel;
            }
        }

        ///// <summary>
        ///// Get or Set the password for encryption.
        ///// Default is internal password;
        ///// </summary>
        //public string Password { get; set; }
        ///// <summary>
        ///// Use Encryption configuration.
        ///// Default is false;
        ///// </summary>
        //public bool Encrypted { get; set; }


    }
}
