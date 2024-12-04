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

using Nistec.Collections;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Contexts;
#pragma warning disable CS1591
namespace Nistec.Data.Advanced
{
    /// <summary>
    /// LookupView
    /// </summary>
    public class LookupView 
    {
        private string m_Value="value";
        private string m_Key="key";
        private KeysView m_Keys;
        private ValuesView m_Values;
        private bool m_ReadOnly = false;

        /// <summary>
        /// LookupView ctor
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="keyMember"></param>
        /// <param name="valueMember"></param>
        public LookupView(DataTable dt, string keyMember, string valueMember)
            : this(dt, keyMember, valueMember, false)
        {
        }

        /// <summary>
        /// LookupView ctor
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="keyMember"></param>
        /// <param name="valueMember"></param>
        /// <param name="valueSorted"></param>
        public LookupView(DataTable dt, string keyMember, string valueMember, bool valueSorted)
        {
            m_Value = valueMember;
            m_Key = keyMember;
            InitLookupView(dt, valueSorted);
        }
        /// <summary>
        /// LookupView ctor
        /// </summary>
        /// <param name="list"></param>
        public LookupView(string[] list)
        {
            DataTable dt = CreateDataView(list);
            InitLookupView(dt, false);
        }
        /// <summary>
        /// LookupView ctor
        /// </summary>
        /// <param name="list"></param>
        public LookupView(object[,] list)
        {
            DataTable dt = CreateDataView(list);
            InitLookupView(dt, false);
        }
        /// <summary>
        /// LookupView ctor
        /// </summary>
        /// <param name="list"></param>
        public LookupView(Dictionary<object,object> list)
        {
            DataTable dt = CreateDataView(list);
            InitLookupView(dt, true);
        }

        internal void InitLookupView(DataTable dt, bool sorted)
        {
            m_Keys = new KeysView(dt, this);
            m_Values = new ValuesView(dt, this, sorted);
        }

        private DataTable CreateDataView(params string[] list)
        {
            DataTable dt = DataUtil.CreateTableSchema("LookupView", new string[] {"key","value" });
            DataUtil.FillDataTable(dt, list,true);
            return dt;
        }

        private DataTable CreateDataView(object[,] list)
        {
            DataTable dt = DataUtil.CreateTableSchema("LookupView", new string[] { "key", "value" });
            DataUtil.FillDataTable(dt, list);
            return dt;
        }

        private DataTable CreateDataView(Dictionary<object, object> list)
        {
            DataTable dt = DataUtil.CreateTableSchema("LookupView", new string[] { "key", "value" });
            DataUtil.FillDataTable(dt, list);
            return dt;
        }

        /// <summary>
        /// KeysView
        /// </summary>
        public class KeysView
        {
            internal DataView m_View;
            internal LookupView owner;
            private bool m_Sorted;

            /// <summary>
            /// KeysView
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="lv"></param>
            public KeysView(DataTable dt, LookupView lv)
            {
                owner = lv;
                m_Sorted = false;
                m_View = new DataView (dt);
                SetSorted();
            }
            /// <summary>
            /// Find
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public int Find(object key)
            {
                return m_View.Find(key);
            }

            /// <summary>
            /// Get Item
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public object this[object key]
            {
                get
                {
                    int index = Find(key);
                    if (index == -1)
                        return null;
                    return m_View[index][owner.ValueMember];
                }
            }

            /// <summary>
            /// GetValue
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public string GetValue(object key)
            {
                object o = this[key];
                return o == null ? "" : o.ToString();
            }

            /// <summary>
            /// GetDataRow
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public DataRowView GetDataRow(object key)
            {
                int index = Find(key);
                if (index == -1)
                    return null;
                return m_View[index];
            }

            internal void SetReadOnly(bool value)
            {
                m_View.AllowDelete = value;
                m_View.AllowEdit = value;
                m_View.AllowNew = value;
            }

            internal void SetSorted()
            {
                if (!m_Sorted)
                {
                    AsyncSort.Start(m_View, owner.KeyMember);
                    m_Sorted = true;
                }
            }
            /// <summary>
            /// Get or Set if is Sorted
            /// </summary>
            public bool Sorted
            {
                get
                {
                    return m_Sorted;
                }
                set
                {
                    SetSorted();
                }
            }
        }
        /// <summary>
        /// ValuesView
        /// </summary>
        public class ValuesView
        {
            internal LookupView owner;
            private DataView m_View;
            private bool m_Sorted;

            /// <summary>
            /// ValuesView
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="lv"></param>
            /// <param name="sorted"></param>
            public ValuesView(DataTable dt, LookupView lv, bool sorted)
            {
                owner = lv;
                m_View = new DataView(dt);
                m_Sorted = false;
                if (sorted)
                {
                    SetSorted();
                }
            }

            internal DataView View
            {
                get
                {
                    if (!m_Sorted)
                    {
                        m_View.Sort = owner.ValueMember;
                        m_Sorted = true;
                    }
                    return m_View;
                }
            }
        
            /// <summary>
            /// Find
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public int Find(object value)
            {
                return m_View.Find(value);
            }

            /// <summary>
            /// Get Item
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public object this[object value]
            {
                get
                {
                    int index = Find(value);
                    if (index == -1)
                        return null;
                    return m_View[index][owner.KeyMember];
                }
            }
            /// <summary>
            /// GetKey
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public string GetKey(object value)
            {
                object o = this[value];
                return o == null ? "" : o.ToString();
            }

            /// <summary>
            /// GetDataRow
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public DataRowView GetDataRow(object value)
            {
                int index = Find(value);
                if (index == -1)
                    return null;
                return m_View[index];
            }

            internal void SetReadOnly(bool value)
            {
                m_View.AllowDelete = value;
                m_View.AllowEdit = value;
                m_View.AllowNew = value;
            }

            internal void SetSorted()
            {
                if (!m_Sorted)
                {
                    AsyncSort.Start(m_View, owner.ValueMember);
                    m_Sorted = true;
                }
            }
            /// <summary>
            /// Get or Set if is Sorted
            /// </summary>
            public bool Sorted
            {
                get
                {
                    return m_Sorted;
                }
                set
                {
                    SetSorted();
                }
            }
        }

        #region View property
        /// <summary>
        /// Get Values
        /// </summary>
        public ValuesView Values
        {
            get
            {
                return m_Values;
            }
        }
        /// <summary>
        /// Get Keys
        /// </summary>
         public KeysView Keys
        {
            get
            {
                return m_Keys;
            }
        }

        /// <summary>
        /// Get DataView by key
        /// </summary>
        public DataView View
        {
            get
            {
                return m_Keys.m_View;
            }
        }

        /// <summary>
        /// Get Count
        /// </summary>
        public int Count
        {
            get
            {
                if (m_Keys == null)
                {
                    return -1;
                }
                return m_Keys.m_View.Count;
            }
        }
        /// <summary>
        /// Get Initilaized
        /// </summary>
        public bool Initilaized
        {
            get
            {
                return (m_Keys != null);
            }
        }
        /// <summary>
        /// Get ValueMember
        /// </summary>
        public string ValueMember
        {
            get
            {
                return m_Value;
            }
        }
        /// <summary>
        /// Get KeyMember
        /// </summary>
        public string KeyMember
        {
            get
            {
                return m_Key;
            }
        }
        /// <summary>
        /// Get or Set ReadOnly
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return m_ReadOnly ;
            }
            set
            {
                m_Keys.SetReadOnly(value);
                m_Values.SetReadOnly(value);
                m_ReadOnly = value;
            }
        }

        /// <summary>
        /// LookupSearch
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public DataRowView LookupSearch(string text)
        {
            try
            {
                if(string.IsNullOrEmpty(text))
                    return null;

                string sText = text.ToUpper();
                int iLength = sText.Length;
                int index = 0;
                int count = Count;
                DataView dv = m_Keys.m_View;
                string key=this.KeyMember;

                //see if what is currently in the text box matches anything in the string list
                for (index = 0; index < count; index++)
                {
                    string sTemp = dv[index][key].ToString().ToUpper();
                    if (sTemp.Length >= sText.Length)
                    {
                        if (sTemp.IndexOf(sText, 0, sText.Length) > -1)
                        {
                            return dv[index];
                        }
                    }
                }

                return null;
 
            }
            catch //(Exception)// ex)
            {
                return null;  //throw new Exception(ex.Message + "\r\nIn CtlLookUp.OnTextChanged(EventArgs).");
            }
        }


        #endregion

        public static LookupView Instance(DataTable dt, string keyMember, string valueMember)
        {
            return new LookupView(dt, keyMember, valueMember);
        }

        #region LookupThreadSort

        internal class LookupThreadSort
        {

            internal static void Start(System.Data.DataView dv, string col)
            {
                m_dv = dv;
                m_col = col;

                ThreadStart myThreadDelegate = new ThreadStart(ThreadWork.DoWork);
                Thread myThread = new Thread(myThreadDelegate);
                myThread.Start();
                Thread.Sleep(0);
                Console.WriteLine("In main. Attempting to restart myThread.");
                try
                {
                    myThread.Start();
                }
                catch (ThreadStateException e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }
            }

            internal static System.Data.DataView m_dv;
            internal static string m_col;

            internal class ThreadWork
            {

                internal static void DoWork()
                {
                    Console.WriteLine("Working thread...");

                    lock (LookupThreadSort.m_dv)
                    {
                        LookupThreadSort.m_dv.Sort = LookupThreadSort.m_col;
                    }
                }
            }

        }

        #endregion


    }

    #region AsyncSort


    /// <summary>
    /// SortSyncronized
    /// </summary>
    [Synchronization()]
    public class SortSyncronized : ContextBoundObject
    {
        // A method that does some work - returns the square of the given number
        public string Sort(System.Data.DataView dv, string colView)
        {
            Console.Write("Syncronized.Sort called.  ");
            Console.WriteLine("The hash of the current thread is: {0}", Thread.CurrentThread.GetHashCode());
            return dv.Sort = colView;
        }
    }

    //
    // Async delegate used to call a method with this signature asynchronously
    //
    public delegate string SortSyncDelegate(System.Data.DataView dv, string colView);

    /// <summary>
    /// AsyncSort
    /// </summary>
    public class AsyncSort
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dv"></param>
        /// <param name="colView"></param>
        public static void Start(System.Data.DataView dv, string colView)
        {
            string callResult = "";
            //Create an instance of a context-bound type SampleSynchronized
            //Because SampleSynchronized is context-bound, the object sampSyncObj 
            //is a transparent proxy
            SortSyncronized sortSyncObj = new SortSyncronized();


            //call the method asynchronously
            Console.Write("Making an asynchronous call on the object.  ");
            Console.WriteLine("The hash of the current thread is: {0}", Thread.CurrentThread.GetHashCode());
            SortSyncDelegate sortDelegate = new SortSyncDelegate(sortSyncObj.Sort);
            //callParameter = 17;

            IAsyncResult aResult = sortDelegate.BeginInvoke(dv, colView, null, null);

            //Wait for the call to complete
            aResult.AsyncWaitHandle.WaitOne();

            callResult = sortDelegate.EndInvoke(aResult);
            //Console.WriteLine("Result of calling sampSyncObj.Square with {0} is {1}.", callParameter, callResult);
        }

    }

    #endregion


}
