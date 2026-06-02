using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IBolRepository
    {
        IActionResult<bool> DeleteBol(object condition);
        IActionResult<IBolViewModel> GetBol(object condition);
        IActionResult<bool> AddBol(IBolModel Model);
        IActionResult<bool> EditBol(dynamic Model);
        IActionResult<bool> ChangeBolStatus(Guid bolUID, BolStatus status);
        IActionResult<IEnumerable<IBolViewModel>> GetList(IBolSearchParameters parameters);
        IActionResult<IEnumerable<IBolModel>> GetList(object condition);
        IActionResult<bool> BatchChangeBolStatus(IEnumerable<Guid> bolUID, BolStatus status, string modifiedBy = "");
        IActionResult<IEnumerable<string>> GetBolRefNo(IEnumerable<string> refNos);
    }
}
