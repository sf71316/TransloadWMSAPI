using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IBinRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="areaUID"></param>
        /// <param name="binUID"></param>
        /// <returns></returns>
        IActionResult<bool> SetBinMappingToArea(Guid areaUID, Guid binUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <param name="areaUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IBinViewModel>> GetBinList(Guid? warehouseUID, Guid? areaUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IComponentViewModel>> GetBinNameList(IWarehouseComponentParameters parameters);

        IActionResult<IEnumerable<IBinModel>> GetList(object condition);
        IActionResult<bool> AddBin(IBinModel Model);
        IActionResult<bool> EditBin(IBinModel Model);
        IActionResult<bool> DeleteBin(Guid[] UID);
    }
}
