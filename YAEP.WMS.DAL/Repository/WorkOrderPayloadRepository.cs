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
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class WorkOrderPayloadRepository<T> : AbstractRepository<T>, IWorkOrderPayloadRepository
         where T : class, IWorkOrderPayloadModel
    {
        public WorkOrderPayloadRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> AddPayload(IEnumerable<IWorkOrderPayloadModel> Model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var dt = this.ToDataTable<IWorkOrderPayloadModel>(Model);
            rs.Content = true;

            //var query = @"INSERT INTO [WMS_WorkOrder_Payload] 
            //   ([UID],
            //    [ID],
            //    [Name],
            //    [Type],
            //    [WorkOrderUID],
            //    [WorkOrderPodUID],
            //    [PayloadUID],
            //    [ItemUID],
            //    [ItemGroupUID],
            //    [SlotUID],
            //    [PackageUID],
            //    [LoadingZoneSlotUID],
            //    [VesselManifestUID],
            //    [PayloadPackageUID],
            //    [TargetSlotUID],
            //    [SeparateByUID],
            //    [Qty],
            //    [Status],
            //    [Volume],
            //    [Weight],
            //    [CreatedBy],
            //    [CreatedOn],
            //    [ModifiedBy],
            //    [ModifiedOn])
            // VALUES(@UID,
            // @ID,
            // @Name,
            // @wType,
            // @WorkOrderUID,
            // @WorkOrderPodUID,
            // @PayloadUID,
            // @ItemUID,
            // @ItemGroupUID,
            // @SlotUID,
            // @PackageUID,
            // @LoadingZoneSlotUID,
            // @VesselManifestUID,
            // @PayloadPackageUID,
            // @TargetSlotUID,
            // @SeparateByUID,
            // @Qty,
            // @Status,
            // @Volume,
            // @Weight,
            // @CreatedBy,
            // @CreatedOn,
            // @ModifiedBy,
            // @ModifiedOn)";
            //SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            //cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
            //cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
            //cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
            //cmd.Parameters.Add("@wType", SqlDbType.Int, 4, "Type");
            //cmd.Parameters.Add("@WorkOrderUID", SqlDbType.UniqueIdentifier, 16, "WorkOrderUID");
            //cmd.Parameters.Add("@WorkOrderPodUID", SqlDbType.UniqueIdentifier, 16, "WorkOrderPodUID");
            //cmd.Parameters.Add("@PayloadUID", SqlDbType.UniqueIdentifier, 16, "PayloadUID");
            //cmd.Parameters.Add("@ItemUID", SqlDbType.UniqueIdentifier, 16, "ItemUID");
            //cmd.Parameters.Add("@ItemGroupUID", SqlDbType.UniqueIdentifier, 16, "ItemGroupUID");
            ////cmd.Parameters.Add("@ShtGroupUID", SqlDbType.UniqueIdentifier, 16, "ShtGroupUID");
            //cmd.Parameters.Add("@SlotUID", SqlDbType.UniqueIdentifier, 16, "SlotUID");
            //cmd.Parameters.Add("@PackageUID", SqlDbType.UniqueIdentifier, 16, "PackageUID");
            //cmd.Parameters.Add("@LoadingZoneSlotUID", SqlDbType.UniqueIdentifier, 16, "LoadingZoneSlotUID");
            //cmd.Parameters.Add("@VesselManifestUID", SqlDbType.UniqueIdentifier, 16, "VesselManifestUID");
            //cmd.Parameters.Add("@PayloadPackageUID", SqlDbType.UniqueIdentifier, 16, "PayloadPackageUID");
            //cmd.Parameters.Add("@TargetSlotUID", SqlDbType.UniqueIdentifier, 16, "TargetSlotUID");
            //cmd.Parameters.Add("@SeparateByUID", SqlDbType.UniqueIdentifier, 16, "SeparateByUID");
            //cmd.Parameters.Add("@Qty", SqlDbType.Int, 4, "Qty");
            //cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
            //cmd.Parameters.Add("@Volume", SqlDbType.Decimal, 17, "Volume");
            //cmd.Parameters.Add("@Weight", SqlDbType.Decimal, 17, "Weight");
            //cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
            //cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
            //cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
            //cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");
            //rs = this.BatchInsertTable(dt, cmd);
            using (var sqlBulkCopy = new SqlBulkCopy(this._Handler.Instance.Connection as SqlConnection,
              SqlBulkCopyOptions.Default, (SqlTransaction)this._Handler.Instance.Transaction))
            {

                sqlBulkCopy.BulkCopyTimeout = 120;
                sqlBulkCopy.BatchSize = 10000;
                foreach (DataColumn column in dt.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                sqlBulkCopy.DestinationTableName = "WMS_WorkOrder_Payload";
                sqlBulkCopy.WriteToServer(dt);
            }
            rs.Success = rs.Content = true;
            return rs;
        }
        public IActionResult<bool> AddPayload(IEnumerable<dynamic> Model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var dt = this.ToDataTable<dynamic>(Model);
            rs.Content = true;
            try
            {
                var query = @"
                INSERT INTO [dbo].[WMS_WorkOrder_Payload]
           ([UID]
           ,[ID]
           ,[Name]
           ,[Type]
           ,[WorkOrderUID]
           ,[WorkOrderPodUID]
           ,[PayloadUID]
           ,[ItemUID]
            ,[ItemGroupUID]
           ,[SlotUID]
           ,[PackageUID]
           ,[LoadingZoneSlotUID]
           ,[VesselManifestUID]
           ,[PayloadPackageUID]
           ,[TargetSlotUID]
           ,[Qty]
           ,[Status]
           ,[Volume]
           ,[Weight]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
     VALUES
           (
		   @UID,
           @ID,
           @Name,
           @Type,
           @WorkOrderUID, 
           @WorkOrderPodUID, 
           @PayloadUID,
           @ItemUID, 
           @ItemGroupUID,
           @SlotUID, 
           @PackageUID, 
           @LoadingZoneSlotUID, 
           @VesselManifestUID,
           @PayloadPackageUID,
           @TargetSlotUID, 
           @Qty,
           @Status, 
           @Volume, 
           @Weight, 
           @CreatedBy,
           @CreatedOn,
           @ModifiedBy, 
           @ModifiedOn)
                ";
                var index = 0;
                var grp = Model.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, items) > 0;
                }
                //rs.Content = this._Handler.Instance.Execute(query, Model) > 0;
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
        public IActionResult<bool> AddPayload(dynamic Model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(Model);
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

        public IActionResult<bool> AssignedPayloadtoPod(Guid workOrderPodUID, IEnumerable<Guid> workOrderPayloadUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(
                    new { WorkOrderPodUID = workOrderPodUID }, new { UID = workOrderPayloadUID });
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

        public IActionResult<bool> ChangePayload(Guid wplUID, Guid payloadUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(new { payloadUID = payloadUID }, new { UID = wplUID });
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
        public IActionResult<bool> ChangeStatusByWorkOrder(Guid workorderUID, WorkOrderPayloadStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_WorkOrder_Payload SET Status=@Status WHERE workorderUID=@workorderUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, new { Status = (int)status, workorderUID = workorderUID }) > 0;
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
        public IActionResult<bool> BatchChangeStatus(IEnumerable<Guid> workorderPayloadUID, WorkOrderPayloadStatus status, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var index = 0;
                var query = "UPDATE WMS_WorkOrder_Payload SET Status=@Status,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn WHERE UID IN @workorderPayloadUID AND Status>0";
                var grp = workorderPayloadUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new
                    {
                        Status = (int)status,
                        modifiedBy = modifiedBy,
                        ModifiedOn = DateTime.UtcNow,
                        workorderPayloadUID = items
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
        public IActionResult<bool> ChangeStatus(Guid workorderPayloadUID, WorkOrderPayloadStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_WorkOrder_Payload SET Status=@Status WHERE UID=@workorderPayloadUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, new { Status = (int)status, workorderPayloadUID = workorderPayloadUID }) > 0;
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
        [Obsolete("不在使用此方式deallocated")]
        public IActionResult<bool> DeallcatedByWorkOrderPayload(Guid[] workorderpayloadUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var index = 0;
                var grp = workorderpayloadUID.GroupBy(g => index++ / 1000);

                var query = @"UPDATE 
                WP 
                set [WP].Type=@PayloadType,[WP].Status=@Status
                FROM  WMS_WorkOrder_Payload [wwpl] 
                INNER JOIN WMS_Payload as [WP] on [WP].UID=[wwpl].PayloadUID
                WHERE  [wwpl].UID in @guids";
                foreach (var item in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query,
                 new
                 {
                     guids = item,
                     PayloadType = (int)PayloadType.Stock,
                     Status = (int)PayloadStatus.Active
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
        public IActionResult<bool> DeallcatedByWorkOrderPayload(IEnumerable<IDeallocatedParameters> deallocatedParameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //Concurrency violation: the UpdateCommand affected 0 of the expected 1 records.
                //var dt = this.ToDataTable<IDeallocatedParameters>(deallocatedParameters);
                //rs.Content = true;
                //var query = @"Update [WMS_Payload] SET 

                //   [Type]=@Type,
                //   [Status]=@Status,
                //   [ModifiedBy]=@ModifiedBy,
                //   [ModifiedOn]=@ModifiedOn
                //    WHERE [UID]=@UID ";
                //dt.Columns.Add("UID", typeof(Guid));
                //dt.Columns.Add("Status", typeof(int));
                //dt.Columns.Add("ModifiedBy", typeof(string));
                //dt.Columns.Add("ModifiedOn", typeof(DateTime));
                //foreach (DataRow dr in dt.Rows)
                //{
                //    dr["Status"] = (int)PayloadStatus.Active;
                //    dr["ModifiedBy"] = this._Handler.AuthenticationInfo.Account;
                //    dr["ModifiedOn"] = DateTime.UtcNow;
                //    dr["UID"] = dr["PayloadUID"];
                //}
                //SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
                //cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "PayloadUID");
                //cmd.Parameters.Add("@Type", SqlDbType.Int, 4, "RecoveryPayloadType");
                //cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
                //cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
                //cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");
                //dt.PrimaryKey = dt.Columns.Cast<DataColumn>()
                //   .Where(p => p.ColumnName.Equals("UID", StringComparison.CurrentCultureIgnoreCase)).ToArray();
                //this.BatchUpdateTable(dt, cmd);
                StringBuilder sql = new StringBuilder();
                foreach (var item in deallocatedParameters)
                {
                    sql.Append(String.Format(@"Update [WMS_Payload] SET [Type]={0},[Status]={1},[ModifiedBy]='{2}', [ModifiedOn]='{3}' WHERE [UID]='{4}' ;",
                    item.RecoveryPayloadType,
                    (int)PayloadStatus.Active,
                    this._Handler.AuthenticationInfo.Account,
                    DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                    item.PayloadUID.ToString()));
                }
                this._Handler.Instance.Execute(sql.ToString());
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
        public IActionResult<bool> DeletePayloadByUID(IEnumerable<Guid> guids)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var index = 0;
                rs.Content = true;
                var query = "UPDATE WMS_Workorder_Payload SET Status=0 WHERE UID in @UID";
                var grp = guids.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new { UID = items }) > 0;
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
        public IActionResult<bool> DeletePayload(object parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(parameters);
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

        public IActionResult<bool> EditPayload(dynamic condition, dynamic Model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(Model, condition);
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
        public IActionResult<bool> BulkPickChangeFromSlot(IEnumerable<Guid> workorderPayloadUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_WorkOrder_Payload SET SlotUID=LoadingZoneSlotUID WHERE UID IN @workorderPayloadUID";
                rs.Content = this._Handler.Instance.Execute(query, new { workorderPayloadUID = workorderPayloadUID }) > 0;
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
        public IActionResult<bool> BulkPickChangebackFromSlot(IEnumerable<Guid> workorderPayloadUID, Guid originalFromSlotUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_WorkOrder_Payload SET SlotUID=@originalFromSlotUID WHERE UID IN @workorderPayloadUID";
                rs.Content = this._Handler.Instance.Execute(query, new
                {
                    workorderPayloadUID = workorderPayloadUID,
                    originalFromSlotUID = originalFromSlotUID
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
        public IActionResult<IEnumerable<ICalVesselAssignedItemInnerModel>> GetAssignWorkOrderItemList(Guid VesselUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ICalVesselAssignedItemInnerModel>>();
            try
            {
                var query = @"SELECT [WWO].[VesselUID],[WWOP].[ItemUID],[WWOP].PackageUID,[WWOP].VesselManifestUID,
                            SUM([WWOP].Qty) PackageQty FROM [WMS_WorkOrder] as [WWO]
                            INNER JOIN [WMS_WorkOrder_Payload] as [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                            WHERE [WWO].VesselUID=@VesselUID  AND [WWO].Status>0 and [WWOP].Status>0
                             GROUP BY  [WWO].[VesselUID],[WWOP].[ItemUID],[WWOP].PackageUID,[WWOP].VesselManifestUID";
                rs.Content = this._Handler.Instance
                    .Query<CalVesselAssignedItemInnerModel>(query, new { VesselUID = VesselUID });
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
        public IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetListByUID(IEnumerable<Guid> workorderUIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPayloadModel>>();
            try
            {
                var collection = new List<WorkOrderPayloadInnerModel>();
                var query = @"SELECT * FROM WMS_WorkOrder_Payload WHERE Status>0 AND UID in @UID";
                //rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition)
                //    .Where(p => p.Status > (int)WorkOrderPayloadStatus.Inactive);
                var index = 0;
                var grp = workorderUIDs.GroupBy(gx => gx).Select(p => p.Key).GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    collection.AddRange(this._Handler.Instance.Query<WorkOrderPayloadInnerModel>(query, new
                    {
                        UID = items
                    }));
                }
                //rs.Content = this._Handler.Instance.Execute(query, Model) > 0;
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
        public IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPayloadModel>>();
            try
            {

                //var query = @"SELECT * FROM WMS_WorkOrderPayload WHERE Status>0 AND UID in @UID";
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition)
                    .Where(p => p.Status > (int)WorkOrderPayloadStatus.Inactive);
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
        public IActionResult<IEnumerable<IWorkOrderPayloadInfoModel>> GetWorkOrderPayloadInfo(IEnumerable<Guid> warehouseUID, IEnumerable<Guid> itemUID, int[] payloadType, int[] slotType)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPayloadInfoModel>>();
            try
            {
                var query = @"SELECT 
                                [WMS_WorkOrder_Payload].* ,
                                [WMS_BOL].[ETA] AS [BOL_ETA_D]
                              FROM  [WMS_WorkOrder_Payload]
                                LEFT JOIN [WMS_WorkOrder] on [WMS_WorkOrder].UID = [WMS_WorkOrder_Payload].[WorkOrderUID] AND [WMS_WorkOrder].[Status] > 0
                                LEFT JOIN [WMS_Vessel] on [WMS_Vessel].UID = [WMS_WorkOrder].[VesselUID] AND [WMS_WorkOrder].[Status] > 0
                                LEFT JOIN [WMS_BOL] on [WMS_BOL].UID = [WMS_Vessel].[BolUID] AND [WMS_Vessel].[Status] > 0
                                INNER JOIN [WMS_Slot] [WS] ON [WMS_WorkOrder_Payload].[SlotUID] = [WS].[UID] AND [WS].[Status] > 0
                                INNER JOIN [WMS_Area] [WA] ON [WA].[UID] = [WS].[AreaUID] AND [WA].[Status] > 0
                              WHERE [WA].WarehouseUID IN @WarehouseUID 
                                AND [WMS_WorkOrder_Payload].itemUID IN @itemNo 
                                AND [WMS_WorkOrder_Payload].Status > 0 AND [WMS_WorkOrder_Payload].Type IN @Type
                                AND [WS].Type IN @slotType";
                rs.Content = this._Handler.Instance.Query<OnWorkOrderPayloadInfoInnerModel>(query,
                    new
                    {
                        warehouseUID = warehouseUID,
                        itemNo = itemUID,
                        type = payloadType,
                        slotType = slotType
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
        public IActionResult<IEnumerable<IRollbackModel>> GetRollbackWorkPayload(IEnumerable<Guid> ticketUIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IRollbackModel>>();
            try
            {

                var query = @"
                SELECT [WTI].UID TicketInfoUID,[WWOPL].* from WMS_TicketInfo AS [WTI]
                INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID
                WHERE [WTI].TicketUID IN @TicketUID AND [WTI].Status>0  AND [WWOPL].Status>0
                UNION ALL
                SELECT [WTI].UID TicketInfoUID,[WWOPL].* from WMS_TicketInfo AS [WTI]
                INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWOP].UID=[WTI].WorkOrderPodUID
                INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOPL].UID
                WHERE [WTI].TicketUID IN @TicketUID  AND [WTI].Status>0 AND [WWOPL].Status>0 AND [WWOP].Status>0
                ";
                rs.Content = this._Handler.Instance.Query<RollbackInnerModel>(query,
                    new { TicketUID = ticketUIDs });
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

        public IActionResult<IEnumerable<IWorkOrderPayloadViewModel>> GetWorkOrderPayload(Guid VesselUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPayloadViewModel>>();
            try
            {
                var query = @"SELECT [WWOPL].*,[WWOP].Name 'PodName',
                            [WA].Name 'AreaName',[WB].Name 'BinName',[WS].Name 'SlotName'
                            FROM  WMS_WorkOrder AS [WWO]
							INNER JOIN  WMS_WorkOrder_Payload AS [WWOPL] ON [WWO].UID=[WWOPL].WorkOrderUID AND [WWOPL].Status>0
                            LEFT JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOP].Status>0
                            LEFT JOIN WMS_Label AS [WL] ON [WL].BelongToUID=[WWOP].PodUID AND [WL].Status>0
                            INNER JOIN WMS_Slot AS [WS] ON [WS].UID=[WWOPL].SlotUID AND [WS].Status>0
                            INNER JOIN WMS_Bin AS [WB] ON [WB].UID=[WS].BinUID AND [WB].Status>0
                            INNER JOIN WMS_Area AS [WA] ON [WA].UID=[WB].AreaUID AND [WA].Status>0
                            WHERE [WWO].VesselUID=@VesselUID  AND [WWO].Status>0 ";
                rs.Content = this._Handler.Instance
                    .Query<WorkOrderPayloadViewInnerModel>(query, new { VesselUID = VesselUID });
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

        public IActionResult<IEnumerable<IWorkOrderPayloadModel>> GetWorkOrderPayloadByOriginalPayload
            (IEnumerable<Guid?> payloadUID, Guid workorderPodUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPayloadModel>>();
            try
            {

                var query = @"SELECT [WWPL].* FROM [WMS_Payload] AS [WP]
                INNER JOIN [WMS_WorkOrder_Payload] AS [WWPL] ON [WP].UID=[WWPL].PayloadUID
                WHERE [WP].OriginalPayloadUID=@PayloadUID AND [WWPL].Status>0 AND [WWPL].WorkOrderPodUID=@WorkOrderPodUID";
                rs.Content = this._Handler.Instance.Query<WorkOrderPayloadInnerModel>(query,
                    new { PayloadUID = payloadUID, WorkOrderPodUID = workorderPodUID });
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

        public IActionResult<IEnumerable<IWorkOrderPayloadWithTicketInfoModel>>
            GetWorkOrderPayloadByTicketInfo(IEnumerable<Guid> ticketInfoUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPayloadWithTicketInfoModel>>();
            try
            {
                var query = @"
                SELECT [WWOP].*,[WTI].UID TicketInfoUID FROM WMS_TicketInfo AS [WTI]
                INNER JOIN WMS_WorkOrder_Payload AS [WWOP] ON [WTI].WorkOrderPayloadUID=[WWOP].UID
                 WHERE [WTI].UID IN @ticketInfoUID
                ";
                rs.Content = this._Handler.Instance.Query<WorkOrderPayloadWithTicketInfoModel>(query,
                    new
                    {
                        ticketInfoUID = ticketInfoUID
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
        /// <summary>
        ///(目前不支援多筆查詢)
        /// </summary>
        /// <param name="checkModel"></param>
        /// <returns></returns>
        public IActionResult<IEnumerable<IPodBarcodeInfo>> GetPodBarcodeInfo(ICheckPodBarcodeInfoParameters checkModel)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPodBarcodeInfo>>();
            try
            {
                List<PodBarcodeInfo> collection = new List<PodBarcodeInfo>();
                var query = @"SELECT distinct [Label].BelongToUID,[Label].Status,
                [Label].Type,[Label].BelongToType,[Label].Content AS Barcode,WWOPL.PackageUID,WWOPL.Qty,[WP].ItemUID ,[POD].IsPack,[WP].SlotUID
                FROM WMS_PayLoad AS [WP] 
                INNER JOIN WMS_Slot AS [Slot] ON [Slot].UID=[WP].SlotUID AND [Slot].Status>0
                INNER JOIN WMS_Pod AS [POD] ON [WP].PODUID=[POD].UID AND [POD].Status>0
                INNER JOIN WMS_Label AS [Label] ON [POD].UID=[Label].BelongToUID AND [Label].Status>0
                INNER JOIN WMS_WorkOrder_Pod WWOP ON WWOP.PodUID = [POD].UID AND WWOP.Status > 0
                INNER JOIN WMS_WorkOrder_Payload WWOPL ON WWOPL.PayloadUID=[WP].UID AND WWOPL.Status > 0
                WHERE 
                  {0}
                              ";
                var execQuery = string.Format(query, this.getPodBarcodeInfoCondition(checkModel));
                var index = 0;
                var grp = checkModel.BelongToUID.GroupBy(x => x).Select(y => y.Key).GroupBy(g => index++ / 1000);
                foreach (var item in grp)
                {
                    checkModel.BelongToUID = item;
                    collection.AddRange(this._Handler.Instance.Query<PodBarcodeInfo>(execQuery, checkModel));
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
        public IActionResult<IEnumerable<IPodBarcodeInfo>> GetReceivingQtyBarcodeInfo(ICheckPodBarcodeInfoParameters checkModel)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPodBarcodeInfo>>();
            try
            {
                var query = @"SELECT distinct WM.Type,[Label].BelongToUID,[Label].Status,WWOPL.uid,
                [Label].Type,[Label].BelongToType,[Label].Content AS Barcode,WWOPL.PackageUID,WWOPL.Qty,WWOPL.ItemUID ,WWOPL.SlotUID
                FROM  WMS_WorkOrder WWO
				INNER JOIN WMS_Manifest WM ON WWO.manifestUID=WM.UID
				INNER JOIN WMS_WorkOrder_Pod WWOP  ON WWO.UID=WWOP.WorkOrderUID
                INNER JOIN WMS_WorkOrder_Payload WWOPL ON WWOPL.WorkOrderPodUID=WWOP.UID AND WWOPL.Status > 0
				INNER JOIN WMS_Slot AS [Slot] ON [Slot].UID=WWOPL.SlotUID AND [Slot].Status>0
				INNER JOIN WMS_Label AS [Label] ON WWOP.PodUID=[Label].BelongToUID AND [Label].Status>0
                WHERE WM.Type=1 AND
                  {0}
                              ";
                rs.Content = this._Handler.Instance.Query<PodBarcodeInfo>(string.Format(query, this.getReceivingQtyBarcodeInfoCondition(checkModel)), checkModel);
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
        private string getReceivingQtyBarcodeInfoCondition(ICheckPodBarcodeInfoParameters checkModel)
        {
            List<string> condition = new List<string>();
            if (checkModel.ItemUID != null && checkModel.ItemUID.Count() > 0)
            {
                condition.Add("([WWOPL].ItemUID IN @ItemUID)");
            }
            if (checkModel.SlotUID != null && checkModel.SlotUID.Count() > 0)
            {
                condition.Add("([WWOPL].SlotUID IN @SlotUID )");
            }
            if (checkModel.WarehouseUID != null && checkModel.WarehouseUID.Count() > 0)
            {
                condition.Add("([Slot].WarehouseUID IN @WarehouseUID )");
            }
            if (checkModel.BelongToUID != null && checkModel.BelongToUID.Count() > 0)
            {
                condition.Add("([Label].BelongToUID IN @BelongToUID )");
            }
            if (checkModel.LabelType > 0)
            {
                condition.Add("([Label].Type =@LabelType )");
            }
            return condition.Count > 0 ? "  " + string.Join("AND ", condition) : string.Empty;
        }
        private string getPodBarcodeInfoCondition(ICheckPodBarcodeInfoParameters checkModel)
        {
            List<string> condition = new List<string>();
            if (checkModel.ItemUID != null && checkModel.ItemUID.Count() > 0)
            {
                condition.Add("([WP].ItemUID IN @ItemUID)");
            }
            if (checkModel.SlotUID != null && checkModel.SlotUID.Count() > 0)
            {
                condition.Add("([WP].SlotUID IN @SlotUID )");
            }
            if (checkModel.WarehouseUID != null && checkModel.WarehouseUID.Count() > 0)
            {
                condition.Add("([Slot].WarehouseUID IN @WarehouseUID )");
            }
            if (checkModel.BelongToUID != null && checkModel.BelongToUID.Count() > 0)
            {
                condition.Add("([Label].BelongToUID IN @BelongToUID )");
            }
            if (checkModel.LabelType > 0)
            {
                condition.Add("([Label].Type =@LabelType )");
            }
            return condition.Count > 0 ? "  " + string.Join("AND ", condition) : string.Empty;
        }
    }
}
