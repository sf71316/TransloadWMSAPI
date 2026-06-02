using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselManager: IDisposable
    {
        IActionResult<bool> AddVessel(IVesselModel Model);
        IActionResult<IEnumerable<IVesselManifestModel>> GetVesselManifest(object condition);
        IActionResult<bool> EditVessel(dynamic Model);
        IActionResult<bool> DeleteVessel(IVesselDeleteParamters Parameters);
        IActionResult<bool> DeleteVesselAPI(IVesselDeleteParamters Parameters);
        IActionResult<bool> AddVesselManifest(IVesselManifestModel Model);
        IActionResult<bool> DeleteVesselManifest(IVesselManifestDeleteParameters Parameters);
        IActionResult<bool> DeleteVesselManifestFromUI(IVesselManifestDeleteParameters Parameters);
        IActionResult<IEnumerable<IVesselModel>> GetVesselList(IVesselSearchParameters Parameters);
        IActionResult<IEnumerable<IUnAssignedListViewModel>> GetUnAssignedList(IVesselManifestSearchParameters parameters);
        IActionResult<IEnumerable<IVesselManifestItemListViewModel>> GetVesselManifestItemList(IVesselManifestSearchParameters Parameters);
        IActionResult<IEnumerable<IVesselAddItemListVewModel>> GetAddItemList(IGetAddItemListparameters parameters);
        IActionResult<bool> ChangeVesselStatus(Guid VesselUID, VesselStatus status, VesselManifestStatus vesselManifestStatus);
        IActionResult<IEnumerable<dynamic>> GetOutboundUnAssignedList(Guid vesselUID);
        IActionResult<IEnumerable<ILocationItemViewModel>> GetAvailableInventoryList(IGetAvailableInventoryListParameters request);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vesselUID"></param>
        /// <returns></returns>
        IActionResult<IVesselModel> GetVessel(Guid vesselUID);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vesselUID"></param>
        /// <returns></returns>
        IActionResult<int> GetUnassignedVesslManifestCount(Guid vesselUID);

        IActionResult<IManifestModel> GetManifestInfo(Guid vesselUID);
        IActionResult<IEnumerable<IVesselManifestModel>> GetVesselManifestByBol(IEnumerable<Guid> condition);
        IActionResult<IEnumerable<ILocationItemViewModel>> GetAvailableInventoryData(IGetAvailableInventoryDataInnerListParameters request);
        IActionResult<IEnumerable<IPodBarcodeInfo>> GetPodBarcodeInfo(ICheckPodBarcodeInfoParameters parameters);
    }

}
