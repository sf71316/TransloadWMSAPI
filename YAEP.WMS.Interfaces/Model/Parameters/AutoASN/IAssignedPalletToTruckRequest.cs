using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAssignedPalletToTruckRequest
    {
        string CarrierTruckName { get; set; }
        Guid CarrierTypeUID { get; set; }
        IEnumerable<string> carrierPalletIDs { get; set; }
        Guid WarehouseUID { get; set; }
    }
}
