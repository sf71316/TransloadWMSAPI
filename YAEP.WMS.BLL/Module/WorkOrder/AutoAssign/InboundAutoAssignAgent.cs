using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    /*
     * 1. 檢查 Vessel 資料是否正確
     * 2. 檢查 Vessel Manifest 資料是否正確
     * 3. 檢查 BOL 資料是否正確
     * 4. 檢查 BOL Manifest 資料是否正確 
     * 5. 取得 關聯的所有 Package 資料 
     * 6. 取得 Warehouse 的 Slot 使用狀況
     * ======= Default Action =======
     * 7. 檢查 Pallet UOM 資料是否有設定 Pallet
     * 8. 過濾除了 Pallet 以外的 Item
     * 9. 過濾無足夠儲存空間 & 負重的 Item
     * 10. 優先 Assign 給剩餘空間較小的 Slot
     * 11. 建立 Work Order / Pod / Payload
     * 12. 檢查如果無未 Assign 的 Item, 則建立 Ticket
     */
    internal partial class InboundAutoAssignAgent : AbstractAutoAssignAgent
    {

        public InboundAutoAssignAgent(IAutoAssignAgentProviders providers) : base(providers)
        {

        }

        public override IActionResult<bool> Execute(IAutoAssignParameters parameters)
        {
            Guid vesselUID = parameters.VesselUID;

            if (vesselUID == Guid.Empty)
            {
                return ActionResultTemplates.ArgumentNullExceptionResult(nameof(vesselUID));
            }

            #region Check Data

            // 查 Vessel  
            var vesselResult = this.Providers.VesselManager.GetVessel(vesselUID);
            if (vesselResult.Content == null)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NOT_FIND_VESSEL);
            }

            // 查 未Assign的 Vessel Manifest
            var freeVesselManifestListResult = this.Providers.VesselManager.GetUnAssignedList(new VesselManifestSearchInnerParameters()
            {
                VesselUID = vesselUID
            });
            var freeVesselManifestList = freeVesselManifestListResult.Content?.Where(o => o.FreeQty > 0).ToArray();
            if ((freeVesselManifestList?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NOT_FIND_VESSELMANIFST);
            }

            // 查 Vessel Manifest
            //var vesselManifestResult = this.Providers.VesselManifestRepository.GetData(new { VesselUID = vesselUID });
            //if ((vesselManifestResult.Content?.Count() ?? 0) == 0)
            //{
            //    return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NOT_FIND_VESSELMANIFST);
            //}
            // 查 BOL
            var bolResult = this.Providers.BolManager.GetBol(new { UID = vesselResult.Content.BolUID });
            if (bolResult.Content == null)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_NOT_FIND_BOL_DATA);
            }
            // 查 Manifest
            var manifestResult = this.Providers.ManifestManager.GetManifestInfo(bolResult.Content.ManifestUID);
            if (manifestResult.Content == null)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_NOT_FIND_MANIFESTINFO_DATA);
            }
            // 取得關聯的所有 Package 資料
            var packageUIDArray = freeVesselManifestList.GroupBy(o => o.ReceivePackageUID).Select(g => g.Key).ToArray();
            var packagesResult = this.Providers.PackageManager.GetPackages(packageUIDArray);
            if ((packagesResult.Content?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NO_REF_PACKAGES);
            }

            // 取得 Warehouse 的 Slot 使用狀況
            var slotUsageInfoResult = this.Providers.WarehouseManager
                .GetSlotUsageInfoByInbound(manifestResult.Content.WarehouseUID);
            if ((slotUsageInfoResult.Content?.Count() ?? 0) == 0)
            {
                return ActionResultTemplates.Error(Resource.MANIFEST_WORKORDER_NO_ALLOWED_SLOT);
            }

            var partyResult = this.Providers.PartyManager.GetParty(manifestResult.Content.PartyUID);
            if (partyResult.Content == null)
            {
                return ActionResultTemplates.Result(false, "Party not found.");
            }

            #endregion

            var manifest = manifestResult.Content;
            var bol = bolResult.Content;
            var vessel = vesselResult.Content;
            var vesselManifestList = freeVesselManifestList;
            var packages = packagesResult.Content;
            var slotUsageInfoList = slotUsageInfoResult.Content;
            var party = partyResult.Content;

            var args = new InboundProcessArgs(manifest, bol, vessel, party, vesselManifestList, packages, slotUsageInfoList);

            var operateWorkOrderResult = this.ExecuteWorkOrderProcess(args);

            if (operateWorkOrderResult.Success)
            {
                var operateTicketResult = this.ExecuteTicketProcess(args);

                if (operateTicketResult.Success)
                {
                    return ActionResultTemplates.Result(true);
                }
                else
                {
                    return ActionResultTemplates.Result(false, Resource.MANIFEST_GENERATE_TICKET_ERROR);
                }
            }
            else
            {
                return ActionResultTemplates.Result(false, operateWorkOrderResult.Message);
            }

            //return ActionResultTemplates.Result(false);
        }


    }
}
