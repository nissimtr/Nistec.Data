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
using Nistec.Generic;
using Nistec.Data.Factory;
using System.Data;
using System.Reflection;
using Nistec.Runtime;

namespace Nistec.Data.Entities
{
    public static class DbContextExtension
    {
      

        #region GenericRecord

        /// <summary>
        /// Create Entity from <see cref="GenericRecord"/>
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T ToEntity<T>(this GenericRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("CreateEntity.record");
            }
            using (EntityContext<T> context = new EntityContext<T>())
            {
                context.Set(record);
                return context.Entity;
            }
        }

        /// <summary>
        /// Create Entity from <see cref="GenericRecord"/>
        /// </summary>
        /// <param name="record"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object ToEntity(this GenericRecord record, Type type)
        {
            if (record == null || type == null)
            {
                throw new ArgumentNullException("ToEntity.recordor type");
            }
            object item = ActivatorUtil.CreateInstance(type);//System.Activator.CreateInstance(type);
            EntityPropertyBuilder.SetEntityContext(item, record);
            return item;
        }
        #endregion

        #region DataRow|DataTable <T>

        /// <summary>
        /// Create Entity from <see cref="DataRow"/>
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T ToEntity<T>(this DataRow dr) //where T : IEntityItem
        {
            if (dr == null)
            {
                throw new ArgumentNullException("ToEntity.dr");
            }
            using (EntityContext<T> context = new EntityContext<T>())
            {
                GenericRecord record = new GenericRecord(dr);
                context.Set(record);
                return context.Entity;
            }
        }

        static void SetEntityContext<T>(T item, DataRow values, IEnumerable<PropertyAttributeInfo<EntityPropertyAttribute>> props)
        {
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

                property.SetValue(item, fieldValue, null);

            }
        }
        /// <summary>
        /// Create Entity collection from <see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> EntityList<T>(this DataTable dt)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("EntityList.dt");
            }

            T instance = ActivatorUtil.CreateInstance<T>();

            var props = DataProperties.GetEntityProperties(instance.GetType());
            List<T> list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = ActivatorUtil.CreateInstance<T>();
                SetEntityContext<T>(item, row, props);
                list.Add(item);
            }

            return list;
        }


        /// <summary>
        /// Create Entity from <see cref="DataRow"/>
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static object ToEntity(this DataRow dr, Type type)
        {
            if (dr == null || type==null)
            {
                throw new ArgumentNullException("ToEntity.dr or type");
            }
            object item = ActivatorUtil.CreateInstance(type);
            EntityPropertyBuilder.SetEntityContext(item, dr);
            return item;
        }

        /// <summary>
        /// Create Entity collection from <see cref="DataTable"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static List<object> EntityList(this DataTable dt, Type type) //where T : IEntityItem
        {
            if (dt == null || type == null)
            {
                throw new ArgumentNullException("EntityList.dt or type");
            }
            List<object> list = new List<object>();

            foreach (DataRow dr in dt.Rows)
            {
                object item = ActivatorUtil.CreateInstance(type);
                EntityPropertyBuilder.SetEntityContext(item, dr);
                list.Add(item);
            }

            return list;
        }
        #endregion

        #region EntityAttribute

        /// <summary>
        /// Get Entity using <see cref="EntityAttribute"/> attribute and <see cref="DataFilter"/> filter. 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CustomAttributeFormatException"></exception>
        public static T ToEntity<T>(this EntityAttribute attribute, DataFilter filter) where T : IEntityItem
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("CreateEntity.attribute");
            }
            var db = attribute.Get();
            if (db == null)
            {
                throw new CustomAttributeFormatException("CreateEntity.attribute.EntityDbContext");
            }
            return db.DoCommand<T>(filter);
        }

        /// <summary>
        /// Get Entity using <see cref="EntityAttribute"/> attribute and keys. 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CustomAttributeFormatException"></exception>
        public static T ToEntity<T>(this EntityAttribute attribute, object[] keys) where T : IEntityItem
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("CreateEntity.attribute");
            }
            var db = attribute.Get();
            if (db == null)
            {
                throw new CustomAttributeFormatException("CreateEntity.attribute.EntityDbContext");
            }
            return db.ToEntity<T>(keys);
        }
        /// <summary>
        /// Get Entity using <see cref="EntityAttribute"/> attribute and keys. 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CustomAttributeFormatException"></exception>
        public static T ToEntity<T>(this EntityAttribute attribute, KeySet keys) where T : IEntityItem
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("CreateEntity.attribute");
            }
            var db = attribute.Get();
            if (db == null)
            {
                throw new CustomAttributeFormatException("CreateEntity.attribute.EntityDbContext");
            }
            return db.ToEntity<T>(keys);
        }
        #endregion

        #region EntityDbContext
        public static T ToEntity<T>(this EntityDbContext db, params object[] keys)
        {

            T instance = ActivatorUtil.CreateInstance<T>();
            using (EntityContext<T> entity = new EntityContext<T>(db, instance))
            {
                //entity.EntityDbContext = db;
                entity.SetEntity(keys);
                return entity.Entity;
            }
        }

 
        public static T EntityFilter<T>(this EntityDbContext db, DataFilter filter)
        {
            T instance = ActivatorUtil.CreateInstance<T>();
            using (EntityContext<T> entity = new EntityContext<T>(db, instance))
            {
                entity.Set(filter);
                return entity.Entity;
            }
        }

        

        /// <summary>
        /// Create Entity collection using Entity Keys
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IList<T> EntityList<T>(this EntityDbContext db, int top = 0) where T : IEntityItem
        {
            if (db.HasConnection)
            {
                DataTable dt = db.ExecuteDataTable(top);
                if (dt == null)
                    return null;
                return dt.EntityList<T>();
            }

            return null;
        }

        /// <summary>
        /// Create Entity collection using <see cref="DataFilter"/> filter with parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IList<T> EntityList<T>(this EntityDbContext db, DataFilter filter) where T : IEntityItem
        {
            if (db.HasConnection)
            {
                DataTable dt = db.DoCommand<DataTable>(filter);
                if (dt == null)
                    return null;
                return dt.EntityList<T>();
            }

            return null;
        }

        /// <summary>
        /// Create Entity collection using command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public static IList<T> EntityList<T>(this EntityDbContext db, string commandText, IDbDataParameter[] parameters, CommandType cmdType) where T : IEntityItem
        {
            if (db.HasConnection)
            {
                DataTable dt = db.DoCommand<DataTable>(commandText, parameters, cmdType);
                if (dt == null)
                    return null;
                return dt.EntityList<T>();
            }

            return null;
        }

        /// <summary>
        /// Create Entity collection using DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static IList<T> EntityList<T>(this EntityDbContext db, DataTable dt) where T : IEntityItem
        {
            if (db.HasConnection)
            {
                if (dt == null)
                    return null;
                return dt.EntityList<T>();
            }

            return null;
        }
        #endregion

    }
}
