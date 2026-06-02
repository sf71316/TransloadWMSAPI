using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IBulkPickNotificationInfoModel
    {
        Guid TicketInfoUID { get; set; }
        Guid WorkOrderPayloadUID { get; set; }
        string RefNo { get; set; }
        string ReceiverUrl { get; set; }
        string ReceiverSecret { get; set; }
        int ActQty { get; set; }
    }
}
