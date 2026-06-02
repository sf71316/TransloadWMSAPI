using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetOnhandRequest
    {
        Guid WarehouseUID { get; set; }
        Guid CustomerUID { get; set; }
        string ReceiverUrl { get; set; }
        string ReceiverSecret { get; set; }
        IList<IGetOnhandItem> Items { get; set; }
    }
}
