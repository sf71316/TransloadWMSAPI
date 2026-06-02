using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    public class CheckOutboundAvailabilityRequest : ICheckAllocatedParameters
    {
        public CheckOutboundAvailabilityRequest()
        {
            this.Items = new List<CheckOutboundAvailabilityItem>().ToArray();
        }
        public Guid VesselMainifestUID { get; set; }
        public IList<ICheckAllocatedItem> Items { get; set; }
    }
    public class CheckOutboundAvailabilityItem : ICheckAllocatedItem
    {
        public Guid ItemUID { get; set; }
        public Guid PayloadUID { get; set; }
        public int AllocatedQty { get; set; }
    }

}