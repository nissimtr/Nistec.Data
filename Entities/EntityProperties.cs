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
using System.Collections;
using System.Globalization;
using Nistec.Generic;
using System.Data;
using Nistec.Xml;
using System.Xml;
using Nistec.Runtime;

namespace Nistec.Data.Entities
{
    
    /// <summary>
    /// EntityProperties Properties collection
    /// </summary>
    public class EntityProperties
    {
        #region Properties

        Dictionary<string, EntityField> properties = new Dictionary<string, EntityField>();

        public EntityField Get(string field)
        {
            return properties[field];
        }

        public string GetCaption(string field)
        {
            EntityField ef = Get(field);
            if (ef == null)
                return "";
           return ef.Caption;
        }

        public ICollection<EntityField> Fields
        {
            get { return properties.Values; }
        }

        public IEnumerable<EntityField> OrderFields
        {
            get
            {
                return from field in this.Fields.ToArray()
                       orderby field.Order
                       select field;
            }
        }



        #endregion

        #region  ctor

        public EntityProperties(EntityField[] epc)
        {
            this.AddRange(epc);
        }

        public EntityProperties(object instance, ILocalizer lang, CultureInfo culture)
        {
            EntityField[] epc = BuildProperties(instance, lang, culture);
            this.AddRange(epc);
        }

        public EntityProperties(object instance, EntityDbContext db)
        {
            EntityField[] epc = null;
            if (db != null)
            {
                epc = BuildProperties(instance, db.Localization, db.EntityCulture);
            }
            else
            {
                epc = BuildProperties(instance, null, EntityLocalizer.DefaultCulture);

            }
            this.AddRange(epc);
        }

        public EntityProperties(GenericEntity entity, EntityDbContext db)
        {
            EntityField[] epc = null;
            if (db != null)
            {
                epc = BuildProperties(entity, db.Localization, db.EntityCulture);
            }
            else
            {
                epc = BuildProperties(entity, null, EntityLocalizer.DefaultCulture);

            }
            this.AddRange(epc);
        }

        void AddRange(EntityField[] fields)
        {
            foreach (EntityField fieled in fields)
            {
                properties[fieled.FieldName] = fieled;
            }
        }

        #endregion

        #region static

        public static EntityProperties Create(object instance, ILocalizer lang, CultureInfo culture)
        {
            return new EntityProperties(BuildProperties(instance, lang, culture));
        }

        public static EntityProperties Create(object instance)
        {
            return new EntityProperties(BuildProperties(instance, null, null));
        }


        internal static EntityField[] BuildProperties(object instance, ILocalizer lang, CultureInfo culture, bool hasCaptionOnly = false)
        {

            List<EntityField> list = new List<EntityField>();
            
            var props = AttributeProvider.GetPropertiesInfo<EntityPropertyAttribute>(instance);

            foreach (var pa in props)
            {
                PropertyInfo property = pa.Property;
                EntityPropertyAttribute attr = pa.Attribute;

                if (!property.CanRead)
                {
                    continue;
                }

               
                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.NA)
                    {
                        continue;
                    }
                    if (hasCaptionOnly && attr.IsCaptionDefined == false)
                    {
                        continue;
                    }
                    object value = property.GetValue(instance, null);
                    if (value == null)
                        value = attr.AsNull;

                    string caption = attr.GetCaption(property.Name);

                    if (attr.IsResourceKeyDefined && lang != null)
                    {
                        caption = lang.GetString(culture, attr.ResourceKey, caption);
                    }
                    attr.Caption = caption;

                    EntityField p = new EntityField(property.Name, value, attr);
                    list.Add(p);
                }
            }

            return list.ToArray();
        }

        internal static EntityField[] BuildProperties(GenericEntity entity, ILocalizer lang, CultureInfo culture)
        {

            List<EntityField> list = new List<EntityField>();
            KeySet keys = entity.PrimaryKey;

            foreach (var pa in entity.Record)
            {

                var propType = EntityPropertyType.Default;

                if (keys.ContainsKey(pa.Key))
                    propType = EntityPropertyType.Key;

                EntityPropertyAttribute attr = new EntityPropertyAttribute(propType, pa.Key);

                if (attr != null)
                {
                    if (attr.ParameterType == EntityPropertyType.NA)
                    {
                        continue;
                    }
                    object value = pa.Value;
                    if (value == null)
                        value = attr.AsNull;

                    string caption = attr.GetCaption(pa.Key);

                    if (attr.IsResourceKeyDefined && lang != null)
                    {
                        caption = lang.GetString(culture, attr.ResourceKey, caption);
                    }
                    attr.Caption = caption;

                    EntityField p = new EntityField(pa.Key, value, attr);
                    list.Add(p);
                }
            }

            return list.ToArray();
        }

 
 
        #endregion

        #region methods

        public KeyValueItem<object> ToInsertParameters(string tableName)
        {
            if (this.Fields == null || this.Fields.Count == 0)
                return null;

            List<object> prm = new List<object>();
            List<string> keys = new List<string>();
            List<string> values = new List<string>();
            StringBuilder sbKeys = new StringBuilder();
            StringBuilder sbValue = new StringBuilder();
            foreach (var p in OrderFields)
            {
                if (p.Attributes.IsValidInsertParameter)
                {
                    prm.Add(p.Column);
                    prm.Add(p.Value);
                    keys.Add(p.Column);
                    values.Add("@" + p.Column);

                }
            }
            string sql = string.Format("insert into [{0}] ({1}) values({2})", tableName, string.Join(",", keys), string.Join(",", values));
            return new KeyValueItem<object>() { Key = sql, Value = prm.ToArray() };
        }

        public object[] ToKeyValueParameters()
        {
            if (this.Fields == null || this.Fields.Count == 0)
                return null;

            List<object> prm = new List<object>();

            foreach (var p in OrderFields)
            {
                if (p.Attributes.IsValidInsertParameter)
                {
                    prm.Add(p.Column);
                    prm.Add(p.Value);
                }
            }
            return prm.ToArray();
        }

        public DataParameter[] ToInsertParameters()
        {
            if (this.Fields == null || this.Fields.Count == 0)
                return null;

            List<DataParameter> prm = new List<DataParameter>();

            foreach (var p in OrderFields)
            {
                if (p.Attributes.IsValidInsertParameter)
                {
                    prm.Add(new DataParameter(p.Column, p.Value));
                }
            }
            return prm.ToArray();
        }

        /// <summary>
        /// Display EntityFields as DataTable with rows 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable ToDataTable(string tableName)
        {
            DataTable dt = ToTableSchema(tableName);
            var row = dt.NewRow();
            foreach (EntityField field in properties.Values)
            {
                row[field.Column] = field.Value;
            }
            dt.Rows.Add(row);
            return dt;
        }


        /// <summary>
        /// Display EntityFields as Html Table, 
        /// class styles :styledTable,styledTableHeader,styledRowKey,styledRowValue
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public string ToHtmlTable(string headerName, string headerValue)
        {
            EntityField[] epc = properties.Values.ToArray();

            return ToHtmlTable(epc, headerName, headerValue);
        }

        /// <summary>
        /// Display EntityFields as Html Table, 
        /// class styles :styledTable,styledTableHeader,styledRowKey,styledRowValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <param name="hasCaptionOnly"></param>
        /// <returns></returns>
        public static string ToHtmlTable<T>(T entity, string headerName, string headerValue, bool hasCaptionOnly) where T : IEntityItem
        {
            EntityField[] epc = EntityProperties.BuildProperties(entity, null, null, hasCaptionOnly);
            if (epc == null)
                return "";
            return ToHtmlTable(epc, headerName, headerValue);
        }



        internal static string ToHtmlTable(EntityField[] epc,string headerName, string headerValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<table class=\"styledTable\" cellspacing=\"5\">");
            if (!(headerName == null || headerValue == null))
            {
                sb.AppendLine("<tr>");

                sb.AppendLine("<th class=\"styledTableHeader\">" + headerName + "</th>");
                sb.AppendLine("<th class=\"styledTableHeader\">" + headerValue + "</th>");

                sb.AppendLine("</tr>");
            }
            for (int i = 0; i <= epc.Length - 1; i++)
            {
                sb.AppendLine("<tr>");

                //string altStyle = (i % 2 == 0) ? "styledTableRow" : "styledTableAltRow";
                sb.AppendLine("<td class=\"styledRowKey\">" + epc[i].Caption + "</td>");
                sb.AppendLine("<td class=\"styledRowValue\">" + epc[i].TextValue + "</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("<table>");
            return sb.ToString();
        }



        /// <summary>
        /// Display EntityFields as Xml string, 
        /// </summary>
        /// <param name="rootName"></param>
        /// <returns></returns>
        public string ToXml(string rootName)
        {
            EntityField[] epc = properties.Values.ToArray();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("<{0}>",rootName));
            foreach (EntityField p in epc)
            {
                if (p.Value == null)
                    sb.AppendLine(string.Format("<{0}/>", p.Column));
                else
                    sb.AppendLine(string.Format("<{0}>{1}<{0}>", p.Column, p.TextValue));
            }
            sb.AppendLine(string.Format("</{0}>", rootName));
            return sb.ToString();
        }

         /// <summary>
        /// Display EntityFields as Xml document, 
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public XmlDocument ToXmlDocument(string entityName)
        {
           return XmlFormatter.DictionaryToXml(ToDictionary(false), entityName);
        }

        /// <summary>
        /// Display EntityFields as IDictionary 
        /// </summary>
        /// <param name="valueAsString"></param>
        /// <returns></returns>
        public IDictionary ToDictionary(bool valueAsString)
        {
            EntityField[] epc = properties.Values.ToArray();
            IDictionary hash = new Hashtable();
            foreach (EntityField p in epc)
            {
                hash[p.FieldName] = valueAsString ? p.TextValue : p.Value;
            }
            return hash;
        }

        /// <summary>
        /// Display EntityFields as DataTable schema 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable ToTableSchema(string tableName)
        {
            DataTable dt = new DataTable(tableName);
            foreach (EntityField field in properties.Values)
            {
                dt.Columns.Add(field.ToDataColumn());
            }
            return dt.Clone();
        }

        /// <summary>
        /// Display EntityFields as Vertical view, 
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public string ToVerticalView(string headerName, string headerValue)
        {
            EntityField[] epc = properties.Values.ToArray();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("{0}\t{1}", headerName, headerValue);
            for (int i = 0; i <= epc.Length - 1; i++)
            {
                sb.AppendLine();
                sb.AppendFormat("{0}: {1}", epc[i].Caption, epc[i].TextValue);
            }
            sb.AppendLine();
            return sb.ToString();
        }
        #endregion

    }

}
