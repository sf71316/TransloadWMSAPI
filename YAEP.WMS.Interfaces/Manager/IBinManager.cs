using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IBinManager
    {
        IActionResult<bool> SetMappingToArea(Guid areaUID, Guid binUID);
        IActionResult<IEnumerable<IBinViewModel>> GetBinList(Guid? warehouseUID, Guid? areaUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IComponentViewModel>> GetBinNameList(IWarehouseComponentParameters parameters);
        IActionResult<IEnumerable<IBinModel>> GetList(dynamic condition);
        IActionResult<bool> AddBin(IBinModel Model);
        IActionResult<bool> EditBin(IBinModel Model);
        IActionResult<bool> DeleteBin(Guid[] UID);
    }
}
