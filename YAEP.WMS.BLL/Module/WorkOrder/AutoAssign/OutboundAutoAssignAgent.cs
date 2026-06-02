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
     *  
     */
    internal partial class OutboundAutoAssignAgent : AbstractAutoAssignAgent
    {

        public OutboundAutoAssignAgent(IAutoAssignAgentProviders providers) : base(providers)
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

            // 檢查 Party 是否存在
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
            var party = partyResult.Content;

            var args = new OutboundProcessArgs(manifest, bol, vessel, party, vesselManifestList, packages);

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
