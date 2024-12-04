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
#pragma warning disable CS1591
namespace Nistec.Data
{

    #region STATUS  struct

    /// <summary>
    /// Enum Status Priority
    /// </summary>
    public enum StatusPriority
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal,
        /// <summary>
        /// Warnning
        /// </summary>
        Warnning,
        /// <summary>
        /// Error
        /// </summary>
        Error
    }

    /// <summary>
    /// STATUS
    /// </summary>
    public struct STATUS
    {
        /// <summary>
        /// displayStatus string
        /// </summary>
        internal string displayStatus;
        /// <summary>
        /// statusPriority
        /// </summary>
        internal StatusPriority statusPriority;
        /// <summary>
        /// STATUS ctor
        /// </summary>
        /// <param name="status"></param>
        /// <param name="priority"></param>
        public STATUS(string status, StatusPriority priority)
        {
            displayStatus = status;
            statusPriority = priority;
        }
        /// <summary>
        /// STATUS ctor
        /// </summary>
        /// <param name="status"></param>
        public STATUS(string status)
        {
            displayStatus = status;
            statusPriority = StatusPriority.Normal;
        }

        /// <summary>
        /// Status string
        /// </summary>
        public string Display
        {
            get { return displayStatus; }
        }
        /// <summary>
        /// StatusPriority
        /// </summary>
        public StatusPriority Priority
        {
            get { return statusPriority; }
        }

    }

 	#endregion

    #region BulkCopy MAPPING

    //public enum MappingType
    //{
    //    NameToName,
    //    NameToIndex,
    //    IndexToName,
    //    IndexToIndex
    //}

    /// <summary>
    /// MAPPING
    /// </summary>
    public class MAPPING
    {
        /// <summary>
        /// SourceColumnName
        /// </summary>
        public string SourceColumnName;
        /// <summary>
        /// DestColumnName
        /// </summary>
        public string DestColumnName;

        /// <summary>
        /// MAPPING ctor
        /// </summary>
        /// <param name="SourceColumnName"></param>
        /// <param name="DestColumnName"></param>
        public MAPPING(string SourceColumnName, string DestColumnName)
        {
            this.SourceColumnName = SourceColumnName;
            this.DestColumnName = DestColumnName;
        }
        /// <summary>
        /// MAPPING ctor with same name for source and dest
        /// </summary>
        /// <param name="ColumnName"></param>
        public MAPPING(string ColumnName)
        {
            this.SourceColumnName = ColumnName;
            this.DestColumnName = ColumnName;
        }
        /// <summary>
        /// Create MAPPING array list
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static MAPPING[] Create(params string[] columns)
        {
            MAPPING[] maps=new MAPPING[columns.Length];
            for (int i=0;i< columns.Length;i++)
            {
                maps[i] = new MAPPING(columns[i]);
            }
            return maps;
        }

   
    }

    #endregion

    #region CONSTRAINT

    /// <summary>
	/// CONSTRAINT
	/// </summary>
	public struct CONSTRAINT
	{
		/// <summary>
		/// Constraint Name
		/// </summary>
		public string Name;
		/// <summary>
		/// Constraint Columns Name
		/// </summary>
		public string[] ColumnsName;
		/// <summary>
		/// Is PrimeryKey
		/// </summary>
		public bool PrimeryKey;
		/// <summary>
		/// CONSTRAINT Ctor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="columns"></param>
		/// <param name="isPriery"></param>
		public CONSTRAINT(string name ,string[] columns,bool isPriery)
		{
			Name = name;
			ColumnsName = columns;
			PrimeryKey = isPriery;
		}
		/// <summary>
		/// SetConstraint
		/// </summary>
		/// <param name="dt"></param>
		/// <param name="constraint"></param>
		public static void SetConstraint(DataTable dt, CONSTRAINT[] constraint)
		{
			foreach (CONSTRAINT c in constraint)
			{
				int i = 0;
				DataColumn[] dc = new DataColumn[c.ColumnsName.Length];
				foreach (string s in c.ColumnsName)
				{
					dc[i] = dt.Columns[s];
					i++;
				}
				dt.Constraints.Add(c.Name, dc, c.PrimeryKey);
			}
		}
	}

	#endregion 

}
