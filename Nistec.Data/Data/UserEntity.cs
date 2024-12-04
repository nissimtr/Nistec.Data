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
using Nistec.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data
{

    public enum UserState
    {
        UnAuthorized = 0, //--0=auth faild
        IpNotAlowed = 1,//--1=ip not alowed
        EvaluationExpired = 2,//--2=Evaluation expired
        Blocked = 3,//--3=account blocked
        NonConfirmed = 4,//--4=non confirmed
        Succeeded = 10//--10=ok
    }

    /// <summary>
    /// Interface provided by the "SignedUser" model. 
    /// </summary>
    public interface IUser
    {
        int State { get; set; }

        string DisplayName { get; set; }
        int UserId { get; set; }
        int UserRole { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
        string Phone { get; set; }
        int AccountId { get; set; }
        string Lang { get; set; }
        int Evaluation { get; set; }
        bool IsBlocked { get; set; }
        DateTime Creation { get; set; }

        string UserData();

        bool IsAuthenticated{ get;}
       
        bool IsAdmin{ get;}
       
    }


}
