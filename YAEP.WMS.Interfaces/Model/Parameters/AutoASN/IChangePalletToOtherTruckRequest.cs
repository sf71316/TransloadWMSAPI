using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IChangePalletToOtherTruckRequest
    {
        string CarrierTruckName { get; set; }
        Guid CarrierTypeUID { get; set; }
        Guid WarehouseUID { get; set; }
        List<Guid> CarrierPalletUIDs { get; set; }
    }
}
