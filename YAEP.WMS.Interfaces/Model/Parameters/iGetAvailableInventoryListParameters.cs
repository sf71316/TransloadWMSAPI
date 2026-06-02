using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IGetAvailableInventoryListParameters
    {
        Guid? VesselManifestUID { get; set; }
        Guid? WarehouseUID { get; set; }
        Guid ItemUID { get; set; }
        Guid? AreaUID { get; set; }
        Guid? BinUID { get; set; }
        Guid? SlotUID { get; set; }
        string OptionText { get; set; }
        string OptionValue { get; set; }
        Guid PackageUID { get; set; }
    }
    public interface IGetAvailableInventoryDataInnerListParameters
    {
        Guid? WarehouseUID { get; set; }
        //Guid[] ItemUID { get; set; }
        Dictionary<int, IEnumerable<Guid>> Items { get; set; }
        Guid? AreaUID { get; set; }
        Guid? BinUID { get; set; }
        Guid? SlotUID { get; set; }
        SlotStatus[] SlotStatuses { get; set; }
        bool IsincludeReceivingQty { get; set; }
        string OptionText { get; set; }
        string OptionValue { get; set; }
    }
}
