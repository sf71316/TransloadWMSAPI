using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class AllocatedPlannerInitParameters
    {
        public IVesselManager VesselManager { get; set; }
        public IPackageManager PackageManager { get; set; }
        public IPackageVersionManager PackageVersionManager { get; set; }
        public IPackageVersionRepository PackageVersionRepository { get; set; }
        public IPackageUomManager PackageUomManager { get; set; }
        public PackageCacheManager PackageMappingCache { get; set; }
        public ProductCacheManager ProductCache { get; set; }
        public IWarehouseManger WarehouseManger { get; set; }
        public ITracingAgent TracingAgent { get; set; }
        public FullAllocatedTemporaryOnhandExecutor AllocatedExecutor { get; set; }
        public int OrderType { get; set; }
    }
}
