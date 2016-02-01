# Nistec.Data
Data access library include entity framework.

Example - Using Db Context

public static IEnumerable<ContactItem> FilterByProperty(int EmailPromotion){
   return AdventureWorks.Instance.QueryEntityList<ContactItem>("Person.Contact", "EmailPromotion", EmailPromotion);
}

public static ContactItem GetContact(int ContactID){
   return AdventureWorks.Instance.QueryEntity<ContactItem>("Person.Contact", "ContactID", ContactID);
}

public static string FilterByPropertyAsJson(int EmailPromotion){
   return AdventureWorks.Instance.QueryJson("Person.Contact", "EmailPromotion", EmailPromotion);
}

public static string GetContactAsJson(int ContactID){
   return AdventureWorks.Instance.QueryJsonRecord("Person.Contact", "ContactID", ContactID);
}

public static ContactItem GetContactUsingProcedure(int ContactID){
   return AdventureWorks.Instance.ExecuteSingle<ContactItem>("sp_GetContact", "ContactID", ContactID);
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
