using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    public class NotificationArg : EventArgs
    {
        public ConsoleColor Color { get; set; }
        public string Message { get; set; }
    }
}
