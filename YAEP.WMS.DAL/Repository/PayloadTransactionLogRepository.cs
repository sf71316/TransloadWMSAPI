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
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class PayloadTransactionLogRepository<T> : AbstractRepository<T>, IPayloadTransactionLogRepository where T : class, IPayloadTransactionLogModel
    {
        public PayloadTransactionLogRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> BatchAddLog(IEnumerable<IPayloadTransactionLogModel> Models)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"
                
INSERT INTO [dbo].[WMS_Payload_TransactionLog]
           ([UID]
           ,[WarehouseUID]
           ,[WorkOrderPodUID]
           ,[WorkOrderPayloadUID]
           ,[ItemUID]
           ,[PayloadUID]
           ,[QtyBeforeTX]
           ,[QtyAfterTX]
           ,[OriginalPackage]
           ,[OriginalSlotUID]
           ,[TargetSlotUID]
           ,[TargetPackage]
           ,[TicketInfoUID]
           ,[Type]
           ,[Status]
           ,[Description]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
     VALUES
           (
		   @UID,
           @WarehouseUID, 
           @WorkOrderPodUID,
           @WorkOrderPayloadUID, 
           @ItemUID,
           @PayloadUID,
           @QtyBeforeTX,
           @QtyAfterTX,
           @OriginalPackage,
           @OriginalSlotUID,
           @TargetSlotUID,
           @TargetPackage,
           @TicketInfoUID,
           @Type,
           @Status, 
           @Description,
           @CreatedBy,
           @CreatedOn, 
           @ModifiedBy,
           @ModifiedOn )
                ";

                rs.Content = true;
                var index = 0;
                var grp = Models.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query,
                        items) > 0;
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
        public IActionResult<bool> AddLog(IPayloadTransactionLogModel Model)
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

        public IActionResult<IEnumerable<IPayloadTransactionLogViewModel>> GetTranascationList(IPayloadTransactionLogParameters parameters)
        {
            string query =
@"
            SELECT  
			--DISTINCT
            WM.RefNo,
			WMS_Warehouse.ID AS WarehouseID, 
			WMS_Warehouse.Name AS WarehouseName, 
			WMS_Area.Name AS TargetAreaName, 
			WMS_Bin.Name AS TargetBinName, 
			WMS_Slot.Name AS TargetSlotName, 
			OrignalArea.Name AS OriginalAreaName,
			OrignalBin.Name AS OriginalBinName, 
			OrignalSlot.Name AS OriginalSlotName,
			WMS_Vessel.RefNo AS ReceivedVesselRefNo, 
			BOL.Name AS BolNo,
			Vessel.Name AS VesselNo,
			Ticket.ID AS TicketID,
            WMS_Payload.ModifiedOn AS PayloadModifiedOn,
            WMS_Payload.Type AS PayloadType,
		    WMS_Payload_TransactionLog.*
            FROM WMS_Payload_TransactionLog
		    INNER JOIN WMS_Warehouse ON WMS_Warehouse.UID = WMS_Payload_TransactionLog.WarehouseUID 
		    INNER JOIN WMS_Slot ON WMS_Slot.UID = WMS_Payload_TransactionLog.TargetSlotUID
		    INNER JOIN WMS_Area ON WMS_Area.UID = WMS_Slot.AreaUID 
		    INNER JOIN WMS_Bin ON WMS_Bin.UID = WMS_Slot.BinUID 
			LEFT JOIN WMS_Slot AS OrignalSlot ON OrignalSlot.UID = WMS_Payload_TransactionLog.OriginalSlotUID AND WMS_Payload_TransactionLog.OriginalSlotUID IS NOT NULL
		    LEFT JOIN WMS_Area AS OrignalArea ON OrignalArea.UID = OrignalSlot.AreaUID 
		    LEFT JOIN WMS_Bin AS OrignalBin ON OrignalBin.UID = OrignalSlot.BinUID 
		    LEFT JOIN WMS_Payload ON WMS_Payload.UID = WMS_Payload_TransactionLog.PayloadUID
			LEFT JOIN WMS_Vessel ON WMS_Vessel.UID = WMS_Payload.VesselUID AND WMS_Vessel.Status>0
			LEFT JOIN WMS_WorkOrder_Payload AS wPayload ON WMS_Payload.UID=wPayload.PayloadUID AND wPayload.Status>0
			LEFT JOIN WMS_WorkOrder AS wOrder ON wOrder.UID=wPayload.WorkOrderUID AND wOrder.Status>0
			LEFT JOIN WMS_Manifest AS WM ON wOrder.ManifestUID=WM.UID AND WM.Status>0
		    LEFT JOIN WMS_Vessel AS Vessel ON Vessel.UID=wOrder.VesselUID AND Vessel.Status>0
			LEFT JOIN WMS_BOL AS BOL ON BOL.UID=Vessel.BolUID AND Bol.Status>0
            LEFT JOIN WMS_TicketInfo AS TicketInfo ON TicketInfo.UID=WMS_Payload_TransactionLog.TicketInfoUID AND TicketInfo.Status>0
            LEFT JOIN WMS_Ticket AS Ticket ON TicketInfo.TicketUID=Ticket.UID AND Ticket.Status>0
            
            ";
            List<string> conditions = new List<string>();
            conditions.Add($" (WMS_Payload_TransactionLog.Status = {(int)PayloadTransactionLogStatus.Active}) ");
            if (parameters != null)
            {
                if (parameters.LogTypes != null && (parameters.LogTypes.Length > 0))
                {
                    conditions.Add(" (WMS_Payload_TransactionLog.Type IN @LogTypes ) ");
                }
                if (parameters.PayloadTypes != null && parameters.PayloadTypes.Count() > 0)
                {
                    conditions.Add(" (WMS_Payload.Type IN @PayloadTypes ) ");
                }
                if (parameters.WarehouseUID.HasValue)
                {
                    conditions.Add(" (WMS_Payload_TransactionLog.WarehouseUID = @WarehouseUID) ");
                }
                if (parameters.TargetArea.HasValue)
                {
                    conditions.Add(" (WMS_Area.UID = @TargetArea) ");
                }
                if (parameters.TargetBin.HasValue)
                {
                    conditions.Add(" (WMS_Bin.UID = @TargetBin) ");
                }
                if (parameters.TargetSlot.HasValue)
                {
                    conditions.Add(" (WMS_Slot.UID = @TargetSlot) ");
                }
                if (parameters.OriginalArea.HasValue)
                {
                    conditions.Add(" (OrignalArea.UID = @OriginalArea) ");
                }
                if (parameters.OriginalBin.HasValue)
                {
                    conditions.Add(" (OrignalBin.UID = @OriginalBin) ");
                }
                if (parameters.OriginalSlot.HasValue)
                {
                    conditions.Add(" (OrignalSlot.UID = @OriginalSlot) ");
                }
                if (parameters.LogStartDate.HasValue)
                {
                    conditions.Add(" (WMS_Payload_TransactionLog.CreatedOn >= @LogStartDate) ");
                }
                if (parameters.LogEndDate.HasValue)
                {
                    parameters.LogEndDate = parameters.LogEndDate.Value.AddDays(1).AddSeconds(-1);
                    conditions.Add(" (WMS_Payload_TransactionLog.CreatedOn <= @LogEndDate) ");
                }
                if (parameters.ItemUIDs != null && parameters.ItemUIDs.Count() > 0)
                {
                    conditions.Add(" (WMS_Payload_TransactionLog.ItemUID IN @ItemUIDs) ");
                }
                if (!string.IsNullOrEmpty(parameters.VesselNo))
                {
                    conditions.Add(" (Vessel.Name LIKE  '%'+@VesselNo+'%') ");
                }
                if (!string.IsNullOrEmpty(parameters.BolNo))
                {
                    conditions.Add(" (BOL.Name LIKE '%'+@BolNo+'%') ");
                }
                if (!string.IsNullOrEmpty(parameters.TicketID))
                {
                    conditions.Add(" (TicketInfo.ID =  @TicketInfo) ");
                }
                if (!string.IsNullOrEmpty(parameters.RefNo))
                {
                    conditions.Add(" (WM.RefNo =  @RefNo) ");
                }
                if (!String.IsNullOrWhiteSpace(parameters.VesselRefNo))
                {
                    conditions.Add(" (WMS_Vessel.RefNo LIKE @VesselRefNo + '%') ");
                }

            }

            query += conditions.Count > 0 ? " WHERE " + String.Join(" AND ", conditions) : "";

            var resultContainer = ActionResultTemplates.Result<IEnumerable<IPayloadTransactionLogViewModel>>();

            try
            {
                var collection = this._Handler.Instance.Query<PayloadTransactionLogViewModel>(query, parameters);

                if (collection?.Count() > 0)
                {
                    foreach (var item in collection)
                    {
                        if (item.PayloadType.HasValue)
                        {
                            item.PayloadTypeName = ((PayloadType)item.PayloadType).ToString();
                        }
                    }
                    resultContainer.Success = true;
                    resultContainer.Content = collection;
                }
                else
                {
                    resultContainer.Message = "Not Found.";
                }
            }
            catch (Exception ex)
            {
                resultContainer.Message = ex.Message;
                resultContainer.InnerException = ex;
                resultContainer.TypeCode = 500;
                this.OnExpcetion(ex);
            }

            return resultContainer;
        }
    }
}
