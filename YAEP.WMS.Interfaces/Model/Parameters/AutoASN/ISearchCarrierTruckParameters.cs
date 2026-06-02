using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISearchCarrierTruckParameters
    {
        string TimeZone { get; set; }
        Guid WarehouseUID { get; set; }
        int[] CarrierTruckStatus { get; set; }
        DateTime? StartDateTime { get; set; }
        DateTime? EndDateTime { get; set; }
    }
}
