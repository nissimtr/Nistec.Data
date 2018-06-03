using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Data.Sqlite
{
    public enum CommitMode
    {
        OnDisk = 0,
        OnMemory = 1,
        None = 2
    }

}
