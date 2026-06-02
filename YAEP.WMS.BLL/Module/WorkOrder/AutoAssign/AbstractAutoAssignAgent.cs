using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.Package.Interfaces.Models;
using YAEP.Package.Constants;
using YAEP.WMS.Constant.Enums;
using YAEP.Core.Party.Interfaces.Models;

namespace YAEP.WMS.BLL.Module
{
    /*
     * 1. 檢查 Vessel 資料是否正確
     * 2. 檢查 Vessel Manifest 資料是否正確
     * 3. 檢查 BOL 資料是否正確
     * 4. 檢查 BOL Manifest 資料是否正確 
     * 5. 取得 關聯的所有 Package 資料 
     * 6. 取得 Warehouse 的 Slot 使用狀況
     */
    internal abstract class AbstractAutoAssignAgent
    {
        protected readonly IAutoAssignAgentProviders Providers;

        public AbstractAutoAssignAgent(IAutoAssignAgentProviders providers)
        {
            this.Providers = providers;
        }

        public abstract IActionResult<bool> Execute(IAutoAssignParameters parameters);

        protected virtual IActionResult<bool> ExecuteWorkOrderProcess(IAutoAssignProcessArgs e)
        {
            var party = e.Party;

            var exector = GetWorkOrderExecutor(this.Providers, e);

            return exector.Execute(e);
        }

        protected virtual IActionResult<bool> ExecuteTicketProcess(IAutoAssignProcessArgs e)
        {
            return ActionResultTemplates.Result(true);
            var party = e.Party;

            var exector = GetTicketExecutor(this.Providers, e);

            return exector.Execute(e);
        }

        protected static IAutoAssignWorkOrderExecutor GetWorkOrderExecutor(IAutoAssignAgentProviders providers, IAutoAssignProcessArgs args)
        {
            // TODO Auto Assign 依照條件切換 Work Order Exector

            IAutoAssignWorkOrderExecutor executor = null;

            switch ((ManifestType)args.Manifest.Type)
            {
                case ManifestType.Inbound:
                    executor = new InboundAutoAssignWorkOrderExecutor(providers);
                    break;
                case ManifestType.Outbound:
                    executor = new OutboundAutoAssignWorkOrderExecutor(providers);
                    break;
                default:

                    break;
            }

            return executor;
        }
        protected static IAutoAssignTicketExecutor GetTicketExecutor(IAutoAssignAgentProviders providers, IAutoAssignProcessArgs args)
        {
            // TODO Auto Assign 依照條件切換 Ticket Exector

            return new DefaultAutoAssignTicketExecutor(providers);
        }

        protected sealed class InboundProcessArgs : IAutoAssignInboundProcessArgs
        {
            public InboundProcessArgs(IManifestModel manifest, IBolModel bol, IVesselModel vessel, IPartyModel party,
                                        IEnumerable<IUnAssignedListViewModel> freeVesselManifests,
                                        IEnumerable<IPackageModel> packages,
                                        IEnumerable<ISlotUsageInfoModel> slotUsageInfos)
            {
                this.Manifest = manifest;
                this.BOL = bol;
                this.Vessel = vessel;
                this.Party = party;
                this.VesselManifests = freeVesselManifests;
                this.Packages = packages;
                this.SlotUsageInfos = slotUsageInfos;
            }
            public IManifestModel Manifest { get; }
            public IBolModel BOL { get; }
            public IVesselModel Vessel { get; }
            public IPartyModel Party { get; }
            public IEnumerable<IPackageModel> Packages { get; }
            public IEnumerable<ISlotUsageInfoModel> SlotUsageInfos { get; }
            public IEnumerable<IUnAssignedListViewModel> VesselManifests { get; }
        }
        protected sealed class OutboundProcessArgs : IAutoAssignOutboundProcessArgs
        {
            public OutboundProcessArgs(IManifestModel manifest, IBolModel bol, IVesselModel vessel, IPartyModel party,
                                        IEnumerable<IUnAssignedListViewModel> freeVesselManifests,
                                        IEnumerable<IPackageModel> packages)
            {
                this.Manifest = manifest;
                this.BOL = bol;
                this.Vessel = vessel;
                this.Party = party;
                this.VesselManifests = freeVesselManifests;
                this.Packages = packages;
            }
            public IManifestModel Manifest { get; }
            public IBolModel BOL { get; }
            public IVesselModel Vessel { get; }
            public IPartyModel Party { get; }
            public IEnumerable<IPackageModel> Packages { get; }
            public IEnumerable<IUnAssignedListViewModel> VesselManifests { get; }
        }


    }
}
