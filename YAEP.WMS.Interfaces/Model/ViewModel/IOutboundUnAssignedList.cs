using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IOutboundUnAssignedListModel
    {
        /// <summary>
        /// VesselManifestUID
        /// </summary>
        Guid UID { get; set; }
        Guid SlotUID { get; set; }
        Guid VesselManifestUID { get; set; }
        string SlotName { get; set; }
        string BinName { get; set; }
        string AreaName { get; set; }
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        Guid PickPackageUID { get; set; }
        string PickPackageName { get; set; }
        string PayloadID { get; set; }
        int PickQty { get; set; }
        int AllocatedQty { get; set; }
        int FreeQty { get; set; }
        decimal Volume { get; set; }
        decimal Weight { get; set; }
    }
    public interface IOutboundAllocatedItemModel
    {
        /// <summary>
        /// VesselManifestUID
        /// </summary>
        Guid UID { get; set; }
        Guid SlotUID { get; set; }
        Guid VesselManifestUID { get; set; }
        string SlotName { get; set; }
        string BinName { get; set; }
        string AreaName { get; set; }
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        Guid PickPackageUID { get; set; }
        string PickPackageName { get; set; }
        string PayloadID { get; set; }
        int PickQty { get; set; }
        decimal Volume { get; set; }
        decimal Weight { get; set; }
    }
}
