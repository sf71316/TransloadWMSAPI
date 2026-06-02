using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICheckManifestItemStatusResultModel
    {
        int ManifestStatus { get; set; }
        int ManifestItemStatus { get; set; }
        Guid BolUID { get; set; }
        Guid VesselUID { get; set; }
        Guid ManifestItemUID { get; set; }
        Guid ItemUID { get; set; }
        int CompleteQty { get; set; }
        Guid CompletePackageUID { get; set; }
        int OriginalQty { get; set; }
        Guid OriginalPackageUID { get; set; }
    }
}
