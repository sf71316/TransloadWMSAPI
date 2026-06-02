using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class ChangeToSlotParameter : IChangeToSlotParameter
    {
        public Guid TicketInfoUID { get; set; }
        public string SlotName { get; set; }
    }
}