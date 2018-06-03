using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntityDemo.Entities;
using Nistec.Data.Entities;
using DataEntityDemo.Entities;
using DataEntityDemo.DB;
using Nistec.Data;

namespace DataEntityDemo.Controller
{
    public class DynamicContactController
    {

        public void PrintAllContacts()
        {
            EntityContext<ContactItem> context = new EntityContext<ContactItem>();
            context.SetDb<AdventureWorks>("Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"));

            var list = context.EntityList(10);

            Console.WriteLine("Contacs:");
 
            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }
        }

        public void PrintContactItem(int contactId)
        {
            EntityContext<ContactItem> context = new EntityContext<ContactItem>();
            context.SetDb<AdventureWorks>("Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"));
            Console.WriteLine("ContactItem:");
            string item = context.EntityProperties.ToVerticalView("Field", "Value");
            Console.WriteLine(item);
        }


        public void SaveContactPhone(Guid g,string phone)
        {
            EntityContext<ContactItem> context = new EntityContext<ContactItem>();
            context.SetDb<AdventureWorks>("Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"));
            var entity = context.Create(DataFilter.Get("rowguid=@rowguid", g));

            string name = entity.FirstName;
            string caption = context.EntityProperties.GetCaption("FirstName");
            Console.WriteLine("{0};{1}", caption, name);

            entity.Phone = phone;
            context.SaveChanges();

            Console.WriteLine(entity.ToString());
        }


    }
}
