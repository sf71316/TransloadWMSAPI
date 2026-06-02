using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.NotificationSender.Client.Model
{
    internal class OutboundTicketInfoCompleteRequest : IOutboundTicketInfoCompleteRequest
    {
        public OutboundTicketInfoCompleteRequest()
        {
            this.ProcessItems = new List<INotificationProcessInfo>();
        }
        public string RefNo { get; set; }
        public IList<INotificationProcessInfo> ProcessItems { get; set; }
    }
}
