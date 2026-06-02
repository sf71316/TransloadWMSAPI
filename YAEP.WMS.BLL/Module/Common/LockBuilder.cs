using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Module
{
    internal static class LockBuilder
    {
        public static ReaderWriterLock ProductPackageLock { get; set; } = new ReaderWriterLock();
        public static object RequestMangerLocker { get; set; } = new object();
    }
}
