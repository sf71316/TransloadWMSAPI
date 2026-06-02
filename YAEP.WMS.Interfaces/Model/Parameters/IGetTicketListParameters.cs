using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IGetTicketListParameters
    {
        TicketCategory?[] type { get; set; }
        ManifestType?[] mtype { get; set; }
        TicketStatus?[] tstatus { get; set; }
        Guid[] groupIds { get; set; }
        string TimeZone { get; set; }
        DateTime? CreatedStartDate { get; set; }
        DateTime? CreatedEndDate { get; set; }
    }
}
