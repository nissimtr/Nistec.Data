using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntityDemo.Entities;
using DataEntityDemo.DB;
using System.Data;
using Nistec.Data;
using Nistec.Generic;
using Nistec.Data.Entities;

namespace DataEntityDemo.Entities
{
    public class EntityTest
    {


        public static void EntitySampleUsingEntityContext()
        {
            using (ContactContext context = new ContactContext(new Guid("79795DB4-5034-4FA7-8FDE-ED602519BCFF")))
            {
                var entity = context.Entity;
                string name = entity.FirstName;
                string caption = context.EntityProperties.GetCaption("FirstName");
                Console.WriteLine("{0};{1}", caption, name);

                entity.Phone = "052-7464292-7";
                context.SaveChanges();

                Console.WriteLine("Properties:");
                string html = context.EntityProperties.ToVerticalView("Field", "Value");
                Console.WriteLine(html);
            }          
        }

        public static void EntitySampleUsingEntityGenericContext()
        {
            using (ContactContext context = new ContactContext(new Guid("79795DB4-5034-4FA7-8FDE-ED602519BCFF")))
            {
                var entity = context.Entity;

                string name = entity.FirstName;
                string caption = context.EntityProperties.GetCaption("FirstName");
                Console.WriteLine("{0};{1}", caption, name);

                entity.Phone = "052-7464292-7";
                context.SaveChanges();

                var list = context.EntityList(ContactContext.GetList());

                DataTable dt = EntityDataExtension.EntityToDataTable<ContactItem>(context, list.ToArray(), "Contact", true, false);
                //or
                //DataTable dt = EntityFormatter.EntityToDataTable<ContactMap, ContactItem>(list.ToArray(), "Contact", true, false);
                int rowscount = dt.Rows.Count;
                foreach (var item in list)
                {
                    context.Set(item);
                    item.Phone = item.Phone + "-0";
                }

                Console.WriteLine("Properties:");
                string html = context.EntityProperties.ToVerticalView("Field", "Value");
                Console.WriteLine(html);
            }
        }

        public static void EntitySampleUsingEntityDynamicContext()
        {
            using (EntityContext<ContactItem> context = new EntityContext<ContactItem>())
            {
                context.SetDb<AdventureWorks>("Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"));
                //or
                //context.EntityDb=AdventureWorks.Instance.DynamicContact;
                //or
                //context.EntityDb = AdventureWorks.Instance.GetEntity("Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"));

                var entity = context.Create(new DataFilter("rowguid=@rowguid", new Guid("79795DB4-5034-4FA7-8FDE-ED602519BCFF")));

                string name = entity.FirstName;
                string caption = context.EntityProperties.GetCaption("FirstName");
                Console.WriteLine("{0};{1}", caption, name);

                entity.Phone = "052-7464292-7";
                context.SaveChanges();

                var list = context.EntityList(ContactContext.GetList());

                foreach (var item in list)
                {
                    context.Set(item);
                    item.Phone = item.Phone + "-0";
                }
                Console.WriteLine("Properties:");
                string html = context.EntityProperties.ToVerticalView("Field", "Value");
                Console.WriteLine(html);
            }
        }

        public static void EntitySampleUsingDbContext()
        {
            EntityDbContext db = AdventureWorks.Instance.Contact;
            ContactItem entity = db.ToEntity<ContactItem>(2);

            string name = entity.FirstName;
            Console.WriteLine("{0}", name);

            var list = db.EntityList<ContactItem>(ContactContext.GetList());

            Console.WriteLine("Properties:Phone ");

            foreach (var item in list)
            {
                Console.WriteLine(item.Phone);
            }
        }


        public static void EntitySampleUsingDbContextAndGenericEntity()
        {
            EntityDbContext db = EntityDbContext.Get<AdventureWorks>("Contact", "Person.Contact", EntitySourceType.Table, EntityKeys.Get("ContactID"));

            GenericEntity entity = GenericEntity.Create(db, 2);

            string name = entity.GetValue<string>("FirstName");

            Console.WriteLine("{0};{1}", "FirstName", name);

            string Phone = entity.GetValue<string>("Phone");

            Console.WriteLine("{0};{1}", "Phone", Phone);

            Console.WriteLine("Properties:");
            string html = ((GenericRecord)entity.EntityDictionary()).Print("Field", "Value");

            Console.WriteLine(html);
        }

        public static void Test(string cmd)
        {
            Console.Write("start...");

            cmd = "context-generic";

            switch (cmd)
            {
               
                case "context":
                    {
                        EntitySampleUsingEntityContext();
                    }
                    break;
                case "context-generic":
                    {
                        EntitySampleUsingEntityGenericContext();
                    }
                    break;
                case "context-dynamic":
                    {
                        EntitySampleUsingEntityDynamicContext();
                    }
                    break;
                case "entity-db":
                    {
                        EntitySampleUsingDbContext();
                    }
                    break;
                case "dynamic":
                    {
                        EntitySampleUsingDbContextAndGenericEntity();
                   }
                    break;
             }
            Console.Write("finished...");
            Console.ReadKey();
        }
    }
}
