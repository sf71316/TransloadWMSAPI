using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISyncProNoRequest
    {
        string RequestBy { get; set; }
        IList<ISyncProNoItem> Items { get; set; }
    }
    public interface ISyncProNoItem
    {
        string Syspon { get; set; }
        string ProNo { get; set; }
        Guid ShipViaRefUID { get; set; }
    }
}
