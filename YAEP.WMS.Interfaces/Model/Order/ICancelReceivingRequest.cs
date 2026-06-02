using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICancelReceivingRequest
    {
        string CustomerPartyName { get; set; }
        string RefNo { get; set; }
        Guid CustomerUID { get; set; }
        Guid WarehouseUID { get; set; }

    }
}
