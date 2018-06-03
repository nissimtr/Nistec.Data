using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntityDemo.Sql;
using System.Data;
using Nistec.Data.Factory;
using DataEntityDemo.Entities;
using DataEntityDemo.DB;
using Nistec.Data;

namespace DataEntityDemo.Controller
{
    public class DalFactoryController
    {

        public void DbCommandController()
        {

            DataTable dtapp = AdventureWorksCommand.Instance.Contacts();
            if (dtapp.Rows.Count > 0)
            {
                DataRow drapp = AdventureWorksCommand.Instance.Contact(2);
                string sdr = drapp[0].ToString();
                int EmailPromotion = AdventureWorksCommand.Instance.Contact_EmailPromotion(2);
                if (EmailPromotion > 0)
                {

                }

            }

            var entity = AdventureWorksCommand.Instance.GetContactEntity(2);
            Console.WriteLine(entity.ToString());


            var list = AdventureWorksCommand.Instance.GetContactsByTitle("Ms.");

            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }

        }

        public void DynamicDbCommandInterface()
        {
            AdventureWorksBase dal = new AdventureWorksBase();
            IAdventureWorksDB db = dal.IAdventureWorks;
            string eml = db.Contact_Email(2);

            DataTable dt = db.Contacts();
            if (dt.Rows.Count > 0)
            {
                DataRow idr = db.Contact(2);
                string sidr = idr[0].ToString();
                int EmailPromotion = db.Contact_EmailPromotion(2);
                if (EmailPromotion > 0)
                {

                }
            }


            string email = db.Contact_Email(2);
            Console.WriteLine(email);

            var entity = db.GetContactEntity(2);
            Console.WriteLine(entity.LastName);

           
        }

        public void DynamicDbCommandAbstaract()
        {
            AdventureWorksDB db = AdventureWorksBase.DB.AdventureWorks;

            DataTable dt = db.Contacts();
            if (dt.Rows.Count > 0)
            {
                DataRow idr = db.Contact(2);
                string sidr = idr[0].ToString();
                int EmailPromotion = db.Contact_EmailPromotion(2);
                if (EmailPromotion > 0)
                {

                }
            }

            string email = db.Contact_Email(2);

            Console.WriteLine(email);

            var entity = db.GetContactEntity(2);
            Console.WriteLine(entity.LastName);

            var list = db.GetContactsByTitle("Ms.");

            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }

            var contact=db.GetContact(4);
            Console.WriteLine(contact.ToString());

            var mss = db.GetContactsListByTitle("Ms.");
            Console.WriteLine(mss.Count().ToString());
        }


        public void DbCommandFactory()
        {
            var cmd = DbFactory.Create<AdventureWorks>();


            var contact = cmd.ExecuteCommand<ContactItem>("SELECT * FROM Person.Contact where ContactID=@ContactID", DataParameter.Get("ContactID", 5));
            Console.WriteLine(contact.ToString());

            var mss = cmd.ExecuteCommand<ContactItem,ContactItem[]>("SELECT * FROM Person.Contact where Title=@Title", DataParameter.Get("Title", "Ms."));
            Console.WriteLine(mss.Count().ToString());
        }

    }
}
