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
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class LabelRepository<T> : AbstractRepository<T>, ILabelRepository where T : class, ILabelModel
    {
        public LabelRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> AddLabelCollection(ILabelModel[] Models)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var dt = this.ToDataTable<ILabelModel>(Models);
                var query = @"INSERT INTO [WMS_Label] 
                 ([UID],
                  [ID],
                  [Name],
                  [Type],
                  [BelongToType],
                  [BelongToUID],
                  [FileUID],
                  [Content],
                  [Status],
                  [Description],
                  [CreatedBy],
                  [CreatedOn],
                  [ModifiedBy],
                  [ModifiedOn])
                 VALUES(@UID,
                  @ID,
                  @Name,
                  @Type,
                  @BelongToType,
                  @BelongToUID,
                  @FileUID,
                  @Content,
                  @Status,
                  @Description,
                  @CreatedBy,
                  @CreatedOn,
                  @ModifiedBy,
                  @ModifiedOn)";

                SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
                cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
                cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
                cmd.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");
                cmd.Parameters.Add("@BelongToType", SqlDbType.Int, 4, "BelongToType");
                cmd.Parameters.Add("@BelongToUID", SqlDbType.UniqueIdentifier, 16, "BelongToUID");
                cmd.Parameters.Add("@FileUID", SqlDbType.UniqueIdentifier, 16, "FileUID");
                cmd.Parameters.Add("@Content", SqlDbType.VarChar, 50, "Content");
                cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000, "Description");
                cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
                cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
                cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
                cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");


                rs = this.BatchInsertTable(dt, cmd);
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
        public IActionResult<IEnumerable<ILabelModel>> BatchReturnCloneLabel(IEnumerable<ICloneLabelModel> cloneLabelModels)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ILabelModel>>();
           
                List<ILabelModel> labelModels = new List<ILabelModel>();
                var c = cloneLabelModels.Select(p => new CloneLabelInnerModel
                {
                    SourceBelongToUID = p.SourceBelongToUID,
                    TargetBelongToUID = p.TargetBelongToUID,
                    CreatedBy = p.CreatedBy
                }).ToList();
                var query = "sp_ClonelabelWithTvp";
                var parameters = new Dapper.DynamicParameters();
                parameters.AddTable("@DataList", "TVP_Clone_Label_Parameter", c);
                var rs1 = this._Handler.Instance.Execute(query, parameters, commandType: CommandType.StoredProcedure) > 0;
                query = "SELECT * FROM WMS_Label WHERE belongtouid in @belongtouid and status >0";
                var index = 0;
                var grp = c.Select(p => p.TargetBelongToUID).GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    labelModels.AddRange(this._Handler.Instance.Query<LabelInnerModel>(query, new
                    {
                        belongtouid = items
                    }));
                }
                rs.Content = labelModels;
                rs.Success = true;
            
            return rs;
        }
        public IActionResult<bool> ChangeLabelBelongToUID(Guid[] BarcodeUID, Guid belongToUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(new { belongToUID = belongToUID }
                , new { UID = BarcodeUID });
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

        public IActionResult<bool> ChangeLabelStatus(Guid[] BarcodeUID, LabelStatus status)
        {
            //可能Payload 沒有附加label ，故不強制要成功
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = @"UPDATE WMS_Label SET Status=@Status WHERE UID IN @BarcodeUID AND Status>0";
                var index = 0;
                var grp = BarcodeUID.GroupBy(g => index++ / 1000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new { status = status, BarcodeUID = items }) > 0;
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
        public IActionResult<bool> ChangeLabelStatus(Guid[] BarcodeUID, LabelStatus status, string description)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_Label SET Status=@Status,description=@description WHERE UID IN @BarcodeUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, new
                {
                    status = status,
                    BarcodeUID = BarcodeUID,
                    description = description
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
        /// <summary>
        /// 修改Label 狀態 使用前需注意該方法無視delete 狀態一律修改
        /// </summary>
        /// <param name="BelongToUID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public IActionResult<bool> ChangeLabelStatusByBelongToUID(IEnumerable<Guid> BelongToUID, LabelStatus status, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = @"UPDATE WMS_Label SET Status=@Status,ModifiedBy=@modifiedBy WHERE BelongToUID IN @BarcodeUID ";
                var index = 0;
                var grp = BelongToUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new
                    {
                        status = status,
                        modifiedBy = modifiedBy,
                        BarcodeUID = items
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
        public IActionResult<bool> ChangeLabelStatus(Guid belongToUID, string[] Barcode, LabelStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_Label SET Status=@Status WHERE Content IN @Barcode AND Status>0 AND BelongToUID=@belongToUID";
                rs.Content = this._Handler.Instance.Execute(query, new { status = status, Barcode = Barcode, belongToUID = belongToUID }) > 0;
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
        public IActionResult<bool> ChangeLabelStatus(string[] Barcode, LabelStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_Label SET Status=@Status WHERE Content IN @Barcode AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, new { status = status, Barcode = Barcode }) > 0;
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
        public IActionResult<bool> DeleteLabel(Guid[] uid)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(new { BelongToUID = uid });
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
        public IActionResult<IEnumerable<ILabelModel>> GetLabelsByTicket(IEnumerable<Guid> TicketUIDs)
        {
            //@TicketUID
            var rs = ActionResultTemplates.Result<IEnumerable<ILabelModel>>();
            try
            {
                var query = @"
                SELECT distinct
                [Podlabel].*
                FROM  WMS_Ticket AS [WT] 
				INNER JOIN WMS_TicketInfo AS [WTI] ON [WT].UID=[WTI].TicketUID
                INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WTI].WorkOrderPodUID=[WWOP].UID 
                INNER JOIN WMS_Label AS [Podlabel] ON [Podlabel].BelongToUID=[WWOP].PodUID AND [Podlabel].BelongToType=300 AND [Podlabel].Status>0
                WHERE 
                [WT].UID IN @TicketUID AND
                [WT] .Status>0 AND [WWOP].Status>0 AND [Podlabel].Status>0
                UNION ALL
                SELECT  distinct
                [Payloadlabel].*
                FROM  WMS_Ticket AS [WT] 
				INNER JOIN WMS_TicketInfo AS [WTI] ON [WT].UID=[WTI].TicketUID
                INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID
                INNER JOIN WMS_Label AS [Payloadlabel] ON [Payloadlabel].BelongToUID=[WWOPL].PayloadUID 
                										AND [Payloadlabel].BelongToType=400 AND [Payloadlabel].Status>0
                WHERE 
                [WT].UID IN @TicketUID AND
                [WT].Status>0  AND [WWOPL].Status>0
                ";
                var collection = this._Handler.Instance.Query<LabelInnerModel>(query, new { TicketUID = TicketUIDs });
                rs.Content = collection.Where(p => p.Status > (int)LabelStatus.Inactive);
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
        public IActionResult<IEnumerable<ILabelModel>> GetLabels(Guid[] belongtoUIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ILabelModel>>();
            try
            {
                var query = string.Format("SELECT * FROM WMS_Label WHERE Status>0 AND BelongToUID in ({0})", string.Join(",",
                    belongtoUIDs.Select(p => "'" + p + "'")));
                var collection = this._Handler.Instance.Query<LabelInnerModel>(query, new { belongtoUIDs = belongtoUIDs });
                rs.Content = collection.Where(p => p.Status > (int)LabelStatus.Inactive);
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
        public IActionResult<IEnumerable<ILabelModel>> GetLabels(object conditions)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ILabelModel>>();
            try
            {
                var collection = this._Handler.RetrieveCollectionByDynamicConditions(conditions);
                rs.Content = collection.Where(p => p.Status > (int)LabelStatus.Inactive);
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
        public IActionResult<bool> ExistByBarcode(string barcodeContent, int labelType)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "SELECT * FROM WMS_Label WHERE Content=@barcodeContent AND Type=@labelType AND Status>0";
                rs.Content = this._Handler.Instance.Query(query, new
                {
                    barcodeContent = barcodeContent,
                    labelType
                = labelType
                }).Count() > 0;
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


    }
}
