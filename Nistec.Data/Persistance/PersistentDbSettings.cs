using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Data.Persistance
{

    public class PersistentDbSettings
    {
        public const string DefaultPassword = "giykse876435365&%$^#%@$#@)_(),kxa;l bttsklf12[]}{}{)(*XCJHG^%%";
        public const string InMemoryFilename = ":memory:";

        public PersistentDbSettings()//string connectionString, DBProvider provider)
        {
            DefaultTimeout = 5000;
            //SyncMode = SynchronizationModes.Off;
            //JournalMode = SQLiteJournalModeEnum.Memory;
            //PageSize = 65536;
            //CacheSize = 16777216;
            FailIfMissing = false;
            ReadOnly = false;
            InMemory = false;

            DbProvider =  DBProvider.SqlServer;
            //_ConnectionString = connectionString;

            //PRAGMA main.locking_mode = EXCLUSIVE;
            SyncMode = (int)SyncModes.Normal;
            JournalMode = (int)JournalModeEnum.Wal;
            PageSize = 4096;
            CacheSize = 100000;


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
            SyncMode = (int)SyncModes.Off;
            JournalMode = (int)JournalModeEnum.Memory;
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

        public virtual void SetConnection()
        {
            //Validate();

            //SQLiteConnectionStringBuilder conString = new SQLiteConnectionStringBuilder();
            //if (InMemory)
            //    conString.DataSource = InMemoryFilename;
            //else
            //    conString.DataSource = DbFilename;
            //_ConnectionString = conString.ConnectionString;

            //conString.DefaultTimeout = DefaultTimeout;
            //conString.SyncMode = SyncMode;
            //conString.JournalMode = JournalMode;
            //conString.PageSize = PageSize;
            //conString.CacheSize = CacheSize;
            //conString.FailIfMissing = FailIfMissing;
            //conString.ReadOnly = ReadOnly;
            //return conString.ConnectionString;

        }

        public int DefaultTimeout { get; set; }
        public int SyncMode { get; set; }
        public int JournalMode { get; set; }
        public int PageSize { get; set; }
        public int CacheSize { get; set; }
        public bool FailIfMissing { get; set; }
        public bool ReadOnly { get; set; }
        public bool InMemory { get; set; }

        //string _ConnectionString;
        public string ConnectionString { get; set; }

        /// <summary>
        /// Get The config full file path.
        /// </summary>
        public string ConfigFilename { get { return Path.Combine(DbPath, ".xconfig"); } }
        /// <summary>
        /// Get or Set The db full file path.
        /// </summary>
        public string DbFilename { get { return Path.Combine(DbPath, Name + ".db"); } }


        /// <summary>
        /// Get or Set DBProvider.
        /// Default is 'xPersistent';
        /// </summary>
        public DBProvider DbProvider { get; set; }

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

    }
}
