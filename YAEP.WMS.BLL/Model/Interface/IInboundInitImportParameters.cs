using System;
using System.Collections.Generic;
using YAEP.Identities.Interfaces.Models;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal interface IInboundInitImportParameters : IIboundInitParameters, IIboundInitExternalProvider
    {

    }
    internal interface IIboundInitParameters
    {
        Guid CustomerUID { get; set; }
        IEnumerable<IGroupUserViewModel> GroupUserViews { get; set; }
        IManifestItemListRepository ManifestItemListRepository { get; set; }
        Guid WarehouseUID { get; set; }
    }
    internal interface IIboundInitExternalProvider
    {
        IManifestRepository ManifestRepository { get; set; }
        PackageCacheManager PackageCacheManager { get; set; }
        ProductCacheManager ProductCacheManager { get; set; }
        SequenceAgent SequenceAgent { get; set; }
    }
}