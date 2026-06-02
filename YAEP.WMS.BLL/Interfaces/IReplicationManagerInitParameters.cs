using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal interface IReplicationManagerInitParameters
    {
        ITicketInfoRepository TicketInfoRepository { get; set; }
        IInventoryManager InventoryManager { get; set; }
        IReplicationlogRepository ReplicationlogRepository { get; set; }
        ProductCacheManager ProductCacheManager { get; set; }
        PackageCacheManager PackageCacheManager { get; set; }
        IAuthenticationInfo AuthenticationInfo { get; set; }
        ITracingAgent TracingAgent { get; set; }
    }
}
