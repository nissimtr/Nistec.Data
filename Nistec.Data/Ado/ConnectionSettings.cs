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
using System.Text;
using Nistec.Data;
using Nistec.Generic;
using System.Data;
using System.Collections.Concurrent;
using System.Xml;
using System.Linq;
using Nistec.Config;
using System.IO;
#pragma warning disable CS1591
namespace Nistec.Data.Ado
{

    public class ConnectionSettings
    {
        public const string fileName = "ConnectionSettings.config";
        public const string RootConfig = "connectionStrings";

        public static readonly ConnectionSettings Instance = new ConnectionSettings(true);

        private ConcurrentDictionary<string, ConnectionProvider> Connections = new ConcurrentDictionary<string, ConnectionProvider>();

        public void OnLog(string log)
        {

        }

        GenericConfig<ConnectionProvider> _ConnectionConfig;
        public ConnectionSettings(bool loadConfig)
        {
            _Started = false;

            if (loadConfig)
            {
                Load();
            }
        }

        bool _Started;
        public void Start(string path, bool enableSyncFileWatcher = true)
        {
            if (_Started)
                return;
            string filePath = Path.Combine(path,fileName);

            if (_ConnectionConfig==null)
            {
                _ConnectionConfig = new GenericConfig<ConnectionProvider>(filePath, RootConfig, OnLog);
                _ConnectionConfig.LoadCompleted += _ConnectionConfig_LoadCompleted;
                _ConnectionConfig.SyncError += _ConnectionConfig_SyncError;
                _ConnectionConfig.Start(enableSyncFileWatcher);
            }
            _Started = true;
        }
        public void Stop(string path)
        {
            if (_ConnectionConfig != null)
            {
                _ConnectionConfig.Stop();
                _ConnectionConfig.LoadCompleted -= _ConnectionConfig_LoadCompleted;
                _ConnectionConfig.SyncError -= _ConnectionConfig_SyncError;
                _ConnectionConfig = null;
            }
            _Started = false;
        }

        private void _ConnectionConfig_LoadCompleted(object sender, GenericEventArgs<ConnectionProvider[]> e)
        {
            LoadConfigItems(e.Args);
        }
        private void _ConnectionConfig_SyncError(object sender, GenericEventArgs<string> e)
        {
            Console.WriteLine("Sync error :" + e.Args);
        }

        public static ConnectionProvider[] GetItems(XmlNodeList list)
        {
            List<ConnectionProvider> items = new List<ConnectionProvider>();

            foreach (XmlNode n in list)
            {
                if (n.NodeType == XmlNodeType.Comment)
                    continue;
                ConnectionProvider cp = new ConnectionProvider(new XmlTable(n));
                if (items.Exists(s => s.FriendlyName == cp.FriendlyName && s.Database == cp.Database))
                {
                    //CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Debug, "Duplicate in SyncFile, entity: " + sync.EntityName);
                    continue;
                }
                items.Add(cp);
            }
            return items.ToArray();
        }

        public void LoadConfigItems(ConnectionProvider[] connections)
        {

            foreach (ConnectionProvider cp in connections)
            {
                if (Connections.ContainsKey(cp.FriendlyName))
                {
                    //CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Debug, "Duplicate in SyncFile, entity: " + sync.EntityName);
                    continue;
                }
                if (Connections.TryAdd(cp.FriendlyName, cp))
                {
                    Console.WriteLine("ConnectionProvider added , " + cp.FriendlyName);
                }
            }
            //return items.ToArray();
        }

        public void LoadConfigItems(XmlNodeList list)
        {

            foreach (XmlNode n in list)
            {
                if (n.NodeType == XmlNodeType.Comment)
                    continue;
                ConnectionProvider cp = new ConnectionProvider(new XmlTable(n));
                if (Connections.ContainsKey(cp.FriendlyName))
                {
                    //CacheLogger.Logger.LogAction(CacheAction.SyncCache, CacheActionState.Debug, "Duplicate in SyncFile, entity: " + sync.EntityName);
                    continue;
                }
                if(Connections.TryAdd(cp.FriendlyName, cp))
                {
                    Console.WriteLine("ConnectionProvider added , " + cp.FriendlyName);
                }
            }
            //return items.ToArray();
        }

        public void Load()
        {
            if (Initilaized)
                return;
            int count = NetConfig.ConnectionStrings.Count;
            for (int i = 0; i < count; i++)
            {
                var connection = NetConfig.ConnectionStrings[i];
                var provider = ConnectionProvider.ParseProvider(connection.ProviderName);
                Connections[connection.Name] = new ConnectionProvider(provider, connection.ConnectionString, connection.Name);
            }
            if(Connections.Count>0)
            {
                Initilaized = true;
            }
        }
        public bool Initilaized
        {
            get;private set;
        }
        public int Count
        {
            get { return Connections.Count; }
        }
        public bool Contains(string name)
        {
            return Connections.ContainsKey(name);
        }
        public bool TryGet(string name, out ConnectionProvider cp)
        {
            return Connections.TryGetValue(name, out cp);
        }
        public ConnectionProvider Get(string name)
        {
            ConnectionProvider cp;
            if (TryGet(name, out cp))
            {
                return cp;
            }
            return null;
        }
        public string GetConnectionString(string name)
        {
            ConnectionProvider cp;
            if (TryGet(name, out cp))
            {
                return cp.ConnectionString;
            }
            return null;
        }
        public bool Add(string name, ConnectionProvider cp)
        {
            return Connections.TryAdd(name, cp);
        }
        public void Set(string name, ConnectionProvider cp)
        {
            Connections[name] = cp;
        }
        public bool Remove(string name)
        {
            ConnectionProvider cp;
            return Connections.TryRemove(name, out cp);
        }

        public string this[string name]
        {
            get
            {
                ConnectionProvider cp;
                if (TryGet(name, out cp))
                {
                    return cp.ConnectionString;
                }
                return null;
            }
        }
    }

}
