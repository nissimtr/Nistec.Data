using Nistec.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Data.Sqlite
{
    public interface IPersistItem: IEntityItem
    {
        // byte[] Serilaize();
        string key { get;}
    }

    public class PersistTextItem : IPersistItem
    {
        public string key { get; set; }
        public string body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class PersistBinaryItem : IPersistItem
    {
        public string key { get; set; }
        public byte[] body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class BagItem : IPersistItem
    {
        public string key { get; set; }
        public string body { get; set; }
        public int state { get; set; }
        public DateTime timestamp { get; set; }
    }
}
