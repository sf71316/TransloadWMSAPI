using System;
using System.Collections.Generic;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.Interfaces
{
    public interface IInventoryManager : ITraceInfiltrator, IDisposable
    {
        IActionResult<Guid> AddPayloadWithModifedSpecialLogical(Guid packageUID, Guid itemUID, int onhandQty, Guid slotUID, Guid allocatedPayloadUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IInventoryViewModel>> GetInventory(IInventorySearchParameters parameters);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IInventoryDetailViewModel>> GetInventoryDetail(Guid itemUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IPayloadTransactionLogViewModel>> GetTranascationList(IPayloadTransactionLogParameters parameters);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        IActionResult<bool> AddLog(IPayloadTransactionLogModel Model);
        IActionResult<bool> BatchAddLog(IEnumerable<IPayloadTransactionLogModel> model);
        IActionResult<bool> UpdateInventory(IEditOnhandParameters editOnhandParameters);
        IActionResult<IInventoryModel> GetInventory(Guid warehouseUID, Guid itemUID, Guid PackageUID, Guid slotUID);
        IActionResult<bool> IsItemInSlot(Guid warehouseUID, Guid itemUID, Guid PackageUID, Guid slotUID);
        IActionResult<bool> DeleteInventory(Guid InventoryUID);
        IEditOnhandParameters CreateEditInventoryParameters();
        IAddOnhandParameters CreateAddInventoryParameters();
        IActionResult<bool> InsertInventory(IEnumerable<IInsertInventoryParameter> parameters);
        IActionResult<bool> AddInventory(IAddOnhandParameters addOnhandParameters, bool isAddPayload = false);
        IActionResult<bool> ProcessAddInventory(IAddOnhandParameters addOnhandParameters, Func<IActionResult<bool>> syncMethod = null, bool isAddPayload = false);

        IActionResult<bool> AddPod(IPodModel model);
        IActionResult<bool> AddPayload(IPayloadModel model);
        IActionResult<bool> BatchAddPayload(IEnumerable<IPayloadModel> Model);
        /// <summary>
        /// 取得payload 資料(會找被刪除的資料)
        /// </summary>
        /// <param name="payloadUID"></param>
        /// <returns></returns>
        IActionResult<IPayloadModel> GetPayload(Guid payloadUID);
        IActionResult<IEnumerable<IPayloadModel>> GetPayloadList(object condition);
        IActionResult<IEnumerable<IPayloadModel>> GetPayload(IEnumerable<Guid> PayloadUID);
        IActionResult<IEnumerable<IPayloadModel>> GetPayload(IEnumerable<Guid> payloadUID, PayloadType type);
        IActionResult<bool> UpdatePayload(IPayloadModel model);
        IActionResult<bool> BatchUpdatePayload(IEnumerable<IPayloadModel> model);
        IActionResult<bool> UpdatePod(IPodModel model);
        IActionResult<bool> UnPack(Guid PodUID);
        IActionResult<bool> BatchUnPack(IEnumerable<Guid> model);
        IActionResult<bool> DeallocatedPayload(IEnumerable<IDeallocatedParameters> deallocatedParameters);
        IActionResult<IEnumerable<IPodModel>> GetPod(Guid[] PodUIDs);
        IActionResult<bool> ChangePodStauts(Guid poduid, PodStatus status);
        IActionResult<bool> ChangePayloadStauts(Guid payloaduid, PayloadStatus status);
        IActionResult<bool> ChangePayloadStauts(IEnumerable<Guid> payloaduid, PayloadStatus status);
        IActionResult<IEnumerable<IAllocatedModel>> GetAllocatedData(Guid[] warehouseUID, Guid[] itemUID);
        IActionResult<IEnumerable<IAvailableInventoryModel>> GeteAvailableInventoryList(IGetAvailableInventoryParameters parameters);
        IActionResult<bool> ChangePayloadType(Guid payloaduid, int type);
        IActionResult<bool> ChangePayloadType(IEnumerable<Guid> payloaduid, int type);
        IActionResult<IEnumerable<ICheckOnhandModel>> GetOnhandData(Guid warhouseUID, Guid itemUID);
        IActionResult<bool> DeletePayloadFromDb(object condition);
        IActionResult<bool> DeletePodFromDb(object condition);
        IActionResult<IEnumerable<IPayloadModel>> GetListByTicket(Guid ticketUID);

        IActionResult<IEnumerable<IEnumFieldInfo>> GetTranascationTypeList();
        IActionResult<IEnumerable<ISlotMappingLocation>> GetSlotMappingList(IEnumerable<Guid> slotlist);
        //IActionResult<IEnumerable<IImportTSReceivingDataResponseModel>> ImportTotalSolutionReceivingData(
        //    IEnumerable<IImportTSReceivingDataRequestModel> data);
        IActionResult<int> GetItemUsageStatus(Guid ItemUID);

        IActionResult<IEnumerable<IProductPackageExtendModel>> GetItemListFromCache(Guid? CustomerUID);
    }
}
