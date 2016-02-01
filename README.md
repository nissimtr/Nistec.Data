# Nistec.Data
Data access library include entity framework.

Example - Using Db Context

public static IEnumerable<ContactItem> FilterByProperty(int EmailPromotion)
{
    return AdventureWorks.Instance.QueryEntityList<ContactItem>("Person.Contact", "EmailPromotion", EmailPromotion);
}

public static ContactItem GetContact(int ContactID)
{
    return AdventureWorks.Instance.QueryEntity<ContactItem>("Person.Contact", "ContactID", ContactID);
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

Example - Using Entity Context

 [Entity(EntityName = "Contact", MappingName = "Person.Contact", ConnectionKey = "AdventureWorks", EntityKey = new string[] { "ContactID" })]
 public class ContactContext : EntityContext<ContactItem>
 {
       public ContactContext()
        {
        }

        public ContactContext(int id)
            : base(id)
        {
        }
        
         public static ContactItem Get(int id)
        {
            using (ContactContext context = new ContactContext(id))
            {
                return context.Entity;
            }
        }
  }    
