using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.DAL.Extension;
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class TicketInfoRepository<T> : AbstractRepository<T>, ITicketInfoRepository
        where T : class, ITicketInfoModel
    {
        public IActionResult<IManifestModel> GetManifest(Guid TicketInfoUID)
        {

            var rs = ActionResultTemplates.Result<IManifestModel>();
            try
            {
                var query = @"SELECT [WM].* FROM WMS_TicketInfo AS [WTI] 
                INNER JOIN WMS_Ticket AS [WT] ON　[WTI].TicketUID=[WT].UID AND [WTI].Status>0
                INNER JOIN WMS_WorkOrder AS [WWO] ON [WWO].UID=[WT].WorkOrderUID AND [WWO].Status>0
                INNER JOIN WMS_Manifest AS [WM] ON [WM].UID=[WWO].ManifestUID  AND [WM].Status>0
                WHERE [WT].Status>0 AND [WTI].UID=@TicketInfoUID";
                rs.Content = this._Handler.Instance.QueryFirst<ManifestInnerModel>(query, new { TicketInfoUID = TicketInfoUID });
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
        public IActionResult<ITicketInfoModel> GetData(Guid TicketInfoUID)
        {

            var rs = ActionResultTemplates.Result<ITicketInfoModel>();
            try
            {
                rs.Content = this._Handler.Retrieve(TicketInfoUID);
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
        public IActionResult<IEnumerable<ITicketInfoModel>> GetListByTicket(Guid[] TicketsUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                var query = string.Format("SELECT * FROM WMS_TicketInfo WHERE TicketUID IN ({0}) AND Status>0",
                    string.Join(",", TicketsUID.Select(p => "'" + p + "'")));
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query, null);
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
        public IActionResult<IEnumerable<ITicketInfoModel>> GetList(Guid[] TicketInfoUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                var query = string.Format("SELECT * FROM WMS_TicketInfo WHERE UID IN ({0}) AND Status>0",
                    string.Join(",", TicketInfoUID.Select(p => "'" + p + "'")));
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query, null);
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
        public IActionResult<IEnumerable<ITicketInfoModel>> GetList(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > 0); ;
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
        public TicketInfoRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> AddTickInfos(IEnumerable<ITicketInfoModel> Collection)
        {

            var rs = ActionResultTemplates.Result<bool>();
            var dt = this.ToDataTable<ITicketInfoModel>(Collection);

            //var query = @"INSERT INTO [dbo].[WMS_TicketInfo]
            //       ([UID]
            //       ,[TicketUID]
            //       ,[ID]
            //       ,[Name]
            //       ,[Type]
            //       ,[WorkOrderPodUID]
            //       ,[WorkOrderPayloadUID]
            //       ,[EstQty]
            //       ,[ActQty]
            //       ,[ShtQty]
            //       ,[SavQty]
            //       ,[Status]
            //       ,[Description]
            //       ,[OperationInstruction]
            //       ,[OperationSuggestion]
            //       ,[CreatedBy]
            //       ,[CreatedOn])
            // VALUES
            //       (
            // @UID,
            //       @TicketUID,
            //       @ID, 
            //       @Name,
            //       @Type,
            //       @WorkOrderPodUID, 
            //       @WorkOrderPayloadUID, 
            //       @EstQty,
            //       @ActQty,
            //       @ShtQty,
            //       @SavQty,
            //       @Status,
            //       @Description, 
            //       @OperationInstruction,
            //       @OperationSuggestion,
            //       @CreatedBy,
            //       @CreatedOn)";
            //SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            //cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
            //cmd.Parameters.Add("@TicketUID", SqlDbType.UniqueIdentifier, 16, "TicketUID");
            //cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
            //cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
            //cmd.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");
            //cmd.Parameters.Add("@WorkOrderPodUID", SqlDbType.UniqueIdentifier, 16, "WorkOrderPodUID");
            //cmd.Parameters.Add("@WorkOrderPayloadUID", SqlDbType.UniqueIdentifier, 16, "WorkOrderPayloadUID");
            //cmd.Parameters.Add("@EstQty", SqlDbType.Int, 4, "EstQty");
            //cmd.Parameters.Add("@ActQty", SqlDbType.Int, 4, "ActQty");
            //cmd.Parameters.Add("@ShtQty", SqlDbType.Int, 4, "ShtQty");
            //cmd.Parameters.Add("@SavQty", SqlDbType.Int, 4, "SavQty");
            //cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
            //cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000, "Description");
            //cmd.Parameters.Add("@OperationInstruction", SqlDbType.NVarChar, 1000, "OperationInstruction");
            //cmd.Parameters.Add("@OperationSuggestion", SqlDbType.NVarChar, 1000, "OperationSuggestion");
            //cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
            //cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
            //cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
            //cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");
            //rs = this.BatchInsertTable(dt, cmd);

            rs.Success = rs.Content = true;
            using (var sqlBulkCopy = new SqlBulkCopy(this._Handler.Instance.Connection as SqlConnection,
                SqlBulkCopyOptions.Default, (SqlTransaction)this._Handler.Instance.Transaction))
            {

                sqlBulkCopy.BulkCopyTimeout = 3600;
                sqlBulkCopy.BatchSize = 10000;
                foreach (DataColumn column in dt.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                sqlBulkCopy.DestinationTableName = "WMS_TicketInfo";
                sqlBulkCopy.WriteToServer(dt);
            }
            return rs;
        }
        public IActionResult<ITicketInfoCollection> GetTicketInfo(IGetTicketInfoParameters parameters)
        {
            TicketInfoInnerCollection ticketInfoInnerCollection = new TicketInfoInnerCollection();
            var rs = ActionResultTemplates.Result<ITicketInfoCollection>();
            try
            {

                var query = @"SELECT DISTINCT 
                            [WWOP].Type AS 'StorageType',
                            [WWOP].Name AS 'PodName',
                            [WWOP].PodUID,
                            [WWOP].ContainerType,
                            [WWOPL].PayloadUID,
                            [WWOPL].WorkOrderPodUID,
                            [WT].ID AS 'TicketID',
                            [WT].ManifestType ,
                            [WT].WarehouseUID ,
                            [WT].Type AS 'TicketType',
                            [WT].OperationInstruction,
                            [WT].OperationSuggestion,
                            [WTI].UID,
                            [WTI].Status AS 'TicketInfoStatus',
                            [WTI].EstQty,
                            [WTI].ActQty,
                            [WTI].ShtQty,
                            [WTI].SavQty,
                            [WTI].Type as 'Service',
                            [WTI].Description,
                            [WWOPL].ItemUID,
                            [WWOPL].LoadingZoneSlotUID 'SourceLoadingZoneSlotUID',
                            [WWOPL].SlotUID 'SourceSlotUID',
                            [WWOPL].PayloadPackageUID,
                            [WWOPL].PackageUID 'SourcePackageUID',
                            2 'MappingType'
                            FROM [WMS_TicketInfo_Assignee_Relation] AS [WTAR]
                            INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].UID=[WTAR].TicketInfoUID
                            INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID
                            INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].UID=[WTI].WorkOrderPayloadUID
                            INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WWOPL].WorkOrderPodUID
                            WHERE {0}    [WTAR].Status>0 AND [WTI].Status>0  AND [WT].Status>0  AND [WWOPL].Status>0  AND [WWOP].Status>0 
                            ;
                            SELECT DISTINCT 
                            [WWOP].Type AS 'StorageType',
                            [WWOP].Name AS 'PodName',
                            [WWOP].PodUID,
                            [WWOP].ContainerType,
                            [WWOPL].PayloadUID,
                            [WWOPL].WorkOrderPodUID,
                            [WT].ID AS 'TicketID',
                            [WT].ManifestType ,
                            [WT].WarehouseUID ,
                            [WT].Type AS 'TicketType',
                            [WT].OperationInstruction,
                            [WT].OperationSuggestion,
                            [WTI].UID,
                            [WTI].Status AS 'TicketInfoStatus',
                            [WTI].EstQty,
                            [WTI].ActQty,
                            [WTI].ShtQty,
                            [WTI].SavQty,
                            [WTI].Type as 'Service',
                            [WTI].Description,
                            [WWOPL].ItemUID,
                            [WWOPL].LoadingZoneSlotUID 'SourceLoadingZoneSlotUID',
                            [WWOPL].SlotUID 'SourceSlotUID',
                            [WWOPL].PayloadPackageUID,
                            [WWOPL].PackageUID 'SourcePackageUID',
                            1 'MappingType'
                            FROM [WMS_TicketInfo_Assignee_Relation] AS [WTAR]
                            INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].UID=[WTAR].TicketInfoUID
                            INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID
                            INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WTI].WorkOrderPodUID
                            INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                            WHERE {0} [WTAR].Status>0 AND [WTI].Status>0  AND [WT].Status>0  AND [WWOPL].Status>0  AND [WWOP].Status>0  ";
                //var = this._Handler.Instance.Query<TicketInfoListInnerModel>(string.Format(query, getCondition(parameters)), parameters);
                query = string.Format(query, getCondition(parameters));
                var result = this._Handler.Instance.QueryMultiple(query, parameters);
                var mappingpy = result.Read<TicketInfoListInnerModel>();
                var mappingpl = result.Read<TicketInfoListInnerModel>();
                foreach (var item in mappingpy)
                {
                    item.TicketInfoStatusName = Enum.GetName(typeof(TicketInfoStatus), item.TicketInfoStatus);
                }
                foreach (var item in mappingpl)
                {
                    item.TicketInfoStatusName = Enum.GetName(typeof(TicketInfoStatus), item.TicketInfoStatus);
                }
                ticketInfoInnerCollection.PayloadData = mappingpy;
                ticketInfoInnerCollection.PodData = mappingpl;
                rs.Content = ticketInfoInnerCollection;
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
        public IActionResult<Tuple<bool, TicketInfoStatus>> TicketIsAllComplete(Guid TicketUID)
        {


            TicketInfoStatus[] _unCompleteStatuss = new TicketInfoStatus[] {
                TicketInfoStatus.Draft,TicketInfoStatus.Assigned,TicketInfoStatus.Open,TicketInfoStatus.Processing };
            var rs = ActionResultTemplates.Result<Tuple<bool, TicketInfoStatus>>();
            try
            {
                TicketInfoStatus _status = TicketInfoStatus.Complete;
                var query = "SELECT Status FROM WMS_TicketInfo WHERE TicketUID=@TicketUID AND Status>0";
                var _statusRs = this._Handler.Instance.Query<TicketInfoStatus>(query, new { TicketUID = TicketUID });
                var _isallcomplete = _statusRs.All(p => !_unCompleteStatuss.Any(x => x == p));
                if (_statusRs.Any(p => p != TicketInfoStatus.Complete))
                    _status = TicketInfoStatus.Glitch;
                Tuple<bool, TicketInfoStatus> result = Tuple.Create<bool, TicketInfoStatus>(_isallcomplete, _status);
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
        public IActionResult<bool> RollbackTicketInfo(IEnumerable<Guid> TicketUID, TicketInfoStatus Status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = "UPDATE WMS_TicketInfo SET Status=@STatus,ActQty=0,ShtQty=0,SavQty=0 WHERE Status>0 AND TicketUID IN @TicketUID";
                rs.Content &= this._Handler.Instance.Execute(query, new { Status = (int)Status, TicketUID = TicketUID }) > 0;
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
        public IActionResult<bool> UpdateTicketInfoStatus(IEnumerable<Guid> TicketInfoUID, TicketInfoStatus Status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_TicketInfo SET Status=@Status WHERE Status>0 AND UID IN @TicketInfoUID";
                rs.Content = this._Handler.Instance.Execute(query,
                    new
                    {
                        Status = (int)Status,
                        TicketInfoUID = TicketInfoUID
                    }) > 0;
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
        public IActionResult<bool> UpdateTicketInfoStatusByTicket(IEnumerable<Guid> TicketUID, TicketInfoStatus Status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_TicketInfo SET Status=@STatus WHERE Status>0 AND TicketUID IN @TicketUID";
                rs.Content = this._Handler.Instance.Execute(query, new { Status = (int)Status, TicketUID = TicketUID }) > 0;
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
        public IActionResult<bool> CompleteTicketInfoByTicket(IEnumerable<Guid> TicketUID, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_TicketInfo SET ActQty=EstQty,Status=@STatus,ModifiedOn=@ModifiedOn,
                            ModifiedBy=@ModifiedBy,Description='process by pick all' WHERE Status>0 AND TicketUID IN @TicketUID";
                rs.Content = true;

                var index = 0;
                var grp = TicketUID.GroupBy(g => index++ / 500);

                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new
                    {
                        Status = (int)TicketInfoStatus.Complete,
                        ModifiedOn = DateTime.UtcNow,
                        ModifiedBy = modifiedBy,
                        TicketUID = items
                    }) > 0;
                }

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
        public IActionResult<bool> UpdateTickInfoProcessQty(ITicketInfoParameter parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(
                        new
                        {
                            ActQty = parameters.ActQty,
                            ShtQty = parameters.ShtQty,
                            SavQty = parameters.SavQty,
                            Status = (int)parameters.Status
                        }, new { UID = parameters.TicketInfoUID }); ;
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
        public IActionResult<IEnumerable<ITicketProcessModel>> GetTicketProcessModel(Guid[] TicketInfoUIDs)
        {
            var notcompleteStatus = new int[] {
                                  (int)TicketInfoStatus.Open,
                                  (int)TicketInfoStatus.Processing,
                                  (int)TicketInfoStatus.OffPosition
                                };
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketProcessModel>>();
            try
            {
                var collection = new List<TicketProcessInnerModel>();
                var query = @"
                SELECT DISTINCT
                [WT].ManifestType ,
                [WTI].*,
                [WWOPL].Qty OriginalQty,
                ISNULL([WP].Quantity,0) PayloadQty,
                [WP].Type AS PayloadType,
                [WPA].Type AS OriginalPayloadType,
                [WWOPL].ItemUID,
                [WWOPL].ItemGroupUID,
                [WWOPL].LoadingZoneSlotUID 'SourceLoadingZoneSlotUID',
                [WWOPL].SlotUID 'SourceSlotUID',
                [WWOPL].PackageUID 'SourcePackageUID',
                [WWOPL].PayloadPackageUID,
                [WWOPL].PayloadUID,
                [WWOP].PodUID,
                [WT].WarehouseUID,
                [WM].RefNo,
                [WM].PartyUID,
                [WWO].VesselUID,
                [WWOP].Type AS 'StorageType',
                '2' MappingType
                FROM  WMS_TicketInfo AS [WTI] 
				INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID
                INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].UID=[WTI].WorkOrderPayloadUID
                LEFT JOIN WMS_PayLoad AS [WP] ON [WP].UID =[WWOPL].PayloadUID and [WP].Status>0
                LEFT JOIN WMS_Payload AS WPA ON WP.OriginalPayloadUID=WPA.UID
				INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WWOPL].WorkOrderPodUID
                INNER JOIN WMS_WorkOrder AS [WWO] ON [WWO].UID=[WWOP].WorkOrderUID
                LEFT JOIN WMS_Manifest AS [WM] ON [WM].UID=[WWO].ManifestUID AND [WM].Status>0
                WHERE [WTI].UID in @TicketInfoUID AND [WTI].Status>0 AND [WT].Status>0 AND [WWOPL].Status>0 AND [WWOP].Status>0
                AND [WWO].Status>0    AND [WTI].Status IN @TickinfoStatus
			    UNION ALL
			    SELECT DISTINCT
                [WT].ManifestType ,
                [WTI].*,
                [WWOPL].Qty OriginalQty,
                ISNULL([WP].Quantity,0) PayloadQty,
                [WP].Type AS PayloadType,
                1 AS OriginalPayloadType,
                [WWOPL].ItemUID,
                [WWOPL].ItemGroupUID,
                [WWOPL].LoadingZoneSlotUID 'SourceLoadingZoneSlotUID',
                [WWOPL].SlotUID 'SourceSlotUID',
                [WWOPL].PackageUID 'SourcePackageUID',
                [WWOPL].PayloadPackageUID,
                [WWOPL].PayloadUID,
                [WWOP].PodUID,
                [WT].WarehouseUID,
                [WM].RefNo,
                [WM].PartyUID,
                [WWO].VesselUID,
                [WWOP].Type AS 'StorageType',
                '1' MappingType
                FROM  WMS_TicketInfo AS [WTI] 
				INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID
                INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WTI].WorkOrderPodUID
				INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                LEFT JOIN WMS_PayLoad AS [WP] ON [WP].UID =[WWOPL].PayloadUID and [WP].Status>0
                INNER JOIN WMS_WorkOrder AS [WWO] ON [WWO].UID=[WWOP].WorkOrderUID
                LEFT JOIN WMS_Manifest AS [WM] ON [WM].UID=[WWO].ManifestUID AND [WM].Status>0
                WHERE [WTI].UID in @TicketInfoUID AND [WTI].Status>0 AND [WT].Status>0 AND [WWOPL].Status>0 AND [WWOP].Status>0
                AND [WWO].Status>0   AND [WTI].Status IN @TickinfoStatus
                ;
                --Item
                SELECT [WLI].Status,[WLI].UID,[WLI].Type,[WLI].BelongToUID,[WLI].BelongToType,[WLI].Content from WMS_Ticket AS [WT]
                INNER JOIN WMS_TicketInfo AS [WTI] on [WT].UID=[WTI].TicketUID
                INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID
                INNER JOIN WMS_Label AS [WLI] ON [WLI].BelongToUID=[WWOPL].PayloadUID
                WHERE [WTI].UID in @TicketInfoUID AND [WTI].Status>0 AND [WLI].Status>0
                AND [WT].Status>0  AND [WWOPL].Status>0
                UNION ALL
                --Pallet
                SELECT [WL].Status,[WL].UID,[WL].Type,[WL].BelongToUID,[WL].BelongToType,[WL].Content from WMS_Ticket AS [WT]
                INNER JOIN WMS_TicketInfo AS [WTI] on [WT].UID=[WTI].TicketUID
                INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WTI].WorkOrderPodUID=[WWOP].UID
                INNER JOIN WMS_Label AS [WL] ON [WL].BelongToUID=[WWOP].PodUID
                WHERE [WTI].UID in @TicketInfoUID AND [WL].Status>0 AND [WL].Status>0 AND [WTI].Status IN @TickinfoStatus
                AND [WT].Status>0 
                ;
                SELECT 
                [WTI].UID TicketInfoUID,[WTI].Status TicketInfoStatus,
                [PWT].* FROM WMS_TicketInfo [WTI]
                INNER JOIN WMS_Ticket [WT] ON [WTI].TicketUID=[WT].UID AND [WT].Status>0
                INNER JOIN WMS_TIcket_Relation [WTR] ON [WTR].TicketUID=[WT].UID AND [WTR].Status>0
                INNER JOIN WMS_Ticket [PWT] ON [PWT].UID=[WTR].ParentUID AND [PWT].Status>0 
                WHERE [WTI].Status>0 AND [WTI].UID IN @TicketInfoUID
                ";
                var parentTicketInfoQuery = @" SELECT 
                [WTI].UID TicketInfoUID,
				[PWTI].*
				FROM WMS_TicketInfo [WTI]
				INNER JOIN WMS_WorkOrder_Payload AS [WWOP] ON [WWOP].WorkOrderPodUID=[WTI].WorkOrderPodUID AND [WWOP].Status>0
                INNER JOIN WMS_Ticket [WT] ON [WTI].TicketUID=[WT].UID AND [WT].Status>0
                INNER JOIN WMS_TIcket_Relation [WTR] ON [WTR].TicketUID=[WT].UID AND [WTR].Status>0
                INNER JOIN WMS_Ticket [PWT] ON [PWT].UID=[WTR].ParentUID AND [PWT].Status>0 
				LEFT JOIN WMS_TicketInfo [PWTI] ON [PWT].UID=[PWTI].TicketUID AND [WWOP].UID=[PWTI].WorkOrderPayloadUID  AND [PWTI].Status>0 
                WHERE [WTI].Status>0 AND [WTI].UID IN @TicketInfoUID ";
                var _results = this._Handler.Instance.QueryMultiple(query, new { TicketInfoUID = TicketInfoUIDs, TickinfoStatus = notcompleteStatus });
                collection = _results.Read<TicketProcessInnerModel>().ToList();
                var _labels = _results.Read<LabelInnerModel>();
                var _parentTickets = _results.Read<ParentTicketRelation>();
                var _belongTicketInfos = new List<TicketProcessParentTicketInfoModel>();
                if (collection.All(x => x.ManifestType == (int)ManifestType.Inbound))
                {
                    _belongTicketInfos.AddRange(this._Handler.Instance.Query<TicketProcessParentTicketInfoModel>(parentTicketInfoQuery
                        , new { TicketInfoUID = TicketInfoUIDs }));
                }
                foreach (var item in collection)
                {
                    var labels = _labels.Where(p => p.BelongToUID == item.PayloadUID || p.BelongToUID == item.PodUID);
                    item.Barcodes = labels.ToList();
                    var parents = _parentTickets.Where(x => x.TicketInfoUID == item.UID);
                    item.ParentTickets = parents;
                    if (_belongTicketInfos != null)
                    {
                        item.InboundPartentTicketInfos = _belongTicketInfos;
                    }
                }
                rs.Content = collection;
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
        public IActionResult<bool> UpdateTickInfo(ITicketInfoModel Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(Model, new
                {
                    UID = Model.UID
                });
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
        public IActionResult<bool> IsTicketComplete(Guid TicketUID, Guid[] notContainsTicketInfoUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var rs1 = this._Handler.RetrieveCollectionByDynamicConditions(new { TicketUID = TicketUID });
                var _tickInfos = rs1.Where(p => p.Status > 0); ;
                rs.Content = _tickInfos.All(p => (TicketInfoStatus)p.Status == TicketInfoStatus.Complete ||
                (TicketInfoStatus)p.Status == TicketInfoStatus.Glitch);
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
        private string getCondition(IGetTicketInfoParameters Parameters)
        {
            List<string> Condition = new List<string>();
            if (Parameters.TicketUIDs?.Count() > 0)
            {
                Condition.Add("([WT].UID in @TicketUIDs)");
            }
            if (Parameters.TicketIDs?.Count() > 0)
            {
                Condition.Add("([WT].ID in @TicketIDs)");
            }
            if (Parameters.TicketInfoUIDs?.Count() > 0)
            {
                Condition.Add("([WTI].UID in @TicketInfoUIDs)");
            }

            return Condition.Count > 0 ? string.Join("AND", Condition) + " AND " : "  ";
        }
        public IActionResult<IEnumerable<ITicketInfoModel>> GetDataByBol(Guid BOLID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                var query = @"SELECT [WTI].* FROM [WMS_BOL] AS[WB]
                INNER JOIN [WMS_Vessel] AS [WV] ON [WB].UID=[WV].BolUID
                INNER JOIN [WMS_WorkOrder] AS [WWO] ON [WWO].VesselUID=[WV].UID
                INNER JOIN [WMS_Ticket] AS [WT] ON [WT].WorkOrderUID=[WWO].UID
                INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WT].UID=[WTI].TicketUID
                WHERE [WT].Status>0 AND [WWO].Status>0 AND [WV].Status>0 AND [WB].Status>0 and [WTI].Status>0
                AND [WB].UID=@UID ";
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query, new { UID = BOLID });
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
        public IActionResult<IEnumerable<ITicketInfoModel>> GetPodBelongTicket(IEnumerable<string> enumerable)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                //結果沒錯，但重覆抓2次，先過濾重覆的資料，暫不調整
                //取得Barcode 所屬Ticket item ，未完成狀態的資料都要回傳(open,Processing)
                var query = @"
                SELECT distinct * FROM(
                SELECT [WTI].* 
                FROM WMS_Label AS [Label] 
                INNER JOIN WMS_WorkOrder_Pod [WWOP] ON [Label].BelongToUID=[WWOP].PodUID AND [WWOP].Status>0
                INNER JOIN WMS_WorkOrder_Payload [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOPL].Status>0
                INNER JOIN WMS_TicketInfo AS [WTI] ON [WTI].WorkOrderPodUID=[WWOP].UID AND [WTI].Status>0
                WHERE [Label].Content in @barcode AND [Label].Status>0 
                AND [WTI].Type IN @TicketType AND [WTI].Status>0
                UNION ALL
                SELECT [WTI].* FROM WMS_Label AS [Label] 
                INNER JOIN WMS_WorkOrder_Pod [WWOP] ON [Label].BelongToUID=[WWOP].PodUID AND [WWOP].Status>0
                INNER JOIN WMS_WorkOrder_Payload [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOPL].Status>0
                INNER JOIN WMS_TicketInfo AS [WTI] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID AND [WTI].Status>0
                WHERE [Label].Content in @barcode AND [Label].Status>0 AND [WTI].Type IN @TicketType AND [WTI].Status IN @TicketInfoStatus
                ) T";
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query,
                    new
                    {
                        barcode = enumerable.Select(p => p.ToVarchar()),
                        TicketInfoStatus = new int[] {
                        (int)TicketInfoStatus.Open,(int)TicketInfoStatus.Processing},
                        TicketType = new int[] {
                        (int)TicketType.Receiving, (int)TicketType.Outbound }
                    });
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
        public IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoByPickAll(IEnumerable<Guid> vesselUID = null, IEnumerable<Guid> workorderPayloadUID = null)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                var query = @"
                SELECT [WTI].* 
                FROM  WMS_WorkOrder AS [WWO]
				INNER JOIN WMS_WorkOrder_Pod [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                INNER JOIN WMS_WorkOrder_Payload [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                INNER JOIN WMS_TicketInfo AS [WTI] ON [WTI].WorkOrderPodUID=[WWOP].UID
                WHERE [WWO].Status>0 AND [WWOP].Status>0  AND [WTI].Status>0 {0}
                UNION ALL
                SELECT [WTI].* 
                FROM  WMS_WorkOrder AS [WWO]
				INNER JOIN WMS_WorkOrder_Pod [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                INNER JOIN WMS_WorkOrder_Payload [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                INNER JOIN WMS_TicketInfo AS [WTI] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID
                WHERE [WWO].Status>0 AND [WWOP].Status>0 AND [WTI].Status>0 {0}";
                var condition = "AND " + this.getTicketInfoByPickAllCondition(vesselUID, workorderPayloadUID);
                query = string.Format(query, condition);
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query,
                    new { VesselUID = vesselUID, WorkorderPayloadUID = workorderPayloadUID });
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IActionResult<IEnumerable<IPickallViewModel>> GetTicketInfoByPickAll(IPickAllParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IPickallViewModel>>();

            //目前只考慮outbuond (Workorderpayload 關聯的 TicketInfo
            var query = @"
                SELECT [WTI].Type TicketInfoType,[WTI].UID AS TicketInfoUID,[WTI].TicketUID,[WWOPL].UID AS WorkOrderPayloadUID
                ,[WWOPL].ItemGroupUID,[WWOP].PodUID,[WP].UID PayloadUID
                ,[WP].SlotUID,[WP].PackageUID,[WP].Quantity,[WP].ItemUID,[WP].Type AS PayloadType,
                [OWP].Type OriginalPayloadType
                FROM WMS_Manifest [WM] 
				INNER JOIN WMS_BOL [WB] ON [WB].ManifestUID=[WM].UID AND [WB].Status>0
				INNER JOIN WMS_Vessel [WV] ON [WV].BolUID=[WB].UID AND [WV].Status>0
				INNER JOIN WMS_WorkOrder AS [WWO] ON [WWO].VesselUID=[WV].UID AND [WWO].Status>0
				INNER JOIN WMS_WorkOrder_Pod [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                INNER JOIN WMS_WorkOrder_Payload [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                INNER JOIN WMS_TicketInfo AS [WTI] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID
				INNER JOIN WMS_PayLoad AS [WP] ON [WP].UID=[WWOPL].PayloadUID AND [WP].Status>0
                LEFT JOIN WMS_PayLoad AS [OWP] ON [WP].OriginalPayloadUID=[OWP].UID 
                WHERE [WWO].Status>0 AND [WWOP].Status>0 AND [WTI].Status>0   {0}
                ";
            var condition = "AND " + this.getTicketInfoByPickAllCondition(parameters);
            query = string.Format(query, condition);
            rs.Content = this._Handler.Instance.Query<PickallViewModel>(query, parameters);
            rs.Success = true;

            return rs;
        }
        private string getTicketInfoByPickAllCondition(IPickAllParameters parameters)
        {
            List<string> condition = new List<string>();
            if (parameters.BolUID != null)
            {
                condition.Add("([WB].UID IN @BolUID)");
            }
            else if (parameters.VesselUID != null)
            {
                condition.Add("([WV].UID IN @VesselUID)");
            }
            else if (parameters.WorkPayloadUID != null)
            {
                condition.Add("([WWOPL].UID IN @WorkPayloadUID)");
            }
            if (parameters.TicketInfoStatus != null)
            {
                condition.Add("([WTI].Status IN @TicketInfoStatus)");
            }
            return string.Join(" AND ", condition);
        }

        private string getTicketInfoByPickAllCondition(IEnumerable<Guid> vesselUID, IEnumerable<Guid> workorderPayloadUID)
        {
            List<string> condition = new List<string>();
            if (vesselUID != null)
            {
                condition.Add("[WWO].VesselUID IN @VesselUID");
            }
            else
            {
                condition.Add("[WWOPL].UID IN @WorkorderPayloadUID");
            }
            return string.Join("", condition);
        }

        public IActionResult<IEnumerable<IReceiviedReplicateModel>> GetReceiviedData(IGetReplicateDataParameter parameter)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IReceiviedReplicateModel>>();
            try
            {
                var query = @"SELECT distinct Manifest.PartyUID,Manifest.RefNo ExternalOrderNo,mapping.LocationID ,
                    mapping.WarehouseID,WTI.UID TicketInfoUID,WTI.Type,WWOPL.UID,
                    Label.Content Barcode,WWOPL.ItemGroupUID,WWOPL.ItemUID,WWOPL.Qty,WTI.ActQty,WWOPL.PackageUID,
                    OriginalSlot.Name OriginalSlotName,LandingZoneSlot.Name LandingZoneSlotName
                    FROM   WMS_TicketInfo AS WTI 
                    INNER JOIN WMS_Ticket AS WT ON WTI.TicketUID=WT.UID AND WT.Status>0
                    INNER JOIN WMS_WorkOrder_Payload AS WWOPL ON WWOPL.uid=WTI.WorkOrderPayloadUID AND WWOPL.Status>0
                    INNER JOIN WMS_WorkOrder_Pod AS WWOP ON WWOP.uid=WWOPL.WorkOrderPodUID AND WWOP.Status>0
                    INNER JOIN WMS_WorkOrder AS WorkOrder ON WorkOrder.UID=WWOPL.WorkOrderUID AND WorkOrder.Status>0
                    INNER JOIN WMS_Manifest AS Manifest ON WorkOrder.ManifestUID=Manifest.UID AND Manifest.Status>0
                    INNER JOIN WMS_Slot AS OriginalSlot ON OriginalSlot.UID=WWOPL.SlotUID AND OriginalSlot.Status>0
                    INNER JOIN WMS_Slot AS LandingZoneSlot ON LandingZoneSlot.UID=WWOPL.LoadingZoneSlotUID AND LandingZoneSlot.Status>0
                    INNER JOIN WMS_LocationMapping mapping ON mapping.WarehouseUID=WT.WarehouseUID
                    LEFT JOIN WMS_Label AS Label ON Label.BelongToUID=WWOP.PodUID AND Label.Status>0
                    WHERE
                    WWOP.Status>0  AND WTI.Status>0  AND
                    {0}";
                parameter.TicketInfoType = (int)TicketInfoType.Receiving;
                query = string.Format(query, this.getConditionByGetReplicate(parameter));
                rs.Content = this._Handler.Instance.Query<ReceiviedReplicateModel>(query, parameter);
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
        /// <summary>
        /// 取得Allocated 同步資料 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IActionResult<IEnumerable<IAllocatedReplicateModel>> GetAllocatedData(IGetReplicateDataParameter parameter)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IAllocatedReplicateModel>>();
            try
            {
                var query = @"SELECT distinct BOL.UID BOLUID ,WWOPL.UID WorkOrderPayloadUID,WWOPL.WorkOrderPodUID,Manifest.PartyUID,Manifest.RefNo ExternalOrderNo,mapping.LocationID 
                ,mapping.WarehouseID,WWOPL.UID,WWOPL.ItemUID,WTI.Type TicketInfoType,WTI.Status TicketInfoSTatus,
                WTI.UID,WTI.ActQty PickQuantity,WWOPL.Qty Quantity,WWOPL.PackageUID,OriginalSlot.Name OriginalSlotName,LandingZoneSlot.Name LandingZoneSlotName,
                CurrentSlot.Name CurrentSlotName,WP.Type [PayloadType],WPA.Type [OriginalPayloadType]
                FROM   WMS_TicketInfo AS WTI 
                INNER JOIN WMS_Ticket AS WT ON WTI.TicketUID=WT.UID AND WT.Status>0
                INNER JOIN WMS_WorkOrder_Payload AS WWOPL ON WTI.WorkOrderPayloadUID=WWOPL.UID AND WWOPL.Status>0
                INNER JOIN WMS_Payload AS WP ON WP.UID=WWOPL.PayloadUID
                LEFT JOIN WMS_Payload AS WPA ON WP.OriginalPayloadUID=WPA.UID
                INNER JOIN WMS_WorkOrder AS WorkOrder ON WorkOrder.UID=WWOPL.WorkOrderUID AND WorkOrder.Status>0
                INNER JOIN WMS_Vessel AS Vessel ON WorkOrder.VesselUID=Vessel.UID AND Vessel.Status>0
				INNER JOIN WMS_BOL AS BOL ON BOL.UID=Vessel.BolUID AND BOL.Status>0
                INNER JOIN WMS_Manifest AS Manifest ON WorkOrder.ManifestUID=Manifest.UID AND Manifest.Status>0
                LEFT JOIN WMS_Slot AS OriginalSlot ON OriginalSlot.UID=WWOPL.SlotUID AND OriginalSlot.Status>0
                LEFT JOIN WMS_Slot AS LandingZoneSlot ON LandingZoneSlot.UID=WWOPL.LoadingZoneSlotUID AND LandingZoneSlot.Status>0
                LEFT JOIN WMS_Slot AS CurrentSlot ON CurrentSlot.UID=WP.SlotUID AND CurrentSlot.Status>0
                INNER JOIN WMS_LocationMapping mapping ON mapping.WarehouseUID=WT.WarehouseUID
                WHERE WWOPL.Status>0 AND WTI.Status>0 AND {0} ";
                parameter.TicketInfoType = (int)TicketInfoType.Outbound;
                query = string.Format(query, this.getConditionByGetReplicate(parameter));
                rs.Content = this._Handler.Instance.Query<AllocatedReplicateModel>(query, parameter);
                if (rs.Content.Count() > 0)
                {
                    //original Onhand type = null 預設則回傳1(stock)
                    foreach (var item in rs.Content)
                    {
                        if (!item.OriginalPayloadType.HasValue)
                        {
                            item.OriginalPayloadType = (int)PayloadType.Stock;
                        }
                    }
                }
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

        private string getConditionByGetReplicate(IGetReplicateDataParameter parameter)
        {
            List<string> condition = new List<string>();
            if (parameter.BOLUID != null && parameter.BOLUID.Count() > 0)
            {
                condition.Add("(BOL.UID IN @BOLUID)");
            }
            if (parameter.ManifestUID != null && parameter.ManifestUID.Count() > 0)
            {
                condition.Add("(Manifest.UID IN @ManifestUID)");
            }
            if (parameter.TicketInfoUID != null && parameter.TicketInfoUID.Count() > 0)
            {
                condition.Add(string.Format("(WTI.UID IN ({0}))", string.Join(",", parameter.TicketInfoUID.Select(p => "'" + p + "'"))));
            }
            if (parameter.TicketUID != null && parameter.TicketUID.Count() > 0)
            {
                condition.Add("(WT.UID IN @TicketUID)");
            }
            if (parameter.WorkOrderPayloadUID != null && parameter.WorkOrderPayloadUID.Count() > 0)
            {
                condition.Add("(WWOPL.UID IN @WorkOrderPayloadUID)");
            }
            condition.Add("(WTI.Type=@TicketInfoType)");
            return string.Join("AND", condition);
        }

        public IActionResult<IEnumerable<IAllocatedReplicateModel>> GetAllocatedDataByItemGroup(Guid? itemGroupUID, Guid TicketUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IAllocatedReplicateModel>>();
            try
            {
                var query = @"SELECT BOL.UID BOLUID ,WWOPL.UID WorkOrderPayloadUID,WWOPL.WorkOrderPodUID,Manifest.PartyUID,Manifest.RefNo ExternalOrderNo,mapping.LocationID 
                ,mapping.WarehouseID,WWOPL.UID,WWOPL.ItemUID,WTI.Type TicketInfoType,WTI.Status TicketInfoSTatus,
                WTI.UID,WTI.ActQty PickQuantity,WWOPL.Qty Quantity,WWOPL.PackageUID,OriginalSlot.Name OriginalSlotName,LandingZoneSlot.Name LandingZoneSlotName
                FROM WMS_WorkOrder_Payload AS WWOPL
				INNER JOIN WMS_TicketInfo AS WTI  ON WTI.WorkOrderPayloadUID=WWOPL.UID
                INNER JOIN WMS_Ticket AS WT ON WTI.TicketUID=WT.UID AND WT.Status>0
                INNER JOIN WMS_WorkOrder AS WorkOrder ON WorkOrder.UID=WWOPL.WorkOrderUID AND WorkOrder.Status>0
                INNER JOIN WMS_Manifest AS Manifest ON WorkOrder.ManifestUID=Manifest.UID AND Manifest.Status>0
				INNER JOIN WMS_BOL AS BOL ON BOL.ManifestUID=Manifest.UID AND BOL.Status>0
                LEFT JOIN WMS_Slot AS OriginalSlot ON OriginalSlot.UID=WWOPL.SlotUID AND OriginalSlot.Status>0
                LEFT JOIN WMS_Slot AS LandingZoneSlot ON LandingZoneSlot.UID=WWOPL.LoadingZoneSlotUID AND LandingZoneSlot.Status>0
                INNER JOIN WMS_LocationMapping mapping ON mapping.WarehouseUID=WT.WarehouseUID
                WHERE WWOPL.Status>0 AND WTI.Status>0 AND WWOPL.ItemGroupUID=@itemGroupUID
                AND WTI.Type=@TicketInfoType AND WTI.TicketUID=@TicketUID";
                rs.Content = this._Handler.Instance.Query<AllocatedReplicateModel>(query, new
                {
                    itemGroupUID = itemGroupUID,
                    TicketInfoType = (int)TicketInfoType.Move,
                    TicketUID = TicketUID
                });
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

        public IActionResult<IEnumerable<IAllocatedReplicateModel>> GetAllocatedDataByItemGroupInbound(Guid? itemGroupUID, Guid TicketUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IAllocatedReplicateModel>>();
            try
            {
                var query = @"SELECT 
                BOL.UID BOLUID ,WWOPL.UID WorkOrderPayloadUID,WWOPL.WorkOrderPodUID,Manifest.PartyUID,Manifest.RefNo ExternalOrderNo,mapping.LocationID 
                ,mapping.WarehouseID,WWOPL.UID,WWOPL.ItemUID,WTI.Type TicketInfoType,WTI.Status TicketInfoSTatus,
                WTI.UID,WTI.ActQty PickQuantity,WWOPL.Qty Quantity,WWOPL.PackageUID,OriginalSlot.Name OriginalSlotName,LandingZoneSlot.Name LandingZoneSlotName
                FROM WMS_Ticket AS WT 
				INNER JOIN WMS_TicketInfo AS WTI  ON WTI.TicketUID=WT.UID
			    INNER JOIN WMS_WorkOrder_Pod AS WWOP ON WWOP.UID=WTI.WorkOrderPodUID and WWOP.Status>0
				INNER JOIN WMS_WorkOrder_Payload AS WWOPL ON WWOPL.WorkOrderPodUID=WWOP.UID AND WWOPL.Status>0
                LEFT JOIN WMS_WorkOrder AS WorkOrder ON WorkOrder.UID=WWOPL.WorkOrderUID AND WorkOrder.Status>0
                LEFT JOIN WMS_Manifest AS Manifest ON WorkOrder.ManifestUID=Manifest.UID AND Manifest.Status>0
				LEFT JOIN WMS_BOL AS BOL ON BOL.ManifestUID=Manifest.UID AND BOL.Status>0
                LEFT JOIN WMS_Slot AS OriginalSlot ON OriginalSlot.UID=WWOPL.SlotUID AND OriginalSlot.Status>0
                LEFT JOIN WMS_Slot AS LandingZoneSlot ON LandingZoneSlot.UID=WWOPL.LoadingZoneSlotUID AND LandingZoneSlot.Status>0
                LEFT JOIN WMS_LocationMapping mapping ON mapping.WarehouseUID=WT.WarehouseUID
                WHERE WWOPL.Status>0 AND WTI.Status>0 
				AND WWOPL.ItemGroupUID=@ItemGroupUID
                AND WTI.Type=300
				AND WTI.TicketUID=@TicketUID";
                rs.Content = this._Handler.Instance.Query<AllocatedReplicateModel>(query, new
                {
                    itemGroupUID = itemGroupUID,
                    TicketInfoType = (int)TicketInfoType.Move,
                    TicketUID = TicketUID
                });
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

        public IActionResult<IEnumerable<ITicketInfoModel>> GetBelongToMoveTicketInfo(IEnumerable<Guid> ticketInfoUIDs)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                var query = @"SELECT  
                WTI2.*
                FROM  WMS_TicketInfo AS WTI  
				INNER JOIN WMS_WorkOrder_Payload AS WWOPL ON WWOPL.UID=WTI.WorkOrderPayloadUID AND WWOPL.Status>0
			    INNER JOIN WMS_TicketInfo AS WTI2 ON WTI2.WorkOrderPodUID=WWOPL.WorkOrderPodUID and WTI2.Status>0
                WHERE WWOPL.Status>0 AND WTI.Status>0 
                AND WTI2.Type=@TicketInfoType
				AND WTI.UID IN @UID";
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query, new
                {
                    TicketInfoType = (int)TicketInfoType.Move,
                    UID = ticketInfoUIDs
                });
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
