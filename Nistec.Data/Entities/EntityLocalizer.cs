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
using System.Linq;
using System.Text;
using System.Globalization;
using System.Resources;
using Nistec.Generic;
using Nistec.Runtime;

namespace Nistec.Data.Entities
{
   
    #region DynamicEntityLang
    /// <summary>
    /// DynamicEntityLang
    /// </summary>
    public sealed class DynamicEntityLocalizer : EntityLocalizer
    {
        string currentCulture;
        public DynamicEntityLocalizer(string cultuer,string resource, Type type)
        {
            currentCulture = cultuer;
            base.Init(cultuer, Localizer.GetResourceManager(resource, type));
        }

        internal DynamicEntityLocalizer(IEntity instance, string resource)
        {
            currentCulture = instance.EntityDb.EntityCulture.Name;
            base.Init(currentCulture, Localizer.GetResourceManager(resource, instance.GetType()));
         }

        protected override string CurrentCulture()
        {
            return currentCulture;
        }
        protected override void BindLocalizer()
        {
        }
    }
    #endregion

    #region EntityLang

    /// <summary>
    /// EntityLang
    /// </summary>
    public abstract class EntityLocalizer : Localizer//, IEntityLocalizer
    {
        public static CultureInfo DefaultCulture { get { return CultureInfo.CurrentCulture; } }

        protected abstract void BindLocalizer();
        protected abstract string CurrentCulture();
        protected void Init(string resource)
        {
            ResourceManager rm = Localizer.GetResourceManager(resource, this.GetType());
            base.Init(CurrentCulture(), rm);
        }

        protected void Init(ResourceManager rm)
        {
            base.Init(CurrentCulture(), rm);
        }

        public EntityLocalizer()
        {
            BindLocalizer();
        }
	
        #region static IEntityRM


        static ILocalizer Create<Erm>() where Erm : ILocalizer
        {
            return ActivatorUtil.CreateInstance<Erm>();
        }

        public static ILocalizer Get<Erm>() where Erm : ILocalizer
        {
            ILocalizer rm = null;
            string name = typeof(Erm).Name;
            if (!Hash.TryGetValue(name, out rm))
            {
                rm = Create<Erm>();
                Hash[name] = rm;
            }
            return rm;
        }

        private static Dictionary<string, ILocalizer> m_hash;

        private static Dictionary<string, ILocalizer> Hash
        {
            get
            {
                if (m_hash == null)
                {
                    m_hash = new Dictionary<string, ILocalizer>();
                }
                return m_hash;
            }
        }
        #endregion

    }
    #endregion
}
