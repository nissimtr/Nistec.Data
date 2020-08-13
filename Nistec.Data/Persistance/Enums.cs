using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Data.Persistance
{
    public enum CommitMode
    {
        OnDisk = 0,
        OnMemory = 1,
        None = 2
    }
    public enum SyncModes
    {
        Normal = 0,
        Full = 1,
        Off = 2
    }
    public enum JournalModeEnum
    {
        Default = -1,
        Delete = 0,
        Persist = 1,
        Off = 2,
        Truncate = 3,
        Memory = 4,
        Wal = 5
    }

}
