using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderRepository
    {

        IActionResult<Guid?> GetWorkOrderUID(Guid VesselUID);
        IActionResult<bool> AddWorkOrder(IEnumerable<dynamic> Model);
        IActionResult<bool> AddWorkOrder(dynamic Model);
        IActionResult<bool> EditWorkOrder(dynamic condition, dynamic model);
        IActionResult<bool> DeleteWorkOrder(object parameters);
        IActionResult<bool> SetSlot(ISetSlotParameters Parameters);
        IActionResult<bool> SetLoadingZoneSlotByWorkOrderPodUID(ISetSlotParameters Parameters);
        IActionResult<bool> SetLoadingZoneSlotByWorkOrderPayloadUID(ISetSlotParameters Parameters);
        IActionResult<bool> SetWorkOrderPodBarcode(ISetWorkOrderPodBarcodeParameters Parameters);
        IActionResult<bool> ChangeStatus(Guid workorderUID, WorkOrderStatus status);
        IActionResult<bool> HaveTicket(IHaveTicketParameters parameters);
        IActionResult<IManifestModel> GetManifestInfo(Guid vesselUID);
        IActionResult<IEnumerable<IManifestModel>> GetManifestInfoByWorkOrder(Guid[] workOrderUID);

        IActionResult<Tuple<IManifestModel, IVesselModel>> GetManifestVesselInfo(Guid WorkOrderPodUID);
        IActionResult<IEnumerable<IOutboundUnAssignedListModel>> GetOutboundUnAssignedList(Guid vesselUID);
        IActionResult<IEnumerable<dynamic>> GetUnAssingedPayload(ITicketGenerateParameter parameter);
        IActionResult<IEnumerable<IWorkOrderModel>> GetList(object condition);
        IActionResult<IEnumerable<IOutboundAllocatedItemModel>> GetOutboundAllocatedList(Guid vesselUID);
        IActionResult<bool> BatchChangeStatus(IEnumerable<Guid> workorderUID, WorkOrderStatus status, string modifiedby = "");
        IActionResult<bool> ChangeAllWorkOrderStatus(IEnumerable<Guid> workOrderUID,
            WorkOrderStatus workOrderStatus, WorkOrderPodStatus workOrderPodStatus, WorkOrderPayloadStatus workOrderPayloadStatus);
    }
}
