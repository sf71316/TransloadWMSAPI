using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class NotificationReceiverModel : INotificationReceiverModel
    {
        public Guid UID { get; set; }
        public Guid BelongToUID { get; set; }
        public string ReceiverUrl { get; set; }
        public string ReceiverSecret { get; set; }
        public int Status { get; set; }
    }
}
