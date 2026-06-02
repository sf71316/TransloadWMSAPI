using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface ISlotRepository
    {
        /// <summary>
        /// 設定 Slot 屬於 Bin
        /// </summary>
        /// <param name="areaUID"></param>
        /// <param name="slotUID"></param>
        /// <param name="binUID"></param>
        /// <returns></returns>
        IActionResult<bool> SetSlotMappingToBin(Guid? areaUID, Guid slotUID, Guid? binUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <param name="areaUID"></param>
        /// <param name="binUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ISlotViewModel>> GetSlotList(Guid? warehouseUID, Guid? areaUID, Guid? binUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IComponentViewModel>> GetSlotNameList(IWarehouseComponentParameters parameters);
        IActionResult<IEnumerable<ISlotModel>> GetList(object condition);
        IActionResult<bool> AddSlot(ISlotModel Model);
        IActionResult<bool> EditSlot(ISlotModel Model);
        IActionResult<bool> DeleteSlot(Guid[] UID);
        IActionResult<IEnumerable<ILocation>> GetLocations(Guid[] slotUIDs);
        IActionResult<IEnumerable<ISlotMappingLocation>> GetSlotMappingList(IEnumerable<Guid> slotlist);
        IActionResult<IEnumerable<ISlotSearchViewModel>> GetSearchSlotList(Guid? warehouseUID, string slotid);
    }
}
