using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntityDemo.Entities;
using Nistec.Data.Entities;
using DataEntityDemo.DB;
using DataEntityDemo.Entities;
using Nistec.Data;

namespace DataEntityDemo.Controller
{
    public class DbContactController
    {

        public void PrintAllContacts()
        {
            EntityDbContext db = AdventureWorks.Instance.Contact;
            var list = db.EntityList<ContactItem>(10);

            Console.WriteLine("Contacs:");
 
            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }
        }

        public void PrintContactItem(int contactId)
        {
            EntityDbContext db = AdventureWorks.Instance.Contact;
            ContactItem entity = db.ToEntity<ContactItem>(contactId);
            Console.WriteLine("ContactItem:");
        }


    }
}
