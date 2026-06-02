using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class GetTicketListParameters : IGetTicketListParameters
    {
        public TicketCategory?[] type { get; set; }
        public ManifestType?[] mtype { get; set; }
        public TicketStatus?[] tstatus { get; set; }
        public Guid[] groupIds { get; set; }
        public string TimeZone { get; set; }
        public DateTime? CreatedStartDate { get; set; }
        public DateTime? CreatedEndDate { get; set; }
    }
}
