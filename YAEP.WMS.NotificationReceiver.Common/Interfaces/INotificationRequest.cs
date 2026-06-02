using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.NotificationReceiver.Common
{
    public interface INotificationRequest
    {
        string EventName { get; set; }
        string Data { get; set; }
    }
}
