using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Notification_Receiver")]
    [DbTable("WMS_Notification_Receiver")]
    public class NotificationReceiverModel : INotificationReceiverModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid BelongToUID { get; set; }
        public string ReceiverUrl { get; set; }
        public string ReceiverSecret { get; set; }
        public int Status { get; set; }
    }
}
