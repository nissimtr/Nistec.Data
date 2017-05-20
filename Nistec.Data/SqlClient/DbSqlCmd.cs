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
using System.IO;
using System.Linq;
using Nistec.Data.Factory;


namespace Nistec.Data.SqlClient
{
 
    [Serializable]
    public sealed class DbSqlCmd : DbFactory
	{
		

        public DbSqlCmd() { }

        public DbSqlCmd(SqlConnection cnn)
            : base(cnn)
        {
            //conn = cnn;
        }

        public DbSqlCmd(string connectionString)
            : base(connectionString, DBProvider.SqlServer)
        {
        }
       
       
	}
}

