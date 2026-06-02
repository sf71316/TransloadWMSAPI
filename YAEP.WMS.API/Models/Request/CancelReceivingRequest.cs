using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    public class CancelReceivingRequest : ICancelReceivingRequest
    {
        public string CustomerPartyName { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public string RefNo { get; set; }
    }
}