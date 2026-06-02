using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAvailableInventoryModel : IPayloadModel
    {
        string AreaName { get; set; }
        string WarehouseName { get; set; }
        string BinName { get; set; }
        string SlotName { get; set; }
        string VesselRefNo { get; set; }
        string BolRefNo { get; set; }
        string PodName { get; set; }
        string ItemName { get; set; }
        string PackageName { get; set; }
        string StatusName { get; set; }
    }
}
