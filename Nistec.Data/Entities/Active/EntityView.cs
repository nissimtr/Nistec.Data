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
using System.Collections; 
using System.Reflection;
using System.Data;

using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Nistec.Generic;

namespace Nistec.Data.Entities
{
  

	/// <summary>
    /// EntityView.
	/// </summary>
    [Serializable]
    public class EntityView : EntityTable//, IEntityTable
    {

        #region Dispose

        protected override void DisposeInner(bool disposing)
        {
            if (_DataView != null)
            {
                _DataView.Dispose();
                _DataView = null;
            }
            base.DisposeInner(disposing);
        }

        #endregion

        #region Ctor

        public EntityView()
            : base()
        {

        }

         /// <summary>
        /// ctor
        /// </summary>
        [Obsolete("Use KeySet insted")]
        public EntityView(params object[] keys)
            : base(keys)
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        public EntityView(KeySet keys)
            : base(keys)
        {

        }

        public EntityView(DataTable table):base(table)
		{
        }
               
       
		#endregion

        #region View property

        [NonSerialized]
        DataView _DataView;
        /// <summary>
        /// Get or Set DataSource
        /// </summary>
        public DataView View
        {
            get 
            {
                if (_DataView == null)
                {
                    SetDataView();
                }
                return _DataView; 
            }
        }

        /// <summary>
        /// Get Count
        /// </summary>
        /// <returns>int</returns>
        [EntityProperty(EntityPropertyType.NA)]
        public new int Count
        {
            get
            {
                if (IsEmpty)
                    return 0;
                return View.Count;
            }
        }

        protected override void OnAcceptChanges(EventArgs e)
        {
            base.OnAcceptChanges(e);
            SetDataView();
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);
            SetDataView();
        }

      
        private void SetDataView()
        {
            if (_DataSource != null)
            {
                _DataView = _DataSource.DefaultView;
            }
        }


        [EntityProperty(EntityPropertyType.NA)]
        public object this[int row, string columnName]
        {
            get
            {
                return View[row][columnName];
            }
        }

        [EntityProperty(EntityPropertyType.NA)]
        public object this[int row, int columnIndex]
        {
            get
            {
                return View[row][columnIndex];
            }
        }

  
        #endregion

        #region find

        /// <summary>
        /// Get ActiveView by filterExpression
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sort"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public EntityView Select(string filterExpression, string sort, DataViewRowState state)
        {
            DataView dv = new DataView(EntityDataSource, filterExpression, sort, state);
            EntityView view = new EntityView(dv.Table);
            return view;
        }
        /// <summary>
        /// Get ActiveView by filterExpression
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public EntityView Select(string filterExpression, string sort)
        {
            return Select(filterExpression, sort, DataViewRowState.CurrentRows);
        }
        /// <summary>
        /// Get ActiveView by filterExpression
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public EntityView Select(string filterExpression)
        {
            return Select(filterExpression, "", DataViewRowState.CurrentRows);
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Find(string columnName, object value)
        {
            if (IsEmpty)
                return -1;
            View.Sort = columnName;
            return View.Find(value);
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Find(string columns, object[] values)
        {
            if (IsEmpty)
                return -1;
            View.Sort = columns;
            return View.Find(values);
        }

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Find(params object[] values)
        {
            if (IsEmpty)
                return -1;
            return View.Find(values);
        }
        /// <summary>
        /// CompareRecord
        /// </summary>
        /// <param name="index"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool CompareRecord(int index, string column, object value)
        {
            return View[index][column].Equals(value);
        }


        /// <summary>
        /// MoveTo
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool MoveTo(string columnName, object value)
        {
            int curIndex = Find(columnName, value);
            if (curIndex > -1)
            {
                Position = curIndex;
                return true;
            }
            return false;
        }
        /// <summary>
        /// MoveTo
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public bool MoveTo(string columns, object[] values)
        {
            int curIndex = Find(columns, values);
            if (curIndex > -1)
            {
                Position = curIndex;
                return true;
            }
            return false;
        }

       

        #endregion


	}
}
