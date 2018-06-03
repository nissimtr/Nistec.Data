using Nistec.Collections;
using Nistec.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nistec.Generic;
using System.Data.SQLite;

namespace Nistec.Data.Sqlite
{

    public class Tasker : QueueListener<PersistanceTask>
    {
        public static readonly Tasker Queue = new Tasker();

        //protected override void OnMessageArraived(Generic.GenericEventArgs<TaskPersistance> e)
        //{
        //    base.OnMessageArraived(e);
        //}

        protected override void OnMessageReceived(GenericEventArgs<PersistanceTask> e)
        {
            base.OnMessageReceived(e);
            e.Args.ExecuteAsync();
        }

    }

    public class PersistanceTask
    {
        public string CommandType { get; set; }
        public string CommandText { get; set; }
        public string ConnectionString { get; set; }
        public SQLiteParameter[] Parameters { get; set; }
        public int Result { get; set; }


        public void ExecuteTask(bool enableTasker)
        {
            //TaskItem item = new TaskItem(() => Execute(), 0);
            if (enableTasker)
                Tasker.Queue.Enqueue(this);
            else
                ExecuteAsync();
        }

        public void ExecuteAsync()
        {
            Task.Factory.StartNew(() => Execute());
        }

        public void Execute()
        {
            using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
            {
                Result = db.ExecuteCommandNonQuery(CommandText, Parameters);
            }
        }

        //public void ExecuteTrans(Action action)
        //{
        //    using (var db = new DbLite(ConnectionString, DBProvider.SQLite))
        //    {
        //        //var cmdText = DbUpsertCommand(key, value);
        //        Result = db.ExecuteTransCommandNonQuery(CommandText, (result, trans) =>
        //        {
        //            if (result > 0)
        //            {
        //                trans.Commit();
        //            }
        //        });
        //    }
        //}

    }
}
