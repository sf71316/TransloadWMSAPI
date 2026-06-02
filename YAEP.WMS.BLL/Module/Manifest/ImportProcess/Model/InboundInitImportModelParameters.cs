using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Identities.Interfaces.Models;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundInitImportParameters : IInboundInitImportParameters
    {
        public IManifestRepository ManifestRepository { get; set; }
        public IManifestItemListRepository ManifestItemListRepository { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public IEnumerable<IGroupUserViewModel> GroupUserViews { get; set; }
        public ProductCacheManager ProductCacheManager { get; set; }
        public SequenceAgent SequenceAgent { get; set; }
        public PackageCacheManager PackageCacheManager { get; set; }
    }
}
