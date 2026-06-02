using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IAssignedWorkOrderCollection
    {
        Guid VesselUID { get; set; }
        Guid? LoadingZoneSlotUID { get; set; }
        int StorageMethod { get; set; }
        string OperationSuggestion { get; set; }
        string ExternalBarcode { get; set; }
        Guid? PodUID { get; set; }
        ManifestType ServiceType { get; set; }
        IList<IAssignedWorkOrderPayload> Items { get; set; }
    }

    public interface IAssignedWorkOrderPayload
    {
        string Name { get; set; }
        Guid? SlotUID { get; set; }
        Guid ItemUID { get; set; }
        Guid? ItemGroupUID { get; set; }
        Guid VesselMainifestUID { get; set; }
        Guid? PayloadUID { get; set; }
        int ReceivePackageQty { get; set; }
        Guid ReceivePackageUID { get; set; }
        PayloadType PayloadType { get; set; }
        WorkOrderPayloadType WorkorderPayloadType { get; set; }
        /// <summary>
        /// 只有FullAllocated 在使用
        /// </summary>
        List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
        //decimal Volume { get; set; }
    }
    public interface IBulkPikcAssignedWorkOrderPayload : IAssignedWorkOrderPayload
    {
        Guid TargetSlotUID { get; set; }
        IEnumerable<Guid> OriginalPayloadUID { get; set; }
        IEnumerable<Guid> OriginalWorkOrderPayloadUID { get; set; }

    }

    public interface IAssignedOutboundWorkOrderCollection
    {
        Guid VesselUID { get; set; }
        Guid? LoadingZoneSlotUID { get; set; }
        string PalletBarcode { get; set; }
        ManifestType ServiceType { get; set; }
        IList<IAssignedOutboundWorkOrderPayload> Items { get; set; }
    }

    public interface IAssignedOutboundWorkOrderPayload
    {
        //string Name { get; set; }
        Guid? SlotUID { get; set; }
        Guid ItemUID { get; set; }
        Guid? ItemGroupUID { get; set; }
        Guid VesselMainifestUID { get; set; }
        Guid? PayloadUID { get; set; }
        int AllocatedQty { get; set; }
        /// <summary>
        /// 只有Move manifest 才會用
        /// </summary>
        Guid PickPackageUID { get; set; }
        AllocateType AllocateType { get; set; }
        /// <summary>
        /// 只有FullAllocated 才有用
        /// </summary>
        List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
    }
    public interface IAssignedBulkPickWorkOrderPayload : IAssignedOutboundWorkOrderPayload
    {
        Guid TargetSlotUID { get; set; }
        IEnumerable<Guid> OriginalPayloadUID { get; set; }
        IEnumerable<Guid> OriginalWordPayloadUID { get; set; }
    }
}
