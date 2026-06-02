using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICheckOutboundAvailabilityResponse
    {
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        Guid RequestPackageUID { get; set; }
        string RequestPackageName { get; set; }
        int RequestPackageQty { get; set; }
        int AllocatedQty { get; set; }
        int FreeQty { get; set; }
        IList<ICheckOutboundEstimateItem> EstimateList { get; set; }
    }
    public interface ICheckOutboundEstimateItem
    {
        Guid PackageUID { get; set; }
        string PackageName { get; set; }
        int FreeQty { get; set; }
    }
}
