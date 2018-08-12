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
using Nistec.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Data.Entities
{
    public interface IPersistItem: IEntityItem
    {
        // byte[] Serilaize();
        string key { get;}
    }

    public class PersistItem : IPersistItem, IPersistEntity
    {
        public string key { get; set; }
        public object body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }

        public object value()
        {
            return body;
        }
    }

    public class PersistTextItem : IPersistItem
    {
        public string key { get; set; }
        public string body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class PersistBinaryItem : IPersistItem
    {
        public string key { get; set; }
        public byte[] body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class BagItem : IPersistItem
    {
        public string key { get; set; }
        public string body { get; set; }
        public int state { get; set; }
        public DateTime timestamp { get; set; }
    }
}
