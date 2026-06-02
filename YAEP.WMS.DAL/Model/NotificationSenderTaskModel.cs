using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{

    internal class NotificationSenderTaskModel : INotificationSenderTaskModel
    {
        public Guid UID { get; set; }
        public Guid ManifestUID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public string EventName { get; set; }
        public string RefNo { get; set; }
        public string ReceiverSecret { get; set; }
        public string ReceiverUrl { get; set; }
    }
}
