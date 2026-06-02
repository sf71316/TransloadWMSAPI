using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class BatchChangeToSlotParameter : IBatchChangeToSlotParameter
    {
        public IEnumerable<Guid> TicketInfoUIDs { get; set; }
        public string SlotName { get; set; }
    }
}