using Nistec.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable CS1591
namespace Nistec.Data.Persistance
{
    public interface IPersistItem : IEntityItem
    {
        // byte[] Serilaize();
        string key { get; }
    }

    public interface IPersistBinaryItem : IPersistItem
    {
        byte[] body { get; set; }
    }

    public class PersistItem : IPersistItem, IPersistEntity
    {
        public string key { get; set; }
        public object body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }

        public object value()
        {
            return body;
        }
    }

    public class PersistTextItem : IPersistItem
    {
        public string key { get; set; }
        public string body { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class PersistBinaryItem : IPersistBinaryItem
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
