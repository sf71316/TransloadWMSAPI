using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ILocationItemViewModel
    {
        string WarehouseName { get; set; }
        string AreaName { get; set; }
        string BinName { get; set; }
        string SlotName { get; set; }
        string SlotId { get; set; }
        Guid UID { get; set; }
        /// <summary>
        /// 僅UI顯示使用
        /// </summary>
        Guid PayloadUID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int Type { get; set; }
        Guid PodUID { get; set; }
        Guid SlotUID { get; set; }
        Guid VesselUID { get; set; }
        Guid ItemUID { get; set; }
        int Quantity { get; set; }
        Guid OriginalPackageUID { get; set; }
        /// <summary>
        /// Full allocated 在使用 ，記錄原本payload.type
        /// </summary>
        Guid? OriginalPayloadUID { get; set; }
        string OriginalPackageName { get; set; }
        int PackageQty { get; set; }
        int AllocatedQty { get; set; }
        string PackageName { get; set; }
        Guid PackageUID { get; set; }
        Guid VesselManifestUID { get; set; }
        string ItemID { get; set; }
        decimal VolumeLimit { get; set; }
        decimal WeightLimit { get; set; }
        int Status { get; set; }
        string Description { get; set; }
        string VesselRefNo { get; set; }
        string BolRefNo { get; set; }
        string PodName { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedOn { get; set; }
        int Sequence { get; set; }
        long PackageSerialNumber { get; set; }
        IEnumerable<ITicketLabelViewModel> Labels { get; set; }
    }
}
