﻿//licHeader
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
using System.Data.OleDb;
using System.IO;
//using Nistec.Data.Common;
using System.Linq;
using Nistec.Data.Factory;
//using Nistec.Printing.Sections;


namespace Nistec.Data.OleDb
{
  
    [Serializable]
    public sealed class DbOleCmd : DbFactory
	{
		
		#region Ctor

        public DbOleCmd() { }

        public DbOleCmd(OleDbConnection cnn)
            : base(cnn)
        {
        }

        public DbOleCmd(string connectionString)
            : base(connectionString, DBProvider.OleDb)
        {
        }

       
		#endregion
        		
       
    }
}

