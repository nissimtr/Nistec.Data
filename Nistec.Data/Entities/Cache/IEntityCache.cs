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
using System.Collections;
using System.Text;
using System.Data;
using Nistec.Generic;

namespace Nistec.Data.Entities.Cache
{
    public interface IEntityCache
    {
        /// <summary>
        /// Get Keys items
        /// </summary>
        List<string> DataKeys { get; }
        /// <summary>
        /// Reset cache
        /// </summary>
        void Reset();

        /// <summary>
        /// Refresh cache
        /// </summary>
        void Refresh();
    }

    public interface IEntityCache<T> : IEntityCache
    {
        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        T GetItem(params string[] key);
        /// <summary>
        /// Get Item by key with number of options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="key"></param>
        /// <returns>return null or empty if not exists</returns>
        T FindItem(params string[] key);
    }

}
