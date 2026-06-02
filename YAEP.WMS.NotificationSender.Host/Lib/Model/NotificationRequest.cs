using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    public class NotificationRequest : INotificationRequest
    {
        public string EventName { get; set; }
        public string Data { get; set; }
    }
}
