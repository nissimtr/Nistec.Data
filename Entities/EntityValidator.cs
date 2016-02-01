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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Nistec.Data.Entities
{

     public class EntityDictionary
    {
        static Dictionary<string, string> dic;

        internal static Dictionary<string, string> RM
        {
            get
            {
                if (dic == null)
                {
                    dic = new Dictionary<string, string>();
                }
                return dic;
            }
        }

        internal static void Add(string name, string defaultLang, string langsKeyValue)
        {
            string mkey = string.Format("{0}:{1}", defaultLang, name);
            string mvalue = "";
            if (RM.TryGetValue(mkey, out mvalue))
            {
                return;
            }

            string[] args = langsKeyValue.Split('|');
            foreach (string a in args)
            {
                string[] names = a.Split(':');
                if (names.Length < 2)
                    continue;
                string key = string.Format("{0}:{1}", names[0], name);
                string value = names[1];
                if (!RM.TryGetValue(key, out value))
                {
                    RM[key] = value;
                }
            }
        }

        public static string Get(string name, string lang)
        {
            string value = name;
            string key = string.Format("{0}:{1}", lang, name);
            if (RM.TryGetValue(key, out value))
            {
                return value;
            }
            return name;
        }

        public static bool TryGet(string name, string lang, out string value)
        {
            string key = string.Format("{0}:{1}", lang, name);
            return RM.TryGetValue(key, out value);
        }

    }

    /// <summary>
    /// This attribute defines properties of method's properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidatorAttribute : Attribute
    {

        #region Private members

        private string m_name = "";
        private string m_lang = "";
        private string m_defaultLang = "en";
        bool m_Required = false;
        private object m_MinValue;
        private object m_MaxValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAttribute"/> class
        /// </summary>
        public ValidatorAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAttribute"/> class
        /// </summary>
        /// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="required">Is required property</param>
        public ValidatorAttribute(string name, bool required)
        {
            Name = name;
            Required = required;
        }

         
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAttribute"/> class with the specified arguments.
        /// </summary>
        /// <param name="name">Is a value of <see cref="Name"/> property</param>
        /// <param name="required">Is required property</param>
        /// <param name="minValue">min value property</param>
        /// <param name="maxValue">max value property</param>
        public ValidatorAttribute(string name, bool required,object minValue, object maxValue)
        {
            Name = name;
            Required = required;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <summary>
        /// An attribute builder method
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static CustomAttributeBuilder GetAttributeBuilder(ValidatorAttribute attr)
        {
            string name = attr.m_name;
            Type[] arrParamTypes = new Type[] { typeof(string), typeof(bool), typeof(object), typeof(object) };
            object[] arrParamValues = new object[] { name, attr.Required, attr.MinValue, attr.MaxValue};
            ConstructorInfo ctor = typeof(ValidatorAttribute).GetConstructor(arrParamTypes);
            return new CustomAttributeBuilder(ctor, arrParamValues);
        }
       #endregion

        #region Properties

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name
        {
            get { return m_name == null ? string.Empty : m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Parameter langs.
        /// </summary>
        public string Langs
        {
            get { return m_lang == null ? string.Empty : m_lang; }
            set { m_lang = value; }
        }

        /// <summary>
        /// Parameter default lang.
        /// </summary>
        public string DefaultLang
        {
            get { return m_defaultLang == null ? "en" : m_defaultLang; }
            set { m_defaultLang = value; }
        }

        /// <summary>
        /// Indicate if parameter is required.
        /// </summary>
        public bool Required
        {
            get { return m_Required; }
            set { m_Required = value; }
        }

        /// <summary>
        /// Indicate the parameter min value.
        /// </summary>
        public object MinValue
        {
            get { return m_MinValue; }
            set { m_MinValue = value; }
        }

        /// <summary>
        /// Indicate the parameter max value.
        /// </summary>
        public object MaxValue
        {
            get { return m_MaxValue; }
            set { m_MaxValue = value; }
        }

        #endregion

        #region Is defined properties
        
        /// <summary>
        /// Is Name Defined
        /// </summary>
        public bool IsNameDefined
        {
            get { return m_name != null && m_name.Length > 0; }
        }

        /// <summary>
        /// Is Langs Defined
        /// </summary>
        public bool IsLangsDefined
        {
            get { return m_lang != null && m_lang.Length > 0; }
        }

        /// <summary>
        /// Is MinValue Defined
        /// </summary>
        public bool IsMinValueDefined
        /// </summary>ameDefined
        {
            get { return m_MinValue != null; }
        }

        /// <summary>
        /// Is MaxValue Defined
        /// </summary>
        public bool IsMaxValueDefined
        {
            get { return m_MinValue != null; }
        }

        /// <summary>
        /// Is MaxValue Defined
        /// </summary>
        public bool IsRangeDefined
        {
            get { return m_MinValue != null && m_MaxValue!=null; }
        }

        #endregion

        public string GetName(string lang)
        {
            if (!IsLangsDefined)
                return Name;
            EntityDictionary.Add(Name,DefaultLang, Langs);

            return EntityDictionary.Get(Name, lang);
        }

    }

    public class EntityResults
    {
        public int EffectiveRecords { get; set; }

        public bool IsValid { get; set; }

        public string Result { get; set; }

    }

    public class EntityValidator
    {

        public string Title { get; set; }
        public string Lang { get; private set; }

        public string RequieredFormat { get; set; }
        public string RangeFormat { get; set; }
        public string CrLf { get; set; }

        
        public bool IsValid 
        {
            get { return Result.Length == 0; }
        }

        public string Result 
        {
            get { return sb.ToString(); }
        }


        StringBuilder sb = new StringBuilder();

        public EntityValidator()
        {
            CrLf = "";
        }

        public EntityValidator(string title, string lang = "en")
        {
            Lang = lang;
            switch (lang)
            {
                case "he":
                    RequieredFormat = "חובה לציין {0}";
                    RangeFormat = "מחוץ לטווח {0}";
                    break;
                default:
                    RequieredFormat = "{0} Is Required.";
                    RangeFormat = "{0} Is out of range.";
                    break;
            }
            CrLf = "\r\n";
            Title = title;
        }

        public void Append(string message)
        {
            sb.Append(message + CrLf);
        }

        public void Required(string value, string field)
        {
            if (Types.IsEmpty(value))
                sb.AppendFormat(RequieredFormat+ CrLf,field);
        }

        public void Required<T>(T value, string field)
        {
            if (Types.IsEmpty(value))
                sb.AppendFormat(RequieredFormat + CrLf, field);
        }

        public void Required(string value, ValidatorAttribute attr)
        {
            if (Types.IsEmpty(value))
                sb.AppendFormat(RequieredFormat + CrLf, attr.GetName(Lang));
        }

        public void Required<T>(T value, ValidatorAttribute attr)
        {
            if (Types.IsEmpty(value))
                sb.AppendFormat(RequieredFormat + CrLf, attr.GetName(Lang));
        }

        public void Validate(string value, string message)
        {
            if (Types.IsEmpty(value))
                sb.AppendFormat(message + CrLf);
        }

        public void Validate(int value, string message)
        {
            if (Types.IsEmpty(value))
                sb.AppendFormat(message + CrLf);
        }
        public void Validate(string value, int min, int max, string message)
        {
            if (Types.IsEmpty(value) || value.Length < min || value.Length > max)
                sb.AppendFormat(message + CrLf);
        }

        public void Validate(DateTime value, string message)
        {
            if (value < value.MinSqlValue())
                sb.AppendFormat(message + CrLf);
        }

        public void ValidateField(int value, int min, int max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(long value, long min, long max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(Int16 value, Int16 min, Int16 max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(byte value, byte min, byte max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(decimal value, decimal min, decimal max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(float value, float min, float max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }
        public void ValidateField(double value, double min, double max, string field)
        {
            if (Types.IsEmpty(value) || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }

        public void ValidateField(string value, int min, int max, string field)
        {
            if (Types.IsEmpty(value) || value.Length < min || value.Length > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }

        public void ValidateField(DateTime value, DateTime min, DateTime max, string field)
        {
            if (value < value.MinSqlValue() || value < min || value > max)
                sb.AppendFormat(RangeFormat + CrLf, field);
        }

        public void ValidateRange(object value, object min, object max, string field)
        {
            if (value == null || min == null || max == null)
                return;
            Type type = value.GetType();
            if (type == typeof(DateTime))
                ValidateField((DateTime)value, (DateTime)min, (DateTime)max, field);
            if (type == typeof(int))
                ValidateField((int)value, (int)min, (int)max, field);
            if (type == typeof(Int16))
                ValidateField((Int16)value, (Int16)min, (Int16)max, field);
            if (type == typeof(long))
                ValidateField((long)value, (long)min, (long)max, field);
            if (type == typeof(decimal))
                ValidateField((decimal)value, (decimal)min, (decimal)max, field);
            if (type == typeof(float))
                ValidateField((float)value, (float)min, (float)max, field);
            if (type == typeof(double))
                ValidateField((double)value, (double)min, (double)max, field);
            if (type == typeof(byte))
                ValidateField((byte)value, (byte)min, (byte)max, field);
            if (type == typeof(string))
                ValidateField((string)value, (int)min, (int)max, field);
        }

        public void ValidateRange(object value, object min, object max, ValidatorAttribute attr)
        {
            if (value == null || min == null || max == null)
                return;
            string field= attr.GetName(Lang);
            Type type = value.GetType();
            if (type == typeof(DateTime))
                ValidateField((DateTime)value, (DateTime)min, (DateTime)max, field);
            if (type == typeof(int))
                ValidateField((int)value, (int)min, (int)max, field);
            if (type == typeof(Int16))
                ValidateField((Int16)value, (Int16)min, (Int16)max, field);
            if (type == typeof(long))
                ValidateField((long)value, (long)min, (long)max, field);
            if (type == typeof(decimal))
                ValidateField((decimal)value, (decimal)min, (decimal)max, field);
            if (type == typeof(float))
                ValidateField((float)value, (float)min, (float)max, field);
            if (type == typeof(double))
                ValidateField((double)value, (double)min, (double)max, field);
            if (type == typeof(byte))
                ValidateField((byte)value, (byte)min, (byte)max, field);
            if (type == typeof(string))
                ValidateField((string)value, (int)min, (int)max, field);
        }

        public void ValidateEntity(object entity)
        {
            if (entity == null)
                return;

            var props = DataProperties.GetEntityValidator(entity.GetType());

            foreach (var pa in props)
            {

                ValidatorAttribute attr = pa.Attribute;

                if (attr != null)
                {

                    PropertyInfo property = pa.Property;
                    if (!property.CanWrite)
                    {
                        continue;
                    }
                    string field = property.Name;
                    var val = property.GetValue(entity, null);

                    if (!attr.IsNameDefined)
                        attr.Name = field;
                    if (attr.Required)
                    {
                        this.Required(val, attr);
                    }
                    if (attr.IsRangeDefined)
                    {
                        this.ValidateRange(val, attr.MinValue, attr.MaxValue, attr);
                    }
                }
            }
            
        }

        /// <summary>
        /// Validate entity.
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="title"></param>
        /// <param name="lang"></param>
        /// <exception cref="EntityException"></exception>
        public static void Validate(object Entity, string title, string lang)
        {
            EntityValidator validator = new EntityValidator(title, lang);
            validator.ValidateEntity(Entity);
            if (!validator.IsValid)
            {
                throw new EntityException(validator.Result);
            }
        }

        public static EntityValidator ValidateEntity(object Entity, string title, string lang)
        {
            EntityValidator validator = new EntityValidator(title, lang);
            validator.ValidateEntity(Entity);
            return validator;
        }

    }
}
