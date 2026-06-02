using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IManifestItemListRepository
    {
        IActionResult<IEnumerable<IManifestItemListModel>> GetManifestItemList(Guid ManifestUID);
        IActionResult<IManifestItemListModel> GetManifestItemInfo(object condition);
        IActionResult<IEnumerable<ICalVesselAddItemInnerModel>> GetManifestItemListByGroupItem(IGetAddItemListparameters paramerters);
        IActionResult<bool> Delete(IManifestItemListDeleteParameters parameters);
        IActionResult<bool> Delete(object parameters);
        IActionResult<bool> Add(IEnumerable<IManifestItemListModel> Model);
        IActionResult<bool> Update(IManifestItemListModel Model);
        IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestItemListStatus status,string modifiedBy="");
        IActionResult<int> GetManifestItemListByPackageQty(Guid packageUID);
        IActionResult<IEnumerable<ICheckManifestItemStatusResultModel>> GetCheckManifestItemStatusResult(Guid manifestUID);
        IActionResult<bool> ChangeManifestStatusByBol(Guid BolUID, ManifestItemListStatus status);
        IActionResult<bool> BatchChangeManifestStatus(IEnumerable<Guid> manifestitemUID, ManifestItemListStatus status);
    }
}
