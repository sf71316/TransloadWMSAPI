using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class DeallocatedRequest : IDeallocatedRequest
    {
        public Guid? BolUID { get; set; }
        public string RefNo { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public string RequestBy { get; set; }
        public string BolNo { get; set; }
    }
}