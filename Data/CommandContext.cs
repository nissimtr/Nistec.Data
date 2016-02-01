//licHeader
//===============================================================================================================
// System  : Nistec.Cache - Nistec.Cache Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
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
using Nistec.Data.Factory;
using Nistec.Generic;
using Nistec.IO;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Data
{
    public class CommandContext : ISerialEntity
    {

        public CommandContext() { }

        public CommandContext(string connectionKey, string commandText, CommandType commandType, int commandTimeout, Type returnType)
        {
            ConnectionKey = connectionKey;
            CommandText = commandText;
            CommandType = commandType;
            CommandTimeout = commandTimeout;
            TypeName = returnType.FullName;
        }

        public CommandContext(NetStream stream)
        {
            EntityRead(stream, null);
        }

        #region properties

        /// <summary>
        /// Get or Set connection key.
        /// </summary>
        public string ConnectionKey { get; set; }
        /// <summary>
        /// Get or Set command text.
        /// </summary>
        public string CommandText { get; set; }
        /// <summary>
        /// Get or Set command type.
        /// </summary>
        public CommandType CommandType { get; set; }
        /// <summary>
        /// Get or Set command timeout.
        /// </summary>
        public int CommandTimeout { get; set; }
        /// <summary>
        /// Get or Set type name.
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// Get or Set command parameters.
        /// </summary>
        public GenericKeyValue Parameters { get; set; }

        #endregion

        #region methods

        public static string GetTypeName(Type type)
        {
            return type.FullName;
        }
       
        public static GenericKeyValue GetParameters(object[] keyValueParameters)
        {
            return new GenericKeyValue(keyValueParameters);
        }

        public void CreateParameters(object[] keyValueParameters)
        {
            Parameters = GetParameters(keyValueParameters);
        }

        public string ToQueryString(bool sortedByKey = false)
        {
            if (Parameters == null || Parameters.Count == 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            if (sortedByKey)
                foreach (var p in Parameters.Sorted())
                {
                    sb.AppendFormat("{0}={1}&", p.Key, p.Value);
                }
            else
                foreach (var p in Parameters)
                {
                    sb.AppendFormat("{0}={1}&", p.Key, p.Value);
                }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public string CreateKey(bool sortedByKey = false)
        {
            string query = ToQueryString(sortedByKey);
            string key = string.Format("{0}|{1}|{2}|{3}|{4}", ConnectionKey, CommandText, CommandType.ToString(), query, TypeName);
            return key;
        }
        public static string CreateKey(string ConnectionKey, string CommandText, CommandType commandType, string query, string TypeName)
        {
            string key = string.Format("{0}|{1}|{2}|{3}|{4}", ConnectionKey, CommandText, commandType.ToString(), query, TypeName);
            return key;
        }
        public IDbDataParameter[] ToDataParameters()
        {
            return DataParameter.GetSqlParameters(Parameters);
        }

        public byte[] Serialize()
        {
            using (NetStream stream = new NetStream())
            {
                EntityWrite(stream, null);
                return stream.ToArray();
            }
        }

        public static CommandContext Deserialize(byte[] bytes)
        {
            return new CommandContext(new NetStream(bytes));
        }

        public static CommandContext Deserialize(NetStream stream)
        {
            return new CommandContext(stream);
        }
        #endregion

        #region command exec

        public T ExecCommand<T>()
        {
            using (var db = DbFactory.Create(ConnectionKey))
            {
                return db.ExecuteCommand<T>(CommandText, ToDataParameters(), CommandType, CommandTimeout);
            }
        }
        public TResult ExecCommand<TItem, TResult>()
        {
            using (var db = DbFactory.Create(ConnectionKey))
            {
                return db.ExecuteCommand<TItem, TResult>(CommandText, ToDataParameters(), CommandType, CommandTimeout);
            }
        }

        public object Exec()
        {
            object result = null;
            
            Type type = Type.GetType(TypeName);

            if (type == typeof(DataSet))
            {
                result = ExecCommand<DataSet>();
            }
            else if (type == typeof(DataTable))
            {
                result = ExecCommand<DataTable>();
            }
            else if (type == typeof(DataRow))
            {
                result = ExecCommand<DataRow>();
            }
            else if (type == typeof(GenericRecord))
            {
                result = ExecCommand<GenericRecord>();
            }
            else if (type == typeof(JsonResults))
            {
                result = ExecCommand<JsonResults>();
            }
            else //if (type == typeof(object))
            {
                result = ExecCommand<object>();
            }

            return result;
        }

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
                streamer = new BinaryStreamer(stream);
            streamer.WriteString(ConnectionKey);
            streamer.WriteString(CommandText);
            streamer.WriteValue((int)CommandType);
            streamer.WriteValue(CommandTimeout);
            streamer.WriteString(TypeName);
            streamer.WriteValue(Parameters);
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
                streamer = new BinaryStreamer(stream);

            ConnectionKey = streamer.ReadString();
            CommandText = streamer.ReadString();
            CommandType = (CommandType)streamer.ReadValue<int>();
            CommandTimeout = streamer.ReadValue<int>();
            TypeName = streamer.ReadString();
            Parameters = streamer.ReadValue<GenericKeyValue>();
        }

        #endregion
    }
}
