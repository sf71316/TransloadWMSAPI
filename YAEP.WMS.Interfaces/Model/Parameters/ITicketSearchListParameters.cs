using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketSearchListParameters
    {
        Guid? WarehouseUID { get; set; }
        string TicketNo { get; set; }
        int? TicketType { get; set; }
        int? TicketStatus { get; set; }
        string ManifestNo { get; set; }
        string Option { get; set; }
        string OptionText { get; set; }
        string manifestref { get; set; }
        string bolno { get; set; }
        string bolref { get; set; }
        string vesselno { get; set; }
        Guid[] PHierarchy { get; set; }
    }
}
