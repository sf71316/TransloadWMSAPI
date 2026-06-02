using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselRepository
    {
        /// <summary>
        /// 關聯Manifest , BOL
        /// </summary>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<IVesselModel>> GetList(IVesselSearchParameters Parameters);
        IActionResult<IVesselModel> GetData(object condition);
        IActionResult<IEnumerable<IVesselModel>> GetList(object condition);
        IActionResult<bool> AddVessel(IVesselModel Model);
        IActionResult<bool> EditVessel(dynamic Model);
        IActionResult<bool> DeleteVessel(IVesselDeleteParamters Parameters);
        IActionResult<bool> DeleteVessel(object Parameters);
        IActionResult<bool> ChangeVesselStatus(Guid vesselguid, VesselStatus status);
        IActionResult<bool> BatchChangeVesselStatus(IEnumerable<Guid> vesselguid, VesselStatus status, string modifiedBy = "");
        IActionResult<bool> BatchAddVessel(IEnumerable<IVesselModel> Collection);

    }
}
