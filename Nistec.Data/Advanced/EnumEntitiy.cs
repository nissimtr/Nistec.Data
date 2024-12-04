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
using System.Text;
using System.Data;
#pragma warning disable CS1591
namespace Nistec.Data.Advanced
{
    [Serializable]
    public abstract class EnumEntity<T>
    {
        #region members

        protected string m_document;
        protected EnumEntityFields<T> Fields;


        #endregion
        
        public virtual string LoadEntity(DataRow dr, string document)
        {
            m_document = document;

            Fields = new EnumEntityFields<T>();

            string[] names = Enum.GetNames(typeof(T));

            for (int i = 0; i < names.Length; i++)
            {
                SetRowField(names[i], Types.NZ(dr[names[i]], ""));
            }

            return m_document;
        }

        public virtual void LoadEntity(DataRow dr)
        {

            Fields = new EnumEntityFields<T>();

            string[] names = Enum.GetNames(typeof(T));

            for (int i = 0; i < names.Length; i++)
            {
                T k = (T)Enum.ToObject(typeof(T), i);
                Fields[k] = Types.NZ(dr[names[i]], "");// dr.Field<string>(names[i]);
            }
        }

        void SetRowField(string fieldType, string value)
        {
            Fields[fieldType] = value;
            m_document = m_document.Replace(FieldTemplate(fieldType), value == null ? "" : value.ToString());
        }

        void SetField(string fieldType, string value)
        {
            m_document = m_document.Replace(FieldTemplate(fieldType), value);
        }


        static string FieldTemplate(string fieldType)
        {
            return "#" + fieldType + "#";
        }


    }
    [Serializable]
    public class EnumEntityFields<T> : Dictionary<T, string>
    {

        public new string this[T key]
        {
            get
            {
                if (ContainsKey(key))
                    return base[key];
                return null;
            }
            set
            {
                base[key] = value;
            }
        }

        public string this[string key]
        {
            get
            {
                T k = (T)Enum.Parse(typeof(T), key, true);
                if (ContainsKey(k))
                    return base[k];
                return null;
            }
            set
            {
                T k = (T)Enum.Parse(typeof(T), key, true);

                base[k] = value;
            }
        }
    }

}
