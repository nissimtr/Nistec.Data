using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data.Advanced;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.IO;

namespace DataEntityDemo.Data
{
    public class CompressedDataTableTest
    {
        public void Test()
        {
            CompressedDataTable table = new CompressedDataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Code", typeof(string));

            for (int i = 0; i < 10000; i++)
            {
                table.Rows.Add(i, i + "akdsmfmfadfas", i + "dkmffas");
            }

            //table.UseCompression = true; default
            Stopwatch watch = new Stopwatch();

            for (int i = 0; i < 10; i++)
            {
                string ser = table.Serialize();

                DataTable tableDes = CompressedDataTable.Deserialize(ser);
            }
            watch.Stop();
            Console.WriteLine("Time to one serialization/deserialization with Compression: " + watch.ElapsedMilliseconds / 10);

            //=============================================

            table.UseCompression = false;
            watch = new Stopwatch();

            for (int i = 0; i < 10; i++)
            {

                string ser = table.Serialize();

                DataTable tableDes = CompressedDataTable.Deserialize(ser);
            }
            watch.Stop();
            Console.WriteLine("Time to one serialization/deserialization no Compression: " + watch.ElapsedMilliseconds / 10);

        }
    
 
        public void DatasetTest()
        {
            DataSet ds = new DataSet();
            CompressedDataTable table = new CompressedDataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Code", typeof(string));
            DataTable table2 = new DataTable();
            table2.Columns.Add("ID", typeof(int));
            table2.Columns.Add("Name", typeof(string));
            table2.Columns.Add("Code", typeof(string));
            ds.Tables.Add(table);
            ds.Tables.Add(table2);
            ds.Relations.Add(new DataRelation("namr", table.Columns[0], table2.Columns[0]));

            for (int i = 0; i < 10000; i++)
            {
                table.Rows.Add(i, i + "akdsmfmfadfas", i + "dkmffas");
                table2.Rows.Add(i, i + "akdsmfmfadfas", i + "dkmffas");
            }

            IFormatter formatter = new BinaryFormatter();
            Stopwatch watch = new Stopwatch();
            using (MemoryStream stream = new MemoryStream())
            {
                watch.Start();
                for (int i = 0; i < 10; i++)
                {
                    stream.Position = 0;
                    formatter.Serialize(stream, ds);

                    stream.Position = 0;
                    DataSet ds2 = (DataSet)formatter.Deserialize(stream);
                }
                watch.Stop();
                Console.WriteLine("Time to one serialization/deserialization: " + watch.ElapsedMilliseconds / 10);
            }
        }
    }
}
