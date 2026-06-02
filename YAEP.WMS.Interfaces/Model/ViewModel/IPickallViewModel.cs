using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IPickallViewModel
    {
        Guid TicketInfoUID { get; set; }
        int TicketInfoType { get; set; }
        Guid TicketUID { get; set; }
        Guid WorkOrderPayloadUID { get; set; }
        int PayloadType { get; set; }
        Guid PodUID { get; set; }
        Guid PayloadUID { get; set; }
        Guid? ItemGroupUID { get; set; }
        Guid ItemUID { get; set; }
        Guid SlotUID { get; set; }
        Guid PackageUID { get; set; }
        int OriginalPayloadType { get; set; }
        int Quantity { get; set; }

    }
}
