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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Nistec.Generic;
using System.Collections;
using System.Data;
using Nistec.Xml;
using System.Xml;
using System.Xml.Serialization;
using Nistec.Data.Entities.Cache;
using Nistec.Serialization;
using Nistec.Runtime;

namespace Nistec.Data.Entities
{


    public static class EntityDataExtension
    {
        public const string DefultNamespace = "http://www.w3.org/2001/XMLSchema";
 
        internal static string ResolveRootElelment(string xml, Type type)
        {
            xml = Regx.RegexReplace("<EntityBase", xml, string.Format("<{0}", type.Name));
            xml = Regx.RegexReplace("</EntityBase", xml, string.Format("</{0}", type.Name));
            return xml;
        }

        #region Serialize entity
 
       
        public static string SerializeDataSource(this IDataEntity entity, FormatterType format)
        {
            if (format == FormatterType.Xml)
                return entity.EntityDataSource.SerializeToXml(null);// NetSerializer.SerializeToXml(this.DataSource);
            else if (format == FormatterType.GZip)
                return NetZipp.Zip(NetSerializer.SerializeToBase64(entity.EntityDataSource));
            else //if (format == SerializationFormat.Binary)
                return NetSerializer.SerializeToBase64(entity.EntityDataSource);
        }

        #endregion

        #region Deserialize etity

        public static GenericDataTable DeserializeDataSource(string serialaized, FormatterType format)//, SerializationType type)
        {
            try
            {
                if (format == FormatterType.Xml)
                    return GenericDataTable.DeserializeFromXml(serialaized, null);
                else if (format == FormatterType.GZip)
                    return (GenericDataTable)NetSerializer.DeserializeFromBase64(NetZipp.UnZip(serialaized));
                else //if (format == SerializationFormat.Binary)
                    return (GenericDataTable)NetSerializer.DeserializeFromBase64(serialaized);
            }
            catch (Exception ex)
            {
                throw new EntityException("DeserializeDataSource error: ex" + ex.Message, ex.InnerException);
            }
        }

        public static GenericRecord DeserializeEntityRecord(string serialaized, FormatterType format)//, SerializationType type)
        {
            try
            {
                if (format == FormatterType.Xml)
                    return GenericRecord.DeserializeFromXml(serialaized, null);
                else if (format == FormatterType.GZip)
                    return (GenericRecord)NetSerializer.DeserializeFromBase64(NetZipp.UnZip(serialaized));
                else //if (format == SerializationFormat.Binary)
                    return (GenericRecord)NetSerializer.DeserializeFromBase64(serialaized);
            }
            catch (Exception ex)
            {
                throw new EntityException("DeserializeEntityRecord error: ex" + ex.Message, ex.InnerException);
            }
        }

        public static T DeserializeEntity<T>(string serialaized, FormatterType format) where T : IDataEntity
        {
            try
            {
                if (format == FormatterType.Xml)
                {
                    serialaized = ResolveRootElelment(serialaized, typeof(T));// serialaized.Replace("EntityBase", typeof(T).Name);

                    T rVal = default(T);

                    XmlSerializer serializer = new XmlSerializer(typeof(T), EntityBase.DefultNamespace);
                    using (System.IO.StringReader ms = new System.IO.StringReader(serialaized))
                    {
                        XmlReader reader = new XmlTextReader(ms);
                        rVal = (T)serializer.Deserialize(reader);
                    }

                    IDataEntity entity = (IDataEntity)rVal;

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(Xml.XmlFormatter.RemoveXmlDeclaration(serialaized.Trim()));

                    XmlNode node = doc.DocumentElement.SelectSingleNode("//GenericDataTable");
                    if (node != null)
                    {
                        string nodeString = node.OuterXml;
                        entity.EntityDataSource = XSerializer.Deserialize<GenericDataTable>(nodeString);
                        return (T)entity;
                    }

                    return rVal;
                }
                else if (format == FormatterType.GZip)
                {
                    string base64String = NetZipp.UnZip(serialaized);
                    return (T)NetSerializer.DeserializeFromBase64(base64String);
                }
                else //if (format == SerializationFormat.Binary)
                {
                    return (T)NetSerializer.DeserializeFromBase64(serialaized);
                }
            }
            catch (Exception ex)
            {
                throw new EntityException("Deserialize error: ex" + ex.Message, ex.InnerException);
            }
        }
        #endregion

        #region Dictionary xml Serialization

        public static string DictionaryToXml(IDictionary gr,string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
                entityName = GenericRecord.EntityName;
            XmlFormatter formatter = new XmlFormatter(entityName, Xml.XmlFormatter.TargetNamespace, "utf-8");
            return formatter.DictionaryToXmlString(gr);
        }

        public static IDictionary DictionaryFromXml(string xmlString, string entityName)
        {
            Hashtable gr = new Hashtable();
            XmlDocument doc = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(entityName))
                    entityName = GenericRecord.EntityName;

                lock (gr.SyncRoot)
                {
                    doc.LoadXml(xmlString);
                    XmlNode node = doc.DocumentElement;//.ChildNodes;
                    if (node == null)
                    {
                        throw new System.Xml.XmlException("Root tag not found");
                    }
                    XmlNodeList list = node.ChildNodes;

                    foreach (XmlNode n in list)
                    {
                        if (n.NodeType == XmlNodeType.Comment)
                            continue;

                        gr.Add(n.Name, n.InnerText);
                    }
                }
                return gr;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }

        }

        #endregion
        
        #region builds entities
     
        /// <summary>
        /// Create Entity from <see cref="DataRow"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T DataRowToEntity<T>(DataRow dr)
        {
            if (dr == null)
            {
                throw new ArgumentNullException("DataRowToEntity.dr");
            }
            T item = ActivatorUtil.CreateInstance<T>();
            GenericRecord record = new GenericRecord(dr);
            if (record != null)
            {
                EntityPropertyBuilder.SetEntityContext(item, record);
            }
            return item;
        }

        public static List<T> DataTableToEntityList<T>(DataTable dt)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("DataTableToEntityList.dt");
            }
            List<T> list=new List<T>();
            for(int i=0;i<dt.Rows.Count;i++)
            {
                list.Add(DataRowToEntity<T>(dt.Rows[i]));
            }
            return list;
        }

        /// <summary>
        /// Display EntityFields as DataTable schema 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tableName"></param>
        /// <param name="writeAbleOnly"></param>
        /// <param name="disableIdentity"></param>
        /// <returns></returns>
        public static DataTable EntityToTableSchema(IEntityItem entity, string tableName, bool writeAbleOnly, bool disableIdentity)
        {
            DataTable dt = new DataTable(tableName);

            PropertyInfo[] properties = EntityExtension.GetEntityProperties(entity, true, writeAbleOnly, disableIdentity);

            foreach (PropertyInfo field in properties)
            {
                dt.Columns.Add(field.Name,field.PropertyType);
            }
            return dt.Clone();
        }

        public static void EntityResolveJsonFields(IEntityItem entity)
        {
            PropertyInfo[] props = DataProperties.GetProperties(entity.GetType());

            foreach (var property in props)
            {
                if (!property.CanWrite)
                    continue;
                string field = property.Name;
                if (property.PropertyType == typeof(string))
                {
                    object fieldValue = property.GetValue(entity, null);
                    if(fieldValue!=null)
                    {
                        fieldValue=fieldValue.ToString().Replace("\"", "\\\"");
                         property.SetValue(entity, fieldValue, null);
                    }
                }
            }
        }
        public static Dictionary<string, object> EntityToDictionary(IEntityItem entity, bool canWrite, bool disableIdentity)
        {
            Dictionary<string, object> dt = new Dictionary<string, object>();

            //PropertyInfo[] properties = EntityExtension.GetEntityProperties(entity, true, canWrite, disableIdentity);

            var props = DataProperties.GetEntityProperties(entity.GetType());

            foreach (var pa in props)
            {
                //dt.Add(field.Name, field.PropertyType);
                PropertyInfo property = pa.Property;
                if (!property.CanRead)
                {
                    continue;
                }
                string field = property.Name;
                EntityPropertyAttribute attr = pa.Attribute;
                if (attr == null)
                {
                    dt.Add(property.Name, property.GetValue(entity, null));
                }

                else if (attr.ParameterType != EntityPropertyType.NA)
                {

                    if (attr.ParameterType == EntityPropertyType.View)
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

                    object fieldValue = property.GetValue(entity, null);
                    dt.Add(field, fieldValue);
                }
            }
            return dt;
        }
        public static DataTable EntityToDataTable<TMap,T>(IEntityItem[] entities, string tableName, bool writeAbleOnly, bool disableIdentity) where T : IEntityItem
        {
            if (entities == null)
            {
                throw new ArgumentNullException("EntityToTableSchema.entity");
            }
            if (entities.Length == 0)
            {
                throw new ArgumentException("EntityToTableSchema.entity is empty");
            }
  
            DataTable dt = EntityToTableSchema(entities[0], tableName, writeAbleOnly, disableIdentity);

            TMap context = ActivatorUtil.CreateInstance<TMap>();

            foreach (IEntityItem entity in entities)
            {
               ((IEntity<T>) context).Set((T)entity);
               ((IEntity<T>)context).EntityRecord.AddTo(dt, !disableIdentity);
            }
            return dt;
        }

        public static DataTable EntityToDataTable<T>(IEntity<T> context,IEntityItem[] entities, string tableName, bool writeAbleOnly, bool disableIdentity) where T : IEntityItem
        {
            if (entities == null)
            {
                throw new ArgumentNullException("EntityToTableSchema.entity");
            }
            if (entities.Length == 0)
            {
                throw new ArgumentException("EntityToTableSchema.entity is empty");
            }
            T current = context.Entity;

            DataTable dt = EntityToTableSchema(entities[0], tableName, writeAbleOnly, disableIdentity);

            foreach (IEntityItem entity in entities)
            {
                context.Set((T)entity);
                context.EntityRecord.AddTo(dt, !disableIdentity);
            }
            //set back the first entity
            context.Set(current);

            return dt;
        }

        #endregion
        
        public static Dictionary<string, T> ToEntityDictionary<T>(this IEntity context, DataTable dt)
        {
            if (dt == null)
            {
                return null;// throw new ArgumentNullException("EntityDictionary.dt");
            }

            Dictionary<string, T> list = new Dictionary<string, T>();

            var records = GenericRecord.ParseList(dt);

            EntityAttribute keyattr = AttributeProvider.GetCustomAttribute<EntityAttribute>(context.GetType());

            if (keyattr == null)
            {
                keyattr = new EntityAttribute(context.EntityDb);
            }

            if (records != null)
            {

                Type type = typeof(T);

                if (type == typeof(GenericRecord))
                {
                    string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();

                    foreach (GenericRecord gr in records)
                    {
                        string key = new EntityKeys(fieldsKey).ToString();//.Get(gr.GetRecordKey(fieldsKey)).ToString();
                        list[key] = GenericTypes.Cast<T>(gr);// TConvert<T>(gr);
                    }
                }
                else if (typeof(IGenericEntity).IsAssignableFrom(type))
                {
                    string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();

                    foreach (GenericRecord gr in records)
                    {
                        IGenericEntity rcd = (IGenericEntity)System.Activator.CreateInstance<T>();
                        //GenericEntity ge = new GenericEntity(gr);

                        string key = new EntityKeys(fieldsKey).ToString();//EntityKeys.Get(gr.GetRecordKey(fieldsKey)).ToString();
                        rcd.Load(gr);

                        list[key] = GenericTypes.Cast<T>(rcd as IGenericEntity);// TConvert<T>(rcd as IGenericEntity);
                    }
                }
                else if (typeof(IEntityItem).IsAssignableFrom(type))
                {
                    foreach (GenericRecord gr in records)
                    {
                        T item = System.Activator.CreateInstance<T>();
                        //EntityKeys entityKey = EntityPropertyBuilder.SetEntityContextWithKey(keyattr, item, gr);
                        KeySet entityKey = EntityPropertyBuilder.SetEntityContextWithKey(keyattr, item, gr);
                        string key = entityKey.ToString();
                        list[key] = item;
                    }
                }
            }

            return list;
        }

    }
}
