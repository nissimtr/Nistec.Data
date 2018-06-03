using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataEntityDemo.Entities;

namespace DataEntityDemo.Controller
{
    public class GenericContactController
    {

        public void PrintAllContacts()
        {
            ContactContext context = new ContactContext();
            var list = context.EntityList(10);

            Console.WriteLine("Contacs:");
 
            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }

        }

       
        public void PrintContactItem(int contactId)
        {
            ContactContext context = new ContactContext(contactId);
            Console.WriteLine("ContactItem:");
            string item = context.EntityProperties.ToVerticalView("Field", "Value");
            Console.WriteLine(item);
        }


        public void SaveContactPhone(Guid g,string phone)
        {
            ContactContext context = new ContactContext(g);
            var entity = context.Entity;

            string name = entity.FirstName;
            string caption = context.EntityProperties.GetCaption("FirstName");
            Console.WriteLine("{0};{1}", caption, name);

            entity.Phone = phone;
            context.SaveChanges();

            Console.WriteLine(entity.ToString());
        }


    }
}
