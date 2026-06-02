using System;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.Interfaces;
using YAEP.Data.ORM.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class AutoAssignAgentProviders : IAutoAssignAgentProviders
    {
        public IPartyManager PartyManager { get; set; }
        public IItemManager ItemManager { get; set; }
        public IPackageManager PackageManager { get; set; }
        public IPackageUomManager PackageUomManager { get; set; }
        public IVesselManifestRepository VesselManifestRepository { get; set; }
        public IWarehouseManger WarehouseManager { get; set; }
        public IBolManager BolManager { get; set; }
        public IManifestManger ManifestManager { get; set; }
        public IVesselManager VesselManager { get; set; }
        public IWorkOrderAssignAgentParameters WorkOrderAssignAgentParameters { get; set; }
        public ITicketRepository TicketRepository { get; set; }
        public ITicketRelationRepository TicketRelationRepository { get; set; }
        public ITicketInfoRepository TicketInfoRepository { get; set; }
        public ILabelRepository LabelRepository { get; set; }

        public PackageCacheManager PackageCacheManager { get; set; }
        public ProductCacheManager ProductCacheManager { get; set; }
        public IWorkOrderManager WorkOrderManager { get; set; }
        public IPackageVersionManager PackageVersionManager { get; set; }
        public ITracingAgent TracingAgent { get; set; }
        public IPackageVersionRepository PackageVersionRepository { get; set; }
        public IObjectRelationalMappingLayer DbEntities { get; set; }
    }
}
