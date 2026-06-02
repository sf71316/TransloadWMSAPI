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
    public class TicketRepository<T> : AbstractRepository<T>, ITicketRepository where T : class, ITicketModel
    {
        public TicketRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> AddTicket(IEnumerable<ITicketModel> Collection)
        {

            var rs = ActionResultTemplates.Result<bool>();

            var query = @"INSERT INTO [dbo].[WMS_Ticket]
           ([UID]
           ,[ID]
           ,[Name]
           ,[Type]
           ,[WarehouseUID]
           ,[ManifestType]
           ,[TicketSequence]
           ,[WorkOrderUID]
           ,[ServiceItemUID]
           ,[Status]
           ,[Description]
           ,[OperationInstruction]
           ,[OperationSuggestion]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
     VALUES
           (@UID
           ,@ID
           ,@Name
           ,@Type
           ,@WarehouseUID
           ,@ManifestType
           ,@TicketSequence
           ,@WorkOrderUID
           ,@ServiceItemUID
           ,@Status
           ,@Description
           ,@OperationInstruction
           ,@OperationSuggestion
           ,@CreatedBy
           ,@CreatedOn
           ,@ModifiedBy
           ,@ModifiedOn)";
            rs.Content = true;
            //foreach (var item in Collection)
            //{
            //    rs.Content &= this._Handler.CreateByDynamic(item);
            //}
            var index = 0;
            var grp = Collection.GroupBy(g => index++ / 2000);

            foreach (var items in grp)
            {
                rs.Content &= this._Handler.Instance.Execute(query, items) > 0;
            }
            rs.Success = rs.Content;

            return rs;
        }
        public IActionResult<bool> DeleteTicket(Guid TicketUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.Delete(TicketUID);
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
        public IActionResult<IEnumerable<ITicketGeneratoreDataModel>> GetGeneratoreTicketData(object condition)
        {
            var parameter = condition as ITicketGenerateParameter;
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketGeneratoreDataModel>>();

            var query = string.Format(@"
                SELECT 
                [WWO].UID as 'WorkOrderUID',[WWOP].UID 'WorkOrderPodUID',[WWOP].Name,[WWOP].PodUID,
                [WWOP].BarcodeUID as 'PodBarcodeUID',[WWOP].Type 'StoreageMethod',[WWOP].StartDate,[WWOP].EndDate,
                 [WVM].PackageUID as 'OriginalPackageUID',[WWOPL].UID AS WorkOrderPayloadUID,[WWOPL].PayloadUID,[WWOPL].LoadingZoneSlotUID,
				 [WWOPL].SlotUID,[WWOPL].ItemUID,[WWOPL].PackageUID,[WWOPL].Qty,AVG([WVM].Qty) AS OriginalQty,
                [WWOP].OperationSuggestion
                FROM  [WMS_Manifest] as [WM]
                INNER JOIN [WMS_BOL] AS [WB] ON [WB].ManifestUID=[WM].UID AND [WB].Status>0
				INNER JOIN [WMS_Vessel] AS [WV] ON [WV].BolUID=[WB].UID AND [WV].Status>0
                INNER JOIN [WMS_WorkOrder] AS [WWO] on [WWO].ManifestUID=[WM].UID AND [WWO].VesselUID=[WV].UID AND [WWO].Status>0
                INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                INNER JOIN [WMS_WorkOrder_Payload] as [WWOPL] ON [WWOP].UID=[WWOPL].WorkOrderPodUID
                INNER JOIN [WMS_Vessel_Manifest] as [WVM] on [WWOPL].VesselManifestUID=[WVM].UID
				LEFT JOIN [WMS_Ticket] AS [WT] ON [WT].WorkOrderUID=[WWO].UID AND [WT].Status>0
                WHERE [WM].Status>0 
				AND [WWO].Status>0 AND [WWOPL].Status>0 AND [WWOP].Status>0 AND [WVM].Status>0
                 AND ( [WT].UID IS NULL) {{0}}
                GROUP BY [WWO].UID,[WWOP].UID,[WWOP].Name,[WWOP].PodUID,
                [WWOP].BarcodeUID,[WWOP].Type,[WWOP].StartDate,[WWOP].EndDate,
                [WWOPL].UID,[WWOPL].PayloadUID,[WWOPL].SlotUID,[WWOPL].ItemUID,
                [WWOPL].PackageUID,[WWOPL].Qty, [WVM].PackageUID,[WWOPL].LoadingZoneSlotUID,[WWOP].OperationSuggestion");
            rs.Content = this._Handler.Instance.Query<TicketGeneratoreDataModel>(string.Format(query, getCondition(parameter)),
                new
                {
                    parameter.ManifestUID,
                    parameter.BolUID,
                    parameter.BolUIDs,
                    parameter.VesselUID,
                    TicketStatus = (int)TicketStatus.Glitch
                });
            rs.Success = true;

            return rs;
        }
        public IActionResult<IEnumerable<ITicketGeneratoreDataModel>> GetGeneratoreTicketDataByMoveManifest(object condition)
        {
            var parameter = condition as ITicketGenerateParameter;
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketGeneratoreDataModel>>();
            try
            {
                var query = string.Format(@"
                SELECT 
                [WWO].UID as 'WorkOrderUID',[WWOP].UID 'WorkOrderPodUID',[WWOP].Name,[WWOP].PodUID,
                [WWOP].BarcodeUID as 'PodBarcodeUID',[WWOP].Type 'StoreageMethod',[WWOP].StartDate,[WWOP].EndDate,
                 [WWOPL].UID AS WorkOrderPayloadUID,
                [WWOPL].PayloadUID,[WWOPL].LoadingZoneSlotUID,
				 [WWOPL].SlotUID,[WWOPL].ItemUID,[WWOPL].PackageUID,[WWOPL].Qty,0 AS OriginalQty,
                [WWOP].OperationSuggestion
			
                FROM  [WMS_WorkOrder] AS [WWO]
                INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWO].UID=[WWOP].WorkOrderUID
                INNER JOIN [WMS_WorkOrder_Payload] as [WWOPL] ON [WWOP].UID=[WWOPL].WorkOrderPodUID
				LEFT JOIN [WMS_Ticket] AS [WT] ON [WT].WorkOrderUID=[WWO].UID AND [WT].Status>0
                WHERE 
				 [WWO].Status>0 AND [WWOPL].Status>0 AND [WWOP].Status>0 
                 AND ( [WT].UID IS  NULL)  {{0}}
                GROUP BY [WWO].UID,[WWOP].UID,[WWOP].Name,[WWOP].PodUID,
                [WWOP].BarcodeUID,[WWOP].Type,[WWOP].StartDate,[WWOP].EndDate,
                [WWOPL].UID,[WWOPL].PayloadUID,[WWOPL].SlotUID,[WWOPL].ItemUID,
                [WWOPL].PackageUID,[WWOPL].Qty,[WWOPL].LoadingZoneSlotUID,[WWOP].OperationSuggestion");
                rs.Content = this._Handler.Instance.Query<TicketGeneratoreDataModel>(string.Format(query, getCondition(parameter)),
                    new
                    {
                        parameter.WorkOrderUID,
                        TicketStatus = (int)TicketStatus.Glitch
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
        public IActionResult<bool> VoidTicketByWorkOrder(IVoidTicketParameters Parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //update Ticket
                //update TicketInfo
                var query = @"
                Update [Ticket] SET [Ticket].STATUS=0,
                [Ticket].ModifiedBy=@ModifiedBy,
                [Ticket].ModifiedOn=getdate()
                FROM  [WMS_WorkOrder] AS [WorkOrder] 
                INNER JOIN [WMS_Ticket] AS [Ticket] ON [Ticket].WorkOrderUID=  [WorkOrder].UID
                {0}
                Update [TicketInfo] SET [TicketInfo].Status=0,
                [TicketInfo].ModifiedBy=@ModifiedBy,
                [TicketInfo].ModifiedOn=getdate()
                FROM  [WMS_WorkOrder] AS [WorkOrder] 
                INNER JOIN [WMS_Ticket] AS [Ticket] ON [Ticket].WorkOrderUID=  [WorkOrder].UID
                INNER JOIN [WMS_TicketInfo] AS [TicketInfo] ON [TicketInfo].TicketUID=[Ticket].UID
                {0}
                ";
                query = string.Format(query, this.getVoidConditionsByWorkOrder(Parameters));
                rs.Content = this._Handler.Instance.Execute(query, Parameters) > 0;
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
        public IActionResult<bool> VoidTicket(IVoidTicketParameters Parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //update Ticket
                //update TicketInfo
                var query = @"
                Update [Ticket] SET [Ticket].STATUS=0,
                [Ticket].ModifiedBy=@ModifiedBy,
                [Ticket].ModifiedOn=getdate()
                FROM [WMS_Manifest] AS [Manifest] 
                INNER JOIN [WMS_BOL] AS [WB] ON [WB].ManifestUID =[Manifest].UID
                INNER JOIN [WMS_Vessel] AS [WV] ON [WV].BolUID=[WB].UID
                INNER JOIN [WMS_WorkOrder] AS [WorkOrder] ON [Manifest].UID=  [WorkOrder].ManifestUID AND [WorkOrder].VesselUID=[WV].UID AND [WorkOrder].Status>0  AND [WB].Status>0  AND [WV].Status>0 AND [Manifest].Status>0
                INNER JOIN [WMS_Ticket] AS [Ticket] ON [Ticket].WorkOrderUID=  [WorkOrder].UID
                {0}
                Update [TicketInfo] SET [TicketInfo].Status=0,
                [TicketInfo].ModifiedBy=@ModifiedBy,
                [TicketInfo].ModifiedOn=getdate()
                FROM [WMS_Manifest] AS [Manifest]  
                INNER JOIN [WMS_BOL] AS [WB] ON [WB].ManifestUID =[Manifest].UID
                INNER JOIN [WMS_Vessel] AS [WV] ON [WV].BolUID=[WB].UID
                INNER JOIN [WMS_WorkOrder] AS [WorkOrder] ON [Manifest].UID=  [WorkOrder].ManifestUID AND [WorkOrder].VesselUID=[WV].UID AND [WorkOrder].Status>0 AND [WB].Status>0  AND [WV].Status>0 AND [Manifest].Status>0
                INNER JOIN [WMS_Ticket] AS [Ticket] ON [Ticket].WorkOrderUID=  [WorkOrder].UID
                INNER JOIN [WMS_TicketInfo] AS [TicketInfo] ON [TicketInfo].TicketUID=[Ticket].UID
                {0}
                ";
                query = string.Format(query, this.getVoidConditions(Parameters));
                rs.Content = this._Handler.Instance.Execute(query, Parameters) > 0;
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

        private string getCondition(ITicketGenerateParameter condition)
        {
            List<string> Condition = new List<string>();
            if (condition.ManifestUID.HasValue && condition.ManifestUID != Guid.Empty)
            {
                Condition.Add("([WM].UID=@ManifestUID)");
            }
            if (condition.BolUID.HasValue && condition.BolUID != Guid.Empty)
            {
                Condition.Add("([WB].UID=@BolUID)");
            }
            else if (condition.BolUIDs != null && condition.BolUIDs.Count() > 0)
            {
                Condition.Add("([WB].UID in @BolUIDs)");
            }
            if (condition.VesselUID.HasValue && condition.VesselUID.Value != Guid.Empty)
            {
                Condition.Add("([WVM].VesselUID=@VesselUID)");
            }
            if (condition.WorkOrderUID.HasValue && condition.WorkOrderUID.Value != Guid.Empty)
            {
                Condition.Add("([WWO].UID=@WorkOrderUID)");
            }
            Condition.Add("([WT].Status<@TicketStatus OR [WT].UID IS NULL)");
            return Condition.Count > 0 ? " AND " + string.Join("AND", Condition) : "";
        }
        public IActionResult<IEnumerable<IComponentViewModel>> GetTicketIDList(Guid BolUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IComponentViewModel>>();
            try
            {
                var query = @"SELECT [WT].Name,[WT].ID,[WT].UID
                FROM  [WMS_WorkOrder] AS [WWO]
                INNER JOIN [WMS_Vessel] AS [WV] on [WWO].VesselUID=[WV].UID
                INNER JOIN [WMS_Ticket] as [WT] on [WWO].UID=[WT].WorkOrderUID
                WHERE  [WV].BolUID=@BOLUID AND [WV].Status>0 AND [WT].Status>0 AND [WWO].Status>0";
                rs.Content = this._Handler.Instance.Query<ComponentViewModel>(query, new { BolUID = BolUID });
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
        public IActionResult<IEnumerable<IComponentViewModel>> GetVesselRefNoList(Guid BolUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IComponentViewModel>>();
            try
            {
                var query = @"SELECT [WV].Name,[WV].ID,[WV].UID
                FROM  [WMS_Vessel] AS [WV]
                WHERE  [WV].BOLUID=@BOLUID ";
                rs.Content = this._Handler.Instance.Query<ComponentViewModel>(query, new { BolUID = BolUID });
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
        public IActionResult<IEnumerable<IBolInfoViewModel>> GetBolInfo(Guid BolUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IBolInfoViewModel>>();
            try
            {
                var query = @"SELECT [WB].Name BolName,[WB].RefNo BolRefNo ,[WB].ETA,[WW].Name WarehouseName,[WW].GroupUID WarehouseGroupUID
                        ,[WB].ShipViaUID,( SELECT  CONVERT(NVARCHAR(200), RefNo)+ ','
                                                    FROM [WMS_Vessel] 
                                                    WHERE [WMS_Vessel].BolUID=@BOLUID AND [WMS_Vessel].Status>0
                                                    FOR xml path(''))AS VesselRefNo FROM  [WMS_BOL] AS [WB]
                        INNER JOIN [WMS_Manifest] AS [WM] ON [WB].ManifestUID=[WM].UID
                        INNER JOIN [WMS_Warehouse] AS [WW] ON [WM].WarehouseUID=[WW].UID
                        WHERE  [WB].UID=@BOLUID AND [WB].Status>0 AND [WM].Status>0 AND [WW].Status>0  ";
                rs.Content = this._Handler.Instance.Query<BolInfoViewModel>(query, new { BolUID = BolUID });
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
        public IActionResult<IEnumerable<IComponentViewModel>> GetBolNameList(Guid ManifestUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IComponentViewModel>>();
            try
            {
                var query = @"SELECT [WB].Name,[WB].ID,[WB].UID
                FROM  [WMS_BOL] AS [WB]
                WHERE  [WB].ManifestUID=@ManifestUID AND [WB].Status>0 ";
                rs.Content = this._Handler.Instance.Query<ComponentViewModel>(query, new { ManifestUID = ManifestUID });
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
        public IActionResult<ITicketAssignedListViewModelCollection> GetTicketAssignedList(ITicketAssignedListParameters Parameters)
        {
            var rs = ActionResultTemplates.Result<ITicketAssignedListViewModelCollection>();
            try
            {
                var query = @"
                SELECT 
                DISTINCT
                [WTI].UID TicketInfoUID,
                [WT].ManifestType,
                [WT].UID 'TicketUID',
                [WT].ID 'TicketNo',
                [WT].Type 'ServiceType',
                [WT].ID,
                [WT].Type,
                [WT].Status,
                [WTI].EstQty,
                [WTI].ActQty,
                [WTI].ShtQty,
                [WTI].SavQty,
                '' [AssignedGroup],
                [WV].Name 'VesselName',
                [WV].UID 'VesselUID',
                [WWOPL].LoadingZoneSlotUID 'SourceLoadingZoneSlotUID',
                [WWOPL].SlotUID 'SourceSlotUID',
                [WWOPL].ItemUID,
                [WWOPL].PayloadPackageUID,
                [WWOPL].PackageUID 'SourcePackageUID',
                2 'MappingType',
				[WH].[GroupUID] AS [WarehouseGroupUID]
                FROM [WMS_WorkOrder] AS[WWO]
				LEFT JOIN  [WMS_Vessel] AS [WV] ON [WV].UID=[WWO].VesselUID  AND [WV].Status>0 
                LEFT JOIN  [WMS_BOL] AS [WB] ON [WB].UID=[WV].BolUID AND [WB].Status>0
				LEFT JOIN [WMS_Manifest] AS [WM] ON [WM].UID =[WWO].ManifestUID AND [WM].Status>0
                INNER JOIN [WMS_Ticket] AS [WT] ON [WT].WorkOrderUID=[WWO].UID
                INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].TicketUID=[WT].UID
                INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].UID=[WTI].WorkOrderPayloadUID
                INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WWOPL].WorkOrderPodUID
                INNER JOIN [WMS_Warehouse] AS [WH] ON [WH].[UID] = [WT].[WarehouseUID]
                {0}
                ;
                SELECT 
                DISTINCT
                [WTI].UID TicketInfoUID,
                [WT].ManifestType,
                [WT].UID 'TicketUID',
                [WT].ID 'TicketNo',
                [WT].Type 'ServiceType',
                [WT].ID,
                [WT].Type,
                [WT].Status,
                [WTI].EstQty,
                [WTI].ActQty,
                [WTI].ShtQty,
                [WTI].SavQty,
                '' [AssignedGroup],
                [WV].Name 'VesselName',
                [WV].UID 'VesselUID',
                [WWOPL].WorkOrderPodUID,
                [WWOPL].LoadingZoneSlotUID 'SourceLoadingZoneSlotUID',
                [WWOPL].SlotUID 'SourceSlotUID',
                [WWOPL].ItemUID,
                [WWOPL].PayloadPackageUID,
                [WWOPL].PackageUID 'SourcePackageUID',
                1 'MappingType',
				[WH].[GroupUID] AS [WarehouseGroupUID]
                FROM [WMS_WorkOrder] AS[WWO]
				LEFT JOIN  [WMS_Vessel] AS [WV] ON [WV].UID=[WWO].VesselUID  AND [WV].Status>0 
                LEFT JOIN  [WMS_BOL] AS [WB] ON [WB].UID=[WV].BolUID AND [WB].Status>0
				LEFT JOIN [WMS_Manifest] AS [WM] ON [WM].UID =[WWO].ManifestUID AND [WM].Status>0
                INNER JOIN [WMS_Ticket] AS [WT] ON [WT].WorkOrderUID=[WWO].UID
                INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].TicketUID=[WT].UID
                INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WTI].WorkOrderPodUID
                INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                INNER JOIN [WMS_Warehouse] AS [WH] ON [WH].[UID] = [WT].[WarehouseUID]
                {0}
                ";
                var collection = new TicketAssignedListInnerViewModelCollection();
                query = string.Format(query, this.getTicketAssignedListConditions(Parameters));
                var result = this._Handler.Instance.QueryMultiple(query, Parameters);
                collection.PayloadData = result.Read<TicketAssignedListInnerViewModel>();
                collection.PodData = result.Read<TicketAssignedListInnerViewModel>();
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
        private string getTicketAssignedListConditions(ITicketAssignedListParameters parameters)
        {
            List<string> condition = new List<string>();
            if (parameters.BolUID.HasValue)
            {
                condition.Add("([WB].UID=@BolUID)");
            }
            if (parameters.ManifestUID.HasValue)
            {
                condition.Add("([WM].UID=@ManifestUID)");
            }
            if (parameters.TicketUID.HasValue)
            {
                condition.Add("([WT].UID=@TicketUID)");
            }
            else
            {
                condition.Add("([WM].Status>0)");
            }

            condition.Add("([WT].Status>0)");
            condition.Add("([WWO].Status>0)");
            return condition.Count > 0 ? "WHERE " + string.Join("AND", condition) : "";
        }
        private string getVoidConditions(IVoidTicketParameters parameters)
        {
            List<string> condition = new List<string>();
            if (parameters.ManifestUID.HasValue && parameters.ManifestUID != Guid.Empty)
            {
                condition.Add("([Manifest].UID=@ManifestUID)");
            }
            if (parameters.BolUID.HasValue && parameters.BolUID != Guid.Empty)
            {
                condition.Add("([WB].UID=@BolUID)");
            }
            if (parameters.VesselUID != null)
            {
                condition.Add("([WV].UID IN @VesselUID)");
            }
            if (parameters.WorkOrderUID != null)
            {
                condition.Add("([WorkOrder].UID=@WorkOrderUID)");
            }
            condition.Add("([Manifest].Status>0)");
            condition.Add("([WB].Status>0)");
            return condition.Count > 0 ? "WHERE " + string.Join("AND", condition) : "";
        }
        private string getVoidConditionsByWorkOrder(IVoidTicketParameters parameters)
        {
            List<string> condition = new List<string>();

            if (parameters.WorkOrderUID != null)
            {
                condition.Add("([WorkOrder].UID= @WorkOrderUID)");
            }
            return condition.Count > 0 ? "WHERE " + string.Join("AND", condition) : "";
        }
        public IActionResult<IEnumerable<ITicketSearchListViewModel>> GetTicketSearchList(ITicketSearchListParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketSearchListViewModel>>();
            try
            {
                List<string> condition = new List<string>();

                var query = @"SELECT
[WMS_Ticket].[UID] AS [TicketUID],
[WMS_Ticket].[Status] AS [TicketStatus],
[WMS_Ticket].[ID] AS [TicketNo],
[WMS_BOL].[UID] AS [BolUID],
[WMS_BOL].[RefNo] AS [BOLRefNo],
[WMS_BOL].[Name] AS [BolNo],
[WMS_BOL].[DeliveryDate],
[WMS_BOL].[ETA],[WMS_BOL].[RevETA],
[WMS_Vessel].[Name] AS [VesselNo],
[WMS_Vessel].[RefNo] AS [VesselRefNo],
[WMS_Manifest].[UID] AS [ManifestUID],
[WMS_Manifest].[PartyUID],
[WMS_Manifest].[ID] AS [ManifestNo],
[WMS_Ticket].[Type] AS [TicketType],
[WMS_Ticket].[OperationInstruction],
[WMS_Ticket].[OperationSuggestion],
[WMS_Ticket].[TicketSequence]
FROM [WMS_TicketInfo_Assignee_Relation]
            INNER JOIN [WMS_TicketInfo] ON [WMS_TicketInfo].UID=[WMS_TicketInfo_Assignee_Relation].TicketInfoUID
            INNER JOIN [WMS_Ticket] ON [WMS_Ticket].UID=[WMS_TicketInfo].TicketUID
            INNER JOIN [WMS_WorkOrder] ON [WMS_WorkOrder].UID=[WMS_Ticket].WorkOrderUID
            LEFT JOIN [WMS_Vessel] ON [WMS_Vessel].UID=[WMS_WorkOrder].VesselUID
            LEFT JOIN [WMS_BOL] ON [WMS_BOL].UID=[WMS_Vessel].BolUID 
            LEFT JOIN [WMS_Manifest] ON [WMS_Manifest].UID=[WMS_BOL].ManifestUID
{0}
GROUP BY 
[WMS_Ticket].[UID],
[WMS_Ticket].[Status],
[WMS_Ticket].[ID],
[WMS_BOL].[UID],
[WMS_BOL].[RefNo],
[WMS_BOL].[Name],
[WMS_BOL].[DeliveryDate],
[WMS_BOL].[ETA],[WMS_BOL].[RevETA],
[WMS_Vessel].[Name],
[WMS_Vessel].[RefNo],
[WMS_Manifest].[UID],
[WMS_Manifest].[PartyUID],
[WMS_Manifest].[ID],
[WMS_Ticket].[Type],
[WMS_Ticket].[OperationInstruction],
[WMS_Ticket].[OperationSuggestion],
[WMS_Ticket].[TicketSequence]
ORDER BY [WMS_Ticket].[ID]";

                if (parameters.WarehouseUID != null)
                {
                    condition.Add("([WMS_Manifest].[WarehouseUID] = @WarehouseUID)");
                }
                if (!String.IsNullOrEmpty(parameters.TicketNo))
                {
                    condition.Add("([WMS_Ticket].[ID] = @TicketNo)");
                }
                if (parameters.TicketType.HasValue)
                {
                    condition.Add("([WMS_Ticket].[Type] = @TicketType)");
                }
                if (parameters.TicketStatus.HasValue)
                {
                    condition.Add("([WMS_Ticket].[Status] = @TicketStatus)");
                }
                if (!String.IsNullOrEmpty(parameters.ManifestNo))
                {
                    condition.Add("([WMS_Manifest].[ID] = @ManifestNo)");
                }
                if (!String.IsNullOrEmpty(parameters.Option))
                {
                    parameters.OptionText = "%" + parameters.OptionText + "%";
                    switch (parameters.Option)
                    {
                        case "manifestref": condition.Add("([WMS_Manifest].[RefNo] LIKE @OptionText)"); break;
                        case "bolno": condition.Add("([WMS_BOL].[Name] LIKE @OptionText)"); break;
                        case "bolref": condition.Add("([WMS_BOL].[RefNo] LIKE @OptionText)"); break;
                        case "vesselno": condition.Add("([WMS_Vessel].[RefNo] LIKE @OptionText)"); break;
                    }
                }
                if ((parameters.PHierarchy?.Count() ?? 0) > 0)
                {
                    string itemConditionString = $"'{String.Join("','", parameters.PHierarchy)}'";
                    condition.Add($@"(
			[WMS_TicketInfo].[WorkOrderPayloadUID] IN (
				        SELECT [UID]
				        FROM [WMS_WorkOrder_Payload]
				        WHERE [WMS_WorkOrder_Payload].[ItemUID] IN ({itemConditionString})
			        ) 
			OR 
			[WMS_TicketInfo].[WorkOrderPodUID] IN (
				        SELECT [WMS_WorkOrder_Pod].[UID]
				        FROM [WMS_WorkOrder_Pod]
						            INNER JOIN [WMS_WorkOrder_Payload] ON [WMS_WorkOrder_Payload].WorkOrderPodUID = [WMS_WorkOrder_Pod].[UID]
				        WHERE [WMS_WorkOrder_Payload].[ItemUID] IN ({itemConditionString})
			        ) 
		)");
                }

                query = string.Format(query, condition.Count > 0 ? " WHERE " + string.Join("AND", condition) : "");
                rs.Content = this._Handler.Instance.Query<TicketListSearchModel>(query, parameters);

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
        public IActionResult<IEnumerable<ITicketListViewModel>> GetTicketList(IGetTicketListParameters parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketListViewModel>>();

            List<string> condition = new List<string>();
            var orderby = "";
            if ((parameters.type != null && parameters.type.Contains(TicketCategory.Receive))
                || (parameters.mtype != null && parameters.mtype.Contains(ManifestType.Inbound)))
            {
                orderby = ",[WB].DeliveryDate";
            }
            else if ((parameters.type != null && parameters.type.Contains(TicketCategory.Outbound))
                || (parameters.mtype != null && parameters.mtype.Contains(ManifestType.Outbound)))
            {
                orderby = ",ISNULL([WB].ETA,[WB].RevETA)";
            }
            var query = @"SELECT [WT].UID AS 'TicketUID',[WT].Status AS 'TicketStatus',[WT].ID,[WB].RefNo AS 'BOLRefNo',[WV].RefNo AS 'VesselRefNo',
                            [WB].Name BolNo,[WV].Name VesselNo,[WT].Type AS 'TicketType',[WM].PartyUID,[WT].OperationInstruction,
                            [WT].OperationSuggestion
                            ,[WB].DeliveryDate,[WT].TicketSequence,[WB].ETA,[WB].RevETA 
                            FROM [WMS_TicketInfo_Assignee_Relation] AS [WTAR]
                            INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].UID=[WTAR].TicketInfoUID  AND [WTI].Status>0
                            INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID AND [WT].Status>0
                            INNER JOIN [WMS_WorkOrder] AS [WWO] ON [WWO].UID=[WT].WorkOrderUID AND [WWO].Status>0
                            LEFT JOIN [WMS_Vessel] AS [WV] ON [WV].UID=[WWO].VesselUID AND [WV].Status>0
                            LEFT JOIN [WMS_BOL] AS [WB] ON [WB].UID=[WV].BolUID AND [WB].Status>0
                            LEFT JOIN [WMS_Manifest] AS [WM] ON [WM].UID=[WB].ManifestUID AND [WM].Status>0
                            WHERE [WTAR].GroupUID IN @groupIds {0} AND [WT].Status>0 AND  [WTAR].Status>0
                                  AND [WWO].Status>0 
                            GROUP BY  [WB].ETA,[WB].RevETA,[WT].UID  ,[WT].Status ,[WT].ID,[WB].RefNo ,[WV].RefNo,
                            [WT].Type,[WM].PartyUID ,[WT].OperationInstruction,[WT].OperationSuggestion,[WB].Name,[WV].Name
                            ,[WB].DeliveryDate,[WT].TicketSequence" +
                        $" ORDER BY [WB].Name, [WT].Type {orderby}  ";
            if ((parameters.type?.Length ?? 0) > 0)
            {
                condition.Add("([WT].Type IN @type)");
            }
            if ((parameters.mtype?.Length ?? 0) > 0)
            {
                condition.Add("([WT].ManifestType IN @mtype)");
            }
            if ((parameters.tstatus?.Length ?? 0) > 0)
            {
                condition.Add("([WT].Status IN @tstatus)");
            }
            if (parameters.CreatedStartDate.HasValue)
            {
                condition.Add("([WT].CreatedOn>=@CreatedStartDate )");
            }
            if (parameters.CreatedEndDate.HasValue)
            {
                condition.Add("([WT].CreatedOn<=@CreatedEndDate)");
            }
            query = string.Format(query, condition.Count > 0 ? " AND " + string.Join("AND", condition) : "");
            rs.Content = this._Handler.Instance.Query<TicketListInnerModel>(query,
                                new
                                {
                                    parameters.groupIds,
                                    parameters.type,
                                    parameters.mtype,
                                    parameters.tstatus,
                                    parameters.CreatedEndDate,
                                    parameters.CreatedStartDate
                                });

            IEnumerable<TicketParentViewInnerModel> relationRs = null;
            IEnumerable<TicketListSlotInfoInnerModel> slotRs = null;
            if (rs.Content.Count() > 0)
            {
                #region 查詢相關Ticket
                query = @"SELECT [WTR].ParentUID,[WT2].Status 'ParentTicketStatus',[WT2].UID 'ParentTicketUID',[WTR].TicketUID 
                          ,[WT2].ID 'ParentTicketID'
                          FROM [WMS_Ticket] AS [WT]
                          LEFT JOIN[WMS_TIcket_Relation] AS[WTR] ON[WT].UID =[WTR].TicketUID
                          LEFT JOIN[WMS_Ticket] AS[WT2] ON[WT2].UID =[WTR].ParentUID
                          WHERE [WTR].Status > 0 AND [WT].UID IN ({0})";
                relationRs = this._Handler.Instance.Query<TicketParentViewInnerModel>(string.Format(query,
                string.Join(",", rs.Content.Select(p => "'" + p.TicketUID + "'"))),
                null);
                #endregion

                #region 查詢 Ticket 中有指定的Slot
                query = @"SELECT  [WTI].TicketUID,[WT].Type,[WT].ManifestType,WWOPL.SlotUID,[WS].Name 'SlotName',WWOPL.LoadingZoneSlotUID,[WSLZ].Name 'LoadingZoneSlotName' 
                FROM WMS_TicketInfo AS [WTI]
                INNER JOIN WMS_Ticket [WT] ON [WT].UID=[WTI].TicketUID AND [WT].Status>0
                INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WWOPL].UID=[WTI].WorkOrderPayloadUID AND [WWOPL].Status>0
                INNER JOIN WMS_Slot AS [WS] ON [WS].UID=[WWOPL].SlotUID AND [WS].Status>0
                INNER JOIN WMS_Slot AS [WSLZ] ON [WSLZ].UID=[WWOPL].LoadingZoneSlotUID AND [WWOPL].Status>0
                WHERE [WTI].status >0 AND [WTI].TicketUID
                IN ({0})
                GROUP BY [WTI].TicketUID,[WT].Type,WWOPL.SlotUID,WWOPL.LoadingZoneSlotUID,[WSLZ].Name,[WS].Name,[WT].ManifestType
                UNION ALL
                SELECT  [WTI].TicketUID,[WT].Type,[WT].ManifestType,WWOPL.SlotUID,[WS].Name 'SlotName',WWOPL.LoadingZoneSlotUID,[WSLZ].Name   'LoadingZoneSlotName'  
                FROM WMS_TicketInfo AS [WTI]
                INNER JOIN WMS_Ticket [WT] ON [WT].UID=[WTI].TicketUID AND [WT].Status>0
                INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWOP].UID=[WTI].WorkOrderPodUID AND [WWOP].Status>0
                INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOPL].Status>0
                INNER JOIN WMS_Slot AS [WS] ON [WS].UID=[WWOPL].SlotUID AND [WS].Status>0
                INNER JOIN WMS_Slot AS [WSLZ] ON [WSLZ].UID=[WWOPL].LoadingZoneSlotUID AND [WWOPL].Status>0
                WHERE [WTI].status >0 AND [WTI].TicketUID
                IN ({0})
                GROUP BY [WTI].TicketUID,[WT].Type,WWOPL.SlotUID,WWOPL.LoadingZoneSlotUID,[WSLZ].Name,[WS].Name,[WT].ManifestType
                ORDER BY TicketUID";
                var ss = string.Format(query,
                   string.Join(",", rs.Content.Select(p => "'" + p.TicketUID + "'"))
                   );
                slotRs = this._Handler.Instance.Query<TicketListSlotInfoInnerModel>(string.Format(query,
                   string.Join(",", rs.Content.Select(p => "'" + p.TicketUID + "'"))
                   ), null);
                #endregion

            }

            foreach (var item in rs.Content)
            {
                item.TicketStatusName = Enum.GetName(typeof(TicketStatus), item.TicketStatus);
                item.TicketTypeName = Enum.GetName(typeof(TicketCategory), item.TicketType);
                var relation = relationRs.Where(p => p.TicketUID == item.TicketUID);
                if (relation.Count() > 0)
                {
                    foreach (var item2 in relation)
                    {
                        item2.ParentTicketStatusName = Enum.GetName(typeof(TicketStatus), item2.ParentTicketStatus.Value);
                    }
                }
                item.Parent = relation;
                var slots = slotRs.Where(p => p.TicketUID == item.TicketUID);
                if (item.TicketType == (int)TicketCategory.Receive)
                {

                    item.FromSlots = slots.Select(p => p.LoadingZoneSlotName).GroupBy(g => g).Select(x => x.Key);
                }
                if (item.TicketType == (int)TicketCategory.Outbound)
                {
                    item.FromSlots = slots.Select(p => p.SlotName).GroupBy(g => g).Select(x => x.Key);
                }
                if (item.TicketType == (int)TicketCategory.Move)
                {
                    if (slots.FirstOrDefault()?.ManifestType == (int)ManifestType.Inbound)
                    {
                        item.FromSlots = slots.Select(p => p.LoadingZoneSlotName).GroupBy(g => g).Select(x => x.Key);
                    }
                    else if (slots.FirstOrDefault()?.ManifestType == (int)ManifestType.Outbound)
                    {
                        item.FromSlots = slots.Select(p => p.SlotName).GroupBy(g => g).Select(x => x.Key);
                    }
                    else if (slots.FirstOrDefault()?.ManifestType == (int)ManifestType.Move)
                    {
                        item.FromSlots = slots.Select(p => p.SlotName).GroupBy(g => g).Select(x => x.Key);
                    }
                }
            }
            rs.Success = true;

            return rs;
        }

        public IActionResult<bool> UpdateTicketStatus(IEnumerable<Guid> ticketUID, TicketStatus status, string modifiedBy = "")
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var index = 0;
                var query = "UPDATE WMS_Ticket SET Status=@Status,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn WHERE UID IN @TicketUID AND Status>0";
                var grp = ticketUID.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query,
                         new
                         {
                             Status = (int)status,
                             modifiedBy = modifiedBy,
                             ModifiedOn = DateTime.UtcNow,
                             ticketUID = items
                         }) > 0;
                }
                // rs.Content = this._Handler.Instance.Execute(query, new { Status = (int)status, ticketUID = ticketUID }) > 0;
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
        //public IActionResult<bool> BatchUpdateTicketStatus(IEnumerable<Guid> ticketUID, TicketStatus status)
        //{
        //    var rs = ActionResultTemplates.Result<bool>();
        //    try
        //    {
        //        var query = "UPDATE WMS_Ticket SET Status=@Status WHERE UID IN @TicketUID AND Status>0";
        //        rs.Content = this._Handler.Instance.Execute(query, new { Status = (int)status, ticketUID = ticketUID }) > 0;
        //        rs.Success = rs.Content;
        //    }
        //    catch (Exception ex)
        //    {
        //        rs.Message = ex.Message;
        //        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
        //        rs.Success = false;
        //        rs.InnerException = ex;
        //        this.OnExpcetion(ex);
        //    }
        //    return rs;

        //}
        public IActionResult<ISlotModel> CheckSlotExistByTicketInfo(Guid ticketInfoUID, string slotName)
        {
            var rs = ActionResultTemplates.Result<ISlotModel>();
            try
            {
                var query = @"SELECT * FROM WMS_Slot WHERE WarehouseUID IN(
                    SELECT  [wt].WarehouseUID from WMS_TicketInfo [wi]
                    INNER JOIN WMS_Ticket [wt] on [wi].TicketUID=wt.UID and [wt].Status>0
                    INNER JOIN WMS_WorkOrder [wo] on [wt].WorkOrderUID=[wo].UID and [wo].Status>0
                    WHERE wi.Type=@TicketInfoType and wi.Status>0 and wi.UID=@TicketInfoUID 
                    ) and WMS_Slot.Status>0 and WMS_Slot.Name=@slotName ";
                var result = this._Handler.Instance.Query<SlotInnerModel>(query,
                    new
                    {
                        TicketInfoUID = ticketInfoUID,
                        slotName = slotName,
                        TicketInfoType = (int)TicketType.Move
                    });
                rs.Content = result.FirstOrDefault();
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
        public IActionResult<IEnumerable<IStatusCheckModel>> GetManifestStatusCollection(IEnumerable<Guid> TicketUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IStatusCheckModel>>();
            try
            {
                List<StatusCheckInnerModel> collection = new List<StatusCheckInnerModel>();
                #region old method
                //         var query = @"  SELECT DISTINCT
                //                     [WTI2].UID	 AS 'TicketInfoUID',
                //                     [WTI2].Status AS 'TicketInfoStatus',
                //                     [WT2].UID	 AS 'TicketUID',
                //                     [WT2].Status  AS 'TicketStatus',
                //[WWOPL].UID    AS 'WorkOrderPayloadUID',
                //                     [WWOPL].Status AS 'WorkOrderPayloadStatus',
                //[WWOP].UID    AS 'WorkOrderPodUID',
                //                     [WWOP].Status AS 'WorkOrderPodStatus',
                //                     [WWO2].UID    AS 'WorkOrderUID',
                //                     [WWO2].Status AS 'WorkOrderStatus',
                //                     [WV].UID     AS 'VesselUID',
                //                     [WV].Status  AS 'VesselStatus',
                //                     [WB].UID     AS 'BolUID',
                //                     [WB].Status  AS 'BolStatus',
                //                     [WM].UID     AS 'ManifestUID',
                //                     [WM].Status  AS 'ManifestStatus'
                //                     FROM [WMS_Ticket] AS [WT]
                //                     INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].TicketUID=[WT].UID AND [WTI].Status>0
                //                     INNER JOIN [WMS_WorkOrder] AS [WWO] ON [WT].WorkOrderUID=[WWO].UID AND [WWO].Status>0
                //INNER JOIN [WMS_WorkOrder] AS [WWO2] ON [WWO2].UID=[WT].WorkOrderUID AND [WWO2].Status>0
                //INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].WorkOrderUID=[WWO2].UID AND [WWOP].Status>0
                //INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOPL].Status>0
                //                     INNER JOIN [WMS_Ticket] AS [WT2] ON [WT2].WorkOrderUID=[WWO2].UID AND [WT2].Status>0
                //                     INNER JOIN [WMS_TicketInfo] AS [WTI2] ON [WTI2].TicketUID=[WT2].UID AND [WTI2].Status>0
                //                     LEFT JOIN [WMS_Manifest] AS [WM] ON [WWO].ManifestUID=[WM].UID AND [WM].Status>0
                //                     LEFT JOIN [WMS_Bol] AS [WB] ON [WB].ManifestUID=[WM].UID AND [WB].Status>0
                //                     LEFT JOIN [WMS_Vessel] AS [WV] ON [WV].BolUID=[WB].UID AND [WV].Status>0
                //                     WHERE [WT].UID=@TicketUID  
                //                     --order by [WWO].UID,[WV].UID,[WB].UID,[WM].UID ";
                #endregion
                var query = @"
--Outbound /Outbound Move/ Receiving
SELECT 
    [WM].RefNo,
    [WV].UID     AS 'VesselUID',
    [WV].Status  AS 'VesselStatus',
    [WB].UID     AS 'BolUID',
    [WB].Status  AS 'BolStatus',
    [WM].UID     AS 'ManifestUID',
    [WM].Status  AS 'ManifestStatus',
	[WWOPL].UID    AS 'WorkOrderPayloadUID',
    [WWOPL].Status AS 'WorkOrderPayloadStatus',
	[WWOP].UID    AS 'WorkOrderPodUID',
    [WWOP].Status AS 'WorkOrderPodStatus',
    [WWO2].UID    AS 'WorkOrderUID',
    [WWO2].Status AS 'WorkOrderStatus',
	[WT].UID	 AS 'TicketUID',
    [WT].Status  AS 'TicketStatus',
	[WTI].UID	 AS 'TicketInfoUID',
    [WTI].Status AS 'TicketInfoStatus'
FROM WMS_Manifest AS [WM]
INNER JOIN WMS_BOL AS [WB] ON [WM].UID=[WB].ManifestUID AND [WB].Status>0
INNER JOIN WMS_Vessel AS [WV] ON [WV].BolUID=[WB].UID AND [WV].Status>0
INNER JOIN [WMS_WorkOrder] AS [WWO2] ON [WWO2].VesselUID=[WV].UID AND [WWO2].Status>0
INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].WorkOrderUID=[WWO2].UID AND [WWOP].Status>0
INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOPL].Status>0
INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID AND [WTI].Status>0 
INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID AND [WT].Status>0 
WHERE [WM].UID IN (
SELECT distinct [WWO].ManifestUID FROM WMS_Ticket AS [WT]
INNER JOIN [WMS_WorkOrder] AS [WWO] ON [WWO].UID=[WT].WorkOrderUID AND [WWO].Status>0
 WHERE [WT].UID IN @TicketUID
 ) AND [WM].Status>0
UNION ALL
--Inbound Move
SELECT 
    [WM].RefNo,
    [WV].UID     AS 'VesselUID',
    [WV].Status  AS 'VesselStatus',
    [WB].UID     AS 'BolUID',
    [WB].Status  AS 'BolStatus',
    [WM].UID     AS 'ManifestUID',
    [WM].Status  AS 'ManifestStatus',
	[WWOPL].UID    AS 'WorkOrderPayloadUID',
    [WWOPL].Status AS 'WorkOrderPayloadStatus',
	[WWOP].UID    AS 'WorkOrderPodUID',
    [WWOP].Status AS 'WorkOrderPodStatus',
    [WWO2].UID    AS 'WorkOrderUID',
    [WWO2].Status AS 'WorkOrderStatus',
	[WT].UID	 AS 'TicketUID',
    [WT].Status  AS 'TicketStatus',
	[WTI].UID	 AS 'TicketInfoUID',
    [WTI].Status AS 'TicketInfoStatus'
FROM WMS_Manifest AS [WM]
INNER JOIN WMS_BOL AS [WB] ON [WM].UID=[WB].ManifestUID AND [WB].Status>0
INNER JOIN WMS_Vessel AS [WV] ON [WV].BolUID=[WB].UID AND [WV].Status>0
INNER JOIN [WMS_WorkOrder] AS [WWO2] ON [WWO2].VesselUID=[WV].UID AND [WWO2].Status>0
INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].WorkOrderUID=[WWO2].UID AND [WWOP].Status>0
INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOPL].Status>0
INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].WorkOrderPodUID=[WWOP].UID AND [WTI].Status>0 
INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID AND [WT].Status>0 
WHERE [WM].UID IN (
SELECT distinct [WWO].ManifestUID FROM WMS_Ticket AS [WT]
INNER JOIN [WMS_WorkOrder] AS [WWO] ON [WWO].UID=[WT].WorkOrderUID AND [WWO].Status>0
 WHERE [WT].UID IN @TicketUID
 ) AND [WM].Status>0
 UNION ALL
 --Warhouse move
 SELECT 
    '' RefNo,
    '00000000-0000-0000-0000-000000000000'  AS 'VesselUID',
    0  AS 'VesselStatus',
    '00000000-0000-0000-0000-000000000000'  AS 'BolUID',
    0  AS 'BolStatus',
    '00000000-0000-0000-0000-000000000000'  AS 'ManifestUID',
    0  AS 'ManifestStatus',
	[WWOPL].UID    AS 'WorkOrderPayloadUID',
    [WWOPL].Status AS 'WorkOrderPayloadStatus',
	[WWOP].UID    AS 'WorkOrderPodUID',
    [WWOP].Status AS 'WorkOrderPodStatus',
    [WWO2].UID    AS 'WorkOrderUID',
    [WWO2].Status AS 'WorkOrderStatus',
	[WT].UID	 AS 'TicketUID',
    [WT].Status  AS 'TicketStatus',
	[WTI].UID	 AS 'TicketInfoUID',
    [WTI].Status AS 'TicketInfoStatus'
FROM  [WMS_WorkOrder] AS [WWO2]
INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].WorkOrderUID=[WWO2].UID AND [WWOP].Status>0
INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID AND [WWOPL].Status>0
INNER JOIN [WMS_TicketInfo] AS [WTI] ON [WTI].WorkOrderPayloadUID=[WWOPL].UID AND [WTI].Status>0 
INNER JOIN [WMS_Ticket] AS [WT] ON [WT].UID=[WTI].TicketUID AND [WT].Status>0 
WHERE [WWO2].UID IN (
SELECT distinct [WWO].UID FROM WMS_Ticket AS [WT]
INNER JOIN [WMS_WorkOrder] AS [WWO] ON [WWO].UID=[WT].WorkOrderUID AND [WWO].Status>0
 WHERE [WT].UID IN @TicketUID
 ) AND [WWO2].ManifestUID ='00000000-0000-0000-0000-000000000000' AND [WWO2].Status>0
";

                var index = 0;
                var grp = TicketUID.GroupBy(g => index++ / 1000);
                foreach (var items in grp)
                {
                    collection.AddRange(this._Handler.Instance.Query<StatusCheckInnerModel>(query,
                        new { TicketUID = items }));
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

        public IActionResult<IEnumerable<ITicketSummaryViewModel>> GetTicketSummaryData(Guid TicketUID, Guid[] groupIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketSummaryViewModel>>();
            try
            {
                //TicketSummaryViewInnerModel
                string query = @"SELECT DISTINCT [WTI].UID, [WT].Type 'TicketType',[WWOPL].ItemUID,[WWOPL].PackageUID AS 'PackageUID'
                    ,[WTI].EstQty,[WTI].ActQty,[WTI].ShtQty,[WTI].SavQty from WMS_Ticket AS [WT]
                    INNER JOIN WMS_TicketInfo AS [WTI] ON [WT].UID=[WTI].TicketUID
                    INNER JOIN WMS_TicketInfo_Assignee_Relation AS [WTAR] ON [WTAR].TicketInfoUID=[WTI].UID
                    INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WTI].WorkOrderPodUID
                    INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].WorkOrderPodUID=[WWOP].UID
                    WHERE [WT].UID=@TicketUID AND [WTAR].GroupUID IN @groupIds and [WTAR].Status>0
                    and [WT].Status>0 and [WTI].Status>0 and [WWOP].Status>0  and [WWOPL].Status>0
                    UNION ALL
                    SELECT DISTINCT [WTI].UID,  [WT].Type 'TicketType',[WWOPL].ItemUID,[WWOPL].PackageUID AS 'PackageUID'
                    ,[WTI].EstQty,[WTI].ActQty,[WTI].ShtQty,[WTI].SavQty from WMS_Ticket AS [WT]
                    INNER JOIN WMS_TicketInfo AS [WTI] ON [WT].UID=[WTI].TicketUID
                    INNER JOIN WMS_TicketInfo_Assignee_Relation AS [WTAR] ON [WTAR].TicketInfoUID=[WTI].UID
                    INNER JOIN [WMS_WorkOrder_Payload] AS [WWOPL] ON [WWOPL].UID=[WTI].WorkOrderPayloadUID
                    INNER JOIN [WMS_WorkOrder_Pod] AS [WWOP] ON [WWOP].UID=[WWOPL].WorkOrderPodUID
                    WHERE [WT].UID=@TicketUID AND [WTAR].GroupUID IN @groupIds  and [WTAR].Status>0
                    and [WT].Status>0 and [WTI].Status>0 and [WWOP].Status>0  and [WWOPL].Status>0";
                rs.Content = this._Handler.Instance.Query<TicketSummaryViewInnerModel>(query,
                                new { TicketUID = TicketUID, groupIds = groupIDs });
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
        public IActionResult<IEnumerable<IDeallocatedInfoDataModel>> GetDeallocatedInfoList(IEnumerable<Guid> workorderguids)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IDeallocatedInfoDataModel>>();
            try
            {
                var query = @"
                            SELECT  
                             [WWO].UID WorkOrderUID,[WWo].VesselUID,
                             [WWOPL].UID WorkOrderPayloadUID,[WWOP].UID WorkOrderPodUID,[WTI].UID TicketInfoUID,[WTI].TicketUID
                             FROM WMS_WorkOrder AS [WWO]
                             INNER JOIN WMS_WorkOrder_Pod AS [WWOP] ON [WWOP].WorkOrderUID=[WWO].UID
                             INNER JOIN WMS_WorkOrder_Payload AS [WWOPL] ON [WWO].UID=[WWOPL].WorkOrderUID AND [WWOPL].Status>0
                             INNER JOIN WMS_Ticket AS [WT] ON [WT].WorkOrderUID=[WWO].UID AND [WT].Status>0
                             INNER JOIN WMS_TicketInfo AS [WTI] ON [WTI].TicketUID=[WT].UID AND [WTI].Status>0
                            WHERE [WWO].UID IN @WorkorderUID AND WHERE [WWO].Status>0";
                rs.Content = this._Handler.Instance.Query<DeallocatedInfoDataInnerModel>(query, new { WorkorderUID = workorderguids });
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
        public IActionResult<IEnumerable<ITicketModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition)
                                .Where(p => p.Status > (int)TicketStatus.Void);
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

        public IActionResult<IEnumerable<ITicketModel>> GetTicketByBol(Guid boluid)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketModel>>();
            try
            {
                var query = @"SELECT [WT].*
                FROM [WMS_BOL] AS [WB]
                INNER JOIN [WMS_Vessel] AS [WV] ON [WB].UID=[WV].BolUID
                INNER JOIN [WMS_WorkOrder] AS [WWO] ON [WWO].VesselUID=[WV].UID
                INNER JOIN [WMS_Ticket] AS [WT] ON [WT].WorkOrderUID=[WWO].UID
                WHERE  [WB].UID=@BOLUID AND [WB].Status>0 AND [WV].Status>0 AND [WWO].Status>0";
                rs.Content = this._Handler.Instance.Query<TicketInnerModel>(query, new { BOLUID = boluid });
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
        public IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoListByWorkOrderUID(Guid workorderUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                var query = @"SELECT [WT].Name,[WT].ID,[WTI].UID
                FROM  [WMS_WorkOrder] AS [WWO]
                INNER JOIN [WMS_Ticket] as [WT] on [WWO].UID=[WT].WorkOrderUID
                INNER JOIN [WMS_TicketInfo] as [WTI] on [WT].UID=[WTI].TicketUID
                WHERE  [WWO].UID=@workorderUID AND [WT].Status>0 AND [WTI].Status>0 ";
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query, new { workorderUID = workorderUID });
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
        public IActionResult<IEnumerable<ITicketInfoModel>> GetTicketInfoList(Guid bolUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ITicketInfoModel>>();
            try
            {
                var query = @"SELECT [WT].Name,[WT].ID,[WTI].UID
                FROM  [WMS_WorkOrder] AS [WWO]
                INNER JOIN [WMS_Vessel] AS [WV] on [WWO].VesselUID=[WV].UID
                INNER JOIN [WMS_Ticket] as [WT] on [WWO].UID=[WT].WorkOrderUID
                INNER JOIN [WMS_TicketInfo] as [WTI] on [WT].UID=[WTI].TicketUID
                WHERE  [WV].BolUID=@BOLUID AND [WV].Status>0 AND [WT].Status>0 AND [WTI].Status>0 ";
                rs.Content = this._Handler.Instance.Query<TicketInfoInnerModel>(query, new { BolUID = bolUID });
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
        public IActionResult<IEnumerable<IAssignedTicketInfoModel>> GetTicketInfoList(IEnumerable<Guid> bolUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IAssignedTicketInfoModel>>();
            try
            {
                var query = @"SELECT [WV].BolUID ,[WT].Name,[WT].ID,[WTI].UID
                FROM  [WMS_WorkOrder] AS [WWO]
                INNER JOIN [WMS_Vessel] AS [WV] on [WWO].VesselUID=[WV].UID
                INNER JOIN [WMS_Ticket] as [WT] on [WWO].UID=[WT].WorkOrderUID
                INNER JOIN [WMS_TicketInfo] as [WTI] on [WT].UID=[WTI].TicketUID
                WHERE  [WV].BolUID IN @BOLUID AND [WWO].Status>0  AND [WV].Status>0 AND [WT].Status>0 AND [WTI].Status>0 AND [WWO].Status>0 ";
                rs.Content = this._Handler.Instance.Query<TicketInfoAssignedInnerModel>(query, new { BolUID = bolUID });
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

        public IActionResult<IBulkPickDataCollection> GetBulkPickOrignalData(IEnumerable<Guid> ticketInfoUIDs)
        {
            BulkPickDataCollection bulkPickDataCollection = new BulkPickDataCollection();
            var rs = ActionResultTemplates.Result<IBulkPickDataCollection>();
            rs.Content = bulkPickDataCollection;
            try
            {
                var query = @"
                SELECT  [WTI].*,[WT].WarehouseUID FROM WMS_TicketInfo AS [WTI]
                INNER JOIN WMS_Ticket AS [WT] ON [WTI].TicketUID=[WT].UID AND [WT].Status>0
                WHERE [WTI].UID IN @ticketInfoUID AND [WTI].Status>0
                ;
                SELECT DISTINCT [WWOP].* FROM WMS_TicketInfo AS [WTI]
                INNER JOIN WMS_WorkOrder_Payload AS [WWOP] ON [WTI].WorkOrderPayloadUID=[WWOP].UID AND [WWOP].Status>0
                WHERE [WTI].UID IN @ticketInfoUID AND [WTI].Status>0";
                var rsResult = this._Handler.Instance.QueryMultiple(query, new { ticketInfoUID = ticketInfoUIDs });
                bulkPickDataCollection.TicketInfoCollection = rsResult.Read<BulkPickTicketInfoModel>();
                bulkPickDataCollection.WorderPayloadCollection = rsResult.Read<WorkOrderPayloadInnerModel>();
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

        public IActionResult<ITicketModel> GetTicketByBulkPick(Guid bulkPickUID)
        {
            var rs = ActionResultTemplates.Result<ITicketModel>();
            try
            {
                var query = @"
                SELECT [WT].* FROM WMS_BulkPick AS [BP]
                INNER JOIN WMS_Ticket AS [WT] ON [BP].TicketUID=[WT].UID
                WHERE [BP].UID=@BulkPickUID
                ";
                rs.Content = this._Handler.Instance.Query<TicketInnerModel>(query, new { BulkPickUID = bulkPickUID })
                    .FirstOrDefault();
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
