using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISearchCarrierPalletParameters
    {
        Guid WarehouseUID { get; set; }
        DateTime? StartDateTime { get; set; }
        DateTime? EndDateTime { get; set; }
        string SearchDateType { get; set; }
        Guid? CarrierType { get; set; }
        string CarrierTruckID { get; set; }
        Guid? CarrierTruckUID { get; set; }
        int?[] CarrierTruckStatus { get; set; }
        int?[] CarrierPalletStatus { get; set; }
    }
}
