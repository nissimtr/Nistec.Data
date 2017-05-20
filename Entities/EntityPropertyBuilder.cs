//licHeader
//===============================================================================================================
// System  : Nistec.Data - Nistec.Data Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of data library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Nistec;
using Nistec.Generic;
using System.Text;
using System.Linq;
using System.Globalization;
using System.Collections.Specialized;

namespace Nistec.Data.Entities
{

     internal static class EntityPropertyBuilder
    {
                 

         public static EntityMode BuildEntityDb(IEntity instance, CultureInfo culture)
         {
             EntityMode mode = EntityMode.NA;
             EntityAttribute[] attributes = instance.GetType().GetCustomAttributes<EntityAttribute>();
             if (attributes == null || attributes.Length == 0)
                 return EntityMode.NA;
             var attribute = attributes[0];
             EntityDbContext db = new EntityDbContext(attribute);//.ConnectionKey, attribute.EntityName, attribute.MappingName, attribute.EntitySourceType, EntityKeys.Get(attribute.EntityKey));
             if (attribute.IsLangResourcesDefined)
             {
                 db.SetLocalizer(instance as IEntity, attribute.LangResources);
             }
             db.EntityCulture = culture;
             instance.EntityDb = db;
             mode = attribute.Mode;

             return mode;
         }


         public static GenericRecord CreateGenericRecord<T>(NameValueCollection collection, bool EnablePropertyTypeView)//, bool setValue)// = true)//IEntity
         {
             if (collection == null)
             {
                 throw new ArgumentNullException("CreateGenericRecord.collection");
             }

             GenericRecord gv = new GenericRecord();

             var props = DataProperties.GetEntityProperties(typeof(T));
             //var v= collection[""];


             foreach (var pa in props)
             {
                 PropertyInfo property = pa.Property;

                 if (!property.CanRead)
                 {
                     continue;
                 }
                 string field = property.Name;
                 EntityPropertyAttribute attr = pa.Attribute;

                 if (attr != null && attr.ParameterType != EntityPropertyType.NA)
                 {

                     if (EnablePropertyTypeView == false && attr.ParameterType == EntityPropertyType.View)
                     {
                         continue;
                     }

                     if (attr.ParameterType == EntityPropertyType.Optional)
                     {
                         if (attr.IsColumnDefined)
                         {
                             field = attr.Column;
                         }
                     }
                     else
                     {
                         field = attr.IsColumnDefined ? attr.Column : property.Name;
                     }
                     if (collection.AllKeys.Contains(field))
                     {

                         object fieldValue = Types.ChangeType(collection.Get(field), property.PropertyType);
                         //object fieldValue = property.GetValue(instance, null);
                         gv.Add(field, fieldValue);
                     }
                 }
             }
             return gv;
         }

        /// <summary>
        /// Create <see cref="GenricRecord"/> from EntityContext instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="EnablePropertyTypeView"></param>
        /// <returns></returns>
         public static GenericRecord CreateGenericRecord(object instance, bool EnablePropertyTypeView)//, bool setValue)// = true)//IEntity
         {

             if (instance == null)
             {
                 throw new ArgumentNullException("BuildEntityContext.instance");
             }

             GenericRecord gv = new GenericRecord();

             var props = DataProperties.GetEntityProperties(instance.GetType());

             foreach (var pa in props)
             {
                 PropertyInfo property = pa.Property;

                 if (!property.CanRead)
                 {
                     continue;
                 }
                 string field = property.Name;
                 EntityPropertyAttribute attr = pa.Attribute;

                 if (attr != null && attr.ParameterType != EntityPropertyType.NA)
                 {

                     if (EnablePropertyTypeView==false && attr.ParameterType == EntityPropertyType.View)
                     {
                         continue;
                     }

                     if (attr.ParameterType == EntityPropertyType.Optional)
                     {
                         if (attr.IsColumnDefined)
                         {
                             field = attr.Column;
                         }
                     }
                     else
                     {
                         field = attr.IsColumnDefined ? attr.Column : property.Name;
                     }

                     object fieldValue = property.GetValue(instance, null);
                     gv.Add(field, fieldValue);
                 }
             }
             return gv;
         }

         public static Dictionary<string, object> EntityToDictionary(IEntityItem instance, bool EnablePropertyTypeView = false)
         {
             //GenericRecord gr=CreateGenericRecord(instance, EnablePropertyTypeView);

             //if (gr == null)
             //{
             //    throw new ArgumentNullException("BuildEntityContext.instance");
             //}
             //return gr.ToDictionary();

             if (instance == null)
             {
                 throw new ArgumentNullException("BuildEntityContext.instance");
             }

             Dictionary<string, object> gv = new Dictionary<string, object>();

             var props = DataProperties.GetEntityProperties(instance.GetType());

             foreach (var pa in props)
             {
                 PropertyInfo property = pa.Property;

                 if (!property.CanRead)
                 {
                     continue;
                 }
                 string field = property.Name;
                 EntityPropertyAttribute attr = pa.Attribute;

                 if (attr != null && attr.ParameterType != EntityPropertyType.NA)
                 {

                     if (EnablePropertyTypeView == false && attr.ParameterType == EntityPropertyType.View)
                     {
                         continue;
                     }

                     if (attr.ParameterType == EntityPropertyType.Optional)
                     {
                         if (attr.IsColumnDefined)
                         {
                             field = attr.Column;
                         }
                     }
                     else
                     {
                         field = attr.IsColumnDefined ? attr.Column : property.Name;
                     }

                     object fieldValue = property.GetValue(instance, null);
                     gv.Add(field, fieldValue);
                 }
             }
             return gv;
         }

         [Obsolete("Use CreateGenericRecord(object instance)")]
         public static GenericRecord BuildEntityContext(object instance, bool setValue)// = true)//IEntity
        {
            if (instance == null)
            {
                throw new ArgumentNullException("BuildEntityContext.instance");
            }

            GenericRecord gv = new GenericRecord();

            var props = DataProperties.GetEntityProperties(instance.GetType());

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;

                if (!property.CanRead)
                {
                    continue;
                }
                string field = property.Name;
                EntityPropertyAttribute attr = pa.Attribute;

                if (attr != null && attr.ParameterType != EntityPropertyType.NA)
                {
                    field = attr.IsColumnDefined ? attr.Column : property.Name;

                }
                object fieldValue = property.GetValue(instance, null);
                gv.Add(field, fieldValue);

                if (setValue)
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(instance, Types.NZ(fieldValue), null);
                    }
                }

            }
            return gv;
        }

 
        public static void SetEntityContext(object instance, GenericRecord values)
        {
            if (values == null)
                return;
            var props = DataProperties.GetEntityProperties(instance.GetType());

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;

                if (!property.CanWrite)
                {
                    continue;
                }

                var attr = pa.Attribute;
                string field = property.Name;

                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.NA)
                        continue;
                    field = attr.IsColumnDefined ? attr.Column : property.Name;

                }


                object fieldValue = null;
                if (values.TryGetValue(field, property.PropertyType, out fieldValue))
                {
                    fieldValue = GenericTypes.Convert(fieldValue, property.PropertyType);

                    if (fieldValue == null && property.PropertyType == typeof(string))
                        fieldValue = "";

                    property.SetValue(instance, fieldValue, null);
                }

                //attr.IsNull(values.GetValue(field));
                //object fieldValue = GenericTypes.Convert(values[field], property.PropertyType);
                //if (fieldValue == null && property.PropertyType == typeof(string))
                //    fieldValue = "";

                //property.SetValue(instance, fieldValue, null);
            }

        }
        public static void SetEntityContext(object instance, DataRow values)
        {

            //SetEntityContext(instance, GenericRecord.Parse(values));

            var props = DataProperties.GetEntityProperties(instance.GetType());

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                if (!property.CanWrite)
                {
                    continue;
                }

                EntityPropertyAttribute attr = pa.Attribute;
                string field = property.Name;

                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.NA)
                        continue;
                    field = attr.IsColumnDefined ? attr.Column : property.Name;
                }

                object fieldValue = GenericTypes.Convert(values[field], property.PropertyType);
                if (fieldValue == null && property.PropertyType == typeof(string))
                    fieldValue = "";

                property.SetValue(instance, fieldValue, null);

            }
        }

        public static string[] GetEntityKeys(object instance)
        {

            var props = DataProperties.GetEntityProperties(instance.GetType());
            List<string> keys = new List<string>();

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;

                        keys.Add(key);
                    }
                }

            }
            return keys.ToArray();
        }
        public static EntityKeys GetEntityPrimaryKey(object instance)
        {
           EntityKeys keys = new EntityKeys();

            var props = DataProperties.GetEntityProperties(instance.GetType());


            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;

                        keys.Add(key);
                    }
                }

            }
            return keys;
        }

        public static KeySet GetEntityKeySet<T>(GenericRecord gr) where T : IEntityItem
        {
            KeySet keys = new KeySet();

            var props = DataProperties.GetEntityProperties(typeof(T));

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                        object val = null;
                        gr.TryGetValue(key, out val);
                        keys.Add(key,val);
                    }
                }

            }
            return keys;
        }
        public static EntityKeys GetEntityPrimaryKey<T>() where T : IEntityItem
        {
            EntityKeys keys = new EntityKeys();

            var props = DataProperties.GetEntityProperties(typeof(T));


            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;

                        keys.Add(key);
                    }
                }

            }
            return keys;
        }

        public static object[] GetEntityKeyValueByOrder<T>(T instance, bool keyTypesOnly=true) where T : IEntityItem
        {
            List<object> keyValues = new List<object>();

            var props = DataProperties.GetEntityProperties(typeof(T));
            props = props.Where(p=> p.Attribute.Order>0).OrderBy(p => p.Attribute.Order);

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (keyTypesOnly)
                    {
                        if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                        {
                            string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                            object val = property.GetValue(instance, null);

                            keyValues.Add(key);
                            keyValues.Add(val);
                        }
                    }
                    else
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                        object val = property.GetValue(instance, null);

                        keyValues.Add(key);
                        keyValues.Add(val);
                    }
                }

            }
            return keyValues.ToArray();
        }
        public static object[] GetEntityKeyValueParameters<T>(T instance, bool useOrder = false) where T : IEntityItem
        {
            List<object> keyValues = new List<object>();

            var props = DataProperties.GetEntityProperties(typeof(T));
            if (useOrder)
                props = props.Where(p => p.Attribute.Order > 0).OrderBy(p => p.Attribute.Order);

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                        object val = property.GetValue(instance, null);

                        keyValues.Add(key);
                        keyValues.Add(val);
                    }
                }

            }
            return keyValues.ToArray();
        }

        public static IDbDataParameter[] GetEntityDbParameters<T>(T instance, bool useOrder = false) where T : IEntityItem
        {
            List<DataParameter> keyValues = new List<DataParameter>();

            var props = DataProperties.GetEntityProperties(typeof(T));
            if (useOrder)
                props = props.Where(p => p.Attribute.Order > 0).OrderBy(p => p.Attribute.Order);

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        string key = attr.IsColumnDefined ? attr.Column : pa.Property.Name;
                        object val=property.GetValue(instance, null);
                        keyValues.Add(new DataParameter(key,val));
                    }
                }

            }
            return keyValues.ToArray();
        }


        public static string[] GetEntityFields(Type type)
        {
            var props = DataProperties.GetProperties(type);
            return props.Select(l => l.Name).ToArray();
        }
        public static EntityKeys SetEntityContextWithKey(IEntity context, object instance)
        {
            //rcd
            GenericRecord values = context.EntityRecord;

            EntityAttribute keyattr = AttributeProvider.GetCustomAttribute<EntityAttribute>(context.GetType());

            if (keyattr == null)
            {
                keyattr = new EntityAttribute(context.EntityDb);
            }

            return SetEntityContextWithKey(keyattr, instance, values);

        }

        public static EntityKeys SetEntityContextWithKey(EntityAttribute keyattr, object instance, GenericRecord values)
        {

            EntityKeys keys = null;
            bool hassAttrib = false;
            if (keyattr != null)
            {
                hassAttrib = true;
                keys = new EntityKeys(keyattr.EntityKey);
            }
            else
                keys = new EntityKeys();


            var props = DataProperties.GetEntityProperties(instance.GetType());

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;

                if (attr != null)
                {

                    if (attr.ParameterType == EntityPropertyType.NA)
                        continue;

                    if (!property.CanWrite)
                        continue;
                    
                    string field = attr.IsColumnDefined ? attr.Column : property.Name;

                    object fieldValue = values.GetValue(field);

                    object value = GenericTypes.ConvertProperty(fieldValue, property);

                    property.SetValue(instance, value, null);

                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        //if (value != null)
                        keys.FieldsKey[field] = value;//.ToString();
                    }

                }
            }
            if (!hassAttrib)
                keys.CreateByKeySet();//.SetKeyInternal();

            return keys;
        }

        public static EntityKeys SetEntityContextWithKey(object instance, GenericRecord values)
        {
            EntityKeys keys = new EntityKeys();
            var props = DataProperties.GetEntityProperties(instance.GetType());
            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;


                if (attr != null)
                {

                    if (attr.ParameterType == EntityPropertyType.NA)
                        continue;

                    if (!property.CanWrite)
                        continue;


                    string field = attr.IsColumnDefined ? attr.Column : property.Name;

                    object fieldValue = values.GetValue(field);

                    object value = GenericTypes.ConvertProperty(fieldValue, property);

                    property.SetValue(instance, value, null);

                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        keys.FieldsKey[field] = value;//.ToString();
                    }

                }
            }

            keys.CreateByKeySet();//.SetKeyInternal();

            return keys;
        }

        public static EntityKeys SetEntityContextWithKey(object instance, IGenericEntity values)
        {
            EntityKeys keys = new EntityKeys();
            var props = DataProperties.GetEntityProperties(instance.GetType());

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;


                if (attr != null)
                {

                    if (attr.ParameterType == EntityPropertyType.NA)
                        continue;

                    if (!property.CanWrite)
                        continue;


                    string field = attr.IsColumnDefined ? attr.Column : property.Name;

                    object fieldValue = values.GetValue(field);

                    object value = GenericTypes.ConvertProperty(fieldValue, property);

                    property.SetValue(instance, value, null);

                    if (attr.ParameterType == EntityPropertyType.Key || attr.ParameterType == EntityPropertyType.Identity)
                    {
                        //if (value != null)
                            keys.FieldsKey[field] = value;//.ToString();

                    }

                }
            }
            keys.CreateByKeySet();
            return keys;
        }

    }
 
}
