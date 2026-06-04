using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Party.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.Interfaces
{
    public interface IOrderManager : ITraceInfiltrator, IDisposable
    {
        IActionResult<Guid?> GetLatestPackageByItem(Guid itemUID);

        IActionResult<IAllocatedResponse> Allocated(IAllocatedRequest request);
        IActionResult<ICancelReceivingResponse> CancelReceiving(ICancelReceivingRequest request);
        IActionResult<IDeallocateResponse> Deallocated(IDeallocatedRequest request);
        IActionResult<IDeleteManifestResponse> DeleteManifestByOrder(IDeleteManifestRequest request);
        IActionResult<bool> VoidOutboundByTransload(Guid manifestUID);
        IActionResult<bool> VoidInboundByTransload(Guid manifestUID);
        IActionResult<IPickAllResponse> PickAll(IPickAllRequest request);
        IActionResult<dynamic> GetAllCurrentProeccingRequest();
        IActionResult<bool> RemoveProcessingRequestStatus(string actionkey, string requestKey);
        IActionResult<bool> AddProcessingRequestStatus(string actionkey, string requestKey);
        IActionResult<IPickItemResponse> PickItem(IPickItemRequest request);
        IActionResult<ISyncTrackingResponse> SyncTrackingNoAPI(ISyncTrackingNoRequest request);
        IActionResult<IReceivingResponse> Receiving(IReceivingRequest request);
        /// <summary>
        /// Transload 收貨：一次呼叫完成「生成 + 完成 + 建庫存」——
        /// 生成 Manifest + Manifest_Item_List + 1 BOL + 每櫃 1 Vessel(含容器屬性) + Vessel_Manifest
        /// + auto-assign WorkOrder/Pod/Payload + Ticket + AddWorkder，commit 後內部呼叫 CompleteReceivingByTransload
        /// 建 WMS_PayLoad(Stock)+WMS_Inventory。不動 production Receiving；重用既有 Repository/交易/auto-assign。
        /// </summary>
        IActionResult<ITransloadReceivingResult> ReceivingByTransload(ITransloadReceivingInput input);
        /// <summary>
        /// Transload 收貨完成（系統觸發）：解析該 manifest 的收貨 Ticket，以 IsAllPass(ActQty=EstQty) 完成
        /// （沿用 CompleteTicketData，內部 Receiving→Move 排序）→ 建 WMS_PayLoad(Stock) + WMS_Inventory + 推進狀態。
        /// 對應 ReceivingByTransload 的第二段；未來可改由工人操作完成。
        /// </summary>
        IActionResult<bool> CompleteReceivingByTransload(Guid manifestUID);
        IActionResult<bool> ImportInboundData(IImportInboundParameter parameter);
        IActionResult<IRollbackTicketResponse> RollbackTicket(IRollbackTicketRequest request);
        IActionResult<IGetOnhandResponse> GetOnhand(IGetOnhandRequest request);
        IActionResult<ISyncProNoResponse> SyncProNoAPI(ISyncProNoRequest request);
        IActionResult<dynamic> ResendSynctoPBSC();
        IActionResult<ICommonResponse> ClearProductCache();
        IActionResult<ICommonResponse> ClearPackageCache();
        IActionResult<IAllocatedResponse> FutureAllocated(IAllocatedRequest request);
        IActionResult<ICheckBolExistResponse> CheckBolExist(IEnumerable<string> request);
        IActionResult<IDeallocateResponse> Deallocated(IEnumerable<IDeallocatedRequest> requests);
        IActionResult<ICommonResponse> ReloadProductPackageCache();
        IActionResult<dynamic> GetItemNo(string itemNo);
        IActionResult<IEnumerable<IProductExtendModel>> GetAllItem();
    }
}
