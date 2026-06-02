using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class TicketSearchListParameters : ITicketSearchListParameters
    {
        public TicketSearchListParameters()
        {
        }

        public Guid? WarehouseUID { get; set; }
        public string TicketNo { get; set; }
        public int? TicketType { get; set; }
        public int? TicketStatus { get; set; }
        public string ManifestNo { get; set; }
        public string Option { get; set; }
        public string OptionText { get; set; }
        public string manifestref { get; set; }
        public string bolno { get; set; }
        public string bolref { get; set; }
        public string vesselno { get; set; }
        public Guid[] PHierarchy { get; set; }
    }
}
