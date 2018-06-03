using Nistec.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nistec.Data;

namespace Sqlite.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string sqlCreateTable = "create table demo_score2 (name varchar(20), score int)";
            string sqlInsert1 = "insert into demo_score2 (name, score) values ('Nissim', 9001)";
            string sqlInsert2 = "insert into demo_score2 (name, score) values ('Neomi', 9002)";
            string sqlInsert3 = "insert into demo_score2 (name, score) values ('Shani', 9003)";
            string sqlUpdate = "update demo_score2 set name='Shani' where score=@score";
            string sqlDelete = "delete from demo_score2 where score=@score";
            string sqlSelect = "select * from demo_score2 order by score desc";

            DataTable dt = null;

            DbLite.CreateFileFromSettings("sqlite.demo");
            using (DbLite db = new DbLite("sqlite.demo"))
            {
                db.OwnsConnection = true;
                db.ExecuteCommandNonQuery(sqlCreateTable);
                db.ExecuteCommandNonQuery(sqlInsert1);
                db.ExecuteCommandNonQuery(sqlInsert2);
                db.ExecuteCommandNonQuery(sqlInsert3);
                db.ExecuteCommandNonQuery(sqlUpdate, DbLite.GetParam("score", 9004));
                db.ExecuteCommandNonQuery(sqlDelete, DbLite.GetParam("score", 9002));

                dt=db.ExecuteDataTable("demo_score", sqlSelect);
                db.OwnsConnection = false;
            }

            Console.Write(dt.TableName);
        }
    }
}
