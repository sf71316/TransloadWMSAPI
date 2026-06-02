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
    public interface IManifestRepository
    {
        IActionResult<IEnumerable<R>> GetManifestList<R>(IManifestSearchParameters Parameters) where R : class, IManifestListViewModel;
        IActionResult<bool> Delete(IManifestDeleteParameters Parameters);
        IActionResult<IManifestModel> GetInfo(Guid ManifestUID);
        IActionResult<bool> Add(IManifestModel Model);
        IActionResult<bool> Update(dynamic Model);
        IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestStatus status, string modifiedBy = "");
        IActionResult<IEnumerable<IManifestModel>> GetList(object condition);
        IActionResult<IManifestModel> GetData(object condition);
        IActionResult<IEnumerable<IManifestModel>> GetDataFromBOL(IEnumerable<Guid> bolUIDs);
        IActionResult<IEnumerable<ILocationMapping>> GetWarehouseMapping();
        IActionResult<IEnumerable<IManifestModel>> GetListBySQLConverter(IQueryConditionExtractor conditionExtractor);

    }
}
