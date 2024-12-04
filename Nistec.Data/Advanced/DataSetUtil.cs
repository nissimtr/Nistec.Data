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
using System.Data;
using System.IO;
using Nistec.IO;

#pragma warning disable CS1591
namespace Nistec.Data.Advanced
{
	public  class DataSetUtil
	{
		private DataSetUtil(){}

		public static void Write(System.IO.Stream destination, System.Data.DataSet source, StreamDataSetFormat format)
		{
			switch (format)
			{
				case StreamDataSetFormat.XML:
					source.WriteXml(destination, System.Data.XmlWriteMode.WriteSchema);
					break;
				case StreamDataSetFormat.Binary:
					BinWriteDataSetToStream(destination, source);
					break;
				default:
					throw new ApplicationException("StreamDataSet Format not supported");
			}
		}

		public static void Read(System.IO.Stream source, System.Data.DataSet destination, StreamDataSetFormat format, bool mergeSchema)
		{
			bool oldEnforce = destination.EnforceConstraints;
			destination.EnforceConstraints = false;
			try
			{
				switch (format)
				{
					case StreamDataSetFormat.XML:
						if (mergeSchema)
							destination.ReadXml(source, System.Data.XmlReadMode.ReadSchema);
						else
							destination.ReadXml(source, System.Data.XmlReadMode.IgnoreSchema);
						break;
					case StreamDataSetFormat.Binary:
						BinReadDataSetFromStream(source, destination, mergeSchema);
						break;
					default:
						throw new ApplicationException("StreamDataSet Format not supported");
				}
			}
			finally
			{
				destination.EnforceConstraints = oldEnforce;
			}
		}

		private const int c_BinaryVersion = 1;
		private static void BinWriteDataSetToStream(System.IO.Stream stream, System.Data.DataSet ds)
		{
			//Version
			StreamHelper.Write(stream, c_BinaryVersion);

			byte[] bytesSchema;
			using (System.IO.MemoryStream schemaStream = new System.IO.MemoryStream())
			{
				ds.WriteXmlSchema(schemaStream);
				schemaStream.Flush();
				schemaStream.Seek(0, System.IO.SeekOrigin.Begin);
				bytesSchema = schemaStream.ToArray();
			}
			StreamHelper.Write(stream, bytesSchema);

			//Tables
			for (int iTable = 0; iTable < ds.Tables.Count; iTable++)
			{
				System.Data.DataTable table = ds.Tables[iTable];
				//Only the current Rows
				System.Data.DataRow[] rows = table.Select(null, null, System.Data.DataViewRowState.CurrentRows);
				StreamHelper.Write(stream, rows.Length);
				//Rows
				for (int r = 0; r < rows.Length; r++)
				{
					//Columns
					for (int c = 0; c < table.Columns.Count; c++)
						BinWriteFieldToStream(stream, rows[r][c], table.Columns[c].DataType);
				}
			}
		}

		private static void BinReadDataSetFromStream(System.IO.Stream stream, System.Data.DataSet ds, bool mergeSchema)
		{
			//Version
			int version;
			version = StreamHelper.ReadInt32(stream);

			if (version != c_BinaryVersion)
				throw new BinaryDataSetVersionException();

			//Schema byte[]
			System.Data.DataSet schemaDS; //Questo dataset viene usato solo per leggere lo schema
			byte[] byteSchema = StreamHelper.ReadByteArray(stream);
			using (System.IO.MemoryStream schemaStream = new System.IO.MemoryStream(byteSchema))
			{
				if (mergeSchema)
				{
					ds.ReadXmlSchema(schemaStream);
					schemaDS = ds;
				}
				else
				{
					schemaDS = new System.Data.DataSet();
					schemaDS.ReadXmlSchema(schemaStream);
				}
			}

			//Tables
			for (int iTable = 0; iTable < schemaDS.Tables.Count; iTable++)
			{
				System.Data.DataTable table = ds.Tables[ schemaDS.Tables[iTable].TableName ];

				int rowsCount = StreamHelper.ReadInt32(stream);
				//Rows
				for (int r = 0; r < rowsCount; r++)
				{
					System.Data.DataRow row = table.NewRow();
					//Columns
					for (int c = 0; c < schemaDS.Tables[iTable].Columns.Count; c++)
					{
						row[ schemaDS.Tables[iTable].Columns[c].ColumnName ] = BinReadFieldFromStream(stream, table.Columns[c].DataType);
					}

					table.Rows.Add(row);
				}
			}

			ds.AcceptChanges();
		}


        public static byte[] DataSetToBytes(System.Data.DataSet ds, bool readSchema)
        {

           
            string xml="";

            if (readSchema)
            {
                xml = ds.GetXmlSchema();
            }
            else
            {
                xml = ds.GetXml();
            }

            return System.Text.Encoding.UTF8.GetBytes(xml);
        }

        /// <summary>
        /// Get the DataSet sise in bytes 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="readSchema"></param>
        /// <returns></returns>
        public static int DataSetToByteCount(System.Data.DataSet ds, bool readSchema)
        {

            int count = 0;
            if (readSchema)
            {
                count = System.Text.Encoding.UTF8.GetByteCount(ds.GetXmlSchema());
            }
            count += System.Text.Encoding.UTF8.GetByteCount(ds.GetXml());

            return count;
        }
        /// <summary>
        /// Get the DataTable sise in bytes 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="readSchema"></param>
        /// <returns></returns>
        public static int DataSetToByteCount(System.Data.DataTable dt, bool readSchema)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt.Copy());
            return DataSetToByteCount(ds, readSchema);
        }


		private static void BinWriteFieldToStream(System.IO.Stream stream, object val, Type columnType)
		{
			//Field Status
			if (val == null) //Null
				StreamHelper.Write(stream, (byte)0);
			else if (val == System.DBNull.Value) //DbNull
				StreamHelper.Write(stream, (byte)1);
			else
			{
				StreamHelper.Write(stream, (byte)2);

				//Field Value
				StreamHelper.Write(stream, columnType, val);
			}
		}

		private static object BinReadFieldFromStream(System.IO.Stream stream, Type columnType)
		{
			//Field Status
			byte bt = StreamHelper.ReadByte(stream);
			
			if (bt == 0)//Null
				return null;
			else if (bt == 1)//DbNull
				return System.DBNull.Value;
			else if (bt == 2)//Value
				//Field Value
				return StreamHelper.Read(stream, columnType);
			else
				throw new BinaryDataSetInvalidException();
		}
	}

	public enum StreamDataSetFormat
	{
		XML = 1,
		Binary = 2
	}

	[Serializable]
	public class BinaryDataSetInvalidException : Exception
	{
		public BinaryDataSetInvalidException():
			base("Binary data not valid")
		{
		}
		public BinaryDataSetInvalidException(Exception p_InnerException):
			base("Binary data not valid", p_InnerException)
		{
		}
		protected BinaryDataSetInvalidException(System.Runtime.Serialization.SerializationInfo p_Info, System.Runtime.Serialization.StreamingContext p_StreamingContext): 
			base(p_Info, p_StreamingContext)
		{
           
		}
	}

	[Serializable]
	public class BinaryDataSetVersionException : Exception
	{
   		public BinaryDataSetVersionException():
			base("Binary data version not valid")
		{
		}
		public BinaryDataSetVersionException(Exception p_InnerException):
			base("Binary data version not valid", p_InnerException)
		{
		}
		protected BinaryDataSetVersionException(System.Runtime.Serialization.SerializationInfo p_Info, System.Runtime.Serialization.StreamingContext p_StreamingContext): 
			base(p_Info, p_StreamingContext)
		{
		}
	}

}
