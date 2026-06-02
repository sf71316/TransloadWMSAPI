using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class OutboundHomeAddressBuilderInitParameters
    {
        public IVesselManager VesselManager { get; set; }
        public IWarehouseManger WarehouseManger { get; set; }
        public ProductCacheManager ProductCacheManager { get; set; }
        public PackageCacheManager PackageCacheManager { get; set; }
        public IPackageVersionRepository PackageVersionRepository { get; set; }
        public IPackageVersionManager PackageVersionManager { get; set; }
        public ILogInfiltrator LogInfiltrator { get; set; }
        public ITracingAgent TracingAgent { get; set; }
    }
}
