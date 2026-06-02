using System;
using System.Collections.Generic;

namespace YAEP.WMS.Interfaces
{
    public interface IReceivingContainer
    {
        Guid UID { get; set; }
        string ExternalData { get; set; }
        string PackageUOM { get; set; }
        IList<Guid> ManifestItemUID { get; set; }
        Guid VesselManifestUID { get; set; }
        IList<IReceivingItemModel> Items { get; }
    }
}