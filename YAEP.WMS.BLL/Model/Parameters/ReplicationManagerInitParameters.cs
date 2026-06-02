using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ReplicationManagerInitParameters : IReplicationManagerInitParameters
    {
        public ITicketInfoRepository TicketInfoRepository { get; set; }
        public ProductCacheManager ProductCacheManager { get; set; }
        public PackageCacheManager PackageCacheManager { get; set; }
        public IAuthenticationInfo AuthenticationInfo { get; set; }
        public IInventoryManager InventoryManager { get; set; }
        public IReplicationlogRepository ReplicationlogRepository { get; set; }
        public ITracingAgent TracingAgent { get; set; }
    }
}
