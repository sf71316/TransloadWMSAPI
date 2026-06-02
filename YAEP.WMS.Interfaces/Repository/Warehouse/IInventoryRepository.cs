using System;
using System.Collections.Generic;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInventoryRepository
    {
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
        IActionResult<IEnumerable<IInventoryModel>> GetList(object condition);


        IActionResult<bool> AddInventory(IInventoryModel Model);
        IActionResult<bool> EditInventory(IInventoryModel Model);
        IActionResult<IInventoryModel> GetInventory(Guid warehouseUID, Guid itemUID, Guid PackageUID, Guid slotUID);
        IActionResult<bool> IsItemInSlot(Guid warehouseUID, Guid itemUID, Guid PackageUID, Guid slotUID);
        IActionResult<bool> DeleteInventory(Guid InventoryUID);
        IActionResult<bool> DeleteInventory(IEnumerable<Guid> InventoryUID);
        IActionResult<IEnumerable<IAvailableInventoryModel>> GeteAvailableInventoryList(IGetAvailableInventoryParameters parameters);
        IActionResult<IEnumerable<ICheckOnhandModel>> GetOnhandData(Guid warhouseUID, Guid itemUID);
        IActionResult<IEnumerable<IGetModifyPayloadListModel>> GetModifyPayloadListData(IGetModifyPayloadListParameters parameters);
        IActionResult<int> GetItemUsageStatus(Guid ItemUID);
        IActionResult<bool> BatchAddInventory(IEnumerable<IInventoryModel> Model);
    }
}
