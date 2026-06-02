using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{

    public class NotificationSenderTaskRepository<T> : AbstractRepository<T>, INotificationSenderTaskRepository
        where T : class, INotificationSenderTaskModel
    {
        public NotificationSenderTaskRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> BatchAdd(IEnumerable<dynamic> Models)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = @"INSERT INTO [dbo].[WMS_Notification_Task]
                   ([UID]
                   ,[TicketInfoUID]
                   ,[Message]
                   ,[ReceiverSecret]
                   ,[ReceiverUrl]
                   ,[EventName]
                   ,[RefNo]
                   ,[Status]
                   ,[RetryCount])
             VALUES
                   (@UID, 
                   @TicketInfoUID, 
                   @Message,
                   @ReceiverSecret, 
                   @ReceiverUrl, 
                   @EventName,
                   @RefNo,
                   @Status, 
                   @RetryCount)";
                var index = 0;
                var grp = Models.GroupBy(g => index++ / 1000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, items) > 0;
                }

                rs.Success = rs.Content;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<bool> Add(object Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(Model);
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<bool> Edit(object Model, object condition)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(Model, condition);
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<INotificationSenderTaskModel> GetData(object condition)
        {
            var rs = ActionResultTemplates.Result<INotificationSenderTaskModel>();
            try
            {
                rs.Content = this._Handler.RetrieveByDynamicConditions(condition);
                if (rs.Content.Status > (int)SenderTaskStatus.Void)
                    rs.Success = true;
                else
                {
                    rs.Success = false;
                    rs.Content = null;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }

            return rs;
        }

    }
}
