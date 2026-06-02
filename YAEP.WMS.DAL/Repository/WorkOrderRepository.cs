using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class WorkOrderRepository<T> : AbstractRepository<T>, IWorkOrderRepository
         where T : class, IWorkOrderModel
    {
        public WorkOrderRepository(IRepositoryHandler<T> handler) : base(handler)
        {

            this._Handler.IsAutoHandleError = false;
        }
        public IActionResult<bool> AddWorkOrder(IEnumerable<dynamic> Model)
        {
            var query = @"
            INSERT INTO [dbo].[WMS_WorkOrder]
           ([UID]
           ,[ID]
           ,[Name]
           ,[ManifestUID]
           ,[VesselUID]
           ,[Status]
           ,[Type]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
     VALUES
           (@UID
           ,@ID
           ,@Name
           ,@ManifestUID
           ,@VesselUID
           ,@Status
           ,@Type
           ,@CreatedBy
           ,@CreatedOn
           ,@ModifiedBy
           ,@ModifiedOn)
            ";
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.Instance.Execute(query, Model) > 0;
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

        public IActionResult<bool> AddWorkOrder(dynamic Model)
        {
            var query = @"
            INSERT INTO [dbo].[WMS_WorkOrder]
           ([UID]
           ,[ID]
           ,[Name]
           ,[ManifestUID]
           ,[VesselUID]
           ,[Status]
           ,[Type]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
     VALUES
           (@UID
           ,@ID
           ,@Name
           ,@ManifestUID
           ,@VesselUID
           ,@Status
           ,@Type
           ,@CreatedBy
           ,@CreatedOn
           ,@ModifiedBy
           ,@ModifiedOn)
            ";
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.Instance.Execute(query, Model) > 0;
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
        public IActionResult<bool> BatchChangeStatus(IEnumerable<Guid> workorderUID, WorkOrderStatus status, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = "UPDATE WMS_Workorder SET Status=@Status,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn WHERE UID IN @UID AND Status>0";
                var index = 0;
                var grp = workorderUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query,
                        new
                        {
                            Status = (int)status,
                            modifiedBy = modifiedBy,
                            ModifiedOn = DateTime.UtcNow,
                            UID = items
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
        public IActionResult<bool> ChangeStatus(Guid workorderUID, WorkOrderStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(
                    new { Status = (int)status },
                    new { UID = workorderUID });
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

        public IActionResult<bool> DeleteWorkOrder(object parameters)
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

        public IActionResult<bool> EditWorkOrder(dynamic condition, dynamic model)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(condition, model);
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

        public IActionResult<Guid?> GetWorkOrderUID(Guid VesselUID)
        {

            var rs = ActionResultTemplates.Result<Guid?>();

            var query = "SELECT UID FROM [WMS_WorkOrder] where VesselUID=@VesselUID AND Status>0";
            rs.Content = this._Handler.Instance.QueryFirst<Guid?>(query, new { VesselUID = VesselUID });
            rs.Success = true;

            return rs;
        }

        public IActionResult<bool> HaveTicket(IHaveTicketParameters parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"
                      SELECT [WT].UID FROM WMS_Ticket AS [WT] 
	                  INNER JOIN WMS_WorkOrder AS [WWO] on [WWO].UID=[WT].WorkOrderUID
	                  INNER JOIN WMS_WorkOrder_Pod AS[WWOP] ON [WWOP].WorkOrderUID=[WWO].UID
					  INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                      WHERE {0} ";
                query = string.Format(query, getCondition(parameters));
                rs.Content = this._Handler.Instance
                        .Query<Guid>(query, parameters).Count() > 0;
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
        private string getCondition(IHaveTicketParameters Parameters)
        {
            List<string> Condition = new List<string>();
            if (Parameters.workOrderPodGuids != null)
            {
                Condition.Add("([WWOP].UID in @workOrderPodGuids)");
            }
            if (Parameters.workOrderPayloadGuids != null)
            {
                Condition.Add("([WWOPL].UID in @workOrderPayloadGuids)");
            }

            Condition.Add("([WT].Status>0)");
            return Condition.Count > 0 ? " " + string.Join("AND", Condition) : "";
        }

        public IActionResult<bool> SetSlot(ISetSlotParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE [WMS_WorkOrder_Payload] SET SlotUID=@SlotUID 
                              WHERE WorkOrderPodUID=@WorkOrderPodUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, Parameters) > 0;
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
        public IActionResult<bool> SetLoadingZoneSlotByWorkOrderPodUID(ISetSlotParameters Parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE [WMS_WorkOrder_Payload] SET LoadingZoneSlotUID=@SlotUID 
                              WHERE WorkOrderPodUID=@WorkOrderPodUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, Parameters) > 0;
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
        public IActionResult<bool> SetWorkOrderPodBarcode(ISetWorkOrderPodBarcodeParameters Parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE [WMS_WorkOrder_Pod] SET BarcodeUID=@BarcodeUID 
                              WHERE PodUID=@PodUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, Parameters) > 0;
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


        public IActionResult<IManifestModel> GetManifestInfo(Guid vesselUID)
        {

            var rs = ActionResultTemplates.Result<IManifestModel>();
            try
            {
                var query = @"SELECT [WM].* FROM [WMS_BOL] AS [WB]
                              INNER JOIN [WMS_Vessel] AS [WV] ON [WB].UID=[WV].BolUID
                              INNER JOIN [WMS_Manifest] AS [WM] ON [WM].UID=[WB].ManifestUID
                              WHERE [WV].UID=@UID AND [WV].Status>0 AND [WB].Status>0";
                rs.Content = this._Handler.Instance.QueryFirst<ManifestInnerModel>(query, new { UID = vesselUID });
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
        public IActionResult<IEnumerable<IManifestModel>> GetManifestInfoByWorkOrder(Guid[] workOrderUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IManifestModel>>();
            try
            {
                var query = @"SELECT [WM].* FROM [WMS_WorkOrder] AS [WWO]
                              INNER JOIN [WMS_Manifest] AS [WM] ON [WM].UID=[WWO].ManifestUID
                              WHERE [WWO].UID in @UID AND [WWO].Status>0 AND [WM].Status>0 ";
                rs.Content = this._Handler.Instance.Query<ManifestInnerModel>(query, new { UID = workOrderUID });
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
        public IActionResult<IEnumerable<IOutboundUnAssignedListModel>> GetOutboundUnAssignedList(Guid vesselUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IOutboundUnAssignedListModel>>();
            try
            {
                var query = @"SELECT [WVM].UID,[WVM].ItemUID,[WVM].PackageUID PickPackageUID
                                ,[WVM].Qty PickQty,0 AllocatedQty ,0 FreeQty
                                FROM WMS_Vessel_Manifest [WVM] 
                                WHERE  [WVM].Status>0 AND  [WVM].VesselUID=@vesselUID ";
                rs.Content = this._Handler.Instance.Query<OutboundUnAssignedListInnerModel>(query, new { vesselUID = vesselUID });
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

        public IActionResult<IEnumerable<dynamic>> GetUnAssingedPayload(ITicketGenerateParameter parameter)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<dynamic>>();
            try
            {
                var query = @" SELECT DISTINCT [WWOPL].UID,[WWOPL].ID,[WWOPL].Name,[WWOP].UID 'WorkOrderPodUID' 
                                FROM WMS_WorkOrder AS [WWO]
                    INNER JOIN WMS_Manifest AS [WM] ON [WM].UID=[WWO].ManifestUID
					INNER JOIN WMS_Vessel  AS [WV] ON [WV].UID=[WWO].VesselUID 
					INNER JOIN WMS_BOL AS [WB] ON [WB].UID=[WV].BolUID
                    INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WWO].UID=[WWOPL].WorkOrderUID
					LEFT JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                    WHERE {0} AND [WWO].Status>0 AND [WV].Status>0 AND [WWOPL].Status>0 AND [WM].Status>0 AND [WB].Status>0  
                    AND [WWOP].UID IS NULL ";
                var result = this._Handler.Instance.Query(string.Format(query, getCondition(parameter)), parameter);
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

        public IActionResult<IEnumerable<IWorkOrderModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IWorkOrderModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > 0);
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

        public IActionResult<IEnumerable<IOutboundAllocatedItemModel>> GetOutboundAllocatedList(Guid vesselUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IOutboundAllocatedItemModel>>();
            try
            {
                var query = @"SELECT NEWID() UID,[WWPL].VesselManifestUID,[WWPL].status,[WA].Name 'AreaName',[WB].Name 'BinName',
                [WP].SlotUID,[WWPL].ItemUID,[WS].Name 'SlotName',[WP].ID 'PayloadID',
                [WWPL].PackageUID 'PickPackageUID', [WWPL].Qty 'PickQty',[WWPL].Volume,[WWPL].Weight
                FROM WMS_Vessel_Manifest [WVM]
                INNER JOIN  WMS_WorkOrder_Payload AS [WWPL] ON [WVM].UID=[WWPL].VesselManifestUID
                INNER JOIN WMS_Payload AS [WP] ON [WWPL].PayloadUID=[WP].UID
                INNER JOIN WMS_Slot AS [WS] ON [WS].UID=[WP].SlotUID
                INNER JOIN WMS_Bin AS [WB] ON [WB].UID=[WS].BinUID
                INNER JOIN WMS_Area AS [WA] ON [WA].UID=[WB].AreaUID
                WHERE [WWPL].Status>0 AND [WVM].VesselUID=@VesselUID";
                rs.Content = this._Handler.Instance.Query<OutboundUnAssignedItemInnerModel>(query, new { vesselUID = vesselUID });
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

        public IActionResult<bool> SetLoadingZoneSlotByWorkOrderPayloadUID(ISetSlotParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE [WMS_WorkOrder_Payload] SET LoadingZoneSlotUID=@SlotUID 
                              WHERE UID=@WorkOrderPayloadUID AND Status>0";
                rs.Content = this._Handler.Instance.Execute(query, Parameters) > 0;
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
        private string getCondition(ITicketGenerateParameter condition)
        {

            List<string> Condition = new List<string>();
            if (condition.ManifestUID != Guid.Empty && !condition.BolUID.HasValue)
            {
                Condition.Add("([WWO].ManifestUID=@ManifestUID)");
            }
            if (condition.BolUID.HasValue && condition.BolUID != Guid.Empty)
            {
                Condition.Add("([WB].UID=@BolUID)");
            }
            else if (condition.BolUIDs != null && condition.BolUIDs.Count() > 0)
            {
                Condition.Add("([WB].UID in @BolUIDs)");
            }
            if (condition.VesselUID.HasValue && condition.VesselUID != Guid.Empty)
            {
                Condition.Add("([WB].UID=@BolUID)");
            }
            return Condition.Count > 0 ? string.Join("AND", Condition) : "";
        }

        public IActionResult<Tuple<IManifestModel, IVesselModel>> GetManifestVesselInfo(Guid WorkOrderPodUID)
        {
            var manifestInfo = new ManifestInnerModel();
            var vesselInfo = new VesselInnerModel();

            var rs = ActionResultTemplates.Result<Tuple<IManifestModel, IVesselModel>>();
            try
            {
                var query = @"SELECT [WM].* FROM
				[WMS_WorkOrder_Pod] AS [WWOP]
				INNER JOIN  [WMS_WorkOrder] AS [WWO] ON [WWO].UID=[WWOP].WorkOrderUID
                INNER JOIN [WMS_Manifest] AS [WM] ON [WM].UID=[WWO].ManifestUID
                WHERE [WWOP].STATUS >0 AND [WWOP].UID=@UID
                ;
                SELECT [WV].* FROM
                [WMS_WorkOrder_Pod] AS [WWOP]
				INNER JOIN  [WMS_WorkOrder] AS [WWO] ON [WWO].UID=[WWOP].WorkOrderUID
                INNER JOIN [WMS_Vessel] AS [WV] ON [WV].UID=[WWO].VesselUID
                WHERE [WWOP].STATUS >0 AND [WWOP].UID=@UID ";
                var rscollection = this._Handler.Instance.QueryMultiple(query, new { UID = WorkOrderPodUID });
                manifestInfo = rscollection.Read<ManifestInnerModel>().FirstOrDefault();
                vesselInfo = rscollection.Read<VesselInnerModel>().FirstOrDefault();
                var result = Tuple.Create<IManifestModel, IVesselModel>(manifestInfo, vesselInfo);
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

        public IActionResult<bool> ChangeAllWorkOrderStatus(IEnumerable<Guid> workOrderUID,
            WorkOrderStatus workOrderStatus, WorkOrderPodStatus workOrderPodStatus,
            WorkOrderPayloadStatus workOrderPayloadStatus)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = @"UPDATE WMS_Workorder SET Status=@Status WHERE UID IN @UID AND Status>0
                              UPDATE WMS_Workorder_Pod SET Status=@PodStatus WHERE WorkorderUID IN @UID AND Status>0
                              UPDATE WMS_Workorder_Payload SET Status=@PayloadStatus WHERE WorkorderUID IN @UID AND Status>0";
                var index = 0;
                var grp = workOrderUID.GroupBy(g => index++ / 1000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query,
                        new
                        {
                            Status = (int)workOrderStatus,
                            PodStatus = (int)workOrderPodStatus,
                            PayloadStatus = (int)workOrderPayloadStatus,
                            UID = items
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
    }
}
