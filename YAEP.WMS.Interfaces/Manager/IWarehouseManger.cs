using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IWarehouseManger : ITraceInfiltrator, IDisposable
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<bool> DeleteWarehouse(IWarehouseDeleteParameters parameters);
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="warehouse">warehouse model</param>
        /// <returns></returns>
        IActionResult<bool> AddWarehouse(IWarehouseModel warehouse);
        /// <summary>
        /// 編輯
        /// </summary>
        /// <param name="warehouse">warehouse model</param>
        /// <returns></returns>
        IActionResult<bool> EditWarehouse(IWarehouseModel warehouse);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GenerateBarcode(BarcodeType type);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <returns></returns>
        IActionResult<IEnumerable<IComponentViewModel>> GetWarehouseNameList();
        IActionResult<IEnumerable<IWarehouseModel>> GetThirdPartyWarehouseNameList();

        IActionResult<Guid> GetPodInSlot(IGetPodInSlotParameters Parameters);
        IActionResult<IEnumerable<IPayloadLocationModel>> GetLocations(IEnumerable<Guid> payloadUIDs);
        IActionResult<ISlotModel> GetFutureSlot(Guid warehouseUID);
        //void Importhomelocation(List<dynamic> homelocations, IGroupManager groupManager);

        /// <summary>
        /// 取得對應識別碼的 Warehouse
        /// </summary>
        /// <param name="warehouseUID">
        /// warehouse識別碼
        /// <para /><see cref="IWarehouseModel.UID"/>
        /// </param>
        /// <returns></returns>
        IActionResult<IWarehouseModel> GetWarehouse(Guid warehouseUID);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IActionResult<IEnumerable<IWarehouseModel>> GetWarehouseList();
        IActionResult<bool> PodIsExist(Guid PodUID);
        IActionResult<IEnumerable<IPodSelectListModel>> GetPodSelectList(Guid wuid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <param name="areaUID"></param>
        /// <param name="binUID"></param>
        /// <param name="slotUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ILocationInfoViewModel>> GetLocationInfoList(Guid? warehouseUID, Guid? areaUID, Guid? binUID, Guid? slotUID);
        IActionResult<int> GetAssignedPackageQty(Guid packageUID);
        IActionResult<ISlotModel> GetDefaultLandingZone(Guid warehouseUID, SlotType slotType);
        IActionResult<IDeallocatedPayloadInfoModel> FindDeallocatedRelatedPayloadCollection(IEnumerable<Guid> allocatedPayloadUID);
        IActionResult<bool> EditPayload(IPayloadModel payloadModel);
        IActionResult<IPayloadModel> GetRecoveryPayload(Guid payloadUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfo(
            Guid warehouseUID, IEnumerable<SlotType> slotTypes, IEnumerable<SlotStatus> slotStatuses);
        IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfoByInbound(Guid warehouseUID);
        IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfoByOutboundTL(Guid warehouseUID);
        IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfoByOutboundPKG(Guid warehouseUID);
        IActionResult<IEnumerable<IHomeAddressRelationModel>> GetHomeAddressRelation(IEnumerable<Guid> ItemUIDs,
            HomeAddressType homeAddressType, HomeAddressOutboundType? homeAddressOutboundType = null);
        IActionResult<ISlotModel> GetDummySlot(Guid warehouseUID);

        IActionResult<IEnumerable<IPayloadModel>> GetOnhandPayload(Guid warehouseUID, IEnumerable<Guid> itemNo, int[] slotStatus);
        IActionResult<IEnumerable<ILocationItemViewModel>> GetAvailableInventoryData(IGetAvailableInventoryDataInnerListParameters param);
        IActionResult<IEnumerable<ISlotModel>> CheckSlot(Guid warehouseUID, IEnumerable<string> slotNames);
        IActionResult<bool> TestInventorySync(int times = 1, int interval = 100);
        IActionResult<bool> TestAllocatedSync(int times = 1, int interval = 100);
        IActionResult<bool> TestReceivingSync(int times = 1, int interval = 100);
        IActionResult<int> GetSequence(Guid belongToUID, string belongToTag);
        IActionResult<bool> ReplenishmentPayload(IPayloadModel payloadModel);
    }
}
