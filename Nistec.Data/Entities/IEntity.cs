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

using System.Runtime.InteropServices;
//using Nistec.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using Nistec.Serialization;
using Nistec.Generic;
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{

    public interface IGenericEntity : IEntityDictionary, IEntityItem
    {

        /// <summary>
        /// Gets the number of key/value pairs contained in the System.Collections.Hashtable.
        /// </summary>
        int Count { get; }


        /// <summary>
        ///  Determines whether the System.Collections.Hashtable contains a specific key.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        bool ContainsKey(string field);

        /// <summary>
        ///  Determines whether the System.Collections.Hashtable contains a specific value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool ContainsValue(object value);

        /// <summary>
        ///  Removes all elements from the System.Collections.Hashtable.
        /// </summary>
        void Clear();

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <returns></returns>
        object GetValue(string field);


        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>T</returns>
        T GetValue<T>(string field);

        /// <summary>
        /// Load entity
        /// </summary>
        /// <param name="dr"></param>
        void Load(DataRow dr);

        /// <summary>
        /// Load entity
        /// </summary>
        /// <param name="dic"></param>
        void Load(IDictionary dic);

        KeySet PrimaryKey { get; }
    }

    public interface IActiveEntity : IEntity
    {
        Dictionary<string, object> GetFieldsChanged();
        GenericDataTable EntityDataSource { get; set; }
    }

    public interface IKeyValueItem
    {
        string Key { get; set; }
        object Value { get; set; }
        //Int64 Index { get; set; }
    }
    public interface IKeyValueItem<T>
    {
        string Key { get; set; }
        T Value { get; set; }
    }
    public interface IEntityItem
    {

    }

    public interface IEntityListItem
    {
        int TotalRows { get; set; }
    }

    public interface IEntityData : IEntityItem
    {
        [EntityProperty(EntityPropertyType.NA)]
        string MappingName { get; set; }
    }
    public interface IEntityDb : IEntityItem
    {
        EntityDbContext EntityDb { get; set; }
    }

    public interface IEntityFields : IEntityDb
    {
        GenericRecord EntityRecord { get; set; }
        EntityProperties EntityProperties { get; }
        EntityFieldsChanges GetFieldsChanged();
    }

    public interface IEntity : IEntityDb, IEntityDictionary,IEntityItem
    {
        GenericRecord EntityRecord { get; set; }

        EntityProperties EntityProperties { get; }

        //IDictionary EntityDictionary(DataFilter filter);
    }

    public interface IEntity<T> : IEntity
    {
        
        /// <summary>
        /// Set the current instance of <see cref="EntityContext"/>
        /// </summary>
        /// <param name="instance"></param>
        void Set(T instance);

        /// <summary>
        /// Get the current entity
        /// </summary>
        T Entity { get; }

        List<T> EntityList(DataFilter filter);

    }

    public interface IEntityList : IList
    {

    }

    public interface IEntityList<T> : IList<T>, IEntityList
    {

    }
      

    public interface IDataEntity : IEntity
    {
        bool IsEmpty { get; }

        GenericDataTable EntityDataSource { get; set; }

        void Refresh();

        int SaveChanges();
    }

    public interface IPersistEntity : IEntityItem
    {
        string key { get; }
        //public object body { get; set; }
        string name { get; }
        DateTime timestamp { get; }

        object value();
    }
}
