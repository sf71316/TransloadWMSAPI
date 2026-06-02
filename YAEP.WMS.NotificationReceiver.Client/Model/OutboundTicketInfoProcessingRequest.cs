using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.NotificationSender.Client.Model
{
    internal class OutboundTicketInfoProcessingRequest : IOutboundTicketInfoProcessingRequest
    {
        public OutboundTicketInfoProcessingRequest()
        {
            ProcessItems = new List<INotificationProcessInfo>();
        }
        public IList<INotificationProcessInfo> ProcessItems { get; set; }
        public string RefNo { get; set; }
    }
}
