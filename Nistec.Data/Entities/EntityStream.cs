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
using System.IO;
using Nistec.Generic;
using System.Data;
using System.Xml;
using System.Collections;
using Nistec.Runtime;
using Nistec.IO;
using Nistec.Serialization;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Collections.Specialized;

namespace Nistec.Data.Entities
{
    /// <summary>
    /// Represent entity as stream, implement <see cref="ISerialEntity"/> and <see cref="IMessageStream"/>.
    /// </summary>
    [Serializable]
    public class EntityStream : ISerialEntity, ISerialJson, IBodyStream, IDisposable// IMessageStream,
    {


        #region properties
        /// <summary>
        /// Get or Set BodyStream.
        /// </summary>
        //NetStream _BodyStream;
        public NetStream BodyStream {get; set; }//{ get { return _BodyStream==null? null: _BodyStream.Ready(); } set { _BodyStream = value; Modified = DateTime.Now; } }
        /// <summary>
        /// Get or Set entity key.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Get or Set type name of body stream.
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// Get or Set the foramtter of body stream.
        /// </summary>
        public Formatters Formatter { get; set; }
        /// <summary>
        /// Get or Set entity expirtaion.
        /// </summary>
        public int Expiration { get; set; }
        /// <summary>
        /// Get or Set the last time modefied.
        /// </summary>
        public DateTime Modified { get; set; }
        /// <summary>
        /// Get or Set entity detail.
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// Get or Set entity GroupId.
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// Get or Set The result type name.
        /// </summary>
        public TransformType TransformType { get; set; }

        public static EntityStream Empty
        {
            get
            {
                return new EntityStream();
            }
        }
   
        int GetSize()
        {
            if (BodyStream == null)
                return 0;
            return BodyStream.iLength;
        }

        /// <summary>
        /// Get Body Size in Bytes
        /// </summary>
        public int Size
        {
            get { return GetSize(); }
        }


        /// <summary>
        /// Get indicate wether the item is empty 
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                 return BodyStream == null || BodyStream.Length==0;
            }
        }

        /// <summary>
        /// Get Type of body
        /// </summary>
        public Type BodyType
        {
            get
            {
                return SerializeTools.GetQualifiedType(TypeName);
            }
        }

        public bool IsKnownType
        {
            get
            {
                return !string.IsNullOrEmpty(TypeName) && !typeof(object).Equals(BodyType);
            }
        }

        ///// <summary>
        ///// Get Type of return type
        ///// </summary>
        //public Type ReturnType
        //{
        //    get
        //    {
        //        return SerializeTools.GetQualifiedType(ReturnTypeName);
        //    }
        //}
        #endregion

        #region  ISerialEntity

        /// <summary>
        /// Write entity properties to stream using <see cref="IBinaryStreamer"/> streamer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream,true);

            streamer.WriteString(Id);
            streamer.WriteValue(BodyStream);
            streamer.WriteString(TypeName);
            streamer.WriteValue((int)Formatter);
            streamer.WriteString(Label);
            streamer.WriteString(GroupId);
            streamer.WriteValue(Expiration);
            streamer.WriteValue(Modified);
            streamer.WriteValue((byte)TransformType);
            streamer.Flush();

        }
       
        /// <summary>
        /// Read entity properties from stream using <see cref="IBinaryStreamer"/> streamer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream, true);
        
            Id = streamer.ReadString();
            BodyStream = (NetStream)streamer.ReadValue();
            TypeName = streamer.ReadString();
            Formatter = (Formatters)streamer.ReadValue<int>();
            Label = streamer.ReadString();
            GroupId = streamer.ReadString();
            Expiration = streamer.ReadValue<int>();
            Modified = streamer.ReadValue<DateTime>();
            TransformType = (TransformType)streamer.ReadValue<byte>();
        }

        #endregion

        #region ISerialJson

        public string EntityWrite(IJsonSerializer serializer, bool pretty=false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);


            object body = null;
            if(BodyStream!=null)
            {
                body = BinarySerializer.ConvertFromStream(BodyStream);
            }

            serializer.WriteToken("Id", Id);
            serializer.WriteToken("BodyStream", BodyStream == null ? null : BodyStream.ToBase64String());
            serializer.WriteToken("TypeName", TypeName);
            serializer.WriteToken("Formatter", Formatter);
            serializer.WriteToken("Label", Label, null);
            serializer.WriteToken("GroupId", GroupId, null);
            serializer.WriteToken("Expiration", Expiration);
            serializer.WriteToken("Modified", Modified);
            serializer.WriteToken("TransformType", TransformType);
            serializer.WriteToken("Body", body);

            return serializer.WriteOutput(pretty);
            //return serializer.Write( this, this.GetType().BaseType);
        }

        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);

            var dic = serializer.Read<Dictionary<string, object>>(json);

            if (dic != null)
            {
                Id = dic.Get<string>("Id");
                var body = dic.Get<string>("BodyStream");
                TypeName = dic.Get<string>("TypeName");
                Formatter = dic.Get<Formatters>("Formatter");
                Label = dic.Get<string>("Label");
                GroupId = dic.Get<string>("GroupId");
                Expiration = dic.Get<int>("Expiration");
                Modified = dic.Get<DateTime>("Modified");
                TransformType = (TransformType)dic.Get<byte>("TransformType");
                if (body != null && body.Length > 0)
                    BodyStream = NetStream.FromBase64String(body);
            }

            return this;
        }

        public object EntityRead(NameValueCollection queryString, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);

            if (queryString != null)
            {
                Id = queryString.Get<string>("Id");
                var body = queryString.Get<string>("BodyStream");
                TypeName = queryString.Get<string>("TypeName");
                Formatter = queryString.GetEnum<Formatters>("Formatter", Formatters.Json);
                Label = queryString.Get<string>("Label");
                GroupId = queryString.Get<string>("GroupId");
                Expiration = queryString.Get<int>("Expiration");
                Modified = queryString.Get<DateTime>("Modified", DateTime.Now);
                TransformType = (TransformType)queryString.GetEnum<TransformType>("TransformType", TransformType.Object);
                if (body != null && body.Length > 0)
                    BodyStream = NetStream.FromBase64String(body);
            }

            return this;
        }

        #endregion

        #region IBodyFormatter

        public NetStream GetStream()
        {
            if (BodyStream == null)
                return null;
            return BodyStream.Ready();
        }

        public NetStream GetCopy()
        {
            if (BodyStream == null)
                return null;
            return BodyStream.Copy();
        }
        public byte[] GetBinary()
        {
            if (BodyStream == null)
                return null;
            return BodyStream.ToArray();
        }
        public virtual void SetBody(object value)
        {
            if (value != null)
            {
 
                TypeName = value.GetType().FullName;
                
                NetStream ns = new NetStream();

                BinaryStreamer streamer = new BinaryStreamer(ns);
                streamer.Encode(value);
                
                ns.Position = 0;
                BodyStream = ns;
            }
            else
            {
                TypeName = typeof(object).FullName;
                BodyStream = null;
            }
        }

        public virtual void SetBody(byte[] value, Type type)
        {
            if (type != null)
            {
                TypeName = type.FullName;
            }
            else
            {
                TypeName = typeof(object).FullName;
            }
           
            if (value != null)
            {
                BodyStream = new NetStream(value);
            }
         }

        public virtual object DecodeBody()
        {
            if (BodyStream == null)
                return null;
            BinaryStreamer streamer = new BinaryStreamer(BodyStream,true);
            return streamer.Decode();
        }
        public virtual T DecodeBody<T>()
        {
            if (BodyStream == null)
                return default(T);
            BinaryStreamer streamer = new BinaryStreamer(BodyStream, true);
            return streamer.Decode<T>(true);
        }

        public static object ReadBodyStream(Type type, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("ReadBodyStream.stream");
            }
            if (type == null)
            {
                throw new ArgumentNullException("ReadBodyStream.type");
            }

            BinaryStreamer streamer = new BinaryStreamer(stream,true);
            return streamer.Decode();

        }

        public static void WriteBodyStream(object entity, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("WriteBodyStream.stream");
            }
            if (entity == null)
            {
                throw new ArgumentNullException("WriteBodyStream.entity");
            }
            Type type = entity.GetType();
 
            BinaryStreamer streamer = new BinaryStreamer(stream);
            streamer.Encode(entity);
            streamer.Flush();
        }

        #endregion

        #region body read/write
 
        public void SetBody(byte[] value, string typeName)
        {
            TypeName = (!string.IsNullOrEmpty(typeName)) ? typeName : typeof(object).FullName;
            if (value != null)
            {
                BodyStream = new NetStream(value);
            }
        }

        public void SetBody(NetStream ns, string typeName, bool copy=true)
        {
            TypeName = (!string.IsNullOrEmpty(typeName)) ? typeName : typeof(object).FullName;
            if (ns != null)
            {
                if (copy)
                {
                    BodyStream = ns.Copy();
                }
                else
                    BodyStream = ns;
            }
        }

        #endregion

        #region Convert

        /// <summary>
        /// Get Copy of <see cref="EntityStream"/> with body.
        /// </summary>
        /// <returns></returns>
        public virtual EntityStream Copy()
        {
            //NetStream ns = null;
            //if (copyBody && BodyStream != null)
            //    ns = BodyStream.Copy();

            EntityStream es = new EntityStream()
            {
                BodyStream = GetCopy(),
                Expiration = this.Expiration,
                Label = this.Label,
                GroupId = this.GroupId,
                Formatter = this.Formatter,
                Id = this.Id,
                Modified = this.Modified,
                TypeName = this.TypeName,
                TransformType = this.TransformType
            };
            return es;
        }

        /// <summary>
        /// Get Copy of <see cref="EntityStream"/> structure without body.
        /// </summary>
        /// <returns></returns>
        public virtual EntityStream Clone()
        {
            EntityStream es = new EntityStream()
            {
                //BodyStream = GetCopy(),
                Expiration = this.Expiration,
                Label = this.Label,
                GroupId = this.GroupId,
                Formatter = this.Formatter,
                Id = this.Id,
                Modified = this.Modified,
                TypeName = this.TypeName,
                TransformType = this.TransformType
            };
            return es;
        }

        public string BodyToBase64()
        {
            if (BodyStream == null)
                return null;
            return BodyStream.ToBase64String();
        }

        public void BodyFromBase64(string base64)
        {
            if (base64 != null)
            {
                BodyStream = NetStream.FromBase64String(base64);
            }
        }
        public string BodyToJson(bool pretty = false)
        {
            if (BodyStream == null)
                return null;
            var o = BinarySerializer.Deserialize(BodyStream.ToArray());
            var json=JsonSerializer.Serialize(o, pretty);
            return json;
        }
        #endregion

        #region ctor

        public EntityStream()
        {
            Modified = DateTime.Now;
            TypeName = typeof(object).FullName;
            Formatter = Formatters.BinarySerializer;
        }

        public EntityStream(Formatters formatter)
        {
            Modified = DateTime.Now;
            TypeName = typeof(object).FullName;
            Formatter = formatter;
        }
        public EntityStream(string key, object value, Formatters formatter = Formatters.BinarySerializer)
        {
            Modified = DateTime.Now;
            Id = key;
            Formatter = formatter;
            SetBody(value);
        }

        public EntityStream(string key, byte[] value, Type type, Formatters formatter = Formatters.BinarySerializer)
        {
            Modified = DateTime.Now;
            Id = key;
            Formatter = formatter;
            SetBody(value, type);
        }

        protected EntityStream(EntityStreamState state)
            : this()
        {
            if (state == EntityStreamState.None)
            {
                throw new ArgumentException("EntityStreamState.None is not valid");
            }
            BodyStream = new NetStream(new byte[] { (byte)state });
        }
        #endregion

        #region Dispose

        ~EntityStream()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        protected bool IsDisposed
        {
            get { return disposed; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Id = null;
                TypeName = null;
                if (BodyStream != null)
                {
                    BodyStream.Dispose();
                    BodyStream = null;
                }
                disposed = true;
            }
        }
        #endregion

        #region CreateEntityStream

        /// <summary>
        /// Create Dictionary of <see cref="EntityStream"/> using <see cref="IEntity"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="filter"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static ConcurrentDictionary<string, EntityStream> CreateConcurrentEntityRecordStream<T>(IEntity<T> context, DataFilter filter, out long totalSize) //where T : GenericEntity
        {
            totalSize = 0;

            if (context == null)
            {
                throw new ArgumentNullException("CreateEntityStream.IEntity.context");
            }

            context.EntityDb.ValidateContext();

            Type type = typeof(T);
            ConcurrentDictionary<string, EntityStream> values = new ConcurrentDictionary<string, EntityStream>();

            long size = 0;
           
            if (typeof(GenericRecord).IsAssignableFrom(typeof(T)) || typeof(EntityStream).IsAssignableFrom(typeof(T)))
            {

                string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();

                GenericEntity[] records = GenericEntity.CreateEntities(context.EntityDb.DoCommand<DataTable>(filter), fieldsKey);

                foreach (var gr in records)
                {

                    if (gr.PrimaryKey == null)
                    {
                        throw new Exception("Invalid PrimaryKey for entity: " + context.EntityDb.EntityName);
                    }
                    string key = gr.PrimaryKey.ToString();

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }

                    var es = new EntityStream(key, gr.Record);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key]=es;
                }
                totalSize = size;
            }
            else if (typeof(IEntityItem).IsAssignableFrom(type))
            {
                List<T> items = context.EntityList(filter);

                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in items)
                {

                    //string key = entityKeys.CreateEntityPrimaryKey(gr);
                    string key = EntityPropertyBuilder.GetEntityPrimaryKey((IEntityItem)gr);
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }
                    var es = new EntityStream(key, gr);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key] = es;
                }
                totalSize = size;
            }
            else
            {
                throw new ArgumentNullException("EntityDictionary<T> type not supported: " + type.ToString());
            }
            if (values.Count == 0)
            {
                return null;
            }
            return values;

        }

        /// <summary>
        /// Create Dictionary of <see cref="EntityStream"/> using <see cref="IEntity"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="filter"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, EntityStream> CreateEntityRecordStream<T>(IEntity<T> context, DataFilter filter, string[] fieldsKey, out long totalSize) //where T : GenericEntity
        {
            totalSize = 0;

            if (context == null)
            {
                throw new ArgumentNullException("CreateEntityStream.IEntity.context");
            }

            context.EntityDb.ValidateContext();

            Type type = typeof(T);
            Dictionary<string, EntityStream> values = new Dictionary<string, EntityStream>();

            long size = 0;

            if (typeof(GenericRecord).IsAssignableFrom(typeof(T)) || typeof(EntityStream).IsAssignableFrom(typeof(T)))
            {

                if (fieldsKey == null)
                    fieldsKey = context.EntityDb.EntityKeys.ToArray();

                GenericEntity[] records = GenericEntity.CreateEntities(context.EntityDb.DoCommand<DataTable>(filter), fieldsKey);

                foreach (var gr in records)
                {

                    if (gr.PrimaryKey == null)
                    {
                        throw new Exception("Invalid PrimaryKey for entity: " + context.EntityDb.EntityName);
                    }
                    string key = gr.PrimaryKey.ToString();

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }

                    var es = new EntityStream(key, gr.Record);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key] = es;
                }
                totalSize = size;
            }
            else if (typeof(IEntityItem).IsAssignableFrom(type))
            {
                List<T> items = context.EntityList(filter);

                if (items == null || items.Count == 0)
                {
                    throw new Exception("Invalid EntityList items");
                }

                if (fieldsKey == null)
                    fieldsKey = EntityPropertyBuilder.GetEntityPrimaryFields((IEntityItem)items[0]).ToArray();

                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in items)
                {

                    //string key = entityKeys.CreateEntityPrimaryKey(gr);

                    string key = EntityPropertyBuilder.GetEntityPrimaryKey((IEntityItem)gr,fieldsKey);

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }
                    var es = new EntityStream(key, gr);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key] = es;
                }
                totalSize = size;
            }
            else
            {
                throw new ArgumentNullException("EntityDictionary<T> type not supported: " + type.ToString());
            }
            if (values.Count == 0)
            {
                return null;
            }
            return values;

        }




        /// <summary>
        /// Create Dictionary of <see cref="EntityStream"/> using <see cref="IEntity"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="filter"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, EntityStream> CreateEntityRecordStream<T>(IEntity<T> context, DataFilter filter, out long totalSize, out string[] fieldsKey) //where T : GenericEntity
        {
            totalSize = 0;
            fieldsKey = null;

            if (context == null)
            {
                throw new ArgumentNullException("CreateEntityStream.IEntity.context");
            }

            context.EntityDb.ValidateContext();

            Type type = typeof(T);
            Dictionary<string, EntityStream> values = new Dictionary<string, EntityStream>();

            long size = 0;

            if (typeof(GenericRecord).IsAssignableFrom(typeof(T)) || typeof(EntityStream).IsAssignableFrom(typeof(T)))
            {

                fieldsKey = context.EntityDb.EntityKeys.ToArray();

                GenericEntity[] records = GenericEntity.CreateEntities(context.EntityDb.DoCommand<DataTable>(filter), fieldsKey);

                foreach (var gr in records)
                {

                    if (gr.PrimaryKey == null)
                    {
                        throw new Exception("Invalid PrimaryKey for entity: " + context.EntityDb.EntityName);
                    }
                    string key = gr.PrimaryKey.ToString();

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }

                    var es = new EntityStream(key, gr.Record);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key] = es;
                }
                totalSize = size;
            }
            else if (typeof(IEntityItem).IsAssignableFrom(type))
            {
                List<T> items = context.EntityList(filter);

                if(items==null || items.Count==0)
                {
                    throw new Exception("Invalid EntityList items");
                }

                fieldsKey = EntityPropertyBuilder.GetEntityPrimaryFields((IEntityItem)items[0]).ToArray();

                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in items)
                {

                    //string key = entityKeys.CreateEntityPrimaryKey(gr);

                    string key = EntityPropertyBuilder.GetEntityPrimaryKey((IEntityItem)gr);

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }
                    var es = new EntityStream(key, gr);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key] = es;
                }
                totalSize = size;
            }
            else
            {
                throw new ArgumentNullException("EntityDictionary<T> type not supported: " + type.ToString());
            }
            if (values.Count == 0)
            {
                return null;
            }
            return values;

        }



        /// <summary>
        /// Create Dictionary of <see cref="EntityStream"/> from <see cref="DataTable"/> using <see cref="IEntity"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, EntityStream> CreateEntityStream<T>(IEntity context, DataTable dt)
        {

            if (context == null)
            {
                throw new ArgumentNullException("CreateEntityStream.IEntity.context.");
            }
            if (dt == null)
            {
                throw new ArgumentNullException("CreateEntityStream.DataTable.dt.");
            }

            bool isGR = (typeof(T) == typeof(GenericEntity));
            GenericEntity[] records = null;
            EntityAttribute keyattr = null;

            Type type = typeof(T);
            Dictionary<string, EntityStream> values = new Dictionary<string, EntityStream>();

            if (isGR)
            {
                string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();
                records = GenericEntity.CreateEntities(dt, fieldsKey);

                foreach (var gr in records)
                {
                    T item = GenericTypes.Cast<T>(gr, true);

                    if (gr.PrimaryKey == null)
                    {
                        throw new Exception("Invalid PrimaryKey for entity: " + context.EntityDb.EntityName);
                    }
                    string key = gr.PrimaryKey.ToString();

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }
                    //values.Add(key, new EntityStream(key, item));
                    values[key]= new EntityStream(key, item);
                    //if (gr.PrimaryKey != null)
                    //{
                    //    string key = gr.PrimaryKey.ToString();
                    //    values.Add(key, new EntityStream(key, item));
                    //}
                }
            }
            else
            {
                keyattr = EntityExtension.GetEntityAttribute(context);
                records = GenericEntity.CreateEntities(dt, false);
                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in records)
                {
                    T item = ActivatorUtil.CreateInstance<T>();

                    //string key = entityKeys.CreateEntityPrimaryKey(gr);

                    string key = EntityPropertyBuilder.GetEntityPrimaryKey((IEntityItem)gr);

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }

                    //values.Add(key, new EntityStream(key, item));
                    values[key] = new EntityStream(key, item);
                }
            }
            if (values.Count == 0)
            {
                return null;
            }
            return values;
        }

        /// <summary>
        /// Create Dictionary of <see cref="EntityStream"/> using <see cref="IEntity"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="filter"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, EntityStream> CreateEntityStream<T>(IEntity<T> context, DataFilter filter, out long totalSize) //where T : GenericEntity
        {
            totalSize = 0;

            if (context == null)
            {
                throw new ArgumentNullException("CreateEntityStream.IEntity.context");
            }

            context.EntityDb.ValidateContext();

            Type type = typeof(T);
            Dictionary<string, EntityStream> values = new Dictionary<string, EntityStream>();

            long size = 0;
            if (typeof(GenericEntity).IsAssignableFrom(typeof(T)) || typeof(EntityStream).IsAssignableFrom(typeof(T)))
            {

                string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();
                
                GenericEntity[] records = GenericEntity.CreateEntities(context.EntityDb.DoCommand<DataTable>(filter), fieldsKey);

                foreach (var gr in records)
                {

                    if (gr.PrimaryKey == null)
                    {
                        throw new Exception("Invalid PrimaryKey for entity: " + context.EntityDb.EntityName);
                    }
                    string key = gr.PrimaryKey.ToString();

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }

                    var es = new EntityStream(key, gr);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key] = es;
                }
                totalSize = size;
            }
            else if (typeof(IEntityItem).IsAssignableFrom(type))
            {
                List<T> items = context.EntityList(filter);


                //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                foreach (var gr in items)
                {

                    //string key = entityKeys.CreateEntityPrimaryKey(gr);

                    string key = EntityPropertyBuilder.GetEntityPrimaryKey((IEntityItem)gr);
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                    }
                    var es = new EntityStream(key, gr);
                    size += es.Size;
                    //values.Add(key, es);
                    values[key] = es;
                }
                totalSize = size;
            }
            else
            {
                throw new ArgumentNullException("EntityDictionary<T> type not supported: " + type.ToString());
            }
            if (values.Count == 0)
            {
                return null;
            }
            return values;

        }


        /// <summary>
        /// Create Dictionary of <see cref="EntityStream"/> using <see cref="IEntity"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="filter"></param>
        /// <param name="onError"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        public static Dictionary<string, EntityStream> CreateEntityStream<T>(IEntity<T> context, DataFilter filter, Action<Exception> onError, out long totalSize) //where T : GenericEntity
        {
            totalSize = 0;
            string key = null;

            try
            {

                if (context == null)
                {
                    throw new ArgumentNullException("CreateEntityStream.IEntity.context");
                }

                context.EntityDb.ValidateContext();

                Type type = typeof(T);
                Dictionary<string, EntityStream> values = new Dictionary<string, EntityStream>();

                long size = 0;

                if (typeof(GenericEntity).IsAssignableFrom(typeof(T)) || typeof(EntityStream).IsAssignableFrom(typeof(T)))
                {

                    string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();

                    GenericEntity[] records = GenericEntity.CreateEntities(context.EntityDb.DoCommand<DataTable>(filter), fieldsKey);

                    foreach (var gr in records)
                    {
                        key = null;

                        if (gr.PrimaryKey == null)
                        {
                            throw new Exception("Invalid PrimaryKey for entity: " + context.EntityDb.EntityName);
                        }
                        key = gr.PrimaryKey.ToString();

                        if (string.IsNullOrEmpty(key))
                        {
                            throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                        }

                        var es = new EntityStream(key, gr);
                        size += es.Size;
                        //values.Add(key, es);
                        values[key] = es;
                    }
                    totalSize = size;
                }
                else if (typeof(IEntityItem).IsAssignableFrom(type))
                {
                    List<T> items = context.EntityList(filter);


                    //EntityKeys entityKeys = EntityKeys.BuildKeys<T>();

                    foreach (var gr in items)
                    {

                        //key = entityKeys.CreateEntityPrimaryKey(gr);
                        key = EntityPropertyBuilder.GetEntityPrimaryKey((IEntityItem)gr);
                        if (string.IsNullOrEmpty(key))
                        {
                            throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                        }
                        var es = new EntityStream(key, gr);
                        size += es.Size;
                        //values.Add(key, es);
                        values[key] = es;
                    }
                    totalSize = size;
                }
                else
                {
                    throw new ArgumentNullException("EntityDictionary<T> type not supported: " + type.ToString());
                }
                if (values.Count == 0)
                {
                    return null;
                }
                return values;
            }
            catch (ArgumentException aex)
            {
                if (onError != null)
                {
                    onError(aex);
                }
                //throw new GenericException<Exception>(ex);
            }
            catch (Exception ex)
            {

                if (onError != null)
                {
                    onError(ex);
                }
                //throw new GenericException<Exception>(ex);
            }
            return null;
        }

        /// <summary>
        /// Create Dictionary of <see cref="EntityStream"/> using <see cref="IEntity"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <param name="filter"></param>
        /// <param name="onError"></param>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        public static Dictionary<string, EntityStream> CreateEntityStream(IEntity context, Type type, DataFilter filter, Action<Exception> onError, out long totalSize) //where T : GenericEntity
        {
            totalSize = 0;

            try
            {

                if (context == null)
                {
                    throw new ArgumentNullException("CreateEntityStream.IEntity.context");
                }

                context.EntityDb.ValidateContext();

                Dictionary<string, EntityStream> values = new Dictionary<string, EntityStream>();

                long size = 0;

                if (typeof(GenericEntity).IsAssignableFrom(type) || typeof(EntityStream).IsAssignableFrom(type) || typeof(IEntityItem).IsAssignableFrom(type))
                {

                    string[] fieldsKey = context.EntityDb.EntityKeys.ToArray();

                    GenericEntity[] records = GenericEntity.CreateEntities(context.EntityDb.DoCommand<DataTable>(filter), fieldsKey);

                    foreach (var gr in records)
                    {

                        if (gr.PrimaryKey == null)
                        {
                            throw new Exception("Invalid PrimaryKey for entity: " + context.EntityDb.EntityName);
                        }
                        string key = gr.PrimaryKey.ToString();

                        if (string.IsNullOrEmpty(key))
                        {
                            throw new Exception("The PrimaryKey is incorrect for entity: " + context.EntityDb.EntityName);
                        }

                        var es = new EntityStream(key, gr);
                        size += es.Size;
                        //values.Add(key, es);
                        values[key] = es;
                    }
                    totalSize = size;
                }
                else
                {
                    throw new ArgumentNullException("EntityDictionary<T> type not supported: " + type.ToString());
                }
                if (values.Count == 0)
                {
                    return null;
                }
                return values;
            }
            catch (Exception ex)
            {
                if (onError != null)
                {
                    onError(ex);
                }
                //throw new GenericException<Exception>(ex);
            }
            return null;
        }


        #endregion

        public static EntityStream GetEntityStream(GenericEntity gr)
        {
            if (gr == null)
            {
                throw new ArgumentNullException("CreateEntityStream.gr");
            }
            EntityStream value = new EntityStream();
            if (gr.PrimaryKey != null)
            {
                value.Id = gr.PrimaryKey.ToString();
            }
            value.SetBody(gr.EntityEncode(), gr.GetType());
            return value;
        }

        public IDictionary<string, object> ToDictionary()
        {
            var dic = DictionaryUtil.ToDictionary(this, "");
            if (BodyStream != null)
            {
                var body = this.DecodeBody();
                if (body != null)
                    dic["Body"] = DictionaryUtil.ToDictionaryOrObject(body, "");
            }
            return dic;
        }

        public DynamicEntity ToEntity()
        {
            dynamic entity = new DynamicEntity();
            entity.TypeName=this.TypeName;
            entity.TransformType = (byte)this.TransformType;
            entity.Expiration = this.Expiration;
            entity.Formatter = this.Formatter;
            entity.Label = this.Label;
            entity.GroupId = this.GroupId;
            entity.Id = this.Id;
            entity.Modified = this.Modified;
            //entity.IsEmpty = this.IsEmpty;
            //entity.IsKnownType = this.IsKnownType;
            //entity.Size = this.Size;

            if (BodyStream != null)
            {
                var body = this.DecodeBody();
                if (body != null)
                    entity.Body = DictionaryUtil.ToDictionaryOrObject(body, "");
            }
            return entity;

        }

        /// <summary>
        /// Get entity as json
        /// </summary>
        /// <param name="pretty"></param>
        /// <returns></returns>
        public string ToJson(bool pretty=false)
        {
            return EntityWrite(new JsonSerializer(JsonSerializerMode.Write, null), pretty);
        }

    }

}
