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
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using Nistec.Data;
using System.Data.OleDb;
using Nistec.Data.Entities;
using Nistec.Runtime;

namespace Nistec.Data.Factory
{

    #region generic AutoBase
    #region summary

    /// <summary>
    /// Represent generic base class of data access that implements <see cref="IAutoBase"/>.
    /// </summary>
    /// <example>Implemention Sample of Data
    /// <code> 
    /// 
    ///[DbContext("AdventureWorks")]
    ///public class AdventureWorks : DbContext
    ///{
    ///    public static string Cnn
    ///    {
    ///        get { return Nistec.Configuration.NetConfig.ConnectionString("AdventureWorks"); }
    ///    }
    ///
    ///    protected override void EntityBind()
    ///    {
    ///    }
    ///
    ///    public override IEntityLang LangManager
    ///    {
    ///        get
    ///        {
    ///            return base.GetLangManager&lt;AdventureWorksResources&gt;();
    ///        }
    ///    }
    ///}
    ///
    /// public sealed class AdventureWorksBase : Nistec.Data.Factory.AutoBase&lt;AdventureWorks&gt;
    ///{
    ///    public static readonly AdventureWorksBase DB = new AdventureWorksBase();
    ///    
    ///    public IAdventureWorksDB IAdventureWorks { get { return DB.CreateInstance&lt;IAdventureWorksDB&gt;(); } }
    ///}
    ///
    ///public interface IAdventureWorksDB : Nistec.Data.IAutoDb
    ///{
    ///    [DBCommand(DBCommandType.Text, "SELECT * FROM Person.Contact", null, MissingSchemaAction.AddWithKey)]
    ///    DataTable Contacts();
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailAddress FROM Person.Contact where ContactID=@ContactID", null)]
    ///    string Contact_Email(int ContactID);
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailPromotion FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    int Contact_EmailPromotion(int ContactID);
    ///
    ///    [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    DataRow Contact(int ContactID);
    ///
    ///    [DBCommand(DBCommandType.Update, "Person.Contact")]
    ///    int Contact_Update
    ///        (
    ///        [DalParam(DalParamType.Key)] int ContactID,
    ///        [DalParam()]DateTime ModifiedDate,
    ///        [DalParam(24)]string Phone
    ///        );
    ///}
    ///
    ///public class DalDemo
    ///{
    ///
    ///    public void PrintContacts()
    ///    {
    ///        DataTable dt = AdventureWorksBase.DB.IAdventureWorks.Contacts();
    ///        dt.WriteXml(Console.Out);
    ///    }
    ///
    ///    public void LookupContact(int contactId)
    ///    {
    ///        string mail = AdventureWorksBase.DB.IAdventureWorks.Contact_Email(contactId);
    ///        Console.WriteLine(mail);
    ///    }
    ///}
    ///</code>
    ///</example> 

    #endregion
    public abstract class AutoBase<T> : AutoBase where T: IDbContext
    {

        /// <summary>
        /// Initializes the object. 
        /// </summary>
        public AutoBase()
        {
            base.Init<T>(true, true);

        }
        /// <summary>
        /// Initializes the object with specified parameters. 
        /// </summary>
        /// <param name="autoCloseConnection"></param>
        /// <param name="ownsConnection"></param>
        public AutoBase(bool autoCloseConnection, bool ownsConnection)
        {
            base.Init<T>(autoCloseConnection, ownsConnection); 
      
        }
    }
    #endregion

    #region summary
    /// <summary>
    /// Represent base class of data access that implements <see cref="IAutoBase"/>.
	/// </summary>
    ///<remarks>
    /// <example>implements Sample of Data
    /// <code> 
    /// <![CDATA[]]>
    ///[DbContext("AdventureWorks")]
    ///public class AdventureWorks : DbContext
    ///{
    ///    public static string Cnn
    ///    {
    ///        get { return Nistec.Configuration.NetConfig.ConnectionString("AdventureWorks"); }
    ///    }
    ///
    ///    protected override void EntityBind()
    ///    {
    ///    }
    ///
    ///    public override IEntityLang LangManager
    ///    {
    ///        get
    ///        {
    ///           return base.GetLangManager<![CDATA[<AdventureWorksResources>]]> ();
    ///        }
    ///    }
    ///}
    ///
    /// public sealed class AdventureWorksBase : Nistec.Data.Factory.AutoBase
    ///{
    ///    public static readonly AdventureWorksBase DB = new AdventureWorksBase();
    ///    
    ///     static AdventureWorksBase()
    ///    {
    ///           DB.Init<![CDATA[<AdventureWorks>]]> ( true, true);
    ///    }
    ///
    ///    public IAdventureWorksDB IAdventureWorks { get { return DB.CreateInstance<![CDATA[<IAdventureWorksDB>]]>(); } }
    ///}
    ///
    ///public interface IAdventureWorksDB : Nistec.Data.IAutoDb
    ///{
    ///    [DBCommand(DBCommandType.Text, "SELECT * FROM Person.Contact", null, MissingSchemaAction.AddWithKey)]
    ///    DataTable Contacts();
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailAddress FROM Person.Contact where ContactID=@ContactID", null)]
    ///    string Contact_Email(int ContactID);
    ///
    ///    [DBCommand(DBCommandType.Lookup, "SELECT EmailPromotion FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    int Contact_EmailPromotion(int ContactID);
    ///
    ///    [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
    ///    DataRow Contact(int ContactID);
    ///
    ///    [DBCommand(DBCommandType.Update, "Person.Contact")]
    ///    int Contact_Update
    ///        (
    ///        [DalParam(DalParamType.Key)] int ContactID,
    ///        [DalParam()]DateTime ModifiedDate,
    ///        [DalParam(24)]string Phone
    ///        );
    ///}
    ///
    ///public class DalDemo
    ///{
    ///
    ///    public void PrintContacts()
    ///    {
    ///        DataTable dt = AdventureWorksBase.DB.IAdventureWorks.Contacts();
    ///        dt.WriteXml(Console.Out);
    ///    }
    ///
    ///    public void LookupContact(int contactId)
    ///    {
    ///        string mail = AdventureWorksBase.DB.IAdventureWorks.Contact_Email(contactId);
    ///        Console.WriteLine(mail);
    ///    }
    ///}
    ///</code>
    ///</example> 
    ///</remarks>
   
 	#endregion
	public abstract class AutoBase : IAutoBase// IDisposable
	{

		#region IDisposable implementation
		
		/// <summary>
		/// Disposed flag.
		/// </summary>
		protected bool m_disposed = false;
        /// <summary>
        /// m_initilaized
        /// </summary>
        private bool m_initilaized = false;
        /// <summary>
        /// Permit
        /// </summary>
        internal bool m_Permit = false;
        /// <summary>
        /// dbProvider
        /// </summary>
        private DBProvider m_dbProvider;

		/// <summary>
		/// Implementation of method of IDisposable interface.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose method with a boolean parameter indicating the source of calling.
		/// </summary>
		/// <param name="calledbyuser">Indicates from whare the method is called.</param>
		protected void Dispose(bool calledbyuser)
		{
			if(!m_disposed)
			{
				if(calledbyuser)
				{
					InnerDispose();
				}
				m_disposed = true;
                m_initilaized = false;
            }
		}

		/// <summary>
		/// Inner implementation of Dispose method.
		/// </summary>
        internal void InnerDispose()
		{
			if(m_connection != null)
			{
				if((m_connection.State != ConnectionState.Closed) && m_ownsConnection) 
				{
					try
					{
						m_connection.Close();
                        m_connection.Dispose();
					}
					catch{}
				}
                m_connection = null;
            }
            if (m_transaction != null)
            {
                m_transaction.Dispose();
                m_transaction = null;
            }
            m_initilaized = false;
			UpdateAllBaseObjects();
		}

		/// <summary>
		/// Class destructor.
		/// </summary>
		~AutoBase()
		{
			Dispose(false);
		}
		#endregion

		#region private members
		/// <summary>
		/// Connection object.
		/// </summary>
		internal IDbConnection m_connection = null;

		/// <summary>
		/// Transaction object.
		/// </summary>
        internal IDbTransaction m_transaction = null;

		/// <summary>
		/// Indicates that <see cref="AutoBase"/> object owns the connection.
		/// </summary>
        internal bool m_ownsConnection = false;

		/// <summary>
		/// Indicates that the connection must be closed each time after a command execution.
		/// </summary>
        internal bool m_autoCloseConnection = false;

		/// <summary>
		/// Contains objects generated in <see cref="GenerateAllObjects"/> method.
		/// </summary>
        internal Hashtable m_ObjectTypes = new Hashtable();

		#endregion

		#region Constructors

		/// <summary>
		/// A constructor with no parameters.
		/// </summary>
		public AutoBase()
		{
		}
  
        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <param name="provider">db provider</param>
        /// <param name="connectionString">Sql connection string parameter.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        protected void Init(DBProvider provider, string connectionString, bool autoCloseConnection)
        {
            m_dbProvider = provider;
            InitInternal(connectionString, autoCloseConnection, true);
        }
        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <param name="provider">db provider</param>
        /// <param name="connectionString">Sql connection string parameter.</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="ownsConnection"></param>
        protected void Init(DBProvider provider,string connectionString, bool autoCloseConnection, bool ownsConnection)
        {
            m_dbProvider = provider;
            InitInternal(connectionString, autoCloseConnection, ownsConnection);
        }
        /// <summary>
        ///  Initializes the object. 
        /// </summary>
        /// <param name="connectionKey">connectionKey from config file</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="ownsConnection"></param>
        protected void Init(string connectionKey, bool autoCloseConnection, bool ownsConnection)
        {
            DbContext context = new DbContext(connectionKey);
            m_dbProvider = context.Provider;
            InitInternal(context.ConnectionString, autoCloseConnection, ownsConnection);
        }
        /// <summary>
        /// Initializes the object. 
        /// </summary>
        /// <param name="connection">ConnectionContext</param>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="ownsConnection"></param>
        protected void Init(DbContext connection, bool autoCloseConnection, bool ownsConnection)
        {
            m_dbProvider = connection.Provider;
            InitInternal(connection.ConnectionString, autoCloseConnection, ownsConnection);
        }

  
        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="autoCloseConnection">Determines if the connection must be closed after the command execution.</param>
        /// <param name="ownsConnection"></param>
        protected void Init<T>(bool autoCloseConnection, bool ownsConnection) where T : IDbContext
        {
            IDbContext connection = ActivatorUtil.CreateInstance<T>();
            m_dbProvider = connection.Provider;
            //m_Permit = Nistec.Net.DalNet.NetFram(path, "SERVER");
            InitInternal(connection.ConnectionString, autoCloseConnection, ownsConnection);
        }

        private void InitInternal(string connection, bool autoCloseConnection, bool ownsConnection)
        {
            InnerDispose();
            if (m_dbProvider == DBProvider.OleDb)
                m_connection = new OleDbConnection(connection);
            else
                m_connection = new SqlConnection(connection);

            m_autoCloseConnection = autoCloseConnection;
            m_ownsConnection = ownsConnection;

            GenerateAllObjects();
            UpdateAllBaseObjects();
            m_initilaized = true;
            m_Permit = true;
        }


		/// <summary>
		/// Generates all objects mention in public properties 
		/// and derived from <see cref="IAutoDb"/> interface.
		/// </summary>
		private void GenerateAllObjects()
		{
			MethodInfo[] mis = this.GetType().GetMethods();
			for(int i = 0; i < mis.Length; ++i)
			{
				Type type = mis[i].ReturnType;
				if(type.GetInterface(typeof(IAutoDb).FullName) == typeof(IAutoDb))
				{
					if(mis[i].Name.StartsWith("get_"))
					{
						if(!m_ObjectTypes.ContainsKey(mis[i].Name))
						{
							IAutoDb sw = AutoFactory.CreateDB(type);
							m_ObjectTypes[mis[i].Name] = sw;
						}
					}
				}
			}

		}

		#endregion

		#region Public members
        /// <summary>
        /// DalPermit
        /// </summary>
        public bool Permit { get { return m_Permit; } }
        /// <summary>
        /// DBProvider
        /// </summary>
        public DBProvider DBProvider { get { return m_dbProvider; } }
        /// <summary>
        /// Get Initilaized
        /// </summary>
        public bool Initilaized { get { return m_initilaized; } }
        /// <summary>
		/// It true then the object owns its connection
		/// and disposes it on its own disposal.
		/// </summary>
        public bool OwnsConnection { get { return m_ownsConnection; } }

		/// <summary>
		/// If true then the object's connection is closed each time 
		/// after sql command execution.
		/// </summary>
        public bool AutoCloseConnection { get { return m_autoCloseConnection; } }

		/// <summary>
		/// Sql connection property.
		/// </summary>
        public IDbConnection Connection { get { return m_connection; } }

		/// <summary>
		/// Sql transaction property.
		/// </summary>
        public IDbTransaction Transaction { get { return m_transaction; } }

	    /// <summary>
		/// Begins sql transaction with a default (<see cref="IsolationLevel.ReadCommitted"/>) isolation level.
		/// </summary>
		/// <returns></returns>
        public IDbTransaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}

		/// <summary>
		/// Begins sql transaction with a specified isolation level.
		/// </summary>
		/// <param name="iso"></param>
		/// <returns></returns>
        public IDbTransaction BeginTransaction(IsolationLevel iso)
		{
			if(m_transaction != null)
			{
				throw new ApplicationException("A previous transaction is not closed");
			}
			m_transaction = m_connection.BeginTransaction(iso);
			UpdateAllBaseObjects();
			return m_transaction;
		}

        

		/// <summary>
		/// Rolls back the current transaction.
		/// </summary>
        public void RollbackTransaction()
		{
			if(m_transaction == null)
			{
				throw new ApplicationException("A transaction has not been opened");
			}
			m_transaction.Rollback();
			m_transaction = null;
			UpdateAllBaseObjects();
		}

		/// <summary>
		/// Commits the current transaction.
		/// </summary>
        public void CommitTransaction()
		{
			if(m_transaction == null)
			{
				throw new ApplicationException("A transaction has not been started");
			}
			m_transaction.Commit();
			m_transaction = null;
			UpdateAllBaseObjects();
		}
		#endregion
		
		#region Protected members

		/// <summary>
		/// Update values of a dal object.
		/// </summary>
		/// <param name="dal">A dal object.</param>
        internal virtual void UpdateBase(IAutoDb dal)
		{
			dal.Connection = m_connection;
			dal.Transaction = m_transaction;
			dal.AutoCloseConnection = m_autoCloseConnection;
        }

		/// <summary>
		/// Updates all generated objects properties.
		/// </summary>
        internal virtual void UpdateAllBaseObjects()
		{
			foreach(object o in m_ObjectTypes.Values)
			{
				UpdateBase((IAutoDb)o);
			}
		}

		/// <summary>
		/// Returns a generated object.
		/// </summary>
		/// <returns></returns>
		protected IAutoDb CreateInstance()
		{
			MethodInfo mi = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
			IAutoDb res = (IAutoDb)m_ObjectTypes[mi.Name];
			if(res == null)
			{
				throw new DalException("Data DalDB object not initialized.");
			}
			return res;
		}

        /// <summary>
        /// Returns a generated object.
        /// </summary>
        /// <returns></returns>
        protected T CreateInstance<T>() where T : IAutoDb
        {
            MethodInfo mi = (MethodInfo)(new StackTrace().GetFrame(1).GetMethod());
            T res = (T)m_ObjectTypes[mi.Name];
            if (res == null)
            {
                throw new DalException("Data DalDB object not initialized.");
            }
            return res;
        }
		#endregion

	}

}
