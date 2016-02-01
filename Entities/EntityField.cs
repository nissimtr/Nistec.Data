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

namespace Nistec.Data.Entities
{


    /// <summary>
    /// This class represent the ActiveProperty for each field in active record class
    /// </summary>
    public class EntityField //:IDisposable
    {

      
        #region ctor

        public EntityField(string fieldName, object value, EntityPropertyAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentException("Invalid EntityPropertyAttribute parameter");
            }
            _FieldName = fieldName;
            Value = value;
            m_attr = attr;
        }

        #endregion

        #region members

        string _FieldName;
        internal readonly object Value;
        EntityPropertyAttribute m_attr;
        public EntityPropertyAttribute Attributes
        {
            get
            {
                return m_attr;
            }
        }
        #endregion

        #region Properties

        public string FieldName
        {
            get
            {
                return _FieldName;
            }
        }

        public string Caption
        {
            get
            {
                if (m_attr == null)
                    return FieldName;
                return m_attr.GetCaption(FieldName);
            }
        }

        public string Column
        {
            get
            {
                if (m_attr == null)
                    return FieldName;
                return m_attr.IsColumnDefined ? m_attr.Column : FieldName;
            }
        }

        public Type FieldType()
        {
            if (m_attr == null && Value != null)
                return Value.GetType();
            if (m_attr.IsTypeDefined)
                return DataParameter.GetTypeFromDbType(m_attr.SqlDbType);
            if (Value != null)
               return Value.GetType();
            return typeof(object);
        }

        public int FieldSize
        {
            get
            {
                if (m_attr != null && m_attr.IsSizeDefined)
                    return m_attr.Size;
               
                return -1;
            }
        }

        public bool AllowNull
        {
            get
            {
                if (m_attr == null)
                    return true;
                return m_attr.AllowNull;
            }
        }

        public int Order
        {
            get
            {
                if (m_attr == null)
                    return 0;
                return m_attr.Order;
            }
        }


        internal string GetCaption(string field)
        {
            if (m_attr == null)
                return field;
            return m_attr.GetCaption(field);
        }

        #endregion

        #region control properties
       

        public string TextValue
        {
            get { return (Value == null || Value==DBNull.Value) ? string.Empty : Value.ToString(); }
        }

        public DataColumn ToDataColumn()
        {
            return new DataColumn(Column, FieldType())
            {
                Caption = FieldName,
                MaxLength = FieldSize,
                AllowDBNull = AllowNull
            };
        }

      
        #endregion

        #region is define properties


        /// <summary>
        /// Is Caption Defined
        /// </summary>
        internal bool IsCaptionDefined
        {
            get { return Caption != null && Caption.Length > 0; }
        }

        #endregion
    }

   
}
