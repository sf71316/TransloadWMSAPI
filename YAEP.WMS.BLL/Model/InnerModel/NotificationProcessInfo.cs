using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.NotificationReceiver.Common;

namespace YAEP.WMS.BLL.Model
{
    internal class NotificationProcessInfo : INotificationProcessInfo
    {
        public Guid ProcessItemUID { get; set; }
        public int PickQty { get; set; }
    }
}
