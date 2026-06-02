using General.Data.SQLConditionConverter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IWarehouseRepository
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<bool> Delete(IWarehouseDeleteParameters parameters);
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="warehouse">warehouse model</param>
        /// <returns></returns>
        IActionResult<bool> Add(IWarehouseModel warehouse);
        /// <summary>
        /// 編輯
        /// </summary>
        /// <param name="warehouse">warehouse model</param>
        /// <returns></returns>
        IActionResult<bool> Update(IWarehouseModel warehouse);
        IActionResult<IEnumerable<IComponentViewModel>> GetWarehouseNameList();
        IActionResult<Guid> GetPodInSlot(IGetPodInSlotParameters Parameters);
        IActionResult<bool> PodIsExist(Guid PodUID);
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
        IActionResult<IEnumerable<IWarehouseModel>> GetWarehouseList(IQueryConditionExtractor conditionExtractor);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <param name="areaUID"></param>
        /// <param name="binUID"></param>
        /// <param name="slotUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ILocationInfoViewModel>> GetLocationInfoList(
            Guid? warehouseUID, Guid? areaUID, Guid? binUID, Guid? slotUID);
        IActionResult<IEnumerable<IPodSelectListModel>> GetPodSelectList(Guid wuid);
        IActionResult<IEnumerable<ILoadingZoneSelectModel>> GetLoadingZoneList(Guid value);
        IActionResult<IEnumerable<ILocationItemViewModel>> GetAvailableInventoryList
            (IGetAvailableInventoryDataInnerListParameters request);
        IActionResult<IEnumerable<ISlotModel>> GetDefaultLoadingZone(Guid warehouseUID, SlotType slotType);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfo(
            Guid warehouseUID, IEnumerable<SlotType> slotTypes, IEnumerable<SlotStatus> slotStatuses,
            ManifestType manifestType);
        IActionResult<IEnumerable<IPayloadLocationModel>> GetLocations(IEnumerable<Guid> payloadUIDs);
        IActionResult<IEnumerable<ISlotModel>> GetSlotByType(Guid warehouseUID, IEnumerable<SlotType> slotTypes);
    }
}
