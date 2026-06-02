using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderManager: ITraceInfiltrator, IDisposable
    {
        IActionResult<Guid> SaveAssignedWorkItmes(IAssignedWorkOrderCollection Parameters);
        IActionResult<Guid> SaveOutboundAssignedWorkItems(IAssignedOutboundWorkOrderCollection Parameters);

        //IActionResult<bool> SaveWorkOrder(IWorkOrderModel model);
        IActionResult<bool> SetSlot(ISetSlotParameters Parameters);
        IActionResult<bool> SetWorkOrderPodBarcode(ISetWorkOrderPodBarcodeParameters Parameters);
        IActionResult<bool> SetWorkOrderPodBarcode(Guid workorderPodUID, string customerBarcode);
        IActionResult<bool> SetWorkOrderPodBarcode(IWorkOrderPodModel workOrderPod, string customerBarcode, bool isInbound = true);
        IActionResult<bool> SetWorkOrderPodBarcode(Dictionary<IWorkOrderPodModel, string> collection);
        IActionResult<bool> ChangeWorkOrderStatus(Guid guid, WorkOrderStatus status);
        IActionResult<bool> ChangeWorkOrderPodStatus(Guid guid, WorkOrderPodStatus status);
        IActionResult<bool> ChangeWorkOrderPayloadStatus(Guid guid, WorkOrderPayloadStatus status);
        IActionResult<IEnumerable<IAvailableInventoryModel>> GeteAvailableInventoryList(IGetAvailableInventoryParameters parameters);
        IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(Guid vesselUID);
        IActionResult<IEnumerable<IWorkOrderPodViewModel>> GetWorkOrderPod(Guid VesselUID);
        IActionResult<IEnumerable<ILoadingZoneSelectModel>> GetLandingZoneList(Guid value);
        IActionResult<IEnumerable<IWorkOrderPayloadViewModel>> GetWorkOrderPayload(Guid VesselUID);

        IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetWorkOrderPayload(object condition);
        IActionResult<IWorkOrderPodModel> GetWorkOrderPod(object condition);
        IActionResult<IWorkOrderPodModel> AddWorkOrderPodAPI(IWorkOrderPodParameter parameter);
        IActionResult<bool> EditWorkOrderPod(dynamic parameter);
        IActionResult<bool> HaveTicket(Guid[] workOrderPodGuids = null, Guid[] workOrderpayloadGuids = null);
        IActionResult<bool> RemoveWorkOrderPodFromUI(Guid[] workorderpodguids);
        IActionResult<bool> RemoveWorkOrderPod(Guid[] workorderpodguids);
        IActionResult<bool> RemoveWorkOrderPod(Guid warehouseUID, Guid[] workorderpodguids,
            IEnumerable<IWorkOrderPayloadModel> includeworkOrderPayloadModels, IEnumerable<ITicketModel> ticketModels,
            IEnumerable<ITicketInfoModel> ticketInfoModels);
        IActionResult<bool> RemoveWorkOrderPayload(Guid[] workorderpayloadUID);
        IActionResult<bool> RemoveWorkOrderPayloadFromUI(Guid[] workorderpayloadUID);
        IActionResult<bool> MergePalletAPI(IWorkOrderMergePalletParameter parameter);
        IActionResult<bool> SetLoadingZoneSlot(ISetSlotParameters model);
        IActionResult<bool> AssignedPayloadtoPod(Guid workOrderPodUID, IEnumerable<Guid> workOrderPayloadUID);
        IActionResult<bool> RemoveWorkOrder(Guid[] workorderguids);
        IActionResult<bool> CheckHaveUnAssingedPodPayload(ITicketGenerateParameter parameter);
        IActionResult<ICheckOutboundAvailabilityResponse> CheckOutboundAvailabilityQty(ICheckAllocatedParameters parameters);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vesselUID"></param>
        /// <returns></returns>
        IActionResult<bool> ExecuteInboundAutoAssign(Guid vesselUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vesselUID"></param>
        /// <returns></returns>
        IActionResult<bool> ExecuteOutboundAutoAssign(Guid vesselUID);


    }
}
