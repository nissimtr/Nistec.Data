using System;
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Linq;
using Nistec.Data;

namespace DataEntityDemo.Sql
{
	using System.Data.SqlClient;
    using Nistec.Data.SqlClient;
    using Nistec.Data.Factory;
    using DataEntityDemo.DB;
    using DataEntityDemo.Entities;
    using System.Collections.Generic;
    using Nistec.Generic;

    public class DBConfig
    {
        public static string ConnectionString
        {
            get { return NetConfig.ConnectionString("AdventureWorks"); }
        }

    }

    public class DalDemo
    {

        public void PrintContacts()
        {
            DataTable dt= AdventureWorksBase.DB.IAdventureWorks.Contacts();
            dt.WriteXml(Console.Out);
        }

        public void LookupContact(int contactId)
        {
            string mail = AdventureWorksBase.DB.IAdventureWorks.Contact_Email(contactId);
            Console.WriteLine(mail);
        }
    }

	#region Sql.DALBase

	/// <summary>
	/// Data class.
	/// </summary>
    public sealed class AdventureWorksBase : Nistec.Data.Factory.AutoBase<AdventureWorks>
    {
        public static AdventureWorksBase _DB;// = new AdventureWorksBase();

        public static AdventureWorksBase DB
        {
            get
            {
                if (_DB == null)
                {
                    _DB = new AdventureWorksBase();
                }
                return _DB;
            }
        }

        public AdventureWorksBase()
        {
            DBProvider pv= this.DBProvider;
            //bool isGac = System.Reflection.Assembly.GetAssembly(typeof(Nistec.Data.Common.DalDB)).GlobalAssemblyCache;
            //DB.Init(DBProvider.SqlServer, DBConfig.ConnectionString, true, true);
            //DB.Init("AdventureWorks", true, true);
            //DB.Init<AdventureWorks>( true, true);
        }

        public IAdventureWorksDB IAdventureWorks { get { return CreateInstance<IAdventureWorksDB>(); } }
        public AdventureWorksDB AdventureWorks { get { return CreateInstance<AdventureWorksDB>(); } }
    }

	#endregion

    #region Sql.IDalDB

    public interface IAdventureWorksDB : Nistec.Data.IAutoDb
    {
        [DBCommand(DBCommandType.Text, "SELECT * FROM Person.Contact", null, MissingSchemaAction.AddWithKey)]
        DataTable Contacts();

        [DBCommand(DBCommandType.Lookup, "SELECT EmailAddress FROM Person.Contact where ContactID=@ContactID", null)]
        string Contact_Email(int ContactID);

        [DBCommand(DBCommandType.Lookup, "SELECT EmailPromotion FROM Person.Contact where ContactID=@ContactID", 0)]
        int Contact_EmailPromotion(int ContactID);

        [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
        DataRow Contact(int ContactID);

        [DBCommand(DBCommandType.Update, "Person.Contact")]
        int Contact_Update
            (
            [DbField(DalParamType.Key)] int ContactID,
            [DbField()]DateTime ModifiedDate,
            [DbField(24)]string Phone
            );

        [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
        ContactItem GetContactEntity(int ContactID);

        [DBCommand("SELECT * FROM Person.Contact where Title=@Title", 0)]
        List<ContactItem> GetContactsByTitle(string Title);
        
    }
    
	/// <summary>
    /// abstract ActiveDB.
	/// </summary>
    public abstract class AdventureWorksDB : Nistec.Data.Factory.AutoDb
	{

        [DBCommand(DBCommandType.Text, "SELECT top 10 * FROM Person.Contact", null, MissingSchemaAction.AddWithKey)]
        public abstract DataTable Contacts();

        [DBCommand(DBCommandType.Lookup, "SELECT EmailAddress FROM Person.Contact where ContactID=@ContactID", null)]
        public abstract string Contact_Email(int ContactID);

        [DBCommand(DBCommandType.Lookup, "SELECT EmailPromotion FROM Person.Contact where ContactID=@ContactID", 0)]
        public abstract int Contact_EmailPromotion(int ContactID);
 
        [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
        public abstract DataRow Contact(int ContactID);
 
        [DBCommand(DBCommandType.Update, "Person.Contact")]
        public abstract int Contact_Update
            (
            [DbField(DalParamType.Key)] int ContactID,
            [DbField()]DateTime ModifiedDate,
            [DbField(24)]string Phone
            );

        [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
        public abstract ContactItem GetContactEntity(int ContactID);

        [DBCommand("SELECT * FROM Person.Contact where Title=@Title", 0)]
        public ContactItem[] GetContactsByTitle(string Title)
        {
            object result=base.Execute(Title);
            return AdapterFactory.ConvertArray<ContactItem>(result);
        }

        public ContactItem GetContact(int contactId)
        {
          return  base.ExecuteCommand<ContactItem>("SELECT * FROM Person.Contact where ContactID=@ContactID", DataParameter.GetSql("ContactID", contactId));
        }

        public ContactItem[] GetContactsListByTitle(string Title)
        {
            return base.ExecuteCommand<ContactItem, ContactItem[]>("SELECT * FROM Person.Contact where Title=@Title", DataParameter.GetSql("Title", Title));
        }
	}
    
	#endregion

    #region Sql.ActiveCommand


    /// <summary>
    /// AdventureWorksCommand class.
    /// </summary>
    public class AdventureWorksCommand : Nistec.Data.SqlClient.DbCommand<AdventureWorks>
    {

        public static AdventureWorksCommand Instance
        {
            get { return new AdventureWorksCommand(); }
        }

        [DBCommand(DBCommandType.Text, "SELECT top 10 * FROM Person.Contact", null, MissingSchemaAction.AddWithKey)]
        public DataTable Contacts()
        {
            return (DataTable)base.Execute();
        }

        [DBCommand(DBCommandType.Lookup, "SELECT EmailAddress FROM Person.Contact where ContactID=@ContactID", null)]
        public string Contact_Email(int ContactID)
        {
            return (string)base.Execute(ContactID);
        }

        [DBCommand(DBCommandType.Lookup, "SELECT EmailPromotion FROM Person.Contact where ContactID=@ContactID", 0)]
        public int Contact_EmailPromotion(int ContactID)
        {
            return (int)base.Execute(ContactID);
        }

        [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
        public DataRow Contact(int ContactID)
        {
            return (DataRow)base.Execute(ContactID);
        }

        [DBCommand(DBCommandType.Update, "Person.Contact")]
        public int Contact_Update
            (
            [DbField(DalParamType.Key)] int ContactID,
            [DbField()]DateTime ModifiedDate,
            [DbField(24)]string Phone
            )
        {
            return (int)base.Execute(ContactID, ModifiedDate, Phone);
     
        }

        public int GetFirstContctId(int EmailPromotion)
        {
            return base.DMin<int>("ContactID", "Person.Contact", "EmailPromotion=@EmailPromotion", new object[] { EmailPromotion });
        }

        [DBCommand("SELECT * FROM Person.Contact where ContactID=@ContactID", 0)]
        public ContactItem GetContactEntity(int ContactID)
        {
            //return base.Execute<ContactItem,ContactItem>(ContactID);
            //or
            //return base.Execute<ContactItem>(ContactID);
            //or
            object o = base.Execute(ContactID);
            return AdapterFactory.ConvertTo<ContactItem>(o);
            //or
            //return (ContactItem)base.Execute(ContactID);
        }

        [DBCommand("SELECT * FROM Person.Contact where Title=@Title", 0)]
        public List<ContactItem> GetContactsByTitle(string Title)
        {
            return base.Execute<ContactItem,List<ContactItem>>(Title);

            //or

            //object result=base.Execute(Title);
            //return AdapterFactory.ConvertArray<ContactItem>(result);
            //return AdapterFactory.ConvertList<ContactItem>(result);
            

        }
    }

    #endregion


}

