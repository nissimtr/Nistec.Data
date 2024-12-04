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
using Nistec.Generic;
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
#pragma warning disable CS1591
namespace Nistec.Data.Entities
{

   
    /// <summary>
    /// Represent db context for entities.
    /// </summary>
    public class EntityDbArgs : IDisposable, ISerialEntity
    {

        #region ctor
        /// <summary>
        /// Crate a new instance of <see cref="EntityDbArgs"/>
        /// </summary>
        public EntityDbArgs()
        {
            Columns = "*";
            SourceType = EntitySourceType.Table;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbArgs"/>
        /// </summary>
        /// <param name="attr"></param>
        public EntityDbArgs(EntityAttribute attr)
            : this()
        {
            EntityName = attr.EntityName;
            MappingName = attr.MappingName;
            ConnectionKey = attr.ConnectionKey;
            SourceType = attr.EntitySourceType;
            Keys = attr.EntityKey;
            Columns = attr.Columns;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbArgs"/>
        /// </summary>
        /// <param name="mappingName"></param>
        /// <param name="connectionKey"></param>
        /// <param name="columns"></param>
        public EntityDbArgs(string mappingName,string connectionKey, string columns="*")
            : this()
        {
            EntityName = mappingName;
            MappingName = mappingName;
            ConnectionKey = connectionKey;
            Keys = null;
            Columns = columns;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbArgs"/>
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="connectionKey"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        public EntityDbArgs(string entityName, string mappingName, string connectionKey,EntitySourceType sourceType, EntityKeys keys, string columns = "*")
        {
            EntityName = entityName;
            MappingName = mappingName;
            ConnectionKey = connectionKey;
            SourceType = sourceType;
            Keys = keys.ToArray();
            Columns = columns;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbArgs"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entityName"></param>
        /// <param name="mappingName"></param>
        /// <param name="sourceType"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        public EntityDbArgs(IDbContext db, string entityName, string mappingName, EntitySourceType sourceType, EntityKeys keys, string columns = "*")
        {
            EntityName = entityName;
            MappingName = mappingName;
            _Db = db;
            ConnectionKey = db.ConnectionName;
            SourceType = sourceType;
            Keys = keys.ToArray();
            Columns = columns;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbArgs"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        public EntityDbArgs(IDbContext db, string mappingName, EntityKeys keys, string columns = "*")
            : this()
        {
            this.MappingName = mappingName;
            this.EntityName = mappingName;
            this._Db = db;
            this.ConnectionKey = db.ConnectionName;
            this.Keys = keys.ToArray();
            Columns = columns;
        }
        /// <summary>
        ///  Crate a new instance of <see cref="EntityDbArgs"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="mappingName"></param>
        /// <param name="keys"></param>
        /// <param name="columns"></param>
        public EntityDbArgs(IDbContext db, string mappingName, KeySet keys, string columns = "*")
            : this()
        {
            this.MappingName = mappingName;
            this.EntityName = mappingName;
            this._Db = db;
            this.ConnectionKey = db.ConnectionName;
            this.Keys = keys.Keys.ToArray();
            Columns = columns;

        }
        #endregion

        #region Dispose

        private bool disposed = false;

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_Db != null)
                    {
                        _Db.Dispose();
                        _Db = null;
                    }
                    EntityName = null;
                    MappingName = null;
                    ConnectionKey = null;
                    Keys = null;
                    m_EntityKeys = null;
                }
                disposed = true;
            }
        }
      
        /// <summary>
        /// This object will be cleaned up by the Dispose method. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // take this object off the finalization queue     
            GC.SuppressFinalize(this);
        }

        //~DbContext()
        //{
        //    Dispose(false);
        //}

        #endregion

        #region properties

        string _Columns;

        IDbContext _Db;
        /// <summary>
        /// Get <see cref="IDbContext"/> Context.
        /// </summary>
        /// <returns></returns>
        public IDbContext Context()
        {
            if (_Db == null)
            {
                _Db = DbContext.Create(ConnectionKey);
            }
            return _Db;
        }
        /// <summary>
        /// Get or Set entity name.
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// Get or Set mapping name.
        /// </summary>
        public string MappingName { get; set; }
        /// <summary>
        /// Get or Set columns names.
        /// </summary>
        public string Columns
        {
            get { return _Columns == null || _Columns == "" ? "*" : _Columns; }
            set { _Columns = value == null || value == "" ? "*" : value; }
        }
        /// <summary>
        /// Get or Set connection key.
        /// </summary>
        public string ConnectionKey { get; set; }
        /// <summary>
        /// Get or Set entity keys.
        /// </summary>
        public string[] Keys { get; set; }
        /// <summary>
        /// Get or Set <see cref="EntitySourceType"/> source type.
        /// </summary>
        public EntitySourceType SourceType { get; set; }

        internal CommandType CmdType()
        {
            if (SourceType == Entities.EntitySourceType.Procedure)
            {
                return CommandType.StoredProcedure;
            }
            return CommandType.Text;
        }
        
        CultureInfo m_CultureInfo;

        /// <summary>
        /// Get or Set current culture
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        [NoSerialize]
        public virtual CultureInfo EntityCulture
        {
            get
            {
                if (m_CultureInfo == null)
                    return EntityLocalizer.DefaultCulture;
                return m_CultureInfo;
            }
            set { m_CultureInfo = value; }
        }

        EntityKeys m_EntityKeys;
        /// <summary>
        /// Get <see cref="EntityKeys"/>.
        /// </summary>
        public EntityKeys EntityKeys
        {
            get
            {
                if (m_EntityKeys == null)
                {
                    m_EntityKeys = Keys == null ? new EntityKeys() : new EntityKeys(Keys);
                }
                return m_EntityKeys;
            }
            //set { m_EntityKeys = value; }
        }

        /// <summary>
        /// Get indicate if entity has connection properties
        /// </summary>
        [EntityProperty(EntityPropertyType.NA)]
        public bool HasConnection
        {
            get
            {
                if ((!string.IsNullOrEmpty(ConnectionKey) || (_Db != null && _Db.HasConnection)) 
                    && !string.IsNullOrEmpty(MappingName))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Get or Set entity args.
        /// </summary>
        public KeyValueArgs Args { get; set; }

        public object[] GetKeyValueArray()
        {
            return Args == null ? null : Args.ToKeyValueArray();
        }

        #endregion

        #region  ISerialEntity


        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteString(EntityName);
            streamer.WriteValue(MappingName);
            streamer.WriteString(ConnectionKey);
            streamer.WriteValue(Keys);
            streamer.WriteValue((int)SourceType);
            streamer.WriteString(Columns);
            streamer.WriteValue(Args);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            EntityName = streamer.ReadString();
            MappingName = streamer.ReadString();
            ConnectionKey = streamer.ReadString();
            Keys = streamer.ReadValue<string[]>();
            SourceType = (EntitySourceType)streamer.ReadValue<int>();
            Columns = streamer.ReadString();
            Args = (KeyValueArgs)streamer.ReadValue();
        }
 
        #endregion

    }
}
