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

namespace Nistec.Data
{
    public class SqlFormatter
    {
        public static CommandType GetCommandType(string cmdText)
        {
          return  Regx.RegexValidate(@"^(\s|)(select|update|insert|exec)\s", cmdText, System.Text.RegularExpressions.RegexOptions.IgnoreCase)? CommandType.Text: CommandType.StoredProcedure;
        }

        public static string TableFormat(string Table)
        {
            return string.Format("[{0}]", Table.Replace("[", "").Replace("]", "").Replace(".", "].["));
        }

        public static string FromString(string From)
        {
            return string.Format("FROM [{0}]", From.Replace("[", "").Replace("]", "").Replace(".", "].["));
        }

        public static string FromString(string From, bool withNolock)
        {
            if (withNolock)
                return string.Format("FROM [{0}] with(nolock)", From.Replace("[", "").Replace("]", "").Replace(".", "].["));
            else
                return string.Format("FROM [{0}]", From.Replace("[", "").Replace("]", "").Replace(".", "].["));
        }

        public static string WhereString(string Where)
        {
            return string.Format(" WHERE ({0})", Where);
        }


        public static string SelectString(string From, bool withNolock)
        {
            return string.Format("SELECT * {0}", FromString(From, withNolock));
        }

        public static string SelectString(string From)
        {
            return string.Format("SELECT * {0}", FromString(From));
        }

        public static string SelectString(int top, string From)
        {
            return string.Format("SELECT TOP {0} * {1}", top, FromString(From));
        }

        public static string SelectString(string Select, string From, string Where, bool withNolock)
        {
            if (string.IsNullOrEmpty(Where))
                return string.Format("SELECT {0} {1}", Select, FromString(From, withNolock));
            else
                return string.Format("SELECT {0} {1} Where {2}", Select, FromString(From, withNolock), Where);
        }


        public static string SelectString(string Select, string From, string Where)
        {
            return SelectString(Select, From, Where, false);
        }

        public static string SelectString(int top, string Select, string From, string Where)
        {
            return SelectString(top, Select, From, Where, false);
        }

        public static string SelectString(int top, string Select, string From, string Where, bool withNolock)
        {
            if (string.IsNullOrEmpty(Where))
            {
                if (top > 0)
                    return string.Format("SELECT TOP {0} {1} {2}", top, Select, FromString(From, withNolock));
                else
                    return string.Format("SELECT {0} {1}", Select, FromString(From, withNolock));
            }
            else if (top > 0)
                return string.Format("SELECT TOP {0} {1} {2} Where {3}", top, Select, FromString(From, withNolock), Where);
            else
                return string.Format("SELECT {0} {1} Where {2}", Select, FromString(From, withNolock), Where);
        }

        public static string InsertOrUpdateString(string TableName, string Insert, string Values,string Update,string Where)
        {
            string insertString = SqlFormatter.InsertString(TableName, Insert, Values);

            string updateString = SqlFormatter.UpdateString(TableName, Update, Where);

            return string.Format("if not exists(select 1 from {0} where {1}) begin {2} end else begin {3} end", TableName, Where, insertString, updateString);
        }
        public static string InsertNotExistsString(string TableName, string Insert, string Values, string Where)
        {
            string insertString = SqlFormatter.InsertString(TableName, Insert, Values);

            return string.Format("if not exists(select 1 from {0} where {1}) begin {2} end", TableName, Where, insertString);
        }
        public static string ExistsString(string From, string Fields, string Where)
        {
            if (string.IsNullOrEmpty(From))
            {
                throw new ArgumentNullException("From");
            }
            if (string.IsNullOrEmpty(Fields))
            {
                throw new ArgumentNullException("Fields");
            }
            if (string.IsNullOrEmpty(Where))
            {
                throw new ArgumentNullException("Where");
            }
            return string.Format("if exists(select {1} from {0} where {2}))", TableFormat(From), Fields, Where);
        }

        public static string InsertString(string From, string Fields, string Values)
        {
            if (string.IsNullOrEmpty(From))
            {
                throw new ArgumentNullException("From");
            }
            if (string.IsNullOrEmpty(Fields))
            {
                throw new ArgumentNullException("Fields");
            }
            if (string.IsNullOrEmpty(Values))
            {
                throw new ArgumentNullException("Values");
            }
            return string.Format("INSERT INTO {0} ({1}) VALUES({2})", TableFormat(From), Fields, Values);
        }
        public static string InsertCommandString(string From, object[] keyValueInsert)
        {
            if (string.IsNullOrEmpty(From))
            {
                throw new ArgumentNullException("From");
            }
            if (keyValueInsert == null || keyValueInsert.Length == 0)
            {
                throw new ArgumentNullException("keyValueInsert");
            }
            return string.Format("INSERT INTO {0} ({1}) VALUES({2})", TableFormat(From), CommandInsertFields(keyValueInsert), CommandInsertValues(keyValueInsert));
        }
        public static string UpdateString(string From,string Set, string Where)
        {
            if (string.IsNullOrEmpty(From))
            {
                throw new ArgumentNullException("From");
            }
            if (string.IsNullOrEmpty(Set))
            {
                throw new ArgumentNullException("Set");
            }
            if (string.IsNullOrEmpty(Where))
            {
                throw new ArgumentNullException("Where");
            }
            return string.Format("UPDATE {0} SET {1} WHERE {2}", TableFormat(From),Set, Where);
        }
        public static string UpdateCommandString(string From, object[] keyValueSet, object[] keyValueWhere)
        {
            if (string.IsNullOrEmpty(From))
            {
                throw new ArgumentNullException("From");
            }
            if (keyValueSet == null || keyValueSet.Length==0)
            {
                throw new ArgumentNullException("Set");
            }
            if (keyValueWhere == null || keyValueWhere.Length == 0)
            {
                throw new ArgumentNullException("Where");
            }
            return string.Format("UPDATE {0} SET {1} WHERE {2}", TableFormat(From), CommandSet(keyValueSet), CommandWhere(keyValueWhere));
        }

        public static string DeleteString( string From, string Where)
        {
            if (string.IsNullOrEmpty(From))
            {
                throw new ArgumentNullException("From");
            } 
            if (string.IsNullOrEmpty(Where))
            {
                throw new ArgumentNullException("Where");
            }
            return string.Format("DELETE {0} Where {1}", FromString(From), Where);
        }

        public static string ParametersString(IDbDataParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return null;// throw new ArgumentNullException("parameters");
            }
            StringBuilder sb = new StringBuilder();
            int i = 0;
            int len = parameters.Length;
            foreach (DataParameter p in parameters)
            {
                sb.AppendFormat("{0}=@{0}", p.ParameterName);
                if (i < len - 1)
                    sb.Append(" and ");
            }
            return sb.ToString();
        }

       
        public static string FieldsString(params string[] fields)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (string s in fields)
            {
                sb.AppendFormat("[{0}],", s);
            }
            //sb.Remove(sb.Length - 1, 1);
            return sb.ToString().TrimEnd(',') + " ";
        }

        public static string AggregateString(string AggregateMode, string Field, string From, string Where)
        {
            return AggregateString(AggregateMode, Field, From, Where, false);
        }

        public static string AggregateString(string AggregateMode, string Field, string From, string Where, bool withNolock)
        {
            if (string.IsNullOrEmpty(Where))
                return string.Format("SELECT {0}([{1}]) {2}", AggregateMode, Field, FromString(From, withNolock));
            else
                return string.Format("SELECT {0}([{1}]) {2} Where {3}", AggregateMode, Field, FromString(From, withNolock), Where);
        }

        /// <summary>
        /// Get Select Command
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="From"></param>
        /// <param name="Where"></param>
        /// <returns></returns>
        public static string SelectCommand(DataTable dataTable, string From,string Where)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (System.Data.DataColumn c in dataTable.Columns)
            {
                sb.AppendFormat("[{0}],", c.ColumnName);
            }
            sb.Remove(sb.Length - 1, 1);

            return SelectString(sb.ToString(), From, Where);
        }

        public static string DeleteCommand(string mappingName, object[] keyValueParameters)
        {
            string where=CommandWhere(keyValueParameters);
            return DeleteString(mappingName,where);
        }
        public static string CreateCommandText(string fields, string mappingName, object[] keyValueParameters)
        {
            return CreateCommandText(0, fields, mappingName, keyValueParameters);
        }
        public static string CreateCommandText(int top, string fields, string mappingName, object[] keyValueParameters)
        {
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("Invalid MappingName CommandText");
            }
            if (string.IsNullOrEmpty(fields))
            {
                throw new ArgumentNullException("Invalid Fields CommandText");
            }
            string where = "";
            if (keyValueParameters != null)
            {
                where = SqlFormatter.CommandWhere(keyValueParameters);
            }
            return SqlFormatter.SelectString(top,fields, mappingName, where);
        }

        public static string GetCommandText(string mappingName, object[] keyValueParameters)
        {
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("Invalid MappingName CommandText");
            }
            if (mappingName.TrimStart().ToLower().StartsWith("select"))
            {
                return mappingName;
            }
            else
            {
                string where = "";
                if (keyValueParameters != null)
                {
                    where = SqlFormatter.CommandWhere(keyValueParameters);
                }
                return SqlFormatter.SelectString("*", mappingName, where);
            }
        }

        public static string CommandInsertFields(object[] keyValueParameters)
        {
            if (keyValueParameters == null)
            {
                return "";
            }
            int count = keyValueParameters.Length;
            if (count > 0)
            {
                StringBuilder sb = new StringBuilder();
                //sb.Append(" WHERE ");
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("{0}", keyValueParameters[i]);
                    i++;
                    if (i < count - 1)
                        sb.Append(", ");

                }

                return sb.ToString();
            }
            return "";
        }
        public static string CommandInsertValues(object[] keyValueParameters)
        {
            if (keyValueParameters == null)
            {
                return "";
            }
            int count = keyValueParameters.Length;
            if (count > 0)
            {
                StringBuilder sb = new StringBuilder();
                //sb.Append(" WHERE ");
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("@{0}", keyValueParameters[i]);
                    i++;
                    if (i < count - 1)
                        sb.Append(", ");

                }

                return sb.ToString();
            }
            return "";
        }
        public static string CommandSet(object[] keyValueParameters)
        {
            if (keyValueParameters == null)
            {
                return "";
            }
            int count = keyValueParameters.Length;
            if (count > 0)
            {
                StringBuilder sb = new StringBuilder();
                //sb.Append(" WHERE ");
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("{0}=@{0}", keyValueParameters[i]);
                    i++;
                    if (i < count - 1)
                        sb.Append(" , ");

                }

                return sb.ToString();
            }
            return "";
        }
        public static string CommandWhere(object[] keyValueParameters)
        {
            if (keyValueParameters == null)
            {
                return ""; 
            }
            int count = keyValueParameters.Length;
            if (count > 0)
            {
                StringBuilder sb = new StringBuilder();
                //sb.Append(" WHERE ");
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("{0}=@{0}", keyValueParameters[i]);
                    i++;
                    if (i < count - 1)
                        sb.Append(" and ");

                }

                return sb.ToString();
            }
            return "";
        }
        public static string GetCommandText(string mappingName, IEnumerable<string> keys)
        {
            if (string.IsNullOrEmpty(mappingName))
            {
                throw new ArgumentNullException("Invalid MappingName CommandText");
            }
            if (mappingName.TrimStart().ToLower().StartsWith("select"))
            {
                return mappingName;
            }
            else
            {
                string where = "";
                if (keys != null)
                {
                    where = SqlFormatter.CommandWhere(keys.ToArray());
                }
                return SqlFormatter.SelectString("*", mappingName, where);
            }
        }
        public static string CommandWhere(string[] keys, bool addWhere)
        {
            if (keys == null)
            {
                return ""; //throw new ArgumentNullException("keys");
            }
            int count = keys.Length;
            if (count > 0)
            {
                StringBuilder sb = new StringBuilder();
                if (addWhere)
                    sb.Append(" WHERE ");
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("{0}=@{0}", keys[i]);
                    if (i < count - 1)
                        sb.Append(" and ");

                }

                return sb.ToString();
            }
            return "";
        }

        public static string CommandWhere(IDbDataParameter[] parameters, bool addWhere)
        {
            if (parameters == null)
            {
                return ""; //throw new ArgumentNullException("keys");
            }
            int count = parameters.Length;
            if (count > 0)
            {
                StringBuilder sb = new StringBuilder();
                if (addWhere)
                    sb.Append(" WHERE ");
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("{0}=@{0}", parameters[i].ParameterName);
                    if (i < count - 1)
                        sb.Append(" and ");

                }

                return sb.ToString();
            }
            return "";
        }

        public static string CommandSp(string spName, string[] keys, bool addWhere)
        {
             if (!addWhere)
                return spName;

            if (keys == null)
            {
                return spName; //throw new ArgumentNullException("keys");
            }
            int count = keys.Length;
            if (count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" ");
                for (int i = 0; i < count; i++)
                {
                    sb.AppendFormat("@{0}", keys[i]);
                    if (i < count - 1)
                        sb.Append(",");

                }

                return spName + sb.ToString();
            }
            return spName;
        }

        /// <summary>
        /// GetSql Integrated Connection text.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static string IntegratedConnection(string serverName, string database)
        {

            //localhost
            if (serverName.Length == 0 || database.Length == 0)
            {
                throw new ArgumentException("Enter Server name and Databse name");
            }
            return string.Format("Data Source={0};Integrated Security=SSPI;Initial Catalog={1}", serverName, database);
        }

        
        /// <summary>
        /// Validate Sql, not allowed (drop|create|alter).
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="excludeWords">drop|create|alter</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SyntaxErrorException"></exception>
        public static void ValidateSql(string commandText, string excludeWords)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("ValidateSql.commandText");
            }
            commandText = commandText.Replace("'", "''");
            if (excludeWords == null || excludeWords == "")
                return;

            string pattern = string.Format(@"(\s|)({0})\s.*", excludeWords);
            if (Regx.RegexValidateIgnoreCase(pattern, commandText))
            {
                throw new SyntaxErrorException("ValidateSql SyntaxErrorException: Invalid sql format");
            }
        }
        /// <summary>
        /// Validate Sql, not allowed (drop|create|alter|delete|insert|update).
        /// </summary>
        /// <param name="commandText"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SyntaxErrorException"></exception>
        public static void ValidateSqlSelect(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("ValidateSql.commandText");
            }
            commandText = commandText.Replace("'", "''");
            string pattern = @"(\s|)(drop|create|alter|delete|insert|update)\s.*";
            if (Regx.RegexValidateIgnoreCase(pattern, commandText))
            {
                throw new SyntaxErrorException("ValidateSql SyntaxErrorException: Invalid sql format");
            }
        }

        public static string ValidateSqlInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            string pattern = @"(\s|)(drop|create|alter|delete|insert|update|execute|exec)\s.*";
            if (Regx.RegexValidateIgnoreCase(pattern, input))
            {
                return "";
            }

            return input.Replace("'", "''").Replace(";", "").Replace("--", "").Replace("/*", "").Replace("*/", "")
               .Replace("xp_", "")
               .Replace("[", "[[]")
               .Replace("%", "[%]")
               .Replace("_", "[_]");

        }

        #region in values

        /// <summary>
        /// Create Sql String for command
        /// </summary>
        /// <param name="Select">Fields for select cluse</param>
        /// <param name="From">from string cluse</param>
        /// <param name="Where">where string cluse</param>
        /// <param name="InValues">Array Values of parameter for IN() Operation </param>
        /// <remarks>To use InValues you should write in Where Predicat 
        /// for string values IN('') , 
        /// for numbers values IN() , 
        /// for nvarchar values IN(N''),
        /// for DateTime in jet IN(##)</remarks>
        /// <returns>String</returns>
        public static string BuildSqlString(string Select, string From, string Where, object[] InValues)
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("SELECT {0} ", Select);
            sb.AppendFormat("FROM {0} ", FromString(From));
            if (Where != null && Where != "")
            {
                string where = BuildSqlWhereString(Where, InValues);
                sb.Append(where);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Create Where Predicat for Sql String command
        /// </summary>
        /// <param name="Where">where string cluse</param>
        /// <param name="InValues">Array Values of parameter for IN() Operation </param>
        /// <remarks>To use InValues you should write in Where Predicat 
        /// for string values IN('') , 
        /// for numbers values IN() , 
        /// for nvarchar values IN(N''),
        /// for DateTime in jet IN(##)</remarks>
        /// <returns>String</returns>
        public static string BuildSqlWhereString(string Where, object[] InValues)
        {
            if (Where == null || Where == "")
                return "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (InValues != null && InValues.Length > 0)
            {
                System.Text.StringBuilder sbi = new System.Text.StringBuilder();
                string where = Where.ToLower();
                string inTemplate = "";
                string prefix = "";
                string sufix = "";
                if (where.IndexOf("in()") > -1)
                {
                    inTemplate = "in()";
                }
                else if (where.IndexOf("in('')") > -1)
                {
                    inTemplate = "in('')";
                    prefix = "'";
                    sufix = "'";
                }
                else if (where.IndexOf("in(N'')") > -1)
                {
                    inTemplate = "in('')";
                    prefix = "N'";
                    sufix = "'";
                }
                else if (where.IndexOf("in(##)") > -1)
                {
                    inTemplate = "in(##)";
                    prefix = "#";
                    sufix = "#";
                }

                sbi.Append(" IN(");
                foreach (object o in InValues)
                {
                    sbi.AppendFormat("{0}{1}{2},", prefix, o.ToString(), sufix);
                }

                sbi.Replace(',', ')', sbi.Length - 1, 1);

                where = where.Replace(inTemplate, sbi.ToString());
                sb.AppendFormat("WHERE ({0}) ", where);

            }
            else
            {
                sb.AppendFormat("WHERE ({0}) ", Where);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create Sql String for command
        /// </summary>
        /// <param name="Select">Fields for select cluse</param>
        /// <param name="From">from string cluse</param>
        /// <param name="Where">where string cluse</param>
        /// <param name="InValues">DataRow Array Values of parameter for IN() Operation </param>
        /// <param name="columnName">columnName in dataRow</param>
        /// <remarks>To use InValues you should write in Where Predicat 
        /// for string values IN('') , 
        /// for numbers values IN() , 
        /// for nvarchar values IN(N''),
        /// for DateTime in jet IN(##)</remarks>
        /// <returns>String</returns>
        public static string GetSqlString(string Select, string From, string Where, DataRow[] InValues, string columnName)
        {
            int len = InValues.Length;
            object[] ob = new object[len];
            int i = 0;
            foreach (DataRow dr in InValues)
            {
                ob[i] = dr[columnName];
                i++;
            }
            return GetSqlString(Select, From, Where, ob);
        }

        /// <summary>
        /// Create Sql String for command
        /// </summary>
        /// <param name="Select">Fields for select cluse</param>
        /// <param name="From">from string cluse</param>
        /// <param name="Where">where string cluse</param>
        /// <param name="InValues">Array Values of parameter for IN() Operation </param>
        /// <remarks>To use InValues you should write in Where Predicat 
        /// for string values IN('') , 
        /// for numbers values IN() , 
        /// for nvarchar values IN(N''),
        /// for DateTime in jet IN(##)</remarks>
        /// <returns>String</returns>
        public static string GetSqlString(string Select, string From, string Where, object[] InValues)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("SELECT {0} ", Select);
            sb.AppendFormat("FROM {0} ", FromString(From));
            if (Where != null && Where != "")
            {
                string where = GetSqlString(Where, InValues);
                sb.Append(where);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Create Where Predicat for Sql String command
        /// </summary>
        /// <param name="Where">where string cluse</param>
        /// <param name="InValues">Array Values of parameter for IN() Operation </param>
        /// <remarks>To use InValues you should write in Where Predicat 
        /// for string values IN('') , 
        /// for numbers values IN() , 
        /// for nvarchar values IN(N''),
        /// for DateTime in jet IN(##)</remarks>
        /// <returns>String</returns>
        public static string GetSqlString(string Where, object[] InValues)
        {
            if (Where == null || Where == "")
                return "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (InValues != null && InValues.Length > 0)
            {
                System.Text.StringBuilder sbi = new System.Text.StringBuilder();
                string where = Where.ToLower();
                string inTemplate = "";
                string prefix = "";
                string sufix = "";
                if (where.IndexOf("in()") > -1)
                {
                    inTemplate = "in()";
                }
                else if (where.IndexOf("in('')") > -1)
                {
                    inTemplate = "in('')";
                    prefix = "'";
                    sufix = "'";
                }
                else if (where.IndexOf("in(N'')") > -1)
                {
                    inTemplate = "in('')";
                    prefix = "N'";
                    sufix = "'";
                }
                else if (where.IndexOf("in(##)") > -1)
                {
                    inTemplate = "in(##)";
                    prefix = "#";
                    sufix = "#";
                }

                sbi.Append(" IN(");
                foreach (object o in InValues)
                {
                    sbi.AppendFormat("{0}{1}{2},", prefix, o.ToString(), sufix);
                }

                sbi.Replace(',', ')', sbi.Length - 1, 1);

                where = where.Replace(inTemplate, sbi.ToString());
                sb.AppendFormat("WHERE ({0}) ", where);

            }
            else
            {
                sb.AppendFormat("WHERE ({0}) ", Where);
            }

            return sb.ToString();
        }
        #endregion
    }
}
