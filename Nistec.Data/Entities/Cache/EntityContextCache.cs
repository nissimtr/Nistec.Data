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
using Nistec.Runtime;

namespace Nistec.Data.Entities.Cache
{
   
    /// <summary>
    /// Represent Entity Context Cache wrapper that contains fast search mechanizem.
    /// </summary>
    public class EntityContextCache<T> : EntityCache<string, T>
    {
        public EntityContextCache(EntityKeys info)
            : base(1)
        {
            base.DataKeys.AddRange(info);
            InitCache();
        }

       
        public EntityContextCache()
            : base(1)
        {
            T instance = ActivatorUtil.CreateInstance<T>();//Activator.CreateInstance<T>();

            EntityAttribute keyattr = AttributeProvider.GetCustomAttribute<EntityAttribute>(instance.GetType());
            string[] keys = keyattr.EntityKey;
            base.DataKeys.AddRange(keys);
            IDictionary dt = EntityExtension.CreateEntityList((IEntity<T>)instance, null, OnError);// ((IEntity<T>)instance).EntityDictionary(null, OnError);
         }
         
        protected override void InitCache()
        {
            T instance = ActivatorUtil.CreateInstance<T>();//Activator.CreateInstance<T>();
            IDictionary dt = EntityExtension.CreateEntityList((IEntity<T>)instance, null, OnError);//((IEntity<T>)instance).EntityDictionary(null, OnError);
            base.CreateCache(dt);
        }

    }
}
