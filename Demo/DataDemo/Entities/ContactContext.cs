using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.Entities;
using Nistec.Data.Factory;
using Nistec.Data;
using DataEntityDemo.DB;

namespace DataEntityDemo.Entities
{
   
    [Entity(EntityName = "Contact", MappingName = "Person.Contact", ConnectionKey = "AdventureWorks", EntityKey = new string[] { "ContactID" })]
    [Serializable]
    public class ContactContext : EntityContext<ContactItem>
    {

        #region ctor

        public ContactContext()
        {
        }

        public ContactContext(int id)
            : base(id)
        {
        }

        public ContactContext(Guid row)
        {
            base.Init(new DataFilter("rowguid=@rowguid",row));
        }

        public static ContactItem Get(int id)
        {
            using (ContactContext context = new ContactContext(id))
            {
                return context.Entity;
            }
        }

        public static DataTable GetList()
        {
            DataTable dt = null;
            using (IDbCmd cmd = AdventureWorks.Instance.NewCmd())
            {
                dt = cmd.ExecuteCommand<DataTable>("select top 10 * from Person.Contact", true);
            }

            return dt;
        }

        public static IList<ContactItem> GetItems()
        {
            using (ContactContext context = new ContactContext())
            {
               return context.EntityList();
            }
        }

        #endregion

        #region binding

        protected override void EntityBind()
        {
            base.EntityDb.EntityCulture = AdventureWorksResources.GetCulture();
            //If EntityAttribute not define you can initilaize the entity here
            //base.InitEntity<AdventureWorks>("Contact", "Person.Contact", EntityKeys.Get("ContactID"));
        }

        #endregion

        #region methods

        public void Test()
        {
            string str = ExecuteCommand<string>("select EmailAddress from Person.Contact where ContactID=@ContactID", new DataParameter[] { new DataParameter("ContactID", 2) });
            Console.WriteLine(str);
        }

        #endregion

    }

    public class ContactItem : IEntityDb
    {
        #region properties

        [EntityProperty(EntityPropertyType.Identity, Caption = "ID")]
        public int ContactID
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "FirstName", Caption = "First name")]
        public string FirstName
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "LastName", Caption = "Last name")]
        public string LastName
        {
            get;
            set;
        }


        [EntityProperty(EntityPropertyType.Default, false, Column = "EmailAddress", Caption = "Email")]
        public string EmailAddress
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Caption = "Display name")]
        public int EmailPromotion
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "Phone", Caption = "Phone")]
        public string Phone
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Caption = "Style")]
        public bool NameStyle
        {
            get;
            set;
        }
        [EntityProperty(EntityPropertyType.Default, false, Caption = "Modified date")]
        public DateTime ModifiedDate
        {
            get;
            set;
        }



        #endregion

        #region override
        public override string ToString()
        {
            return string.Format("FirstName:{0},LastName:{1},Phone:{2}", FirstName, LastName, Phone);
        }
        #endregion

        #region IEntityDb Members

        public EntityDbContext EntityDb
        {
            get
            {
                return EntityDbContext.Get<AdventureWorks>("Contact", "Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"));
            }
            set { }
        }

        #endregion
    }
}
