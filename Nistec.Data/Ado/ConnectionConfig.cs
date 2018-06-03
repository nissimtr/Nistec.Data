using Nistec.Caching.Config;
using Nistec.Caching.Data;
using Nistec.Caching.Sync.Remote;
using Nistec.Config;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Nistec.Data.Ado
{

    public class ConnectionConfig : GenericConfig<ConnectionProvider>
    {
        public const string fileName = "ConnectionSettings.config";
        public const string Root = "connectionStrings";

        public ConnectionConfig(string path, Action<string> onLog) : base(path, Root, onLog)
        {

        }
    }

#if (false)
    public class ConnectionConfig
    {
        public const string fileName = "ConnectionSettings.config";

        int synchronized;

        bool _reloadOnChange;
        bool _enableSyncFileWatcher;
        bool _initialized;


        SysFileWatcher _SyncFileWatcher;
        string filePath;

        Action<string> OnLog;
        public bool EnableAsyncTask { get; set; }
        public bool ReloadOnChange { get; set; }
        public bool EnableSyncFileWatcher { get; set; }

        protected void WriteLog(string log)
        {
            if (OnLog != null)
                OnLog(log);
        }

        public ConnectionConfig(string path, Action<string> onLog)
        {
            filePath = Path.Combine(path,fileName);
            OnLog = onLog;
        }

        /// <summary>
        /// Start Cache Synchronization.
        /// </summary>
        public void Start(bool enableSyncFileWatcher, bool reloadOnChange = true)
        {
            if (_initialized)
            {
                return;
            }
            _enableSyncFileWatcher = enableSyncFileWatcher;
            _reloadOnChange = reloadOnChange;
            if (enableSyncFileWatcher)
            {
                _SyncFileWatcher = new SysFileWatcher(filePath, true);
                _SyncFileWatcher.FileChanged += new FileSystemEventHandler(_SyncFileWatcher_FileChanged);

                OnSyncFileChange(new FileSystemEventArgs(WatcherChangeTypes.Created, _SyncFileWatcher.SyncPath, _SyncFileWatcher.Filename));
            }

            _initialized = true;
            WriteLog("ConnectionConfig Started!");
        }

        /// <summary>
        /// Stop Cache Synchronization.
        /// </summary>
        public void Stop()
        {
            _initialized = false;
            WriteLog( "ConnectionConfig Stoped!");
        }


        void _SyncFileWatcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            Task task = Task.Factory.StartNew(() => OnSyncFileChange(e));
        }

        void OnSyncFileChange(object args)//GenericEventArgs<string> e)
        {
            FileSystemEventArgs e = (FileSystemEventArgs)args;

            LoadConfigFile(e.FullPath, 3);
        }


        #region events

        /// <summary>
        /// Sync Reload Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncReload;

        /// <summary>
        /// Occured On Sync Reload.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSyncReload(GenericEventArgs<string> e)
        {
            if (SyncReload != null)
                SyncReload(this, e);

            WriteLog("OnSyncReload :" + e.Args);

        }

        /// <summary>
        /// Sync Error Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncError;
        /// <summary>
        /// Sync LoadCompleted Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<ConnectionProvider[]>> LoadCompleted;

        /// <summary>
        /// On Error Occured
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnError(string e)
        {
            if (SyncError != null)
                SyncError(this, new GenericEventArgs<string>(e));
        }

        protected virtual void OnLoadCompleted(ConnectionProvider[] e)
        {
            if (LoadCompleted != null)
                LoadCompleted(this, new GenericEventArgs<ConnectionProvider[]>(e));
        }

        #endregion

        #region Load xml config
        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        public void LoadConfig()
        {

            string file = filePath;
            LoadConfigFile(file);
        }

        //public void LoadConfigFile(string file, int retrys)
        //{

        //    int counter = 0;
        //    bool reloaded = false;
        //    while (!reloaded && counter < retrys)
        //    {
        //        reloaded = LoadConfigFile(file);
        //        counter++;
        //        if (!reloaded)
        //        {
        //            WriteLog("LoadConfigFile retry: " + counter);
        //            Thread.Sleep(100);
        //        }
        //    }
        //    if (reloaded)
        //    {
        //        OnSyncReload(new GenericEventArgs<string>(file));
        //    }
        //}

        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        /// <param name="file"></param>
        public bool LoadConfigFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;
            Thread.Sleep(1000);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);

                LoadConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog("LoadLoadConfigFile error: " + ex.Message);
                OnError("LoadSyncConfigFile error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from xml string argument.
        /// </summary>
        /// <param name="xml"></param>
        public bool LoadConfig(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return true;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                LoadConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog("LoadConfig error: " + ex.Message);
                OnError("LoadConfig error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from <see cref="XmlDocument"/> document.
        /// </summary>
        /// <param name="doc"></param>
        public void LoadConfig(XmlDocument doc)
        {
            if (doc == null)
                return;

            XmlNode items = doc.SelectSingleNode("//connectionStrings");
            if (items == null)
                return;
            LoadConnectionItems(items);//, CacheSettings.EnableAsyncTask,true);
        }

        //internal abstract void LoadSyncTables(XmlNode node);//, bool EnableAsyncTask, bool enableLoader);

        internal virtual void LoadConnectionItems(XmlNode node)
        {
            if (node == null)
                return;
            try
            {
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    XmlNodeList list = node.ChildNodes;
                    if (list == null)
                    {
                        WriteLog("Load connections is empty");
                        return;
                    }

                    var newItems = ConnectionSettings.GetItems(list);

                    if (newItems == null || newItems.Length == 0)
                    {
                        throw new Exception("Can not Load connections, connections Items not found");
                    }

                    OnLoadCompleted(newItems);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Load connections error: {0}", ex.Message));

                OnError("Load connections error " + ex.Message);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }
        #endregion load xml config

    }

#endif
    }
