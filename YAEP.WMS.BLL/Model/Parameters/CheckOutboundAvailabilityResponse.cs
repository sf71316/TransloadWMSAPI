using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class CheckOutboundAvailabilityResponse : ICheckOutboundAvailabilityResponse
    {
        public CheckOutboundAvailabilityResponse()
        {
            this.EstimateList = new List<ICheckOutboundEstimateItem>();
        }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public Guid RequestPackageUID { get; set; }
        public string RequestPackageName { get; set; }
        public int RequestPackageQty { get; set; }
        public int AllocatedQty { get; set; }
        public int FreeQty { get; set; }
        public IList<ICheckOutboundEstimateItem> EstimateList { get; set; }
    }
    internal class CheckOutboundEstimateItem : ICheckOutboundEstimateItem
    {
        public Guid PackageUID { get; set; }
        public string PackageName { get; set; }
        public int FreeQty { get; set; }
    }
}
