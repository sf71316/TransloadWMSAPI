using System;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.Interfaces;
using YAEP.Data.ORM.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal interface IAutoAssignAgentProviders
    {
        /* Auto Assign */
        IPartyManager PartyManager { get; set; }
        IPackageManager PackageManager { get; set; }
        IPackageUomManager PackageUomManager { get; set; }
        IPackageVersionRepository PackageVersionRepository { get; set; }
        IPackageVersionManager PackageVersionManager { get; set; }
        PackageCacheManager PackageCacheManager { get; set; }
        ProductCacheManager ProductCacheManager { get; set; }
        IVesselManifestRepository VesselManifestRepository { get; set; }
        IWarehouseManger WarehouseManager { get; set; }
        IBolManager BolManager { get; set; }
        IManifestManger ManifestManager { get; set; }
        IVesselManager VesselManager { get; set; }

        /* Work Order */
        IWorkOrderAssignAgentParameters WorkOrderAssignAgentParameters { get; set; }
        IWorkOrderManager WorkOrderManager { get; set; }
        /* Ticket */
        ITicketRepository TicketRepository { get; set; }
        ITicketRelationRepository TicketRelationRepository { get; set; }
        ITicketInfoRepository TicketInfoRepository { get; set; }
        ILabelRepository LabelRepository { get; set; }
        IItemManager ItemManager { get; set; }
        ITracingAgent TracingAgent { get; set; }
        IObjectRelationalMappingLayer DbEntities { get; set; }
    }
}
