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
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nistec.Data
{
    public class DataProperties
    {
        static readonly ConcurrentDictionary<string, Dictionary<string, FieldInfo>> _fieldCache = new ConcurrentDictionary<string, Dictionary<string, FieldInfo>>();
        static readonly ConcurrentDictionary<string, Dictionary<string, PropertyInfo>> _propertyCache = new ConcurrentDictionary<string, Dictionary<string, PropertyInfo>>();
        static readonly ConcurrentDictionary<string, Dictionary<string, PropertyAttributeInfo<EntityPropertyAttribute>>> _propertyEntityCache = new ConcurrentDictionary<string, Dictionary<string, PropertyAttributeInfo<EntityPropertyAttribute>>>();
        static readonly ConcurrentDictionary<string, Dictionary<string, PropertyAttributeInfo<ValidatorAttribute>>> _validatorEntityCache = new ConcurrentDictionary<string, Dictionary<string, PropertyAttributeInfo<ValidatorAttribute>>>();

        public static T Copy<T>(T current)
        {

            PropertyInfo[] props=GetProperties(typeof(T));

            T copy= ActivatorUtil.CreateInstance<T>();

            foreach (var property in props)
            {

                if (!property.CanWrite)
                    continue;

                string field = property.Name;

                object fieldValue = property.GetValue(current, null);

                property.SetValue(copy, fieldValue, null);

            }
            return copy;
        }

        public static PropertyAttributeInfo<EntityPropertyAttribute> GetEntityProperty(Type type, EntityPropertyType attributeType)
        {
            var props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance,false)
                        let attr = p.GetCustomAttributes(typeof(EntityPropertyAttribute), true)
                        where attr.Length == 1 && ((EntityPropertyAttribute)attr[0]).ParameterType == attributeType
                        select new PropertyAttributeInfo<EntityPropertyAttribute>() { Property = p, Attribute = (EntityPropertyAttribute)attr.First() };
            return props.FirstOrDefault();
        }
        
        public static IEnumerable<PropertyAttributeInfo<EntityPropertyAttribute>> GetEntityProperties(Type type, bool ignorePropertyAttribute = true)
        {
            string typename = type.FullName;

            Dictionary<string, PropertyAttributeInfo<EntityPropertyAttribute>> td = null;
            if (_propertyEntityCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                IEnumerable<PropertyAttributeInfo<EntityPropertyAttribute>> props = null;
                if (ignorePropertyAttribute)
                {
                    props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance, false)
                            let attr = p.GetCustomAttributes(typeof(EntityPropertyAttribute), true)
                            //where attr.Length == 1
                            select new PropertyAttributeInfo<EntityPropertyAttribute>() { Property = p, Attribute = attr.Length < 1 ? EntityPropertyAttribute.Default : (EntityPropertyAttribute)attr.FirstOrDefault() };
                }
                else
                {
                    props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance, false)
                            let attr = p.GetCustomAttributes(typeof(EntityPropertyAttribute), true)
                            where attr.Length == 1
                            select new PropertyAttributeInfo<EntityPropertyAttribute>() { Property = p, Attribute = (EntityPropertyAttribute)attr.First() };
                    //props=props.OrderBy(p => p.Attribute.Order);
                }
                td = new Dictionary<string, PropertyAttributeInfo<EntityPropertyAttribute>>();
               
                foreach (var p in props)//.OrderBy(p=> p.Attribute.Order))
                {
                    td.Add(p.Property.Name, p);
                }
                if (td.Count > 0)
                    _propertyEntityCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }

        public static IEnumerable<PropertyAttributeInfo<ValidatorAttribute>> GetEntityValidator(Type type, bool ignorePropertyAttribute = true)
        {
            string typename = type.FullName;

            Dictionary<string, PropertyAttributeInfo<ValidatorAttribute>> td = null;
            if (_validatorEntityCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                IEnumerable<PropertyAttributeInfo<ValidatorAttribute>> props = null;
                if (ignorePropertyAttribute)
                    props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance, false)
                            let attr = p.GetCustomAttributes(typeof(ValidatorAttribute), true)
                            //where attr.Length == 1
                            select new PropertyAttributeInfo<ValidatorAttribute>() { Property = p, Attribute = attr == null ? default(ValidatorAttribute) : (ValidatorAttribute)attr.FirstOrDefault() };
                else
                    props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance, false)
                            let attr = p.GetCustomAttributes(typeof(ValidatorAttribute), true)
                            where attr.Length == 1
                            select new PropertyAttributeInfo<ValidatorAttribute>() { Property = p, Attribute = (ValidatorAttribute)attr.First() };

                td = new Dictionary<string, PropertyAttributeInfo<ValidatorAttribute>>();
                foreach (var p in props)
                {
                    td.Add(p.Property.Name, p);
                }
                if (td.Count > 0)
                    _validatorEntityCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }

        public static PropertyInfo[] GetProperties(Type type)
        {
            string typename = type.FullName;

            Dictionary<string, PropertyInfo> td = null;
            if (_propertyCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                td = new Dictionary<string, PropertyInfo>();
                PropertyInfo[] pr = type.GetProperties(BindingFlags.Public | BindingFlags.Instance, false);
                foreach (PropertyInfo p in pr)
                {
                    td.Add(p.Name, p);
                }
                if (td.Count > 0)
                    _propertyCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }

        public static FieldInfo[] GetFields(Type type)
        {
            string typename = type.FullName;
            Dictionary<string, FieldInfo> td = null;
            if (_fieldCache.TryGetValue(typename, out td))
            {
                return td.Values.ToArray();
            }
            else
            {
                td = new Dictionary<string, FieldInfo>();
                FieldInfo[] fi = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo f in fi)
                {
                    td.Add(f.Name, f);
                }
                if (td.Count > 0)
                    _fieldCache.TryAdd(typename, td);
                return td.Values.ToArray();
            }
        }

        public static void ClearCache()
        {
            _propertyEntityCache.Clear();
            _fieldCache.Clear();
            _propertyCache.Clear();
        }
    }
}
