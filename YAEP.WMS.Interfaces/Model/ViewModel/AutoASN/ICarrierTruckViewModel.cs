using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICarrierTruckViewModel
    {
        string CarrierTruckID { get; set; }
        string CarrierTruckName { get; set; }
        int PalletCount { get; set; }
        DateTime? DepartureDate { get; set; }
        Guid CarrierTruckUID { get; set; }
        int CarrierTruckStatus { get; set; }
        string CarrierTruckStatusName { get; set; }
        string CarrierType { get; set; }
        Guid CarrierTypeUID { get; set; }
    }
}
