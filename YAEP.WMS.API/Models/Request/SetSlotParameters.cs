using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class SetSlotParameters : ISetSlotParameters
    {
        public Guid? WorkOrderPodUID { get; set; }
        public Guid? WorkOrderPayloadUID { get; set; }
        public Guid SlotUID { get; set; }
    }
}