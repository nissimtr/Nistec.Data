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
using System.Linq;
using Nistec.Config;

namespace Nistec.Data.Ado
{

    /// <summary>
    /// DataSource 
    /// </summary>
    [Serializable]
    public class ConnectionProvider : IDisposable, IConfigurable
    {
        public const DBProvider DefaultProvider = DBProvider.SqlServer;

        #region ctor

        public ConnectionProvider()
        {

            //_ConnectionString = "";
            //_Server = "";
            //_Database = "";
            //_FriendlyName = "";

            //_IntegratedSecurity = "";//SSPI
            //_PersistSecurityInfo = "";
            //_UserId = "";
            //_Password = "";
            //WorkstationID = "";

            TimeOut = 30;
            IsConnected = false;
            AsynchronousProcessing = false;
            IsEncrypted = false;
            PacketSize = 8192;
            ID = -1;
        }

        public ConnectionProvider(
            DBProvider provider,
            string connectionString,
            string friendlyName)
        {
            Provider = provider;
            FriendlyName = friendlyName;
            _ConnectionString = connectionString;

            TimeOut = 30;
            IsConnected = false;
            AsynchronousProcessing = false;
            IsEncrypted = false;
            PacketSize = 8192;
            ID = -1;

            ParseConnectionString(false);
        }

        public ConnectionProvider(
        DBProvider provider,
        string friendlyName,
        string server,
        string database,
        string userId,
        string password,
        int timeOut)
        {
            Provider = provider;
            FriendlyName = friendlyName;
            Server = server;
            Database = database;
            IntegratedSecurity = null;
            PersistSecurityInfo = null;
            UserId = userId;
            Password = password;

            TimeOut = timeOut;
            IsConnected = false;
            AsynchronousProcessing = false;
            IsEncrypted = false;
            PacketSize = 8192;
            ID = -1;

            CreateConnectionString();
        }

        public ConnectionProvider(
         DBProvider provider,
         string friendlyName,
         string server,
         string database,
         bool integratedSecurity,
         bool persistSecurityInfo,
         int timeOut)
        {
            Provider = provider;
            FriendlyName = friendlyName;
            Server = server;
            Database = database;
            IntegratedSecurity = integratedSecurity?"sspi":"false";
            PersistSecurityInfo = persistSecurityInfo?"true":"false";
            UserId = null;
            Password = null;

            TimeOut = timeOut;
            IsConnected = false;
            AsynchronousProcessing = false;
            IsEncrypted = false;
            PacketSize = 8192;
            ID = -1;

            CreateConnectionString();
        }

        public ConnectionProvider(
         DBProvider provider,
         string friendlyName,
         string server,
         string database,
         string integratedSecurity,
         string persistSecurityInfo,
         string userId,
         string password,
         int timeOut,
         bool isEncrypted,
         bool asynchronousProcessing)
        {
            Provider = provider;
            FriendlyName = friendlyName;
            Server = server;
            Database = database;
            IntegratedSecurity = integratedSecurity;
            PersistSecurityInfo = persistSecurityInfo;
            UserId = userId;
            Password = password;

            TimeOut = timeOut;
            IsConnected = false;
            AsynchronousProcessing = false;
            IsEncrypted = isEncrypted;
            PacketSize = 8192;
            ID = -1;


            TimeOut = timeOut;
            IsEncrypted = isEncrypted;
            AsynchronousProcessing = asynchronousProcessing;
            CreateConnectionString();
        }

        public ConnectionProvider(XmlTable xml)
        {
            TimeOut = 30;
            IsConnected = false;
            AsynchronousProcessing = false;
            IsEncrypted = false;
            PacketSize = 8192;
            ID = -1;

            LoadConfig(xml);


            //Server = xml.Get<string>("Server");
            //Database = xml.Get<string>("Database");
            //IntegratedSecurity = xml.Get<string>("IntegratedSecurity");
            //PersistSecurityInfo = xml.Get<string>("PersistSecurityInfo");
            //UserId = xml.Get<string>("UserId");
            //Password = xml.Get<string>("Password");
            //TimeOut = xml.Get<int>("TimeOut");
            //IsEncrypted = xml.Get<bool>("IsEncrypted",false);
            //PacketSize = xml.Get<int>("PacketSize",8192);
            //ID = xml.Get<int>("ID",-1);
        }

        public void Dispose()
        {
            _ConnectionString = null;
            Database = null;
            FriendlyName = null;
            IntegratedSecurity = null;
            PersistSecurityInfo = null;
            UserId = null;
            Password = null;
            WorkstationID = null;
        }



        #endregion

        #region IConfigurable

        public void LoadConfig(XmlTable xml)
        {
            //< add name = "AdventureWorks" connectionString = "Data Source=NISTEC-PC;Initial Catalog=AdventureWorks;Integrated Security=True;" providerName = "System.Data.SqlClient" />

            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            Provider = ParseProvider(xml.Get<string>("providerName"));
            FriendlyName = xml.Get<string>("name");
            _ConnectionString = xml.Get<string>("connectionString");

            ParseConnectionString(false);
        }

        public bool IsEqual(object item)
        {
            if (item == null)
                return false;

            ConnectionProvider cp = (ConnectionProvider)item;
            if (cp == null)
                return false;
            return cp.FriendlyName == FriendlyName && cp.Database == Database && cp.Server == Server;
        }

        #endregion

        #region members


        //private DBProvider _Provider;

        private string _ConnectionString = "";
        //private string _Server = "";
        //private string _Database = "";
        //private string _FriendlyName = "";



        //private string _IntegratedSecurity = "";//SSPI
        //private string _PersistSecurityInfo = "";
        //private string _UserId = "";
        //private string _Password = "";
        //private int _TimeOut = 30;
        //private bool _IsConnected;
        //private bool _AsynchronousProcessing = false;
        //private bool _IsEncrypted = false;
        //private int _PacketSize = 8192;
        //private string _WorkstationID = "";
        //private int _DBID = -1;

        #endregion

        #region properties
        public int ID { get; private set; }

        public DBProvider Provider { get; private set; }

        public string FriendlyName { get; private set; }//{ get { return _FriendlyName; } set { if (value == null)return; _FriendlyName = value; } }
        public string Server { get; private set; }//{ get { return _Server; } set { if (value == null)return; _Server = value; } }
        public string Database { get; private set; }//{ get { return _Database; } set { if (value == null)return; _Database = value; } }


        public string IntegratedSecurity { get; private set; }//{ get { return _IntegratedSecurity; } set { if (value == null)return; _IntegratedSecurity = value; } }
        public string PersistSecurityInfo { get; private set; }//{ get { return _PersistSecurityInfo; } set { if (value == null)return; _PersistSecurityInfo = value; } }
        public string InitialCatalog { get; private set; }//{ get { return _Database; } set { if (value == null)return; _Database = value; } }
        public string UserId { get; private set; }//{ get { return _UserId; } set { if (value == null)return; _UserId = value; } }
        public string Password { get; private set; }//{ get { return _Password; } set { if (value == null)return; _Password = value; } }
        public int TimeOut { get; private set; }
        public bool IsConnected { get; set; }
        public bool AsynchronousProcessing { get; private set; }
        public bool IsEncrypted { get; private set; }
        public string WorkstationID { get; private set; }//{ get { return _WorkstationID; } set { if (value == null)return; _WorkstationID = value; } }
        public string ConnectionString { get { return _ConnectionString; } }//{ get { return _ConnectionString; } set { if (value == null)return; _ConnectionString = value; } }
        public int PacketSize { get; private set; }

        /// <summary>
        /// DataSource empty
        /// </summary>
        public ConnectionProvider Empty { get { return new ConnectionProvider(); } }
        /// <summary>
        /// IsEmpty
        /// </summary>
        public bool IsEmpty { get { return ConnectionString == null || ConnectionString == "" /*|| ConnectionID==""*/; } }

        private object GetValue(string properyName)
        {
            switch (properyName.ToLower())
            {
                case "friendlyname"://"connectionid":
                    return FriendlyName;
                case "data source":
                case "datasource":
                case "server"://"servername":
                    return Server;
                case "initial catalog":
                case "database"://"dbname":
                    return Database;
                case "provider":
                    return Provider;
                //case "dbpath":
                //    return DBPath;
                case "integratedsecurity":
                    return IntegratedSecurity;
                case "persistsecurityinfo":
                    return PersistSecurityInfo;
                case "initialcatalog":
                    return InitialCatalog;
                case "user id":
                case "uid":
                case "user":
                case "userid":
                    return UserId;
                case "pwd":
                case "jet oledb:database password":
                case "password":
                    return Password;
                case "connection timeout":
                case "timeout":
                    return TimeOut;
                case "isconnected":
                    return IsConnected;
                case "asynchronousprocessing":
                    return AsynchronousProcessing;
                case "isencrypted":
                    return IsEncrypted;
                case "workstationid":
                    return WorkstationID;
                case "connectionstring":
                    return ConnectionString;
                case "packetsize":
                    return PacketSize;
            }
            return null;
        }

        private void SetValue(string properyName, object value)
        {
            switch (properyName.ToLower())
            {
                case "friendlyname"://"connectionid":
                    FriendlyName = value.ToString();
                    break;
                case "data source":
                case "datasource":
                case "server"://"servername":
                    Server = value.ToString();
                    break;
                case "initial catalog":
                case "database"://"dbname":
                    Database = value.ToString();
                    break;
                case "provider":
                    Provider = (DBProvider)Enum.Parse(typeof(DBProvider), value.ToString(), true);
                    break;
                case "integratedsecurity":
                    IntegratedSecurity = value.ToString();
                    break;
                case "persistsecurityinfo":
                    PersistSecurityInfo = value.ToString();
                    break;
                case "initialcatalog":
                    InitialCatalog = value.ToString();
                    break;
                case "user id":
                case "uid":
                case "user":
                case "userid":
                    UserId = value.ToString();
                    break;
                case "pwd":
                case "jet oledb:database password":
                case "password":
                    Password = value.ToString();
                    break;
                case "connection timeout":
                case "timeout":
                    TimeOut = Types.ToInt(value, 30);
                    break;
                case "isconnected":
                    IsConnected = Types.ToBool(value, false);
                    break;
                case "asynchronousprocessing":
                    AsynchronousProcessing = (bool)value;
                    break;
                case "isencrypted":
                    IsEncrypted = Types.ToBool(value, false);
                    break;
                case "workstationid":
                    WorkstationID = value.ToString();
                    break;
                case "connectionstring":
                    _ConnectionString = value.ToString();
                    break;
                case "packetsize":
                    PacketSize = Types.ToInt(value, 8192);
                    break;
            }
        }

        /// <summary>
        /// Get or Set Item
        /// </summary>
        /// <param name="properyName"></param>
        /// <returns></returns>
        public object this[string properyName]
        {
            get
            {
                return GetValue(properyName);
            }

            set
            {
                SetValue(properyName, value);
            }
        }

        #endregion

        #region methods

        public static DBProvider ParseProvider(string dbProvider)
        {
            if(dbProvider==null)
            {
                return DefaultProvider;
                //throw new ArgumentNullException("dbProvider");
            }

            string provider = dbProvider.ToLower();
            string[] args = provider.SplitTrim('.');
            if(args.Length>1)
            {
                provider = args.LastOrDefault();
            }

            switch (provider.ToLower())
            {
                case "system.data.sqlclient":
                case "sqlclient":
                case "sqlserver":
                    return DBProvider.SqlServer;
                case "oledb":
                    return DBProvider.OleDb;
                case "oracle":
                    return DBProvider.Oracle;
                case "mysql":
                    return DBProvider.MySQL;
                case "sybasease":
                    return DBProvider.SybaseASE;
                case "firebird":
                    return DBProvider.Firebird;
                case "db2":
                    return DBProvider.DB2;
                default:
                    return DBProvider.SqlServer;
            }
        }

        /// <summary>
        /// GetAsynchronousProcessing
        /// </summary>
        public string GetAsynchronousProcessing()
        {
            if (_ConnectionString.IndexOf("Asynchronous Processing") == -1)
                return _ConnectionString + ";Asynchronous Processing=true";
            else
                return _ConnectionString;

        }

        /// <summary>
        /// CreateConnectionString
        /// </summary>
        public void CreateConnectionString()
        {
            string cString = "";
            switch (Provider)
            {
                case DBProvider.SqlServer:
                    cString = SqlClientConnectionString;
                    break;
                case DBProvider.OleDb:
                    cString = OleDbConnectionString;
                    break;
                case DBProvider.Oracle:
                    cString = OracleOleDbConnectionString;
                    break;
                case DBProvider.MySQL:
                    cString = MySQLClientConnectionString;
                    break;
                case DBProvider.SybaseASE:
                    cString = SqlClientConnectionString;
                    break;
                case DBProvider.Firebird:
                    cString = FirebirdConnectionString;
                    break;
                case DBProvider.DB2:
                    cString = DB2ConnectionString;
                    break;
                default:
                    cString = SqlClientConnectionString;
                    break;
            }

            _ConnectionString = cString;
        }

        public void ParseConnectionString()
        {
            ParseConnectionString(true);
        }

        public void ParseConnectionString(bool reCreate)
        {

            if (reCreate || string.IsNullOrEmpty(_ConnectionString))
            {
                CreateConnectionString();
            }
            string[] list = _ConnectionString.Split(';');
            foreach (string s in list)
            {
                bool isMatch;
                string[] kv = Nistec.Regx.ParseKeyValue(s, out isMatch);
                if (isMatch)
                {
                    switch (kv[0].ToLower())
                    {
                        case "server":
                        case "data source":
                        case "datasource":
                            Server = kv[1];
                            break;
                        case "database":
                        case "initial catalog":
                            Database = kv[1];
                            break;
                        case "user id":
                        case "uid":
                        case "user":
                            UserId = kv[1];
                            break;
                        case "password":
                        case "pwd":
                        case "jet oledb:database password":
                            Password = kv[1];
                            break;
                        case "persist security info":
                            PersistSecurityInfo = kv[1];
                            break;
                        case "integrated security":
                            IntegratedSecurity = kv[1];
                            break;
                        case "connection timeout":
                        case "timeout":
                            TimeOut = Types.ToInt(kv[1], 30);
                            break;
                        case "asynchronous processing":
                            AsynchronousProcessing = Types.ToBool(kv[1], false);
                            break;

                    }
                }
            }
        }

        private void ParseConnectionString_old()
        {

            _ConnectionString = GetValidConnectionString();
            int index = -1;
            index = _ConnectionString.ToLower().IndexOf("server");
            if (index > -1)
            {
                Server = GetValue(index + "server".Length, ";");
            }
            else //if (index == -1)
            {
                index = _ConnectionString.ToLower().IndexOf("data source");
                if (index > -1)
                {
                    Server = GetValue(index + "data source".Length, ";");
                }
            }
            index = _ConnectionString.ToLower().IndexOf("database");
            if (index > -1)
            {
                Database = GetValue(index + "database".Length, ";");
            }
            else //if (index == -1)
            {
                index = _ConnectionString.ToLower().IndexOf("initial catalog");
                if (index > -1)
                {
                    Database = GetValue(index + "initial catalog".Length, ";");
                }
            }
            index = _ConnectionString.ToLower().IndexOf("integrated security");
            if (index > -1)
            {
                IntegratedSecurity = GetValue(index + "integrated security".Length, ";");
            }
            index = _ConnectionString.ToLower().IndexOf("user id");
            if (index > -1)
            {
                UserId = GetValue(index + "user id".Length, ";");
            }
            else //if (index == -1)
            {
                index = _ConnectionString.ToLower().IndexOf("uid");
                if (index > -1)
                {
                    UserId = GetValue(index + "uid".Length, ";");
                }
            }

            index = _ConnectionString.ToLower().IndexOf("password");
            if (index > -1)
            {
                Password = GetValue(index + "password".Length, ";");
            }
            else //if (index == -1)
            {
                index = _ConnectionString.ToLower().IndexOf("pwd");
                if (index > -1)
                {
                    UserId = GetValue(index + "pwd".Length, ";");
                }
            }
            index = _ConnectionString.ToLower().IndexOf("persist security info");
            if (index > -1)
            {
                PersistSecurityInfo = GetValue(index + "persist security info".Length, ";");
            }
            index = _ConnectionString.ToLower().IndexOf("connection timeout");
            if (index > -1)
            {
                TimeOut = Types.ToInt(GetValue(index + "connection timeout".Length, ";"), 30);
            }
            index = _ConnectionString.ToLower().IndexOf("asynchronous processing");
            if (index > -1)
            {
                AsynchronousProcessing = Types.ToBool(GetValue(index + "asynchronous processing".Length, ";"), false);
            }

        }

        private string GetValue(int pos, string value)
        {
            int start = _ConnectionString.ToLower().IndexOf("=", pos);
            if (start == -1)
            {
                return "";
            }
            start++;
            int last = _ConnectionString.ToLower().IndexOf(value, start);
            if (last == -1)
                last = _ConnectionString.Length;
            string result = _ConnectionString.Substring(start, last - start);
            result = result.Replace("\"", "");
            return result.TrimStart().TrimEnd();

        }

 
        /// <summary>
        /// Get _ConnectionString
        /// </summary>
        public string GetValidConnectionString()
        {
            CreateConnectionString();
            return ConnectionString;
  
        }

        public string OleDbConnectionString
        {
            get
            {

                switch (Provider)
                {
                    case DBProvider.OleDb:
                        if (!string.IsNullOrEmpty(Password))
                        {
                            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Database Password={1}", Database, Password);
                        }
                        else
                        {
                            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", Database);
                        }
                    case DBProvider.SqlServer:
                        if (IntegratedSecurity.Length == 0)
                        {
                            return String.Format("Provider=sqloledb;Data Source={0};Initial Catalog={1};User Id={2};Password={3};",
                                Server,
                                Database,
                                UserId,
                                Password);
                        }
                        else
                        {
                            return String.Format("Provider=sqloledb;Data Source={0};Initial Catalog={1};Integrated Security=SSPI;",
                                Server,
                                Database);
                        }

                    case DBProvider.Oracle:

                        if (IntegratedSecurity.Length == 0)
                        {
                            return String.Format("Provider=MSDAORA;Data Source={0};User Id={1};Password={2};",
                                Server,
                                UserId,
                                Password);
                        }
                        else
                        {
                            return String.Format("Provider=MSDAORA;Data Source={0};Persist Security Info=False;Integrated Security=Yes;",
                                Server);
                        }
                    default:// case DBProvider.OleDb:
                        if (!string.IsNullOrEmpty(Password))
                        {
                            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Database Password={1}", Database, Password);
                        }
                        else
                        {
                            return String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", Database);
                        }
                }

 
            }
        }

        private string SqlClientConnectionString
        {
            get
            {
                string cString = "Data Source=" + Server + "; Initial Catalog=" + Database;

                if (!string.IsNullOrEmpty(this.UserId))
                    cString += "; User ID=" + this.UserId;
                if (!string.IsNullOrEmpty(this.Password))
                    cString += "; Password=" + this.Password;
                if (!string.IsNullOrEmpty(this.PersistSecurityInfo))
                    cString += "; Persist Security Info=" + this.PersistSecurityInfo;
                if (!string.IsNullOrEmpty(this.IntegratedSecurity))
                    cString += "; Integrated Security=" + this.IntegratedSecurity;
                cString += "; Connection Timeout=" + this.TimeOut;
                if (this.TimeOut == 0)
                    cString += "; context connection=false";
                if (this.AsynchronousProcessing)
                    cString += "; Asynchronous Processing=true";
                return cString;
            }
        }
        private string OracleOleDbConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(IntegratedSecurity))
                {
                    return String.Format("Data Source={0};User Id={1};Password={2}",
                        Server,
                        UserId,
                        Password);
                }
                else
                {
                    return String.Format("Data Source={0}",
                        Server);
                }
            }
        }
        private string MySQLClientConnectionString
        {
            get
            {
                string pattern = "Data Source={0};user id={1}; password={2}; database={3};";
                return String.Format(pattern,
                    Server,
                    UserId,
                    Password,
                    Database);
            }
        }
        private string FirebirdConnectionString
        {
            get
            {


                string pattern = "ServerType={0};User={1};Password={2};DataSource={3};Database={4};Dialect=3";
                return String.Format(pattern,
                    "0",
                    UserId,
                    Password,
                    Server,
                    Database);
            }
        }
        private string DB2ConnectionString
        {
            get
            {
                string pattern = "Server={0};Database={3};UID={1};PWD={2}";
                return String.Format(pattern,
                    Server,
                    UserId,
                    Password,
                    Database);
            }
        }
        #endregion
    }

}
