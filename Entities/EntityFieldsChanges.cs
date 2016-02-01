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
using System.Collections.Generic; 
using System.Reflection;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Xml;
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.Runtime;

namespace Nistec.Data.Entities
{
    
     public class EntityFieldsChanges
    {
         //rcd
        GenericRecord _Data;
        IEntityFields _entity;
        object _instance;
        UpdateCommandType _commandType;
  

        public EntityFieldsChanges(IEntityFields entity, object instace, UpdateCommandType commandType,bool initChanges = true)
        {
            _Data = entity.EntityRecord;
            _entity = entity;
            _instance = instace;
            _commandType = commandType;

            if (instace == null)
            {
                _instance = entity;
            }
            if (initChanges)
            {
                SetChanges();
            }
        }

        public GenericRecord Data
        {
            get { return _Data; }
        }
        public void SetChanges()
        {
            FieldsChanged.Clear();
            GenericRecord gr = EntityPropertyBuilder.CreateGenericRecord(_instance,false);//.BuildEntityContext(_instance);
            foreach (var entry in gr)
            {
                SetChanges(entry.Key, entry.Value);
            }

         }

        /// <summary>
        /// Set Value in specified field
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        /// <param name="isInsert">indicate if the value should be inserted</param>
        void SetChanges(string field, object value)
        {
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Data ", field);
            }
            if (!_Data.ContainsKey(field))
            {
                AddNewField(field);
            }
            else if (_commandType== UpdateCommandType.Insert)
            {
                AddChanges(field);
            }
            else if (!_Data.CompareValues(field, value))
            {
                AddChanges(field);
            }
            _Data.SetValue(field, value);
        }

        public int SaveChanges()
        {
            EntityDbContext db = _entity.EntityDb;

            if (db == null)
            {
                throw new EntityException("Invalid MappingName or ConnectionContext");
            }
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Entity Data ");
            }
            db.ValidateContext();

            if (!IsDirty())
                return 0;
            
            EntityCommandResult res = null;


            using (EntityCommandBuilder ac = new EntityCommandBuilder(_entity,_instance, db.DbConnection(), db.MappingName))
            {
                res = ac.ExecuteCommand(_commandType);
            }
            if (res == null)
            {
                throw new EntityException("SaveChanges was not succeed.");
            }
            if (_commandType == UpdateCommandType.Insert)
            {
                foreach (KeyValuePair<string, object> p in res.OutputValues)
                {
                   _Data.SetValue(p.Key, p.Value);

                   PropertyInfo pi= _instance.GetType().GetProperty(p.Key);
                   if (pi != null)
                   {
                       pi.SetValue(_instance, p.Value, null);
                   }
                }
            }
            CommitChanges();
            return res.AffectedRecords;
        }

        public static int SaveChanges(UpdateCommandType commandType, GenericEntity entity, EntityDbContext db)
        {
            if (db == null)
            {
                throw new EntityException("Invalid MappingName or ConnectionContext");
            }
            if (entity == null)
            {
                throw new ArgumentException("Invalid Entity Data ");
            }
            db.ValidateContext();

            if (!entity.IsDirty)
                return 0;

            EntityCommandResult res = null;

            using (EntityCommandBuilder ac = new EntityCommandBuilder(entity, db.DbConnection(), db.MappingName))
            {
                res = ac.ExecuteCommand(commandType);
            }
            if (res == null)
            {
                throw new EntityException("SaveChanges was not succeed.");
            }
            if (commandType == UpdateCommandType.Insert)
            {
                foreach (KeyValuePair<string, object> p in res.OutputValues)
                {
                    entity.SetValue(p.Key, p.Value);
                }
            }
            entity.CommitChanges();
            return res.AffectedRecords;
        }
         //rcd
        public GenericRecord UpdateOutputValues(EntityCommandResult res)
        {
            foreach (KeyValuePair<string, object> p in res.OutputValues)
            {
                _Data.SetValue(p.Key, p.Value);

                PropertyInfo pi = _instance.GetType().GetProperty(p.Key);
                if (pi != null)
                {
                    pi.SetValue(_instance, p.Value, null);
                }
            }

            return Data;
        }


        /// <summary>
        /// Set Value in specified field
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        void SetValue(string field, object value)
        {
            if (_Data == null)
            {
                throw new ArgumentException("Invalid Data ", field);
            }
            if (!_Data.ContainsKey(field))
            {
                AddNewField(field);
            }
            else if (!_Data.CompareValues(field, value))
            {
                AddChanges(field);
            }
            _Data.SetValue(field, value);
        }

        #region FieldsChanged

        Dictionary<string, object> _FieldsChanged;
        const string EntityNewField = "$$EntityNewField$$";

        [EntityProperty(EntityPropertyType.NA)]
        public Dictionary<string, object> FieldsChanged
        {
            get { if (_FieldsChanged == null) { _FieldsChanged = new Dictionary<string, object>(); } return _FieldsChanged; }
        }

        void ValidateData()
        {
            if (_Data == null)
            {
                _Data = new GenericRecord();//rcd
            }
        }

        void AddNewField(string field)
        {
            FieldsChanged.Add(field, EntityNewField);
        }

        void AddChanges(string field)
        {
            FieldsChanged[field] = _Data[field];
        }

        /// <summary>
        /// Clear all changes
        /// </summary>
        public void ClearChanges()
        {
            if (IsDirty())
            {
                Restor();
            }
        }

        /// <summary>
        /// End edit and save all changes localy
        /// </summary>
        internal void CommitChanges()
        {
     
            if (IsDirty())
            {
                FieldsChanged.Clear();
            }
        }

        private void Restor()
        {
            ValidateData();

            foreach (KeyValuePair<string, object> entry in FieldsChanged)
            {
                if (entry.Value != null && entry.Value.ToString() == EntityNewField)
                    _Data.Remove(entry.Key);
                else
                    _Data[entry.Key] = entry.Value;
            }

            FieldsChanged.Clear();
        }

        public Dictionary<string, object> GetFieldsChanged()
        {
            Dictionary<string, object> fc = new Dictionary<string, object>();
            foreach (string k in FieldsChanged.Keys)
            {
                fc[k] = _Data[k];
            }

            return fc;
        }

        /// <summary>
        /// Get indicate if data source has changes
        /// </summary>
        public bool IsDirty()
        {
            if (_FieldsChanged != null && _FieldsChanged.Count > 0) return true; return false;
        }

        #endregion

    }

 }
