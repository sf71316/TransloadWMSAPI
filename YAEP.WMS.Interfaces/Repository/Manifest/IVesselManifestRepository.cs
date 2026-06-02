using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselManifestRepository
    {

        /// <summary>
        /// 取得Manifest 有多少Item 被Assign 到Vessel 清單
        /// </summary>
        /// <param name="ManifestUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ICalVesselAddItemInnerModel>> GetVesselAssignItemList(IGetAddItemListparameters parameters);
        IActionResult<IEnumerable<IVesselManifestItemListViewModel>> GetVesselManifestItemList(IVesselManifestSearchParameters Parameters);
        IActionResult<IEnumerable<IVesselManifestViewModel>> GetList(IVesselManifestSearchParameters Parameters);
        IActionResult<IEnumerable<IVesselManifestModel>> GetList(object condition);
        IActionResult<bool> AddVesselManifest(IVesselManifestModel Model);
        IActionResult<bool> BatchAddVesselManifest(IEnumerable<IVesselManifestModel> collection);
        IActionResult<bool> DeleteVesselManifest(IVesselManifestDeleteParameters Parameters);
        IActionResult<bool> DeleteVesselManifest(object Parameters);
        IActionResult<bool> ChangeVesselManifestStatus(Guid vesselUID, VesselManifestStatus status);
        IActionResult<bool> ChangeVesselManifestStatusByBOL(Guid bolUID, VesselManifestStatus status);
        IActionResult<bool> ChangeVesselManifestStatusByVesselManifestUID(Guid vessemanifestlUID, VesselManifestStatus status);
        IActionResult<dynamic> GetPartyBolInfo(Guid vesselUID);
        IActionResult<IManifestModel> GetManifestInfo(Guid VesselManifestItemUID);
        IActionResult<IEnumerable<IVesselManifestModel>> GetListByBol(IEnumerable<Guid> bolUIDs);
        IActionResult<bool> AddVesselManifest(IEnumerable<IVesselManifestModel> collection);
        IActionResult<bool> BatchChangeVesselManifestStatus(IEnumerable<Guid> vesselUID, VesselManifestStatus status,string modifiedBy="");
    }
}
