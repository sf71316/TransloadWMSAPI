using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models
{
    public class ManinfestRequestModel
    {
        public Guid UID { get; set; }
        public string Name { get; set; }
        public string RefNo { get; set; }
        public int Type { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid PartyUID { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Weight { get; set; }
    }
}