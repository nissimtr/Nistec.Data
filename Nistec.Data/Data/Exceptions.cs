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
using System.Runtime.Serialization;
#pragma warning disable CS1591
namespace Nistec.Data
{
	/// <summary>
	/// Exception that DBLayer can raise
	/// </summary>
    [Serializable]
	public sealed class DalException : ApplicationException 
	{

		/// <summary>
		/// A constructor without parameters
		/// </summary>
		internal DalException () {}

		/// <summary>
		/// A constructor with a message parameter
		/// </summary>
		/// <param name="msg">Message parameter</param>
        public DalException(string msg) : base(msg) { }

		/// <summary>
		/// A constructor with message and inner exception parameters
		/// </summary>
		/// <param name="msg">Message parameter</param>
		/// <param name="inner">Inner exception</param>
		public DalException (string msg, Exception inner) : base (msg, inner) { }

        /// <summary>
        /// A constructor for Serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public DalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
	}
}
