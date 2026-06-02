using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.Utilities.Extensions;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class BulkPickRepository<T> : AbstractRepository<T>, IBulkPickRepository where T : class, IBulkPickModel
    {
        public BulkPickRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<IBulkPickModel> GetBulkPickModel(Guid bulkPickUID)
        {
            var result = ActionResultTemplates.Result<IBulkPickModel>();
            try
            {
                var model = this._Handler.Retrieve(bulkPickUID);

                result.Content = model;
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
        public IActionResult<IEnumerable<IBulkPickModel>> GetBulkPickCollection(IEnumerable<Guid> bulkPickUID)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickModel>>();
            try
            {
                var collection = this._Handler.RetrieveCollectionByDynamicConditions(new { UID = bulkPickUID });

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

        public IActionResult<IEnumerable<IBulkPickViewModel>> GetList(IBulkPickSearchParameters parameters)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickViewModel>>();

            string searchConditionString = this.getSearchCondition(parameters);

            string query =
            $@"SELECT [WMS_BulkPick].*,[WMS_Ticket].ID TicketNo
            FROM [WMS_BulkPick] 
            INNER JOIN [WMS_Ticket] ON [WMS_BulkPick].TicketUID=[WMS_Ticket].UID AND [WMS_Ticket].Status>0
            {searchConditionString}
            ";

            try
            {
                var multiResult = this._Handler.Instance.QueryMultiple(query, parameters);
                var bulkPicks = multiResult.Read<BulkPickViewModel>();


                result.Content = bulkPicks;
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

        public IActionResult<IEnumerable<IBulkPickManifestViewModel>> GetManifestList(IBulkPickManifestSearchParameters parameters)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickManifestViewModel>>();

            if (parameters == null)
            {
                return result;
            }

            // default conditions
            if ((parameters.TicketInfoStatus?.Count() ?? 0) == 0)
            {
                if (parameters.TicketInfoStatus == null)
                {
                    parameters.TicketInfoStatus = new List<int>();
                }
                parameters.TicketInfoStatus.Add((int)TicketInfoStatus.Open);
            }
            if ((parameters.TicketInfoType?.Count() ?? 0) == 0)
            {
                if (parameters.TicketInfoType == null)
                {
                    parameters.TicketInfoType = new List<int>();
                }
                parameters.TicketInfoType.Add((int)TicketInfoType.Move);
            }
            if ((parameters.ManifestType?.Count() ?? 0) == 0)
            {
                if (parameters.ManifestType == null)
                {
                    parameters.ManifestType = new List<int>();
                }
                parameters.ManifestType.Add((int)ManifestType.Outbound);
            }

            string searchConditionString = this.getSearchCondition(parameters);

            string query =
$@"SELECT	DISTINCT
						[TicketInfo].[UID] AS [TicketInfoUID], 
						[TicketInfo].[ID] AS [TicketInfoID], 
						[Manifest].[UID] AS [ManifestUID], 
						[Manifest].[ID] AS [ManifestNo], 
						[ManifestItemList].[UID] AS [ManifestItemListUID], 
						[ManifestItemList].[ID] AS [ManifestItemListID],
						[Manifest].[Name] AS [CustomerPartyName],
						[Manifest].[PartyUID] AS [CustomerUID], 
						[Manifest].[RefNo] AS [RefNo],   
						[ManifestItemList].[ItemUID],
						[ManifestItemList].Status,
						[ManifestItemList].PackageQty,
						[TicketInfo].EstQty AS [EstQty] ,
						[Slot].[ID] AS [FromSlot], 
						[LoadingZoneSlot].[ID] AS [ToSlot], 
						[BOL].[ShipViaUID]						 
FROM [WMS_Manifest] AS [Manifest] 
						INNER JOIN [WMS_BOL] AS [BOL] ON ([BOL].[ManifestUID] = [Manifest].[UID]) 
						INNER JOIN [WMS_Manifest_Item_List] AS [ManifestItemList] ON ([ManifestItemList].[ManifestUID] = [Manifest].[UID]) 
																						AND ([ManifestItemList].[Status] > 0)
						INNER JOIN [WMS_Vessel_Manifest] AS [VesselManifest] ON ([VesselManifest].ManifestItemUID = [ManifestItemList].[UID]) 
																						AND ([VesselManifest].[Status] > 0)
						INNER JOIN [WMS_WorkOrder_Payload] AS [WorkPayload] ON ([WorkPayload].VesselManifestUID = [VesselManifest].[UID]) 
																						AND ([WorkPayload].[Status] > 0)
						INNER JOIN [WMS_TicketInfo] AS [TicketInfo] ON ([TicketInfo].[WorkOrderPayloadUID] = [WorkPayload].[UID]) 
																						AND ([TicketInfo].[Status] > 0)
						INNER JOIN [WMS_Slot] AS [Slot] ON [Slot].[UID] = [WorkPayload].[SlotUID]
						INNER JOIN [WMS_Slot] AS [LoadingZoneSlot] ON ([LoadingZoneSlot].[UID] = [WorkPayload].[LoadingZoneSlotUID]) 
						LEFT OUTER JOIN [WMS_BulkPick_TicketInfoRelation] AS [TickInfoRelation] ON ([TickInfoRelation].[TicketInfoUID] = [TicketInfo].[UID]) 
						LEFT OUTER JOIN [WMS_BulkPick] AS [BulkPick] ON ([BulkPick].[UID] = [TickInfoRelation].[BulkPickUID])
{searchConditionString} 
{@"  AND ([BulkPick].[Status] = 0 
				    OR [TickInfoRelation].[Status] = 0
				    OR [TickInfoRelation].[TicketInfoUID] IS NULL) "}
";

            try
            {
                var collection = this._Handler.Instance.Query<BulkPickManifestViewModel>(query, parameters);

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

        public IActionResult<IEnumerable<IBulkPickSaveModel>> GetBulkPickSaveDataByTicket(IEnumerable<Guid> ticketInfoUID)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickSaveModel>>();

            if ((ticketInfoUID?.Count() ?? 0) == 0)
            {
                result.Message = $"Incorrect Parameters: ${ticketInfoUID}";
                return result;
            }

            string query =
@"SELECT DISTINCT
		        [TicketInfo].[UID] AS [TicketInfoUID],  
		        [WorkPayload].[SlotUID] AS [OriginalSlotUID],
		        [WorkPayload].[LoadingZoneSlotUID] AS [TargetSlotUID] 
FROM [WMS_TicketInfo] AS [TicketInfo] 
			    INNER JOIN [WMS_WorkOrder_Payload] AS [WorkPayload] ON ([WorkPayload].[UID] = [TicketInfo].[WorkOrderPayloadUID])
			    INNER JOIN [WMS_WorkOrder] AS [WorkOrder] ON ([WorkOrder].[UID] = [WorkPayload].[WorkOrderUID])
			    INNER JOIN [WMS_Manifest] AS [Manifest] ON ([Manifest].[UID] = [WorkOrder].[ManifestUID])
WHERE ([TicketInfo].[UID] IN @TicketInfoUID) 
                AND ([Manifest].[Type] = @ManifestType) 
                AND ([Manifest].[Status] = @ManifestStatus) 
                AND ([TicketInfo].[Type] = @TicketInfoType)
                AND ([TicketInfo].[Status] = @TicketInfoStatus)
";

            try
            {
                var collection = this._Handler.Instance.Query<BulkPickSaveModel>(query, new
                {
                    TicketInfoUID = ticketInfoUID,
                    ManifestType = (int)ManifestType.Outbound,
                    ManifestStatus = (int)ManifestStatus.Open,
                    TicketInfoType = (int)TicketCategory.Move,
                    TicketInfoStatus = (int)TicketInfoStatus.Open,
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

        public IActionResult<IEnumerable<IBulkPickModel>> GetBulkPickByTicketInfo(IEnumerable<Guid> ticketInfoUID)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickModel>>();
            List<IBulkPickModel> collection = new List<IBulkPickModel>();
            if ((ticketInfoUID?.Count() ?? 0) == 0)
            {
                result.Message = $"Incorrect Parameters: ${ticketInfoUID}";
                return result;
            }

            string query =
@"SELECT DISTINCT [BulkPick].*
FROM [WMS_BulkPick] AS [BulkPick] 
			    INNER JOIN [WMS_BulkPick_TicketInfoRelation] AS [TicketInfoRelation] ON ([TicketInfoRelation].[BulkPickUID] = [BulkPick].[UID])  
WHERE ([TicketInfoRelation].[TicketInfoUID] IN @TicketInfoUID) 
                AND ([BulkPick].[Status] > @BulkPickVoidStatus)
				AND ([TicketInfoRelation].[Status] = @TicketInfoRelationStatus)
";

            try
            {
                var index = 0;
                var grp = ticketInfoUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    collection.AddRange(this._Handler.Instance.Query<T>(query, new
                    {
                        TicketInfoUID = items,
                        BulkPickVoidStatus = (int)BulkPickStatus.Void,
                        TicketInfoRelationStatus = (int)BulkPickTicketInfoRelationStatus.Active,
                    }));
                }


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

        public IActionResult<bool> ChangeBulkPickStatus(IEnumerable<Guid> bulkPickTicketUID, int status, string modifiedBy = "")
        {
            var result = ActionResultTemplates.Result<bool>();
            try
            {
                result.Content = true;
                var query = "UPDATE WMS_BulkPick SET Status=@Status,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn WHERE UID IN @UID AND Status>0";
                var index = 0;
                var grp = bulkPickTicketUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    result.Content &= this._Handler.Instance.Execute(query,
                        new
                        {
                            Status = (int)status,
                            modifiedBy = modifiedBy,
                            ModifiedOn = DateTime.UtcNow,
                            UID = items
                        }) > 0;
                }

                result.Success = result.Content;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                result.Success = false;
                result.InnerException = ex;
            }
            return result;
        }

        public IActionResult<bool> Create(IBulkPickModel model)
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

        public IActionResult<bool> Create(IEnumerable<IBulkPickModel> collection)
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
                this._Handler.Instance.Connection.Open();
                bool success = this._Handler.BatchCreateByDynamic(collection, this._Handler.GetTableName());
                this._Handler.Instance.Connection.Close();
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

        public IActionResult<bool> Delete(IEnumerable<Guid> bulkPickUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                bool success = this._Handler.DeleteByDynamicConditions(new { UID = bulkPickUID, Status = new int[] { (int)BulkPickStatus.Open } });
                rs.Content = success;
                rs.Success = success;
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

        public IActionResult<IEnumerable<IBulkPickInfoModel>> GetBulkPickInfoList(Guid bulkPickUID)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickInfoModel>>();

            if (bulkPickUID == Guid.Empty)
            {
                result.Message = $"Incorrect Parameters: ${bulkPickUID}";
                return result;
            }

            string query =
@"SELECT 
		            [BulkPick].[UID] AS [BulkPickUID], 
		            [BulkPick].[ID] AS [BulkPickID], 
		            [BulkPick].[PartyName], 
		            [WorkPayload].[ItemUID], 
		            [TicketInfo].[EstQty], 
		            [TicketInfo].[ActQty], 
		            [TicketInfo].[ShtQty], 
		            [TicketInfo].[SavQty], 
		            [FromSlot].[Name] AS [From], 
		            [ToSlot].[Name] AS [To], 
		            [TicketInfoRelation].[TicketInfoUID], 
		            [TicketInfoRelation].[ID] AS [TicketInfoRelationID]
FROM [WMS_BulkPick] AS [BulkPick] 
		            INNER JOIN [WMS_BulkPick_TicketInfoRelation] AS [TicketInfoRelation] ON [TicketInfoRelation].[BulkPickUID] = [BulkPick].[UID]
		            INNER JOIN [WMS_TicketInfo] AS [TicketInfo] ON [TicketInfo].[UID] = [TicketInfoRelation].[TicketInfoUID]
		            INNER JOIN [WMS_WorkOrder_Payload] AS [WorkPayload] ON ([WorkPayload].[UID] = [TicketInfo].[WorkOrderPayloadUID])
		            LEFT OUTER JOIN [WMS_Slot] AS [FromSlot] ON [FromSlot].[UID] = [TicketInfoRelation].[FromSlotUID]
		            LEFT OUTER JOIN [WMS_Slot] AS [ToSlot] ON [ToSlot].[UID] = [TicketInfoRelation].[ToSlotUID]
WHERE ([BulkPick].[UID] = @BulkPickUID)
";
            try
            {
                var collection = this._Handler.Instance.Query<BulkPickInfoModel>(query, new { BulkPickUID = bulkPickUID });

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

        public IActionResult<IEnumerable<IBulkPickInfoViewModel>> GetBulkPickInfoViewList(Guid bulkPickUID)
        {
            var result = ActionResultTemplates.Result<IEnumerable<IBulkPickInfoViewModel>>();

            if (bulkPickUID == Guid.Empty)
            {
                result.Message = $"Incorrect Parameters: ${bulkPickUID}";
                return result;
            }

            string query =
@"SELECT 
		        [BulkPick].[UID] AS [BulkPickUID], 
		        [BulkPick].[ID] AS [BulkPickID], 
		        [BulkPick].[PartyName] AS [CustomerName], 
		        [Manifest].[ID] AS [ManifestNo], 
		        [Manifest].[RefNo] AS [RefNo], 
		        [WorkPayload].[ItemUID], 
		        [TicketInfo].[EstQty], 
		        [TicketInfo].[ActQty], 
		        [FromSlot].[Name] AS [FromSlot], 
		        [ToSlot].[Name] AS [ToSlot], 
		        [TicketInfo].[ID] AS [TicketInfoID], 
		        [TicketInfo].[UID] AS [TicketInfoUID], 
		        [BOL].[ShipViaUID]
FROM [WMS_BulkPick] AS [BulkPick]  
		        INNER JOIN [WMS_TicketInfo] AS [TicketInfo] ON [TicketInfo].[TicketUID] = [BulkPick].[TicketUID]
		        INNER JOIN [WMS_WorkOrder_Payload] AS [WorkPayload] ON ([WorkPayload].[UID] = [TicketInfo].[WorkOrderPayloadUID])
		        INNER JOIN [WMS_WorkOrder] AS [WorkOrder] ON ([WorkOrder].[UID] = [WorkPayload].[WorkOrderUID])
		        INNER JOIN [WMS_Manifest] AS [Manifest] ON ([Manifest].[UID] = [WorkOrder].[ManifestUID])
		        INNER JOIN [WMS_BOL] AS [BOL] ON ([BOL].[ManifestUID] = [Manifest].[UID]) 
		        LEFT OUTER JOIN [WMS_Slot] AS [FromSlot] ON [FromSlot].[UID] = [WorkPayload].[SlotUID]
		        LEFT OUTER JOIN [WMS_Slot] AS [ToSlot] ON [ToSlot].[UID] = [WorkPayload].[LoadingZoneSlotUID]
WHERE ([BulkPick].[UID] = @BulkPickUID)
";
            try
            {
                var collection = this._Handler.Instance.Query<BulkPickInfoViewModel>(query, new { BulkPickUID = bulkPickUID });

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

        private string getSearchCondition(IBulkPickSearchParameters parameters)
        {
            var formatter = this._Handler.SqlFormatter;

            var conditions = new List<string>();

            if ((parameters.UID?.Length ?? 0) > 0)
            {
                conditions.Add($" ({formatter.Table("WMS_BulkPick")}.{formatter.Column(nameof(IBulkPickModel.UID))} IN {formatter.Parameter(nameof(parameters.UID))}) ");
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(parameters.ID))
                {
                    conditions.Add($" ({formatter.Table("WMS_BulkPick")}.{formatter.Column(nameof(IBulkPickModel.ID))} = {formatter.Parameter(nameof(parameters.ID))}) ");
                }
                if (!String.IsNullOrWhiteSpace(parameters.Name))
                {
                    conditions.Add($" ({formatter.Table("WMS_BulkPick")}.{formatter.Column(nameof(IBulkPickModel.Name))} = {formatter.Parameter(nameof(parameters.Name))}) ");
                }
                if (!String.IsNullOrWhiteSpace(parameters.PartyName))
                {
                    conditions.Add($" ({formatter.Table("WMS_BulkPick")}.{formatter.Column(nameof(IBulkPickModel.PartyName))} = {formatter.Parameter(nameof(parameters.PartyName))}) ");
                }

                if ((parameters.Status?.Count() ?? 0) > 0)
                {
                    conditions.Add($" ({formatter.Table("WMS_BulkPick")}.{formatter.Column(nameof(IBulkPickModel.Status))} IN {formatter.Parameter(nameof(parameters.Status))}) ");

                }
                else
                {
                    conditions.Add($" ({formatter.Table("WMS_BulkPick")}.{formatter.Column(nameof(IBulkPickModel.Status))} > {(int)BulkPickStatus.Void}) ");

                }
            }

            return $" WHERE {String.Join(" AND ", conditions)} ";
        }
        private string getSearchCondition(IBulkPickManifestSearchParameters parameters)
        {
            var formatter = this._Handler.SqlFormatter;

            var conditions = new List<string>();

            if ((parameters.TicketInfoStatus?.Count() ?? 0) > 0)
            {
                conditions.Add($" ({formatter.Table("TicketInfo")}.{formatter.Column(nameof(ITicketInfoModel.Status))} IN {formatter.Parameter(nameof(parameters.TicketInfoStatus))}) ");
            }
            if ((parameters.TicketInfoType?.Count() ?? 0) > 0)
            {
                conditions.Add($" ({formatter.Table("TicketInfo")}.{formatter.Column(nameof(ITicketInfoModel.Type))} IN {formatter.Parameter(nameof(parameters.TicketInfoType))}) ");
            }

            if (parameters.CustomerUID.HasValue)
            {
                conditions.Add($" ({formatter.Table("Manifest")}.{formatter.Column(nameof(IManifestModel.PartyUID))} = {formatter.Parameter(nameof(parameters.CustomerUID))}) ");
            }
            if (!String.IsNullOrWhiteSpace(parameters.RefNo))
            {
                conditions.Add($" ({formatter.Table("Manifest")}.{formatter.Column(nameof(IManifestModel.RefNo))} = {formatter.Parameter(nameof(parameters.RefNo))}) ");
            }
            if (!String.IsNullOrWhiteSpace(parameters.Name))
            {
                conditions.Add($" ({formatter.Table("Manifest")}.{formatter.Column(nameof(IManifestModel.Name))} = {formatter.Parameter(nameof(parameters.Name))}) ");
            }
            if ((parameters.ManifestType?.Count() ?? 0) > 0)
            {
                conditions.Add($" ({formatter.Table("Manifest")}.{formatter.Column(nameof(IManifestModel.Type))} IN {formatter.Parameter(nameof(parameters.ManifestType))}) ");
            }

            switch (parameters.DateBy)
            {
                case "ETD":
                default:
                    if (parameters.StartDate.HasValue)
                    {
                        conditions.Add($" ({formatter.Table("BOL")}.{formatter.Column(nameof(IBolModel.ETA))} >= {formatter.Parameter(nameof(parameters.StartDate))}) ");
                    }
                    if (parameters.EndDate.HasValue)
                    {
                        conditions.Add($" ({formatter.Table("BOL")}.{formatter.Column(nameof(IBolModel.ETA))} <= {formatter.Parameter(nameof(parameters.EndDate))}) ");
                    }
                    break;
            }

            string optionText = parameters.OptionText?.ToLower();
            switch (optionText)
            {
                case "shipvia":
                default:

                    break;
            }



            return $" WHERE {String.Join(" AND ", conditions)} ";
        }

        public IActionResult<IEnumerable<IBulkPickInfobyOutboundViewModel>> GetBulkPickInfoByTicketInfo(IEnumerable<Guid> ticketInfoUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IBulkPickInfobyOutboundViewModel>>();
            try
            {
                var query = @"SELECT WBP.TicketUID,WBTI.* FROM [WMS_BulkPick_TicketInfoRelation] AS WBTI
                            INNER JOIN [WMS_BulkPick] AS WBP ON WBTI.BulkPickUID=WBP.UID AND WBP.Status>0
                            WHERE WBP.UID IN (
                            SELECT BulkPickUID FROM [WMS_BulkPick_TicketInfoRelation] 
                            WHERE TicketInfoUID IN @TicketInfoUID AND Status>0
                            ) AND WBTI.Status>0";
                rs.Content = this._Handler.Instance.Query<BulkPickInfobyOutboundViewModel>(query, new { TicketInfoUID = ticketInfoUID });
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
        public IActionResult<IEnumerable<IBulkPickModel>> GetBulkPickByTicketCollection(IEnumerable<Guid> ticketUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IBulkPickModel>>();
            try
            {
                var query = @"SELECT WBP.* FROM [WMS_BulkPick] AS WBP 
                            WHERE WBP.TicketUID IN @ticketUID AND WBP.Status>0";
                rs.Content = this._Handler.Instance.Query<BulkPickModel>(query, new { ticketUID = ticketUID });
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
