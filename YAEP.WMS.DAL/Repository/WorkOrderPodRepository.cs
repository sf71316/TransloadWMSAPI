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
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class WorkOrderPodRepository<T> : AbstractRepository<T>, IWorkOrderPodRepository
         where T : class, IWorkOrderPodModel
    {
        public WorkOrderPodRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<IWorkOrderPodModel> GetWorkOrderPod(object condition)
        {

            var rs = ActionResultTemplates.Result<IWorkOrderPodModel>();
            try
            {
                var result = this._Handler.RetrieveByDynamicConditions(condition);
                if (result.Status > 0)
                {
                    rs.Content = result;
                    rs.Success = true;
                }
                else
                {
                    rs.Message = "not find data.";
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
        public IActionResult<bool> AddWorkOrderPod(IEnumerable<dynamic> entitys)
        {
            var rs = ActionResultTemplates.Result<bool>();
            rs.Content = true;
            try
            {
                var query = @"INSERT INTO [dbo].[WMS_WorkOrder_Pod]
                ([UID]
                ,[ID]
                ,[Name]
                ,[Type]
                ,[WorkOrderUID]
                ,[ContainerType]
                ,[OperationSuggestion]
                ,[PodUID]
                ,[BarcodeUID]
                ,[StartDate]
                ,[EndDate]
                ,[Status]
                ,[Volume]
                ,[Weight]
                ,[CreatedBy]
                ,[CreatedOn]
                ,[ModifiedBy]
                ,[ModifiedOn])
                 VALUES
                (@UID
                , @ID
                , @Name
                , @Type
                , @WorkOrderUID
                , @ContainerType
                , @OperationSuggestion
                , @PodUID
                , @BarcodeUID
                , @StartDate
                , @EndDate
                , @Status
                , @Volume
                , @Weight
                , @CreatedBy
                , @CreatedOn
                , @ModifiedBy
                , @ModifiedOn
                )";
                var index = 0;
                var grp = entitys.GroupBy(g => index++ / 2000);
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
        public IActionResult<bool> AddWorkOrderPod(dynamic Model)
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
        public IActionResult<bool> DeleteWorkOrderPod(object parameters)
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
        public IActionResult<bool> DeleteWorkOrderPod(IEnumerable<Guid> parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_Workorder_pod SET Status=0 WHERE UID IN @UID";
                var index = 0;
                var grp = parameters.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new
                    {
                        UID = items
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
        public IActionResult<bool> EditWorkOrderPod(dynamic conditon, dynamic Model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(Model, conditon);
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
        public IActionResult<bool> WorkOrderPodIsExist(Guid WorkOrderPodUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.Exists("UID", WorkOrderPodUID);
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
        public IActionResult<bool> ChangeStatus(Guid workorderPodUID, WorkOrderPodStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_WorkOrder_Pod SET Status=@Status WHERE UID=@workorderPodUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, new { Status = (int)status, workorderPodUID = workorderPodUID }) > 0;
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
        public IActionResult<bool> BatchChangeStatus(IEnumerable<Guid> workorderPodUID, WorkOrderPodStatus status, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = "UPDATE WMS_WorkOrder_Pod SET Status=@Status,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn  WHERE UID IN @workorderPodUID AND Status>0";
                var index = 0;
                var grp = workorderPodUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new
                    {
                        Status = (int)status,
                        modifiedBy = modifiedBy,
                        ModifiedOn = DateTime.UtcNow,
                        workorderPodUID = items
                    }) > 0;
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
        public IActionResult<bool> ChangeStatusByWorkOrder(Guid workorderUID, WorkOrderPodStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_WorkOrder_Pod SET Status=@Status WHERE WorkOrderUID=@WorkOrderUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, new { Status = (int)status, WorkOrderUID = workorderUID }) > 0;
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
        public IActionResult<IEnumerable<IWorkOrderPodViewModel>> GetWorkOrderPod(Guid VesselUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPodViewModel>>();
            try
            {
                string query = @"SELECT T.*,[WS].Name 'SlotName',[WA].Name 'AreaName',[WB].Name 'BinName',[WS2].Name 'LoadingZoneName' FROM(
                                SELECT [WWOP].*,
                                (SELECT TOP 1 WWOPL.LoadingZoneSlotUID 
                                FROM WMS_WorkOrder_Payload AS [WWOPL] WHERE [WWOPL].WorkOrderPodUID=[WWOP].UID ) AS 'LoadingZoneSlotUID',
                                (SELECT TOP 1 WWOPL.SlotUID 
                                FROM WMS_WorkOrder_Payload AS [WWOPL] WHERE [WWOPL].WorkOrderPodUID=[WWOP].UID ) AS 'SlotUID',
                                [WL].Content 'Barcode'
                                FROM WMS_WorkOrder AS [WWO]
                                INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                                LEFT JOIN WMS_Label AS [WL] ON [WL].BelongToUID=[WWOP].PodUID AND [WL].Status>0
                                WHERE [WWO].VesselUID=@VesselUID 
                                ) T 
                                LEFT JOIN WMS_Slot AS [WS] ON [WS].UID=T.SlotUID
                                LEFT JOIN WMS_Bin AS [WB] ON [WB].UID=[WS].BinUID
                                LEFT JOIN WMS_Area AS [WA] ON [WA].UID=[WB].AreaUID
                                LEFT JOIN WMS_Slot AS [WS2] ON [WS2].UID=T.LoadingZoneSlotUID
                                LEFT JOIN WMS_Bin AS [WB2] ON [WB2].UID=[WS2].BinUID
                                LEFT JOIN WMS_Area AS [WA2] ON [WA2].UID=[WB2].AreaUID
                                WHERE T.STatus>0";

                rs.Content = this._Handler.Instance.Query<WorkOrderViewInnerModel>(query, new { VesselUID = VesselUID });
                foreach (var item in rs.Content)
                {
                    item.TypeName = ((StorageType)item.Type).ToString();
                    item.StatusName = ((WorkOrderPodStatus)item.Status).ToString();
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
        public IActionResult<bool> MergePod(IWorkOrderMergePalletParameter parameter)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_WorkOrder_Payload SET WorkOrderPodUID=@MergeTo WHERE WorkOrderPodUID in @Mergefrom ";
                rs.Content = this._Handler.Instance.Execute(query, new { MergeTo = parameter.Mergeto, Mergefrom = parameter.Mergefrom }) > 0;
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
        public IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(Guid vesselUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPodModel>>();
            try
            {
                var query = @"SELECT [WWOP].* FROM WMS_WorkOrder AS [WWO]
                    INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                    WHERE [WWOP].Status>0 AND [WWO].VesselUID=@vesselUID ";
                rs.Content = this._Handler.Instance.Query<WorkOrderPodInnerModel>(query, new { vesselUID = vesselUID });
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
        public IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodListByVessel(IEnumerable<Guid> VesselUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPodModel>>();
            try
            {
                var query = @"SELECT [WWOP].* FROM WMS_WorkOrder AS [WWO]
                    INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                    WHERE [WWOP].Status>0 AND [WWO].VesselUID in  @VesselUID ";
                rs.Content = this._Handler.Instance.Query<WorkOrderPodInnerModel>(query, new { VesselUID = VesselUID });
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
        public IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(IEnumerable<Guid> workorderUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPodModel>>();
            try
            {
                var query = @"SELECT [WWOP].* FROM WMS_WorkOrder AS [WWO]
                    INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                    WHERE [WWOP].Status>0 AND [WWO].UID in  @UID ";
                rs.Content = this._Handler.Instance.Query<WorkOrderPodInnerModel>(query, new { UID = workorderUID });
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
        public IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderPodModel>>();
            try
            {
                var result = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > 0);
                rs.Content = result;
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
