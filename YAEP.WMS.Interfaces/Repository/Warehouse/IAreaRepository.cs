using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IAreaRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IAreaViewModel>> GetAreaList(Guid? warehouseUID, Guid? areaUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IComponentViewModel>> GetAreaNameList(IWarehouseComponentParameters parameters);

        IActionResult<IEnumerable<IAreaModel>> GetList(dynamic condition);
        IActionResult<bool> AddArea(IAreaModel Model);
        IActionResult<bool> EditArea(IAreaModel Model);
        IActionResult<bool> DeleteArea(Guid[] UID);

    }
}
