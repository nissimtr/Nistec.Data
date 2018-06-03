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
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Nistec;
using System.Text;
using Nistec.Data.Entities;
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Serialization;

namespace Nistec.Data.Entities
{

    #region EntityCommandResult

    /// <summary>
    /// Represet Entity command result
    /// </summary>
    public class EntityCommandResult
    {
        public static EntityCommandResult Empty
        {
            get { return new EntityCommandResult(0, null, null); }
        }
        public static int GetAffectedRecords(EntityCommandResult result)
        {
            return result == null ? 0 : result.AffectedRecords;
        }

        public readonly int AffectedRecords;
        public readonly Dictionary<string, object> OutputValues;
        public readonly string IdentityField;

        public int Status { get; set; }
        public string Message { get; set; }
        public int OutputId { get; set; }
        public string Title { get; set; }

        public void Set(string message, string title)
        {
            Status = AffectedRecords > 1 ? 1 : AffectedRecords;
            Message = message;
            Title = title;
            OutputId = GetIdentityValue<int>();
        }
        
        public string ToJson()
        {
           return  JsonSerializer.Serialize(this);
        }

        public int GetReturnValue()
        {
            if (OutputValues == null || IdentityField == null)
                return -1;
            return OutputValues.Get<int>(IdentityField);
        }
        public T GetIdentityValue<T>()
        {
            if (OutputValues == null || IdentityField == null)
                return default(T);
            return OutputValues.Get<T>(IdentityField);
        }
        public T GetValue<T>(string key)
        {
            if (OutputValues==null)
                return default(T);
            return OutputValues.Get<T>(key);
        }
        public T GetValue<T>(string key, T defaultValue)
        {
            if (OutputValues == null)
                return default(T);
            return OutputValues.Get<T>(key, defaultValue);
        }
        public EntityCommandResult(int affectedRecords,Dictionary<string, object> outputValues, string identityField)
        {
            AffectedRecords = affectedRecords;
            OutputValues = outputValues;
            IdentityField = identityField;
        }
        public EntityCommandResult(int affectedRecords, int outputValue, string identityField)
        {
            AffectedRecords = affectedRecords;
            OutputValues = new Dictionary<string,object>();
            OutputValues[identityField] = outputValue;
            IdentityField = identityField;
        }

    }

    
    #endregion

    /// <summary>
    /// This class is similar to the System.Data.SqlClient.SqlCommandBuilder, but use only the Class Properties to create the required commands.
    /// </summary>
    public class EntityCommandBuilder:IDisposable
    {

        #region static

    
        public static int DeleteCommand(object instance, EntityDbContext db)
        {
            using (EntityCommandBuilder ac = new EntityCommandBuilder(instance, db, null))
            {
                var result = ac.ExecuteCommand(UpdateCommandType.Delete);
                if (result == null)
                    return 0;
                return result.AffectedRecords;
            }
        }

        public static int UpdateCommand(object instance, EntityDbContext db, Dictionary<string, object> fieldsChanged)
        {
            using (EntityCommandBuilder ac = new EntityCommandBuilder(instance, db, fieldsChanged))
            {
                var result = ac.ExecuteCommand(UpdateCommandType.Update);
                if (result == null)
                    return 0;
                return result.AffectedRecords;
            }
        }
        public static int InsertCommand(object instance, EntityDbContext db, Dictionary<string, object> fieldsChanged)
        {
            using (EntityCommandBuilder ac = new EntityCommandBuilder(instance, db, fieldsChanged))
            {
                var result = ac.ExecuteCommand(UpdateCommandType.Insert);
                if (result == null)
                    return 0;
                return result.AffectedRecords;
            }
        }

        #endregion

        #region Dispose

        private bool disposed = false;

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (m_connection != null)
                    {
                        if ((m_connection.State != ConnectionState.Closed))// && m_ownsConnection) 
                        {
                            try
                            {
                                m_connection.Close();
                                m_connection.Dispose();
                            }
                            catch { }
                        }
                        m_connection = null;
                    }
                    if (m_command != null)
                    {
                        m_command.Dispose();
                        m_command = null;
                    }
                 }

                //dispose unmanaged resources here.
                disposed = true;
            }
        }
        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);     
         
            // take this object off the finalization queue     
            GC.SuppressFinalize(this); 
        }

        ~EntityCommandBuilder()
        {
            Dispose(false);
        }
        #endregion

        #region members

        object m_Instance;
        IDbConnection m_connection;
        PropertyInfo[] m_properties;
        string m_tableName;
        Dictionary<string, object> m_FieldsChanged;
        IDbCommand m_command;
        string m_autoNumberField;
        #endregion

        public PropertyInfo[] Properties
        {
            get { return m_properties; }
        }

        Dictionary<string, object> m_OutputValues;
 
        public string TableName
        {
            get { return m_tableName; }
        }
        public string AutoNumberField
        {
            get { return m_autoNumberField; }
        }
        internal EntityCommandBuilder(object instance, EntityDbContext db, Dictionary<string, object> fieldsChanged)
        {
            m_Instance = instance;
            m_connection = db.DbConnection();
            m_tableName = db.MappingName;
            m_properties = instance.GetType().GetProperties(true);
            m_FieldsChanged = fieldsChanged;
        }

        public EntityCommandBuilder(IEntityItem instance, IDbContext db, string tableName, Dictionary<string, object> fieldsChanged)
        {
            m_Instance = instance;
            m_connection = db.Connection;
            m_tableName = tableName;
            m_properties = instance.GetType().GetProperties(true);
            m_FieldsChanged = fieldsChanged;
        }

        public EntityCommandBuilder(GenericEntity instance, Type entity, IDbConnection idb, string tableName)
        {
            m_Instance = instance;
            m_connection = idb;
            m_tableName = tableName;
            m_properties = entity.GetProperties(true);
            m_FieldsChanged = instance.GetFieldsChanged();
        }

        //public EntityCommandBuilder(object instance, IDbConnection idb, string tableName)
        //{
        //    m_Instance = instance;
        //    m_connection = idb;
        //    m_tableName = tableName;
        //    m_properties = instance.GetType().GetProperties(true);
        //    if (instance is ActiveEntity)
        //    {
        //        m_FieldsChanged = ((ActiveEntity)instance).GetFieldsChanged();
        //    }
        //    else if (instance is GenericEntity)
        //    {
        //        m_FieldsChanged = ((GenericEntity)instance).GetFieldsChanged();
        //    }
        //}

        public EntityCommandBuilder(ActiveEntity instance, IDbConnection idb, string tableName)
        {
            m_Instance = instance;
            m_connection = idb;
            m_tableName = tableName;
            m_properties = instance.GetType().GetProperties(true);
            m_FieldsChanged = instance.GetFieldsChanged();
        }
        //public EntityCommandBuilder(GenericEntity instance, IDbConnection idb, string tableName)
        //{
        //    m_Instance = instance;
        //    m_connection = idb;
        //    m_tableName = tableName;
        //    m_properties = instance.GetType().GetProperties(true);
        //    m_FieldsChanged = instance.GetFieldsChanged();
        //}
        public EntityCommandBuilder(IEntityFields entityFileds, object instance, IDbConnection idb, string tableName)
        {
            m_Instance = instance;
            m_connection = idb;
            m_tableName = tableName;
            m_properties = instance.GetType().GetProperties(true);

            EntityFieldsChanges gf = entityFileds.GetFieldsChanged();
            if (gf != null)
            {
                m_FieldsChanged = gf.FieldsChanged;
            }

        }

        public EntityCommandBuilder(EntityFieldsChanges fg, object instance, IDbConnection idb, string tableName)
        {
            m_Instance = instance;
            m_connection = idb;
            m_tableName = tableName;
            m_properties = instance.GetType().GetProperties(true);
            m_FieldsChanged = fg.FieldsChanged;

        }

        public EntityCommandBuilder(GenericEntity fg, object instance, IDbConnection idb, string tableName)
        {
            m_Instance = instance;
            m_connection = idb;
            m_tableName = tableName;
            m_properties = fg.GetProperties(true);
            m_FieldsChanged = fg.GetFieldsChanged();

         }
 

        private bool ValidateUpdate()
        {

            if (m_connection == null)
            {
                throw new Exception("Invalid connection object");
            }
            else if (string.IsNullOrEmpty(m_connection.ConnectionString))
            {
                throw new Exception("Invalid connection");
            }
            if (string.IsNullOrEmpty(m_tableName))
            {
                throw new Exception("Invalid Table Name");
            }
       
            return true;
        }

        private bool ValidateConnection()
        {

            if (m_connection == null)
            {
                throw new Exception("Invalid connection object");
            }
            else if (string.IsNullOrEmpty(m_connection.ConnectionString))
            {
                throw new Exception("Invalid connection");
            }
           
            return true;
        }

        #region Methods


        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="commandType"><see cref="UpdateCommandType"/> enumeration value</param>
        /// <returns>int</returns>
        public virtual int ExecCommand(UpdateCommandType commandType)
        {

            ValidateUpdate();

            // create command object
            m_command = m_connection.CreateCommand();

            // define default command properties (command text, command type and missing schema action)
            string commandText = "";

            // set command text
            m_command.CommandText = commandText;

            // set command type
            m_command.CommandType = CommandType.Text;


            // define command parameters.
            SetParameters(commandType);

            // execute command
            int result = 0;

            try
            {

                if (m_command.Connection.State == ConnectionState.Closed)
                {
                    m_command.Connection.Open();
                }

                result = m_command.ExecuteNonQuery();
                //m_OutputValues = new Dictionary<string, object>();
                //foreach (IDbDataParameter prm in m_command.Parameters)
                //{
                //    if (prm.Direction != ParameterDirection.Input)
                //    {
                //        m_OutputValues.Add(prm.ParameterName.Replace("@", ""), prm.Value);
                //    }
                //}
                return result;
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                throw new DalException(ex.Message);
            }
            catch (DalException dbex)
            {
                throw new DalException(dbex.Message);
            }
            finally
            {
                if (m_command.Connection.State != ConnectionState.Closed)
                {
                    m_command.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="commandType"><see cref="UpdateCommandType"/> enumeration value</param>
        /// <returns>int</returns>
        public virtual EntityCommandResult ExecuteCommand(UpdateCommandType commandType)
        {

            ValidateUpdate();

            // create command object
            m_command = m_connection.CreateCommand();

            // define default command properties (command text, command type and missing schema action)
            string commandText = "";

            // set command text
            m_command.CommandText = commandText;

            // set command type
            m_command.CommandType = CommandType.Text; 


            // define command parameters.
            SetParameters(commandType);

            // execute command
            int result = 0;

            try
            {

                if (m_command.Connection.State == ConnectionState.Closed)
                {
                    m_command.Connection.Open();
                }

                result = m_command.ExecuteNonQuery();
                m_OutputValues=new Dictionary<string,object>();
                foreach (IDbDataParameter prm in m_command.Parameters)
                {
                    if (prm.Direction != ParameterDirection.Input)
                    {
                        m_OutputValues.Add(prm.ParameterName.Replace("@", ""), prm.Value);
                    }
                }
                return new EntityCommandResult(result, m_OutputValues, m_autoNumberField);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                throw new DalException(ex.Message);
            }
            catch (DalException dbex)
            {
                throw new DalException(dbex.Message);
            }
            finally
            {
                if (m_command.Connection.State != ConnectionState.Closed)
                {
                    m_command.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes Sql command and returns execution result. 
        /// Command text, type and parameters are taken from method using reflection.
        /// Command parameter values are taken from method parameter values.
        /// </summary>
        /// <param name="commandText">StoredProcedure name</param>
        /// <returns>int</returns>
        public virtual EntityCommandResult ExecuteQuery(string commandText)
        {

            ValidateConnection();

            // create command object
            m_command = m_connection.CreateCommand();

            // set command text
            m_command.CommandText = commandText;

            // set command type
            m_command.CommandType = CommandType.StoredProcedure;


            // define command parameters.
            SetParameters(UpdateCommandType.StoredProcedure);

            // execute command
            int result = 0;

            try
            {

                if (m_command.Connection.State == ConnectionState.Closed)
                {
                    m_command.Connection.Open();
                }

                result = m_command.ExecuteNonQuery();
                m_OutputValues = new Dictionary<string, object>();
                foreach (IDbDataParameter prm in m_command.Parameters)
                {
                    if (prm.Direction != ParameterDirection.Input)
                    {
                        m_OutputValues.Add(prm.ParameterName.Replace("@", ""), prm.Value);
                    }
                }
                return new EntityCommandResult(result, m_OutputValues, m_autoNumberField);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                throw new DalException(ex.Message);
            }
            catch (DalException dbex)
            {
                throw new DalException(dbex.Message);
            }
            finally
            {
                if (m_command.Connection.State != ConnectionState.Closed)
                {
                    m_command.Connection.Close();
                }
            }
        }
        private string GetValidString(string str, string defaultValue)
        {
            return string.IsNullOrEmpty(str) ? defaultValue : str;
        }

        /// <summary>
        /// Generates command parameters. 
        /// For some command types the command text can be changed during parameter generating.
        /// </summary>
        /// <param name="commandType"><see cref="DBCommandType"/> enumeration value</param>
        internal void SetParameters(UpdateCommandType commandType)
        {
            #region InsertUpdate parts declaration
            string cmdPart1 = "";
            string cmdPart2 = "";
            //string autNumberField = "";
            string cmdPart3 = "";
            string cmdPart4 = "";

            #endregion

            int sqlParamIndex = 0;

            foreach (PropertyInfo property in m_properties)
            {
                // get parameter attribute and set command parameter settings

                EntityPropertyAttribute paramAttribute =
              Attribute.GetCustomAttribute(property, typeof(EntityPropertyAttribute)) as EntityPropertyAttribute;


                if (paramAttribute == null)
                {
                    //continue;
                    paramAttribute = new EntityPropertyAttribute(EntityPropertyType.Default);
                }
                EntityPropertyType paramCustType = paramAttribute.ParameterType;

                switch (paramCustType)
                {
                    case EntityPropertyType.NA:
                    case EntityPropertyType.View:
                    case EntityPropertyType.Optional:
                        continue;
                }


                string propName = paramAttribute.GetColumn(property.Name);

                if ((commandType == UpdateCommandType.Update || commandType == UpdateCommandType.Upsert) && paramCustType == EntityPropertyType.Default)
                {
                    if (m_Instance is IActiveEntity)
                    {
                        if (m_FieldsChanged == null)
                        {
                            throw new DalException("Invalid FieldsChanged parameters");
                        }
                        if (!m_FieldsChanged.ContainsKey(propName))
                        {
                            continue;
                        }
                    }
                    if (m_FieldsChanged != null)
                    {
                        if (!m_FieldsChanged.ContainsKey(propName))
                        {
                            continue;
                        }
                    }
                }

                // set default values
                string paramName = null;
                object v = null;// property.GetValue(m_Instance, null);

                if (m_FieldsChanged == null)// && commandType == UpdateCommandType.Delete)
                {
                    m_FieldsChanged = new Dictionary<string, object>();
                }

                if (!m_FieldsChanged.TryGetValue(propName, out v))
                {
                    v = property.GetValue(m_Instance, null);
                }

                // create command parameter
                IDbDataParameter sqlParameter = m_command.CreateParameter();

                if (paramAttribute.IsColumnDefined)
                    sqlParameter.ParameterName = paramName = paramAttribute.Column;
                else if (paramAttribute.IsNameDefined)
                    sqlParameter.ParameterName = paramName = paramAttribute.Name;
                else
                    sqlParameter.ParameterName = paramName = property.Name;

                if (v == null && !paramAttribute.AllowNull)
                {
                    string caption = paramName;


                    if (paramAttribute != null)
                    {
                        caption = ((IEntity)m_Instance).EntityProperties.GetCaption(paramName);//.EntityCaption(paramName);

                        //caption = GetValidString(captionAttribute.Caption, paramName);
                    }
                    throw new System.ArgumentNullException(caption);// ("Parameter " + caption + " does not allow null");
                }


                if (paramCustType == EntityPropertyType.Identity)
                {
                    if (commandType == UpdateCommandType.Insert)
                    {
                        m_autoNumberField = paramName;
                        sqlParameter.Direction = ParameterDirection.Output;// paramInfo.IsOut ? ParameterDirection.Output : ParameterDirection.InputOutput;
                    }
                    else
                    {
                        sqlParameter.Direction = ParameterDirection.Input;
                    }
                }
                else
                {
                    sqlParameter.Direction = ParameterDirection.Input;
                }


                if (paramAttribute.IsTypeDefined)
                    sqlParameter.DbType = paramAttribute.SqlDbType;

                if (paramAttribute.IsSizeDefined)
                    sqlParameter.Size = paramAttribute.Size;

                if (CompareAsNullValues(paramAttribute.AsNull, v, property.PropertyType))
                {
                    v = DBNull.Value;
                }



                // generate parts of InsertUpdate expresion
                #region generate parts of InsertUpdateDelete expresion

                if (commandType == UpdateCommandType.Upsert)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;

                    //insert part
                    if (paramCustType != EntityPropertyType.Identity)
                    {
                        cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName);
                        cmdPart2 = AddWithDelim(cmdPart2, ", ", cmdparamName);
                    }
                    //update part
                    if ((paramCustType == EntityPropertyType.Key) || (paramCustType == EntityPropertyType.Identity))
                    {
                        cmdPart4 = AddWithDelim(cmdPart4, " and ", fieldName + "=" + cmdparamName);
                    }
                    else if (paramCustType != EntityPropertyType.Identity)
                    {
                        cmdPart3 = AddWithDelim(cmdPart3, ", ", fieldName + "=" + cmdparamName);
                    }
                }
                else if (commandType == UpdateCommandType.Insert)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;

                    if (paramCustType != EntityPropertyType.Identity)
                    {
                        cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName);
                        cmdPart2 = AddWithDelim(cmdPart2, ", ", cmdparamName);
                    }
                }

                else if (commandType == UpdateCommandType.Update)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;

                    if ((paramCustType == EntityPropertyType.Key) || (paramCustType == EntityPropertyType.Identity))
                    {
                        cmdPart2 = AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
                    }
                    else if (paramCustType != EntityPropertyType.Identity)
                    {
                        cmdPart1 = AddWithDelim(cmdPart1, ", ", fieldName + "=" + cmdparamName);
                    }
                }
                else if (commandType == UpdateCommandType.Delete)
                {
                    string fieldName = "[" + paramName + "]";
                    string cmdparamName = "@" + paramName;
                    cmdPart1 = "";
                    if ((paramCustType == EntityPropertyType.Key) || (paramCustType == EntityPropertyType.Identity))
                    {
                        cmdPart2 = AddWithDelim(cmdPart2, " and ", fieldName + "=" + cmdparamName);
                    }
                }

                #endregion

                // set parameter name
                sqlParameter.ParameterName = "@" + paramName.Replace("@", "");


                // set parameter value
                if (v == null)
                {
                    v = DBNull.Value;
                }
                sqlParameter.Value = v;

                // add parameter to the command object
                m_command.Parameters.Add(sqlParameter);
                //indexes[paramIndex] = sqlParamIndex;
                sqlParamIndex++;
            }

            #region CommandBuilder

            string cmdString = "";
            string CommandText = "";

            if (commandType == UpdateCommandType.Upsert)
            {
                if (string.IsNullOrEmpty(cmdPart1) || string.IsNullOrEmpty(cmdPart2) || string.IsNullOrEmpty(cmdPart3) || string.IsNullOrEmpty(cmdPart4))
                {
                    throw new DalException("Command Builder has no values to update");
                }

                //insert
                string cmdInsertString = "";
                string insertString = SqlFormatter.InsertString(TableName, cmdPart1, cmdPart2);
                if (m_autoNumberField == null)
                {
                    cmdInsertString += String.Format("{0} if (@@rowcount = 0) {1}", insertString, "print 'Warning: No rows were updated'");
                }
                else
                {
                    cmdInsertString += String.Format("if(@{0} is NULL) begin {1} select @{0} = SCOPE_IDENTITY() end ", m_autoNumberField, insertString);
                    cmdInsertString += String.Format("else begin {0} end", insertString);
                }


                //update
                string cmdUpdateString = "";
                string updateString = SqlFormatter.UpdateString(TableName, cmdPart3, cmdPart4);
                cmdUpdateString += String.Format("{0} if (@@rowcount = 0) {1}", updateString, "print 'Warning: No rows were updated'");

                CommandText = string.Format("if not exists(select 1 from {0} where {1}) begin {2} end else begin {3} end", TableName, cmdPart4, cmdInsertString, cmdUpdateString);
                m_command.CommandText = CommandText;
            }
            else if (commandType == UpdateCommandType.Insert)
            {
                if (string.IsNullOrEmpty(cmdPart1) || string.IsNullOrEmpty(cmdPart2))
                {
                    throw new DalException("Command Builder has no values to update");
                }
                cmdString = SqlFormatter.InsertString(TableName, cmdPart1, cmdPart2);// String.Format(" INSERT INTO [{0}]({1}) VALUES({2}) ", TableName, cmdPart1, cmdPart2);

                if (m_autoNumberField == null)
                {
                    CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                }
                else
                {
                    CommandText += String.Format("if(@{0} is NULL) begin {1} select @{0} = SCOPE_IDENTITY() end ", m_autoNumberField, cmdString);
                    CommandText += String.Format("else begin {0} end", cmdString);
                }
                m_command.CommandText = CommandText;
            }

            else if (commandType == UpdateCommandType.Update)
            {
                if (string.IsNullOrEmpty(cmdPart1))
                {
                    throw new DalException("Command Builder has no values to update");
                }
                if (string.IsNullOrEmpty(cmdPart2))
                {
                    throw new DalException("No Identity or Autoincrement field is defined.");
                }

                cmdString = SqlFormatter.UpdateString(TableName, cmdPart1, cmdPart2);// String.Format(" UPDATE [{0}] SET {1} WHERE {2} ", TableName, cmdPart1, cmdPart2);

                CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                m_command.CommandText = CommandText;

            }
            else if (commandType == UpdateCommandType.Delete)
            {
                if (cmdPart2 == "")
                {
                    throw new DalException("No Identity or Autoincrement field is defined.");
                }

                cmdString = SqlFormatter.DeleteString(TableName, cmdPart2);// String.Format(" DELETE FROM {0} where {1} ", TableName, cmdPart2);
                CommandText += String.Format("{0} if (@@rowcount = 0) {1}", cmdString, "print 'Warning: No rows were updated'");
                m_command.CommandText = CommandText;

            }

            #endregion

        }


        #endregion

        #region Functions

        /// <summary>
        /// Concats two strings with a delimiter.
        /// </summary>
        /// <param name="s1">string 1</param>
        /// <param name="delim">delimiter</param>
        /// <param name="s2">string 2</param>
        /// <returns></returns>
        private string AddWithDelim(string s1, string delim, string s2)
        {
            if (s1 == "") return s2;
            else return s1 + delim + s2;
        }


        /// <summary>
        /// Returns type from reference type.
        /// </summary>
        /// <param name="type">Reference type value.</param>
        /// <returns></returns>
        internal Type GetRefType(Type type)
        {
            Type reftype = null;

            string typeName = type.FullName;
            if (typeName.EndsWith("&"))
            {
                reftype = Type.GetType(typeName.Substring(0, typeName.Length - 1));
            }

            return reftype;
        }

        /// <summary>
        /// Compares parameter value with a value that must be treated as DBNull.
        /// </summary>
        /// <param name="AsNull">The value that must be treated as DBNull</param>
        /// <param name="ParamValue">The parameter value.</param>
        /// <param name="ParamType">Type of the parameter value.</param>
        /// <returns></returns>
        private bool CompareAsNullValues(object AsNull, object ParamValue, Type ParamType)
        {
            bool b = false;
            if (AsNull.ToString() == EntityPropertyAttribute.NullValueToken) return false;
            if (AsNull == null) return false;

            if (ParamType == typeof(DateTime))
            {
                DateTime d = new DateTime((int)AsNull);
                b = (d == (DateTime)ParamValue);
            }
            else if (ParamType == typeof(byte) || ParamType == typeof(int) || ParamType == typeof(long))
            {
                long v = Convert.ToInt64(AsNull);
                b = v == Convert.ToInt64(ParamValue);
            }
            else if (ParamType == typeof(float) || ParamType == typeof(double))
            {
                double v = Convert.ToDouble(AsNull);
                b = v == Convert.ToDouble(ParamValue);
            }
            else if (AsNull.Equals(ParamValue))
            {
                b = true;
            }
            return b;
        }
        #endregion


    }
}
