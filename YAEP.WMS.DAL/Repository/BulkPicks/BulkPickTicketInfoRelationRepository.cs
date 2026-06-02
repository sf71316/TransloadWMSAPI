using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class BulkPickTicketInfoRelationRepository<T> : AbstractRepository<T>, IBulkPickTicketInfoRelationRepository where T : class, IBulkPickTicketInfoRelationModel
    {
        public BulkPickTicketInfoRelationRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> Create(IBulkPickTicketInfoRelationModel model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(model);
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

        public IActionResult<bool> Create(IEnumerable<IBulkPickTicketInfoRelationModel> collection)
        {
            string account = this._Handler.AuthenticationInfo?.Account;
            if (!String.IsNullOrWhiteSpace(account))
            {
                foreach (var model in collection)
                {
                    model.CreatedBy = account;
                    model.ModifiedBy = account;
                }
            }

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //this._Handler.Instance.Connection.Open();
                //bool success = this._Handler.BatchCreateByDynamic(collection, this._Handler.GetTableName());
                //this._Handler.Instance.Connection.Close();
                var query = @"
                INSERT INTO [dbo].[WMS_BulkPick_TicketInfoRelation]
                           ([UID]
                           ,[ID]
                           ,[Type]
                           ,[Status]
                           ,[BulkPickUID]
                           ,[TicketInfoUID]
                           ,[FromSlotUID]
                           ,[ToSlotUID]
                           ,[CreatedBy]
                           ,[CreatedOn]
                           ,[ModifiedBy]
                           ,[ModifiedOn])
                     VALUES
                           (@UID
                           ,@ID
                           ,@Type
                           ,@Status
                           ,@BulkPickUID
                           ,@TicketInfoUID
                           ,@FromSlotUID
                           ,@ToSlotUID
                           ,@CreatedBy
                           ,@CreatedOn
                           ,@ModifiedBy
                           ,@ModifiedOn)
                
                ";
                var index = 0;
                bool success = true;
                var grp = collection.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    success &= this._Handler.Instance.Execute(query, items) > 0;
                }

                rs.Content = success;
                rs.Success = success;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.InnerException = ex;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<bool> DeleteByBulkPick(Guid bulkPickUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "Update WMS_BulkPick_TicketInfoRelation Set Status=@Status WHERE BulkPickUID=@BulkPickUID";
                rs.Content = this._Handler.Instance.Execute(query, new
                {
                    BulkPickUID = bulkPickUID,
                    Status = (int)BulkPickTicketInfoRelationStatus.Void
                }) > 0;
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
        public IActionResult<bool> Delete(Guid uid)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(new { UID = uid });
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

        public IActionResult<IEnumerable<IBulkPickTicketInfoRelationModel>> GetTicketRelations(IEnumerable<Guid> bulkPickUID)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickTicketInfoRelationModel>>();

            if ((bulkPickUID?.Count() ?? 0) == 0)
            {
                result.Message = $"Incorrect Parameters: ${bulkPickUID}";
                return result;
            }

            string query =
@"SELECT DISTINCT [TicketInfoRelation].*
FROM [WMS_BulkPick] AS [BulkPick] 
			    INNER JOIN [WMS_BulkPick_TicketInfoRelation] AS [TicketInfoRelation] ON ([TicketInfoRelation].[BulkPickUID] = [BulkPick].[UID])  
WHERE ([BulkPick].[UID] IN @BulkPickUID) 
                AND ([BulkPick].[Status] > @BulkPickVoidStatus)
				AND ([TicketInfoRelation].[Status] = @TicketInfoRelationStatus)
";

            try
            {
                var collection = this._Handler.Instance.Query<T>(query, new
                {
                    BulkPickUID = bulkPickUID,
                    BulkPickVoidStatus = (int)BulkPickStatus.Void,
                    TicketInfoRelationStatus = (int)BulkPickTicketInfoRelationStatus.Active,
                });

                result.Content = collection;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.InnerException = ex;
                result.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
            }

            return result;
        }
    }
}
