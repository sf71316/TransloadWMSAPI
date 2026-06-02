using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class VesselManifestRequestModel
    {
        public Guid? UID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid ReceivePackage { get; set; }
        public Guid ManifestItemUID { get; set; }
        public Guid VesselUID { get; set; }
        public int ReceiveQty { get; set; }
        public string Name { get; set; }
        public string RefNo { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
    }
}