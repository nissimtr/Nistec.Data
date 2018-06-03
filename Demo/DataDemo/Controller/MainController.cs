using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataEntityDemo.Controller
{
    public class MainController
    {

        public static void Run(string cmd)
        {
            int contactId = 2;
            Guid g = new Guid("79795DB4-5034-4FA7-8FDE-ED602519BCFF");
            string phone="052-7464292-"+ DateTime.Now.Second.ToString();
            
            
            if (cmd == "all")
            {
                Run("dal");
                Run("generic");
                Run("dynamic");
                Run("db");
            }

            switch (cmd)
            {
                case "dal":
                    DalFactoryController dal = new DalFactoryController();
                    dal.DbCommandController();
                    dal.DynamicDbCommandAbstaract();
                    dal.DynamicDbCommandInterface();
                    dal.DbCommandFactory();
                    break;

                case "generic":
                    {
                        GenericContactController item = new GenericContactController();
                        item.PrintAllContacts();
                        item.PrintContactItem(contactId);
                        item.SaveContactPhone(g, phone);
                    }
                    break;
                case "dynamic":
                    {
                        DynamicContactController item = new DynamicContactController();
                        item.PrintAllContacts();
                        item.PrintContactItem(contactId);
                        item.SaveContactPhone(g, phone);
                    }
                    break;
                case "db":
                    {
                        DbContactController item = new DbContactController();
                        item.PrintAllContacts();
                        item.PrintContactItem(contactId);
                    }
                    break;
            }

        }
    }
}
