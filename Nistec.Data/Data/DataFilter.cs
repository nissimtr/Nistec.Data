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
using System.Data;
using Nistec.Data.Entities;
using System.Data.SqlClient;

namespace Nistec.Data
{
    public class DataFilter
    {

        public string Filter { get; set; }
        public IDbDataParameter[] Parameters { get; private set; }

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(Filter) || Parameters==null; }
        }

        public static DataFilter Empty
        {
            get
            {
                return new DataFilter();
            }
        }

        public DataFilter()
        {
           
        }


        private DataFilter(bool isSql, string filter, object[] values)
        {
            this.Filter = filter;
            if (isSql)
                this.Parameters = CreateSqlFilter(filter, values);
            else
                this.Parameters = CreateFilter(filter, values);
        }

        public DataFilter(string filter, object[] values)
        {
            this.Filter = filter;
            this.Parameters = CreateFilter(filter, values);
        }

        public DataFilter(params object[] keyValueParameters)
        {
            string filter="";
            this.Parameters = CreateParameters(keyValueParameters,out filter);
            Filter = filter;
        }
        public DataFilter(IDictionary<string, object> dic)
        {
            string filter = "";
            this.Parameters = CreateParameters(dic, out filter);
            Filter = filter;
        }

        public EntityKeys GetKeys()
        {
            if (Parameters != null)
            {
                string[] pNames = Parameters.Select(p => p.ParameterName).ToArray();
                return EntityKeys.Get(pNames);
            }
            return new EntityKeys();
        }

        public string Select(string mappingName)
        {
            if (string.IsNullOrEmpty(Filter))
                return string.Format("select * from [{0}]", mappingName);
            return string.Format("select * from [{0}] where {1}", mappingName, Filter);

        }

        public static DataFilter Get(string filter, params object[] values)
        {
            if (string.IsNullOrEmpty(filter))
                return null;
            return new DataFilter(filter, values);
        }

        public static DataFilter GetSql(string filter, params object[] values)
        {
            if (string.IsNullOrEmpty(filter))
                return null;
            return new DataFilter(true, filter, values);
        }

        public static DataParameter[] CreateFilter(string filter, params object[] values)
        {
            if (values == null || string.IsNullOrEmpty(filter))
                return null;
            DataParameter[] parameters = null;
            try
            {
                string[] parm = filter.Split('@');

                if (parm == null || parm.Length <= 0)
                {
                    throw new Exception("Wrong parameter definition");
                }
                int length = (int)(parm.Length - 1);

                if (length != values.Length)
                {
                    throw new Exception("Wrong parameter definition");
                }
                
                parameters = new DataParameter[length];
                char[] sap = new char[] { ')', ' ', ';' };
                string s = null;
                for (int i = 0; i < length; i++)
                {
                    s = parm[i + 1].TrimStart();
                    int isap = s.IndexOfAny(sap);
                    if (isap < 0)
                        isap = s.Length;
                    string name = s.Substring(0, isap);
                    parameters[i] = new DataParameter(name, values[i]);
                }

                return parameters;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static SqlParameter[] CreateSqlFilter(string filter, params object[] values)
        {
            if (values == null || string.IsNullOrEmpty(filter))
                return null;
            SqlParameter[] parameters = null;
            try
            {
                string[] parm = filter.Split('@');

                if (parm == null || parm.Length <= 0)
                {
                    throw new Exception("Wrong parameter definition");
                }
                int length = (int)(parm.Length - 1);

               
                if (length != values.Length)
                {
                    throw new Exception("Wrong parameter definition");
                }
                
                parameters = new SqlParameter[length];
                char[] sap = new char[] { ')', ' ', ';' };
                string s = null;
                for (int i = 0; i < length; i++)
                {
                    s = parm[i + 1].TrimStart();
                    int isap = s.IndexOfAny(sap);
                    if (isap < 0)
                        isap = s.Length;
                    string name = s.Substring(0, isap);
                    parameters[i] = new SqlParameter(name, values[i]);
                }
                return parameters;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static DataParameter[] CreateParameters(params object[] keyValueParameters)
        {
            if (keyValueParameters == null)
                return null;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<DataParameter> list = new List<DataParameter>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static SqlParameter[] CreateSqlParameters(params object[] keyValueParameters)
        {
            if (keyValueParameters == null)
                return null;

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<SqlParameter> list = new List<SqlParameter>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new SqlParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static DataParameter[] CreateParameters(object[] keyValueParameters, out string filter)
        {
            if (keyValueParameters == null)
            {
                filter = null;
                return null;
            }
            StringBuilder sb = new StringBuilder();

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            List<DataParameter> list = new List<DataParameter>();
            for (int i = 0; i < count; i++)
            {
                sb.AppendFormat("{0}=@{0} and ", keyValueParameters[i]);
                list.Add(new DataParameter(keyValueParameters[i].ToString(), keyValueParameters[++i]));
            }
            sb.Remove(sb.Length - 5, 5);
            filter = sb.ToString();
            return list.ToArray();
        }


        /// <summary>
        /// Create KeyValueParameters
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static DataParameter[] CreateParameters(IDictionary<string,object> dic, out string filter)
        {
            if (dic == null)
            {
                filter = null;
                return null;
            }
            StringBuilder sb = new StringBuilder();
            List<DataParameter> list = new List<DataParameter>();
            foreach (var p in dic)
            {
                sb.AppendFormat("{0}=@{0} and ", p.Key);
                list.Add(new DataParameter(p.Key, p.Value));
            }
            sb.Remove(sb.Length - 5, 5);
            filter = sb.ToString();
            return list.ToArray();
        }

    }
}
