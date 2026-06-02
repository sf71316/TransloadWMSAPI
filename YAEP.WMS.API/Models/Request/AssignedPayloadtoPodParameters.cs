using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class AssignedPayloadtoPodParameters
    {
        public Guid WorkOrderPodUID { get; set; }
        public Guid WorkOrderPayloadUID { get; set; }
    }
}