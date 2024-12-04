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
using System.IO.Compression;
using System.Data;
using System.Runtime.Serialization;
using System.Runtime;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
#pragma warning disable CS1591
namespace Nistec.Data.Advanced
{
    [Serializable]
    public class CompressedDataTable : DataTable
    {
        public CompressedDataTable()
        {
            this.RemotingFormat = SerializationFormat.Binary;
        }

        protected CompressedDataTable(SerializationInfo info, StreamingContext context)
        {
            MethodInfo miDeTS = typeof(CompressedDataTable).GetMethod("DeserializeTableSchema",
                BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo miDeTD = typeof(CompressedDataTable).GetMethod("DeserializeTableData",
                BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo miResIn = typeof(CompressedDataTable).GetMethod("ResetIndexes",
                BindingFlags.NonPublic | BindingFlags.Instance);

            using (MemoryStream stream = new MemoryStream())
            {
                byte[] bytes = (byte[])info.GetValue("chunk", typeof(byte[]));
                useCompression = (bool)info.GetValue("compressed", typeof(bool));

                compressedSize = bytes.Length;

                if (useCompression)
                {
                    using (MemoryStream streamUnzip = new MemoryStream(bytes))
                    {
                        Decompress(streamUnzip, stream);
                    }
                }
                else
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                stream.Position = 0;
                originalSize = (int)stream.Length;
                IFormatter formatter = new BinaryFormatter();

                FieldInfo fiData = typeof(SerializationInfo).GetField("m_data", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fiMembers = typeof(SerializationInfo).GetField("m_members", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fiTypes = typeof(SerializationInfo).GetField("m_types", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo fiCurrMember = typeof(SerializationInfo).GetField("m_currMember", BindingFlags.NonPublic | BindingFlags.Instance);

                object[] data = (object[])formatter.Deserialize(stream);
                string[] members = (string[])formatter.Deserialize(stream);
                Type[] types = (Type[])formatter.Deserialize(stream);
                int curMember = (int)formatter.Deserialize(stream);

                fiData.SetValue(info, data);
                fiMembers.SetValue(info, members);
                fiTypes.SetValue(info, types);
                fiCurrMember.SetValue(info, curMember);
            }


            miDeTS.Invoke(this, new object[] { info, context, true });
            miDeTD.Invoke(this, new object[] { info, context, 0 });
            miResIn.Invoke(this, new object[] { });
        }

        #region Compression methods

        public static void Compress(Stream source, Stream destination)
        {
            using (GZipStream output = new GZipStream(destination, CompressionMode.Compress))
            {
                Pump(source, output);
            }
        }

        public static void Decompress(Stream source, Stream destination)
        {
            using (GZipStream input = new GZipStream(source, CompressionMode.Decompress))
            {
                Pump(input, destination);
            }
        }

        public static void Pump(Stream input, Stream output)
        {
            int n;
            byte[] bytes = new byte[4096];
            while ((n = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                output.Write(bytes, 0, n);
            }
        }
        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializationInfo zipInfo = new SerializationInfo(typeof(CompressedDataTable), new FormatterConverter());
            base.GetObjectData(zipInfo, context);

            FieldInfo fiData = typeof(SerializationInfo).GetField("m_data", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fiMembers = typeof(SerializationInfo).GetField("m_members", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fiTypes = typeof(SerializationInfo).GetField("m_types", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] data = (object[])fiData.GetValue(zipInfo);
            string[] members = (string[])fiMembers.GetValue(zipInfo);
            Type[] types = (Type[])fiTypes.GetValue(zipInfo);

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                formatter.Serialize(stream, members);
                formatter.Serialize(stream, types);
                formatter.Serialize(stream, zipInfo.MemberCount);

                using (MemoryStream streamZip = new MemoryStream())
                {
                    stream.Position = 0;
                    byte[] arr = null;
                    if (useCompression && stream.Length > compressThreshold)
                    {
                        Compress(stream, streamZip);
                        arr = streamZip.ToArray();
                    }
                    else
                    {
                        arr = stream.ToArray();
                    }

                    info.AddValue("chunk", arr);
                    info.AddValue("compressed", useCompression && stream.Length > compressThreshold);
                }
            }
        }

        #region Variales and properties

        bool useCompression = true;

        public bool UseCompression
        {
            get { return useCompression; }
            set { useCompression = value; }
        }

        int compressThreshold = 16384;

        public int CompressThreshold
        {
            get { return compressThreshold; }
            set { compressThreshold = value; }
        }

        int compressedSize = 0;

        public int CompressedSize
        {
            get { return compressedSize; }
        }

        int originalSize = 0;

        public int OriginalSize
        {
            get { return originalSize; }
        }

        public double CompressionRate
        {
            get { return ((double)originalSize - (double)compressedSize) * 100 / (double)originalSize; }
        }

        #endregion

        public string Serialize()
        {
            string data = null;
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Position = 0;
                formatter.Serialize(stream, this);
                stream.Position = 0;
                using (StreamReader sr = new StreamReader(stream))
                {
                    data = sr.ReadToEnd();
                }

            }
            return data;
        }
        public static DataTable Deserialize(string serialaized)
        {
            DataTable data = null;
            IFormatter formatter = new BinaryFormatter();
            byte[] bytes = Encoding.UTF8.GetBytes(serialaized);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                stream.Position = 0;
                data=(DataTable) formatter.Deserialize(stream);
            }
            return data;
        }
        public static string SerializeDataset(DataSet ds)
        {
            string data = null;
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Position = 0;
                formatter.Serialize(stream, ds);
                stream.Position = 0;
                using (StreamReader sr = new StreamReader(stream))
                {
                    data = sr.ReadToEnd();
                }

            }
            return data;
        }
        public static DataSet DeserializeDataset(string serialaized)
        {
            DataSet data = null;
            IFormatter formatter = new BinaryFormatter();
            byte[] bytes = Encoding.UTF8.GetBytes(serialaized);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                stream.Position = 0;
                data = (DataSet)formatter.Deserialize(stream);
            }
            return data;
        }
    }
}
