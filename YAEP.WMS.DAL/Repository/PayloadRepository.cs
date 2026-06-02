using Dapper;
using DapperParameters;
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
    public class PayloadRepository<T> : AbstractRepository<T>, IPayloadRepository where T : class, IPayloadModel
    {
        public PayloadRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> AddPayload(IPayloadModel Model)
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
        public IActionResult<bool> UpatePayload(IPayloadModel Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(Model, new { UID = Model.UID });
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
        public IActionResult<bool> BatchUpatePayload(IEnumerable<IPayloadModel> Model)
        {
            var dt = this.ToDataTable<IPayloadModel>(Model);
            var rs = ActionResultTemplates.Result<bool>();
            #region TVP
            //var c = Model.Select(p => p as PayloadInnerModel).ToList();
            //var query = "sp_UpdatePayloadWithTvp";
            //var parameters = new DynamicParameters();
            //parameters.AddTable("@DataList", "TVP_Update_Payload_Parameter", c);
            //this._Handler.Instance.Execute(query, parameters, commandType: CommandType.StoredProcedure);
            //rs.Success = rs.Content = true;
            #endregion
            var query = @"UPDATE [dbo].[WMS_PayLoad]
                  SET 
                [ID] =@ID
                ,[Name] = @Name
                ,[Type] = @Type
                ,[PODUID] =@PODUID
                ,[SlotUID] = @SlotUID
                ,[VesselUID] = @VesselUID
                ,[Quantity] = @Quantity
                ,[OriginalPayloadUID] = @OriginalPayloadUID
                ,[PackageUID] =@PackageUID
                ,[VolumeLimit] =@VolumeLimit
                ,[WeightLimit] =@WeightLimit
                ,[Status] = @Status
                ,[ModifiedBy] =@ModifiedBy
                ,[ModifiedOn] =@ModifiedOn
                       WHERE UID=@UID";
            SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
            cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
            cmd.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");
            cmd.Parameters.Add("@PODUID", SqlDbType.UniqueIdentifier, 16, "PODUID");
            cmd.Parameters.Add("@SlotUID", SqlDbType.UniqueIdentifier, 16, "SlotUID");
            cmd.Parameters.Add("@VesselUID", SqlDbType.UniqueIdentifier, 16, "VesselUID");
            cmd.Parameters.Add("@ItemUID", SqlDbType.UniqueIdentifier, 16, "ItemUID");
            cmd.Parameters.Add("@Quantity", SqlDbType.Int, 4, "Quantity");
            cmd.Parameters.Add("@OriginalPayloadUID", SqlDbType.UniqueIdentifier, 16, "OriginalPayloadUID");
            cmd.Parameters.Add("@PackageUID", SqlDbType.UniqueIdentifier, 16, "PackageUID");
            cmd.Parameters.Add("@VolumeLimit", SqlDbType.Decimal, 17, "VolumeLimit");
            cmd.Parameters.Add("@WeightLimit", SqlDbType.Decimal, 17, "WeightLimit");
            cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000, "Description");
            cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
            cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
            cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
            cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");

            dt.PrimaryKey = dt.Columns.Cast<DataColumn>()
                .Where(p => p.ColumnName.Equals("UID", StringComparison.CurrentCultureIgnoreCase)).ToArray();
            rs = this.BatchUpdateTable(dt, cmd);

            return rs;
        }
        public IActionResult<bool> BatchAddPayload(IEnumerable<IPayloadModel> Model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            rs.Content = true;

            var dt = this.ToDataTable<IPayloadModel>(Model);

            #region Dapper
            //      var query = @"
            //      INSERT INTO [dbo].[WMS_PayLoad]
            //      ([UID]
            //      ,[ID]
            //      ,[Name]
            //      ,[Type]
            //      ,[PODUID]
            //      ,[SlotUID]
            //      ,[VesselUID]
            //      ,[ItemUID]
            //      ,[Quantity]
            //      ,[OriginalPayloadUID]
            //      ,[PackageUID]
            //      ,[VolumeLimit]
            //      ,[WeightLimit]
            //      ,[Status]
            //      ,[Description]
            //      ,[CreatedBy]
            //      ,[CreatedOn]
            //      ,[ModifiedBy]
            //      ,[ModifiedOn])
            //       VALUES
            //      (@UID				
            //      ,@ID					
            //      ,@Name				
            //      ,@Type				
            //      ,@PODUID				
            //      ,@SlotUID			
            //      ,@VesselUID			
            //      ,@ItemUID			
            //      ,@Quantity			
            //      ,@OriginalPayloadUID	
            //      ,@PackageUID			
            //      ,@VolumeLimit		
            //      ,@WeightLimit		
            //      ,@Status				
            //      ,@Description		
            //      ,@CreatedBy			
            //      ,@CreatedOn			
            //      ,@ModifiedBy			
            //      ,@ModifiedOn			
            //)";
            //      var index = 0;
            //      var grp = Model.GroupBy(g => index++ / 2000);
            //      foreach (var items in grp)
            //      {
            //          rs.Content &= this._Handler.Instance.Execute(query, items) > 0;
            //      }
            #endregion
            #region SQLdbAdapter /SqlBulkCopy
            //var query =
            // @"
            //      INSERT INTO [dbo].[WMS_PayLoad]
            //      ([UID]
            //      ,[ID]
            //      ,[Name]
            //      ,[Type]
            //      ,[PODUID]
            //      ,[SlotUID]
            //      ,[VesselUID]
            //      ,[ItemUID]
            //      ,[Quantity]
            //      ,[OriginalPayloadUID]
            //      ,[PackageUID]
            //      ,[VolumeLimit]
            //      ,[WeightLimit]
            //      ,[Status]
            //      ,[Description]
            //      ,[CreatedBy]
            //      ,[CreatedOn]
            //      ,[ModifiedBy]
            //      ,[ModifiedOn])
            //       VALUES
            //      (@UID				
            //      ,@ID					
            //      ,@Name				
            //      ,@Type				
            //      ,@PODUID				
            //      ,@SlotUID			
            //      ,@VesselUID			
            //      ,@ItemUID			
            //      ,@Quantity			
            //      ,@OriginalPayloadUID	
            //      ,@PackageUID			
            //      ,@VolumeLimit		
            //      ,@WeightLimit		
            //      ,@Status				
            //      ,@Description		
            //      ,@CreatedBy			
            //      ,@CreatedOn			
            //      ,@ModifiedBy			
            //      ,@ModifiedOn	)		
            //";
            //SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            //cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 14, "UID");
            //cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
            //cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
            //cmd.Parameters.Add("@Type", SqlDbType.Int, 8, "Type");
            //cmd.Parameters.Add("@PODUID", SqlDbType.UniqueIdentifier, 14, "PODUID");
            //cmd.Parameters.Add("@SlotUID", SqlDbType.UniqueIdentifier, 14, "SlotUID");
            //cmd.Parameters.Add("@VesselUID", SqlDbType.UniqueIdentifier, 14, "VesselUID");
            //cmd.Parameters.Add("@ItemUID", SqlDbType.UniqueIdentifier, 14, "ItemUID");
            //cmd.Parameters.Add("@Quantity", SqlDbType.Int, 8, "Quantity");
            //cmd.Parameters.Add("@OriginalPayloadUID", SqlDbType.UniqueIdentifier, 14, "OriginalPayloadUID");
            //cmd.Parameters.Add("@PackageUID", SqlDbType.UniqueIdentifier, 14, "PackageUID");
            //cmd.Parameters.Add("@VolumeLimit", SqlDbType.Decimal, 18, "VolumeLimit");
            //cmd.Parameters.Add("@WeightLimit", SqlDbType.Decimal, 18, "WeightLimit");
            //cmd.Parameters.Add("@Status", SqlDbType.Int, 8, "Status");
            //cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 50, "Description");
            //cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 50, "CreatedBy");
            //cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 50, "CreatedOn");
            //cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar, 50, "ModifiedBy");
            //cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 50, "ModifiedOn");
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

                sqlBulkCopy.DestinationTableName = "WMS_PayLoad";
                sqlBulkCopy.WriteToServer(dt);
            }
            rs.Success = rs.Content = true;
            #endregion
            #region TVP
            //var query = "sp_InsertPayloadWithTvp";
            //var parameters = new Dapper.DynamicParameters();
            //parameters.AddTable("@DataList", "TVP_Update_Payload_Parameter", Model.ToList());
            //this._Handler.Instance.Execute(query, parameters, commandType: CommandType.StoredProcedure);
            //rs.Success = rs.Content = true;
            #endregion


            return rs;
        }
        public IActionResult<bool> ChangeSlotUID(Guid PayloadUID, Guid SlotUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_Payload SET SlotUID=@SlotUID WHERE UID=@UID";
                rs.Content = this._Handler.Instance.Execute(query, new { SlotUID = SlotUID, UID = PayloadUID }) > 0;
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
        public IActionResult<IPayloadModel> GetPayload(Guid PayloadUID)
        {

            var rs = ActionResultTemplates.Result<IPayloadModel>();
            try
            {
                rs.Content = this._Handler.Retrieve(PayloadUID);
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

        public IActionResult<IEnumerable<IPayloadModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(x => x.Status > 0);
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
        public IActionResult<IEnumerable<IPayloadWithOriginalPayloadTypeModel>> GetListWithOriginalPayloadType(Guid itemuid, Guid slotuid)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadWithOriginalPayloadTypeModel>>();
            try
            {
                var sql = @"SELECT WP.*,owp.type originalpayloadtype FROM WMS_PAYLOAD wp 
                            LEFT JOIN WMS_PAYLOAD owp on wp.originalPayloaduid=owp.uid
                            WHERE WP.ITEMUID=@itemuid AND wp.slotuid=@slotuid AND wp.status >0";
                rs.Content = this._Handler.Instance.Query<PayloadWithOriginalPayloadTypeModel>(sql, new
                {
                    itemuid = itemuid,
                    slotuid = slotuid
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
        public IActionResult<IEnumerable<IPayloadModel>> FindList(IEnumerable<Guid> PayloadUID)
        {
            List<IPayloadModel> payloadModels = new List<IPayloadModel>();
            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadModel>>();
            var index = 0;
            var grp = PayloadUID.GroupBy(g => index++ / 2000);
            var query = "SELECT * FROM WMS_Payload WHERE Status >@Status AND UID IN @UID ";

            foreach (var items in grp)
            {
                payloadModels.AddRange(this._Handler.Instance.Query<PayloadInnerModel>(query,
                    new { UID = items, Status = (int)PayloadStatus.Inactive }));
            }
            rs.Success = true;
            rs.Content = payloadModels;

            return rs;
        }
        public IActionResult<IEnumerable<IPayloadModel>> FindList(IEnumerable<Guid> PayloadUID, PayloadType type)
        {
            List<IPayloadModel> payloadModels = new List<IPayloadModel>();
            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadModel>>();
            var index = 0;
            var grp = PayloadUID.GroupBy(g => index++ / 2000);
            var query = "SELECT * FROM WMS_Payload WHERE Status >@Status AND UID IN @UID AND type=@type";

            foreach (var items in grp)
            {
                payloadModels.AddRange(this._Handler.Instance.Query<PayloadInnerModel>(query,
                    new { UID = items, type = type, Status = (int)PayloadStatus.Inactive }));
            }
            rs.Success = true;
            rs.Content = payloadModels;

            return rs;
        }

        public IActionResult<int> GetPayloadByPackageQty(Guid packageUID)
        {
            var rs = ActionResultTemplates.Result<int>();
            try
            {
                var query = @"SELECT SUM(ISNULL(Quantity,0)) from WMS_Payload
                              WHERE PackageUID=@packageuid and Status>0";
                var rs2 = this._Handler.Instance.QueryFirst<int?>(query, new { packageuid = packageUID });
                rs.Content = rs2.HasValue ? rs2.Value : 0;
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

        public IActionResult<IEnumerable<IAllocatedModel>> GetAllocatedData(Guid[] warehouseUID, Guid[] itemUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IAllocatedModel>>();
            try
            {
                var query =
@"	
SELECT WMS_Slot.WarehouseUID, WMS_Payload.ItemUID, WMS_Payload.PackageUID, SUM(WMS_Payload.Quantity) AS Quantity,
OWP.Type OriginalPayloadType
FROM WMS_Payload
		    INNER JOIN WMS_Slot ON WMS_Slot.UID = WMS_Payload.SlotUID  
            INNER JOIN WMS_Payload OWP ON OWP.UID=WMS_Payload.OriginalPayloadUID
WHERE (WMS_Payload.Type = 2) AND (WMS_Payload.Status > 0) 
		    AND (WMS_Slot.WarehouseUID IN @WarehouseUID)
		    AND (WMS_Payload.ItemUID IN @ItemUID)
GROUP BY WMS_Slot.WarehouseUID, WMS_Payload.ItemUID, WMS_Payload.PackageUID,OWP.Type 
";
                rs.Content = this._Handler.Instance.Query<AllocatedModel>(query, new { WarehouseUID = warehouseUID, ItemUID = itemUID });
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

        public IActionResult<bool> ChangePayloadStauts(Guid poduid, PayloadStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(new { Status = (int)status }, new { UID = poduid });
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
        public IActionResult<bool> ChangePayloadStauts(IEnumerable<Guid> payloaduid, PayloadStatus status, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_Payload SET Status=@Status,ModifiedBy=@modifiedBy WHERE UID in @UID";
                rs.Content = true;
                var index = 0;
                var grp = payloaduid.GroupBy(g => index++ / 500);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query,
                        new { Status = (int)status, modifiedBy = modifiedBy, UID = items }) > 0;
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
        public IActionResult<bool> ChangePayloadType(IEnumerable<Guid> payloaduid, int type)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(new { Type = type }, new { UID = payloaduid });
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
        public IActionResult<bool> ChangePayloadType(Guid payloaduid, int type)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(new { Type = type }, new { UID = payloaduid });
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
        public IActionResult<IPayloadModel> GetRecoveryPayload(object condition)
        {

            var rs = ActionResultTemplates.Result<IPayloadModel>();
            try
            {
                rs.Content = this._Handler.RetrieveByDynamicConditions(condition);
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
        public IActionResult<bool> DeletePayloadFromDb(object condition)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteFromDatabaseByDynamicConditions(condition) > 0;
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

        public IActionResult<IDeallocatedPayloadInfoModel> FindDeallocatedRelatedPayloadCollection(IEnumerable<Guid> allocatedPayloadUID)
        {
            var rs = ActionResultTemplates.Result<IDeallocatedPayloadInfoModel>();
            try
            {
                var index = 0;
                //因payload 被分配完後會把paylaod 刪除，也要讀取被刪除的資料
                DeallocatedPayloadInfoModel deallocatedPayloadInfoModel = new DeallocatedPayloadInfoModel();
                var query = @"SELECT * FROM WMS_Payload WHERE UID in @allocatedPayloadUID 
                              ;
                              SELECT * FROM WMS_Payload WHERE UID in 
                                (SELECT OriginalPayloadUID FROM WMS_Payload WHERE UID in @allocatedPayloadUID ) ";
                var grp = allocatedPayloadUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    var result = this._Handler.Instance.QueryMultiple(query, new { allocatedPayloadUID = items });
                    deallocatedPayloadInfoModel.AllocatedPayload.AddRange(result.Read<PayloadInnerModel>());
                    deallocatedPayloadInfoModel.OriginalPayload.AddRange(result.Read<PayloadInnerModel>());
                }



                rs.Content = deallocatedPayloadInfoModel;
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

        public IActionResult<IEnumerable<IPayloadModel>> GetListByTicket(Guid ticketUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadModel>>();
            try
            {
                var query = @"SELECT [WP].* FROM WMS_Ticket AS [WT] 
                INNER JOIN WMS_TicketInfo AS [WTI] ON [WT].UID=[WTI].TicketUID AND [WTI].Status>0
                INNER JOIN WMS_WorkOrder_Payload AS [WWOP] ON [WWOP].UID=[WTI].WorkOrderPayloadUID AND [WWOP].Status>0
                INNER JOIN WMS_PayLoad AS [WP] ON [WP].UID=[WWOP].PayloadUID AND [WP].Status>0
                WHERE [WT].UID=@ticketUID";
                rs.Content = this._Handler.Instance.Query<PayloadInnerModel>(query, new { ticketUID = ticketUID });
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

        public IActionResult<IEnumerable<IPayloadModel>> GetOnhandPayload(Guid warehouseUID,
            IEnumerable<Guid> itemNo, int[] slotStatus)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadModel>>();
            try
            {
                var query = @"SELECT [WP].* FROM WMS_Payload AS [WP]
                              INNER JOIN WMS_Slot [WS] ON [WP].SlotUID=[WS].UID AND [WS].Status>0
                              INNER JOIN WMS_Area [WA] ON [WA].UID=[WS].AreaUID AND [WA].Status>0
                              WHERE [WA].WarehouseUID=@WarehouseUID AND [WP].itemUID IN @itemNo 
                                    AND [WP].Status>0 AND [WP].Type=@Type AND [WS].Status IN @slotStatus";
                rs.Content = this._Handler.Instance.Query<PayloadInnerModel>(query,
                    new
                    {
                        warehouseUID = warehouseUID,
                        itemNo = itemNo,
                        type = (int)PayloadType.Stock,
                        SlotStatus = slotStatus
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

        public IActionResult<IEnumerable<IPayloadModel>> GetOnhandPayload(IEnumerable<Guid> warehouseUID, IEnumerable<Guid> itemUID, int[] payloadType, int[] slotType)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadModel>>();
            try
            {
                var query = @"SELECT 
                                [WP].*
                              FROM  [WMS_PayLoad] AS [WP]
                                INNER JOIN [WMS_Slot] [WS] ON [WP].[SlotUID] = [WS].[UID] AND [WS].[Status] > 0
                                INNER JOIN [WMS_Area] [WA] ON [WA].[UID] = [WS].[AreaUID] AND [WA].[Status] > 0
                              WHERE [WA].WarehouseUID IN @WarehouseUID 
                                AND [WP].itemUID IN @itemNo 
                                AND [WP].Status > 0 AND [WP].Type IN @payloadType
                                AND [WS].Type IN @slotType";
                rs.Content = this._Handler.Instance.Query<PayloadInnerModel>(query,
                    new
                    {
                        warehouseUID = warehouseUID,
                        itemNo = itemUID,
                        payloadType = payloadType,
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

        public IActionResult<bool> ReplenishmentPayload(IPayloadModel payloadModel)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"Update WMS_Payload set Quantity=Quantity+@Quantity,
                        VolumeLimit=@VolumeLimit,WeightLimit=@WeightLimit,Status=@Status,
                        ModifiedOn=getdate(),ModifiedBy=@ModifiedBy  
                        WHERE UID=@UID";
                rs.Content = this._Handler.Instance.Execute(query, payloadModel) > 0;
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




        //public IActionResult<IEnumerable<IPayloadModel>> FindCollection(Guid[] PayloadUID)
        //{
        //    var rs = ActionResultTemplates.Result<IEnumerable<IPayloadModel>>();
        //    try
        //    {
        //        var query = @"SELECT * FROM WMS_Payload WHERE UID in @UID AND Status>0";
        //        rs.Content = this._Handler.Instance.Query<PayloadInnerModel>(query, new { UID = PayloadUID });
        //        rs.Success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        rs.Message = ex.Message;
        //        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
        //        rs.Success = false;
        //        rs.InnerException = ex;
        //    }
        //    return rs;
        //}


    }
}
