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
using System.Collections;

#pragma warning disable CS1591
namespace Nistec.Data.Advanced
{
    /// <summary>
    /// Relation
    /// </summary>
    public class Relation
    {
        private Relation()
        {
            throw new ArgumentException("Could Not Use empty relation");
        }
        /// <summary>
        /// Initilaized new Relation instance
        /// </summary>
        /// <param name="parentColumnName"></param>
        /// <param name="childColumnName"></param>
        public Relation(string parentColumnName, string childColumnName)
            : this(new string[] { parentColumnName }, new string[] { childColumnName })
        {

        }
        /// <summary>
        /// Initilaized new Relation instance
        /// </summary>
        /// <param name="parentColumnsName"></param>
        /// <param name="childColumnsName"></param>
        public Relation(string[] parentColumnsName, string[] childColumnsName)
            : this(parentColumnsName, childColumnsName, null)
        { }
        /// <summary>
        /// Initilaized new Relation instance
        /// </summary>
        /// <param name="parentColumnsName"></param>
        /// <param name="childColumnsName"></param>
        /// <param name="foreignKey"></param>
        public Relation(string[] parentColumnsName, string[] childColumnsName, string foreignKey)
            : this(parentColumnsName, childColumnsName, foreignKey, null)
        { }
        /// <summary>
        /// Initilaized new Relation instance
        /// </summary>
        /// <param name="parentColumnsName"></param>
        /// <param name="childColumnsName"></param>
        /// <param name="foreignKey"></param>
        /// <param name="tbleMapping"></param>
        public Relation(string[] parentColumnsName, string[] childColumnsName, string foreignKey, ITableMapping tbleMapping)
        {
            if (parentColumnsName.Length != childColumnsName.Length)
            {
                throw new ArgumentException("DataRelation_KeyLengthMismatch");
            }

            _ParentColumnsName = parentColumnsName;
            _ChildColumnsName = childColumnsName;
            _ForiegnKey = foreignKey;
            tableMapping = tbleMapping;

  
        }
        /// <summary>
        /// Convert Data Relation
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static Relation[] DataRelationConvert(DataSet ds)
        {

            System.Data.DataRelationCollection relations = ds.Relations;
            if (relations == null || relations.Count == 0)
            {
                return null;
            }
            Relation[] rels = new Relation[ds.Relations.Count];
            string[] colsP = null;
            string[] colsC = null;

            for (int i = 0; i < rels.Length; i++)
            {
                colsP = new string[relations[i].ParentColumns.Length];
                for (int j = 0; j < relations[i].ParentColumns.Length; j++)
                {
                    colsP[j] = relations[i].ParentColumns[j].ColumnName;
                }
                colsC = new string[relations[i].ChildColumns.Length];
                for (int j = 0; j < relations[i].ChildColumns.Length; j++)
                {
                    colsC[j] = relations[i].ParentColumns[j].ColumnName;
                }

                rels[i] = new Relation(colsP, colsC);
            }
            return rels;
        }
        /// <summary>
        /// Get Parent Columns Name
        /// </summary>
        public string[] ParentColumnsName
        {
            get { return _ParentColumnsName; }
        }
        /// <summary>
        /// Get Child Columns Name
        /// </summary>
        public string[] ChildColumnsName
        {
            get { return _ChildColumnsName; }
        }
        /// <summary>
        /// Get or Set Relation Name
        /// </summary>
        public string RelationName
        {
            get { return _RelationName; }
            set { _RelationName = value; }
        }
        /// <summary>
        /// Get Relation Foriegn Key
        /// </summary>
        public string ForiegnKey
        {
            get { return _ForiegnKey; }
        }
        /// <summary>
        /// Get or Set Command Select
        /// </summary>
        public string CommandSelect
        {
            get { return _commandSelect; }
            set { _commandSelect = value; }
        }
        /// <summary>
        /// Get Relation Table Mapping
        /// </summary>
        public ITableMapping TableMapping
        {
            get { return tableMapping; }
        }
      

        internal string _RelationName;
        internal string[] _ParentColumnsName;
        internal string[] _ChildColumnsName;
        internal string _ForiegnKey;
        internal string _commandSelect;
        internal System.Data.ITableMapping tableMapping; //=new TableMapping();
        
 
    }
    /// <summary>
    /// Represent Relation Collection
    /// </summary>
    public class RelationCollection : CollectionBase
    {
        /// <summary>
        /// Add item to Relation Collection
        /// </summary>
        /// <param name="rl"></param>
        public void Add(Relation rl)
        {
            base.List.Add(rl);
        }
        /// <summary>
        /// Remove Relation from collection
        /// </summary>
        /// <param name="rl"></param>
        public void Remove(Relation rl)
        {
            base.List.Remove(rl);
        }

        /// <summary>
        /// Get item Relation
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Relation this[int index]
        {
            get { return (Relation)base.List[index] as Relation; }
        }

        /// <summary>
        /// Get item Relation
        /// </summary>
        /// <param name="relationName"></param>
        /// <returns></returns>
        public Relation this[string relationName]
        {
            get
            {
                int i = 0;
                foreach (Relation r in this.List)
                {
                    if (r._RelationName.Equals(relationName))
                    { break; }
                    i++;
                }
                return (Relation)base.List[i] as Relation;
            }
        }

    }

}
