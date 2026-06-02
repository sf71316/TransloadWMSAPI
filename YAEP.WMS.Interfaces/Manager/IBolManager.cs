using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IBolManager: IDisposable
    {
        IActionResult<bool> DeleteBol(IBolDeleteParameters Parameters);
        IActionResult<bool> DeleteBolAPI(IBolDeleteParameters Parameters);
        IActionResult<bool> AddBol(IBolModel Model);
        IActionResult<bool> EditBol(dynamic Model);
        IActionResult<IEnumerable<IBolViewModel>> GetBolList(IBolSearchParameters Parameters);
        IActionResult<IEnumerable<IBolModel>> GetBolList(object condition);
        IActionResult<bool> ChangeBolStatus(Guid bolUID, BolStatus status);
        IActionResult<IBolViewModel> GetBol(object condition);
        IActionResult<bool> ForceApproveBol(Guid boluid, Guid warehouseUID, int manifestType);
        IActionResult<IBolModel> ApproveBol(Guid boluid);
        IActionResult<IBolModel> RejectBol(Guid boluid);
        IActionResult<bool> CheckHaveUnassignedTicket(Guid boluid);
        IActionResult<IEnumerable<string>> GetAllVesslWorkPayload(Guid bolUID);
    }
}
