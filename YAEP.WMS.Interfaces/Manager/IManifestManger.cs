using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Constant.Enums;
using YAEP.Identities.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IManifestManger : ITraceInfiltrator, IDisposable
    {

        IActionResult<IEnumerable<R>> GetManifestList<R>(IManifestSearchParameters Parameters) where R : class, IManifestListViewModel;
        IActionResult<bool> DeleteManifestAPI(IManifestDeleteParameters Parameters, bool forcedelete = false);
        IActionResult<bool> DeleteManifest(IManifestDeleteParameters Parameters, bool forcedelete = false);
        IActionResult<bool> DeleteManifestItem(IManifestItemListDeleteParameters parameters, bool isIgnoreCheck = false);
        IActionResult<IManifestModel> AddManifest(IManifestModel Model);
        IActionResult<IManifestViewModel> SubmitManifest(Guid manifestUID);
        IActionResult<IManifestViewModel> RejectManifest(Guid manifestuid);
        IEnumerable<IEnumFieldInfo> GetPayloadStatusList();
        IEnumerable<IEnumFieldInfo> GetPayloadTypeList();
        IActionResult<bool> EditManifest(dynamic Model);
        IActionResult<bool> AddManifestItems(IEnumerable<IManifestItemListModel> Model);
        IActionResult<bool> EditManifestItem(IManifestItemListModel Model);
        IEnumerable<IEnumFieldInfo> GetManifestTypeList();
        IActionResult<IManifestViewModel> GetManifestInfo(Guid uid);
        IActionResult<IEnumerable<IManifestItemListModel>> GetManifestItemList(Guid ManifestUID);
        IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestStatus status, ManifestItemListStatus ManifestStatus);
        IActionResult<string> GetDefaultFolderName(Guid btu, int btp);
        IActionResult<bool> CheckOnhand(Guid warhouseUID, Guid itemUID, Guid packageUID, int qty);

        IActionResult<IEnumerable<IShipviaPaymentInfoModel>> GetShipviaPaymentInfo(Guid partyUID);
        IActionResult<IEnumerable<IShipMethodModel>> GetShipMethodList(Guid? partyUID);
        IActionResult<IEnumerable<ICheckManifestItemStatusResultModel>> GetCheckManifestItemStatusResult(Guid manifestUID);
        void ReplicationTest();
        IActionResult<IManifestModel> GetManifest(object condition);
        IActionResult<IEnumerable<IGetModifyPayloadListModel>> GetModifyPayloadList(IGetModifyPayloadListParameters parameters);
        IActionResult<bool> CreateAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> requests);
        IActionResult<bool> CreateSakanaAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> requests);
        IActionResult<bool> SetGroupMoveAdjustment(IEnumerable<ISetGroupMoveAdjustmentRequest> requests);
        IActionResult<int> SetFillFutureAllocated(IEnumerable<IReplenishmentModel> fillList, IEnumerable<Guid> target_future_allocated = null);
        IActionResult<int> SetNominateFillFutureAllocated(INominateReplenishmentModel target_future_allocated);
        IActionResult<bool> CheckReplenishmentSync(IEnumerable<IReplenishmentModel> Data);
        IActionResult<bool> CheckReplenishmentSync(IEnumerable<ITicketProcessModel> Data);

        IActionResult<bool> ProcessPBSCItemAndPackage(IEnumerable<IPBSCItemPackagingModel> Data);
        IActionResult<bool> CheckAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> parameters);
        IActionResult<bool> CheckSakanaAdjustmentTicket(IEnumerable<ICreateAdjustmentTicketRequest> parameters);
        #region Auto ASN
        IActionResult<IEnumerable<ICarrierTruckViewModel>> GetCarrierTruckList(ISearchCarrierTruckParameters parameters);
        IActionResult<bool> AddCarrierTruck(IAddCarrierTruckDTO addCarrierTruckDTO);
        IActionResult<bool> AddShipCarrierPallet(IAddCarrierPalletDTO addCarrierPalletDTO);
        IActionResult<IEnumerable<IShipCarrierCategory>> GetShipCarrierCategories();
        IActionResult<bool> AssignedPalletToTruck(IAssignedPalletToTruckRequest request);
        IActionResult<bool> CarrierTruckDepartured(List<Guid> carrierTruckUID);
        IActionResult<bool> DeleteCarrierTruck(IEnumerable<Guid> carrierTruckUIds);
        IActionResult<bool> ChangePalletToOtherTruck(IChangePalletToOtherTruckRequest request);
        IActionResult<bool> RemovePalletFromTruck(List<Guid> carrierPalletUIDs);
        IActionResult<IEnumerable<ICarrierPalletViewModel>> GetCarrierPallets(ISearchCarrierPalletParameters paramters);
        IActionResult<bool> AssignedPackageToPallet(IAssignedPackageToPalletRequest request);
        IActionResult<bool> DeleteCarrierPallet(List<Guid> carrierPalletUIds);
        IActionResult<bool> ChangePackageToOtherPallet(IChangePackageToOtherPalletRequest request);
        IActionResult<bool> RemovePackageFromPallet(List<Guid> palletinfoUIDs);
        IActionResult<IEnumerable<IShipCarrierPalletInfo>> GetCarrierPalletInfos(ISearchCarrierPalletInfoParameters parameters);
        #endregion

    }


}
