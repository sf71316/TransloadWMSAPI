using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    internal class NotificationSenderTaskRepository :
        AbstractRepository<NotificationSenderTaskModel>
    {
        public NotificationSenderTaskRepository()
        {

        }
        public override NotificationSenderTaskModel GetData(object condition)
        {
            var collection = this.RetrieveCollectionByDynamicConditions(condition);
            return collection.FirstOrDefault(p => p.Status > 0);
        }
        public  IEnumerable<NotificationSenderTaskModel> GetProcessTasks(int MaxRetryCount)
        {
            var query = "SELECT * FROM WMS_Notification_Task WHERE Status in @Status AND " +
                "RetryCount <=@MaxRetryCount  ORDER BY CreatedOn ";
            return this.Instance.Query<NotificationSenderTaskModel>(query,new {
                MaxRetryCount= MaxRetryCount,
                Status =new int[] {(int)SenderTaskStatus.Failure,(int)SenderTaskStatus.InQueue }
            });
        }
        public override IEnumerable<NotificationSenderTaskModel> GetList(object condition)
        {
            var collection = this.RetrieveCollectionByDynamicConditions(condition);
            return collection.Where(p => p.Status > 0);
        }
    }
}
