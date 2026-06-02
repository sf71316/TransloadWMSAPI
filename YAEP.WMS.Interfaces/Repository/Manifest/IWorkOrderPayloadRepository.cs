using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderPayloadRepository
    {
        /// <summary>
        /// 取得Manifest 有多少Item 被Assign WorkOrder 清單
        /// </summary>
        /// <param name="ManifestUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ICalVesselAssignedItemInnerModel>> GetAssignWorkOrderItemList(Guid VesselUID);
        IActionResult<bool> AddPayload(dynamic Model);
        IActionResult<bool> EditPayload(dynamic condition, dynamic Model);
        IActionResult<bool> DeletePayload(object parameters);
        IActionResult<bool> ChangeStatus(Guid workorderPayloadUID, WorkOrderPayloadStatus status);
        IActionResult<IEnumerable<IWorkOrderPayloadViewModel>> GetWorkOrderPayload(Guid VesselUID);
        IActionResult<bool> DeallcatedByWorkOrderPayload(Guid[] guids);
        IActionResult<bool> DeallcatedByWorkOrderPayload(IEnumerable<IDeallocatedParameters> deallocatedParameters);
        IActionResult<bool> ChangePayload(Guid wplUID, Guid payloadUID);
        IActionResult<bool> AssignedPayloadtoPod(Guid workOrderPodUID, IEnumerable<Guid> workOrderPayloadUID);

        IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetList(object condition);
        IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetListByUID(IEnumerable<Guid> workorderUIDs);
        IActionResult<IEnumerable<IWorkOrderPayloadInfoModel>> GetWorkOrderPayloadInfo(IEnumerable<Guid> warehouseUID, IEnumerable<Guid> itemUID, int[] payloadType, int[] slotType);
        IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetWorkOrderPayloadByOriginalPayload(IEnumerable<Guid?> enumerable, Guid workorderPodUID);
        IActionResult<IEnumerable<IRollbackModel>> GetRollbackWorkPayload(IEnumerable<Guid> ticketUIDs);
        IActionResult<bool> ChangeStatusByWorkOrder(Guid workorderUID, WorkOrderPayloadStatus status);
        IActionResult<bool> AddPayload(IEnumerable<dynamic> Model);
        IActionResult<bool> AddPayload(IEnumerable<IWorkOrderPayloadModel> Model);

        IActionResult<bool> BulkPickChangeFromSlot(IEnumerable<Guid> workorderPayloadUID);
        IActionResult<IEnumerable<IWorkOrderPayloadWithTicketInfoModel>> GetWorkOrderPayloadByTicketInfo(IEnumerable<Guid> ticketInfoUID);
        IActionResult<bool> BulkPickChangebackFromSlot(IEnumerable<Guid> workorderPayloadUID, Guid originalFromSlotUID);
        IActionResult<IEnumerable<IPodBarcodeInfo>> GetPodBarcodeInfo(ICheckPodBarcodeInfoParameters checkModel);
        IActionResult<IEnumerable<IPodBarcodeInfo>> GetReceivingQtyBarcodeInfo(ICheckPodBarcodeInfoParameters checkModel);
        IActionResult<bool> BatchChangeStatus(IEnumerable<Guid> workorderPayloadUID, WorkOrderPayloadStatus status, string modifiedBy = "");
        IActionResult<bool> DeletePayloadByUID(IEnumerable<Guid> guids);
    }
}
