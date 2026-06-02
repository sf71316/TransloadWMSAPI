using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICalVesselAddItemInnerModel
    {
        Guid ManifestItemUID { get; set; }
        Guid ItemUID { get; set; }
        Guid PackageUID { get; set; }
        int PackageQty { get; set; }
    }
    public interface ICalVesselAssignedItemInnerModel
    {
        Guid VesselManifestUID { get; set; }
        Guid ItemUID { get; set; }
        Guid PackageUID { get; set; }
        int PackageQty { get; set; }
    }
}
