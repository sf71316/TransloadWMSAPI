using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class VesselRequestModel
    {
        public Guid? UID { get; set; }
        public Guid BolUID { get; set; }
        public string RefNo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}