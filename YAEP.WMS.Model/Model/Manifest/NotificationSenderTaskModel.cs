using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Notification_Task")]
    [DbTable("WMS_Notification_Task")]
    public class NotificationSenderTaskModel : INotificationSenderTaskModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid ManifestUID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public string Message { get; set; }
        public string ReceiverSecret { get; set; }
        public string ReceiverUrl { get; set; }
        public int Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public int RetryCount { get; set; }
        public string EventName { get; set; }
        public string RefNo { get; set; }
    }
}
