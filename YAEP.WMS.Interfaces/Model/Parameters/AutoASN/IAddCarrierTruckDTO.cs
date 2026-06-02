using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAddCarrierTruckDTO
    {
        Guid UID { get; set; }
        string TruckName { get; set; }
        Guid WarehouseUID { get; set; }
        Guid CarrierType { get; set; }
        string CreatedBy { get; set; }
        int BatchCount { get; set; }
    }
}
