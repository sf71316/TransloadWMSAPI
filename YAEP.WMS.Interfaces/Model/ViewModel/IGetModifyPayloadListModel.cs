using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetModifyPayloadListModel
    {
        Guid WarehouseUID { get; set; }
        string PayloadID { get; set; }
        Guid PayloadUID { get; set; }
        Guid PodUID { get; set; }
        string ItemName { get; set; }
        string ItemDescription { get; set; }
        Guid ItemUID { get; set; }
        IList<IGetModifyPayloadPackageItem> Package { get; set; }
        Guid PackageUID { get; set; }
        string PackageName { get; set; }
        string AreaID { get; set; }
        string BinID { get; set; }
        string SlotID { get; set; }
        int SlotStatus { get; set; }
        string SlotStatusName { get; set; }
        int SlotType { get; set; }
        string SlotTypeName { get; set; }

        Guid SlotUID { get; set; }
        Guid AreaUID { get; set; }
        Guid BinUID { get; set; }

        int EachQty { get; set; }
        int Qty { get; set; }
        string ManifestRefNo { get; set; }
        string ManifestName { get; set; }
        int ManifestType { get; set; }
        string ManifestTypeName { get; set; }
        string BolID { get; set; }
        string VesselID { get; set; }
        int PayloadStatus { get; set; }
        int PayloadType { get; set; }
        string PayloadTypeName { get; set; }
        string PayloadStatusName { get; set; }
        bool IsVirtualItem { get; set; }

    }
    public interface IGetModifyPayloadPackageItem
    {
        string VersionID { get; set; }
        string ItemName { get; set; }
        string PackageName { get; set; }
        Guid PackageUID { get; set; }
        Guid ItemUID { get; set; }
    }
}
