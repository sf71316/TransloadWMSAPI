using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface INotificationSenderTaskModel
    {
        Guid UID { get; set; }
        string EventName { get; set; }
        string ReceiverSecret { get; set; }
        string ReceiverUrl { get; set; }
        string RefNo { get; set; }
        Guid TicketInfoUID { get; set; }
        string Message { get; set; }
        int Status { get; set; }
        int RetryCount { get; set; }
        DateTime CreatedOn { get; set; }
    }
}
