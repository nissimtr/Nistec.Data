using Nistec.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Nistec.Data
{
    /// <summary>
    /// This attribute defines properties of method's properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValAttribute : Attribute
    {

        public static DefaultValAttribute Default
        {
            get { return new DefaultValAttribute(); }
        }

        //public static DefaultValAttribute Create(PropertyInfo p)
        //{
        //    return new DefaultValAttribute() { m_Type = p.PropertyType };
        //}


        /// <summary>
        ///  Null Value Return
        /// </summary>
        public const string NullValueToken = "";//NullValue";

        #region Private members

        private string m_caption = "";
        //Range m_Range;
        //private Type m_Type;
        int m_SizeLimit;
        bool m_ExceptionIfNull;
        private object m_defaultValue;
        private bool m_allowNull;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValAttribute"/> class
        /// </summary>
        public DefaultValAttribute()
        {
            //m_Type = typeof(string);
            //m_Range = Range.Empty;
            m_SizeLimit = 0;
            m_ExceptionIfNull = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValAttribute"/> class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="allowNull"></param>
        /// <param name="defaultValue"></param>
        /// <param name="exceptionIfNull"></param>
        public DefaultValAttribute(bool allowNull, object defaultValue, bool exceptionIfNull=false) 
        {
            m_allowNull = allowNull;
            m_defaultValue = defaultValue;
            m_ExceptionIfNull = exceptionIfNull;
            m_SizeLimit = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValAttribute"/> class
        /// </summary>
        /// <param name="allowNull"></param>
        /// <param name="sizeLimit"></param>
        public DefaultValAttribute(bool allowNull, int sizeLimit)
        {
            m_allowNull = allowNull;
            m_defaultValue = null;
            m_ExceptionIfNull = false;
            m_SizeLimit = sizeLimit;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValAttribute"/> class
        /// </summary>
        /// <param name="sizeLimit"></param>
        public DefaultValAttribute(int sizeLimit)
        {
            m_allowNull = true;
            m_defaultValue = null;
            m_ExceptionIfNull = false;
            m_SizeLimit = sizeLimit;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValAttribute"/> class with the specified arguments.
        /// </summary>
        /// <param name="allowNull"></param>
        /// <param name="defaultValue"></param>
        /// <param name="exceptionIfNull"></param>
        /// <param name="sizeLimit"></param>
        /// <param name="caption"></param>
        public DefaultValAttribute(bool allowNull, object defaultValue,int sizeLimit, bool exceptionIfNull, string caption)
        {
            m_allowNull = allowNull;
            m_defaultValue = defaultValue;
            m_caption = caption;
            m_ExceptionIfNull = exceptionIfNull;
            m_SizeLimit = sizeLimit;
        }

        /// <summary>
        /// An attribute builder method
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static CustomAttributeBuilder GetAttributeBuilder(DefaultValAttribute attr)
        {
            Type[] arrParamTypes = new Type[] {typeof(bool), typeof(object), typeof(int), typeof(bool), typeof(string)};
            object[] arrParamValues = new object[] {  attr.AllowNull, attr.DefaultValue, attr.SizeLimit, attr.ExceptionIfNull, attr.Caption };
            ConstructorInfo ctor = typeof(DefaultValAttribute).GetConstructor(arrParamTypes);
            return new CustomAttributeBuilder(ctor, arrParamValues);
        }


        #endregion

        #region Properties

        /// <summary>
        /// Parameter caption. usefull for UI
        /// If this property is not defined 
        /// then a method parameter Column is used.
        /// </summary>
        public string Caption
        {
            get { return string.IsNullOrEmpty(m_caption) ? "" : m_caption; }
            set { m_caption = value; }
        }

        /// <summary>
        /// This parameter contains a value that will be interpreted as null. 
        /// This is usefull if you want to pass a null to a value type parameter.
        /// </summary>
        public object DefaultValue
        {
            get { return m_defaultValue; }
            set { m_defaultValue = value; }
        }

        /// <summary>
        /// Indicate if parameter allow null value.
        /// </summary>
        public bool AllowNull
        {
            get { return m_allowNull; }
            set { m_allowNull = value; }
        }
        /// <summary>
        /// Get or Set SizeLimit
        /// </summary>
        public int SizeLimit
        {
            get { return m_SizeLimit; }
            set { m_SizeLimit = value; }
        }
        /// <summary>
        /// Get or Set value that Indicating to throw Exception when value is  Null
        /// </summary>
        public bool ExceptionIfNull
        {
            get { return m_ExceptionIfNull; }
            set { m_ExceptionIfNull = value; }
        }
        /// <summary>
        /// Get Caption or property name if Caption not Defined
        /// </summary>
        public string GetCaption(string propertyName)
        {
            return IsCaptionDefined ? Caption : propertyName;
        }

        #endregion

        #region Is defined properties

    
        /// <summary>
        /// Is Caption Defined
        /// </summary>
        public bool IsCaptionDefined
        {
            get { return m_caption != null && m_caption.Length > 0; }
        }

        /// <summary>
        /// Is Size Defined
        /// </summary>
        public bool IsSizeDefined
        {
            get { return m_SizeLimit > 0; }
        }

        #endregion

        public object IsNull(object fieldValue)
        {
            if (Types.IsDbNull(fieldValue))
                return DefaultValue;
            return fieldValue;
        }

        private void EnsureField(PropertyInfo property, object instance, object fieldValue)
        {
           // PropertyInfo property= m_Type.GetRuntimeProperty(m_Type.FullName);

            if (Types.IsNull(fieldValue) || string.IsNullOrEmpty(fieldValue.ToString()))
            {
                if (DefaultValue != null)
                   property.SetValue(instance, DefaultValue, null);
                else if(ExceptionIfNull)
                    throw new ArgumentNullException(string.Format("Field {0} does not allow null", property.Name));
                else if(IsSizeDefined && fieldValue.ToString().Length> SizeLimit)
                    throw new ArgumentNullException(string.Format("Field {0} length exceeds the limit: {1}", property.Name, SizeLimit));
            }
        }

       

        internal void EnsureEntity(object instance)
        {
            if (instance == null)
                return;
            //var validateArgs = DataParameter.ToDictionary<object>(args);
            var props = DataProperties.GetEntityDefaultVal(instance.GetType());

            foreach (var pa in props)
            {

                DefaultValAttribute attr = pa.Attribute;

                if (attr != null)
                {

                    PropertyInfo property = pa.Property;
                    if (!property.CanWrite)
                    {
                        continue;
                    }
                    string field = property.Name;
                    var val = property.GetValue(instance, null);

                    attr.EnsureField(property, instance,val);
                }
            }

        }

        /// <summary>
        /// Validate entity.
        /// </summary>
        /// <param name="instance"></param>
        public static void EnsureProperties(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentException("EntityValidator.Error Invalid Entity");
            }
            DefaultValAttribute validator = new DefaultValAttribute();
            validator.EnsureEntity(instance);
            //if (!validator.IsValid)
            //{
            //    throw new EntityException(validator.Result);
            //}
        }

    }
}
