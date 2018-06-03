using DataEntityDemo.DB;
using DataEntityDemo.Entities;
using Nistec.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Entities
{
    class DbContextDemo
    {
        public static IEnumerable<ContactItem> FilterByProperty(int EmailPromotion)
        {
            return AdventureWorks.Instance.EntityItemList<ContactItem>("Person.Contact", "EmailPromotion", EmailPromotion);
        }

        public static ContactItem GetContact(int ContactID)
        {
            return AdventureWorks.Instance.EntityItemGet<ContactItem>("Person.Contact", "ContactID", ContactID);
        }

        public static string FilterByPropertyAsJson(int EmailPromotion)
        {
            return AdventureWorks.Instance.QueryJson("Person.Contact", "EmailPromotion", EmailPromotion);
        }
        public static string GetContactAsJson(int ContactID)
        {
            return AdventureWorks.Instance.QueryJsonRecord("Person.Contact", "ContactID", ContactID);
        }

        public static ContactItem GetContactUsingProcedure(int ContactID)
        {
            return AdventureWorks.Instance.ExecuteSingle<ContactItem>("sp_GetContact", "ContactID", ContactID);
        }

        public static int DoSaveUsingProcedure(ContactItem item)
        {

            var args = new object[]{
            "ContactID",item.ContactID
            ,"FirstName",item.FirstName
            ,"LastName",item.LastName
            ,"EmailAddress",item.EmailAddress
            ,"EmailPromotion",item.EmailPromotion
            ,"Phone",item.Phone
            ,"NameStyle",item.NameStyle
            ,"ModifiedDate",item.ModifiedDate
            };
            var parameters = DataParameter.GetSql(args);
            parameters[0].Direction = System.Data.ParameterDirection.InputOutput;
            int res = AdventureWorks.Instance.ExecuteNonQuery("sp_AddContact", parameters, System.Data.CommandType.StoredProcedure);
            item.ContactID = Types.ToInt(parameters[0].Value);
            return item.ContactID;
        }

    }
}
