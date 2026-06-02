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
    public class NotificationReceiverRepository<T> : AbstractRepository<T>, INotificationReceiverRepository where T : class, INotificationReceiverModel
    {
        public NotificationReceiverRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

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
        public new IActionResult<bool> Delete(object condition)
        {
            var resultContainer = ActionResultTemplates.Result<bool>();

            try
            {
                bool success = this._Handler.DeleteByDynamicConditions(condition);

                resultContainer.Success = true;
                resultContainer.Content = success;
            }
            catch (Exception ex)
            {
                resultContainer.Message = ex.Message;
                resultContainer.InnerException = ex;
            }

            return resultContainer;
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
        public IActionResult<bool> IsNotify(Guid belongtoUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var result = this.GetNotifyConfig(new { BelongToUID = belongtoUID });
                if (result.Content != null)
                {
                    rs.Content =
                    rs.Success = true;
                }
                else
                {
                    rs.Content =
                    rs.Success = false;
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

        /// <summary>
        /// WMS_Notification_Receiver 外部系統對wms操作時，就會先紀錄之後要回傳結果的URL到這張表
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public IActionResult<INotificationReceiverModel> GetNotifyConfig(object condition)
        {
            var rs = ActionResultTemplates.Result<INotificationReceiverModel>();
            try
            {
                rs.Content = this._Handler.RetrieveByDynamicConditions(condition);
                if (rs.Content != null && rs.Content.Status > (int)ReceiverStatus.Void)
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

        public IActionResult<IEnumerable<INotificationReceiverModel>> GetNotifyConfigCollection(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<INotificationReceiverModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > (int)ReceiverStatus.Void);
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
    }
}
