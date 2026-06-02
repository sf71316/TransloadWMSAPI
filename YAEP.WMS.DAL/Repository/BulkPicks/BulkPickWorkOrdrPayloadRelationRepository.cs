using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class BulkPickWorkOrdrPayloadRelationRepository<T> : AbstractRepository<T>, IBulkPickWorkOrdrPayloadRelationRepository
        where T : class, IBulkPickWorkOrderPayloadRelationModel
    {
        public BulkPickWorkOrdrPayloadRelationRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> Create(IEnumerable<IBulkPickWorkOrderPayloadRelationModel> modelCollection)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var executeRs = true;
            try
            {
                var grp = modelCollection.Select((p, index) => new { p, index }).GroupBy(g => g.index / 2000, i => i);
                var query = @"INSERT INTO [dbo].[WMS_BulkPick_WorkOrderPayloadRelation]
           ([BulkPickWorkOrderPayloadUID]
           ,[OriginalWorkOrderPayloadUID]
           ,[Status])
            VALUES
           (@BulkPickWorkOrderPayloadUID
           ,@OriginalWorkOrderPayloadUID
           ,@Status)";
                foreach (var item in grp)
                {
                    executeRs &= this._Handler.Instance.Execute(query, item.Select(o => o.p)) > 0;
                }
                rs.Success = executeRs;
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

        public IActionResult<bool> Exist(Guid WorkOrderPayloadUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"SELECT * FROM [WMS_BulkPick_WorkOrderPayloadRelation] 
                              WHERE BulkPickWorkOrderPayloadUID=@WorkOrderPayloadUID AND Status>0";
                rs.Content = this._Handler.Instance.Query(query, WorkOrderPayloadUID).Count() > 0;
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

        public IActionResult<IEnumerable<IBulkPickNotificationInfoModel>> GetBulkPickOriginalNotificationInfo(IEnumerable<Guid> ticketInfoUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IBulkPickNotificationInfoModel>>();
            try
            {
                var query = @"SELECT [WM].RefNo,[WNR].ReceiverSecret,[WNR].ReceiverUrl
,[OWWOPL].UID WorkOrderPayloadUID,[OWTI].UID TicketInfoUID,
(CASE WHEN [WTI].ShtQty+[WTI].SavQty=0 THEN 
[OWTI].ActQty
ELSE 
[OWWOPL].Qty
END) ActQty
FROM WMS_TicketInfo AS [WTI]
INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID AND [WWOPL].Status>0
INNER JOIN WMS_BulkPick_WorkOrderPayloadRelation AS [WBWOPLR] ON [WBWOPLR].BulkPickWorkOrderPayloadUID=[WWOPL].UID AND [WBWOPLR].Status>0
INNER JOIN WMS_WorkOrder_Payload AS [OWWOPL] ON [OWWOPL].UID=[WBWOPLR].OriginalWorkOrderPayloadUID AND [OWWOPL].Status>0
INNER JOIN WMS_TicketInfo AS [OWTI] ON [OWTI].WorkOrderPayloadUID=[OWWOPL].UID AND [OWTI].Status>0 AND [OWTI].Type=300
INNER JOIN WMS_WorkOrder AS [WWO] ON [WWO].UID=[OWWOPL].WorkOrderUID
INNER JOIN WMS_Manifest AS [WM] ON [WM].UID=[WWO].ManifestUID
INNER JOIN WMS_Notification_Receiver AS [WNR] ON [WNR].BelongToUID=[WWO].ManifestUID
WHERE [WTI].UID IN @TicketInfoUID AND [WTI].Status>0";
                rs.Content = this._Handler.Instance.Query<BulkPickNotificationInfoModel>(query, ticketInfoUID);
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
