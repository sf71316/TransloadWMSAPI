using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models
{
    public class DeparturedRequest
    {
        public Guid[] carrierTruckUID { get; set; }
    }
}