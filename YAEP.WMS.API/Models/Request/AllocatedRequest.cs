using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class AllocatedRequest : IAllocatedRequest
    {
        public AllocatedRequest()
        {
            this.Items = new List<AllocatedItemRequest>().ToArray();
        }
        public string RefNo { get; set; }
        public IList<IAllocatedItemRequest> Items { get; set; }
        public Guid WarehouseUID { get; set; }
        public string ReceiverUrl { get; set; }
        public string ReceiverSecret { get; set; }
        public string CustomerPartyName { get; set; }
        public int OrderType { get; set; }
        public string RequestBy { get; set; }
        public DateTime ETD { get; set; }
        public bool UsePackingStation { get; set; }
        public string ShipToAddress { get; set; }
        public string ShipToZip { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToCountry { get; set; }
        public Guid CustomerUID { get; set; }
        public bool PassPackageVersion { get; set; }
        public AllocateType AllocateMode { get; set; }
    }
}