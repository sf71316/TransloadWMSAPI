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
using YAEP.WMS.DAL.Extension;
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;


namespace YAEP.WMS.DAL.Repository
{
    public class InventroyRepository<T> : AbstractRepository<T>, IInventoryRepository where T : class, IInventoryModel
    {
        ILabelRepository _LabelRepository;
        public InventroyRepository(IRepositoryHandler<T> handler, ILabelRepository labelRepository) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;
            _LabelRepository = labelRepository;
        }

        public IActionResult<bool> AddInventory(IInventoryModel Model)
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
        public IActionResult<bool> BatchAddInventory(IEnumerable<IInventoryModel> Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = @"INSERT INTO [dbo].[WMS_Inventory]
           (
           [SlotUID]
           ,[PackageUID]
           ,[WarehouseUID]
           ,[Type]
           ,[Status]
           ,[ItemUID]
           ,[Qty]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
            VALUES
           (@SlotUID,@PackageUID,@WarehouseUID,@Type, 
           @Status,@ItemUID,@Qty,@CreatedBy,@CreatedOn,@ModifiedBy,@ModifiedOn)";
                var index = 0;
                var grp = Model.GroupBy(g => index++ / 2000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, items) > 0;
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
        public IActionResult<bool> DeleteInventory(Guid InventoryUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(new { UID = InventoryUID });
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
        public IActionResult<bool> DeleteInventory(IEnumerable<Guid> InventoryUID)
        {
            var rs = ActionResultTemplates.Result<bool>();

            rs.Content = true;
            //rs.Content = this._Handler.DeleteByDynamicConditions(new { UID = InventoryUID });
            //rs.Success = rs.Content;
            var query = @"Update WMS_INVENTORY SET Status=0,ModifiedOn=getdate() WHERE UID IN @InventoryUID";
            var index = 0;
            var grp = InventoryUID.GroupBy(g => index++ / 2000);
            foreach (var items in grp)
            {
                rs.Content &= this._Handler.Instance.Execute(query, new { InventoryUID = items }) > 0;
            }
            rs.Success = rs.Content;

            return rs;
        }
        public IActionResult<bool> EditInventory(IInventoryModel Model)
        {
            this._Handler.IsAllUpdate = false;

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(new { Qty = Model.Qty, Status = Model.Status }, new { UID = Model.UID });
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

        public IActionResult<IEnumerable<IInventoryViewModel>> GetInventory(IInventorySearchParameters parameters)
        {
            List<string> conditions = new List<string>();

            if (parameters != null)
            {
                if (parameters.PHierarchy?.Count() > 0)
                {
                    conditions.Add(" (ItemUID IN @PHierarchy) ");
                }
                if (parameters.WarehouseUID.HasValue)
                {
                    conditions.Add(" (WarehouseUID = @WarehouseUID) ");
                }
            }
            conditions.Add(" (Status >0) ");
            string where = conditions.Count > 0 ? " WHERE " + String.Join(" AND ", conditions) : "";

            string query =
$@"
SELECT 
		    WMS_Warehouse.ID AS WarehouseID, 
		    WMS_Warehouse.Name AS WarehouseName,   
		    Inventory.* 
FROM 
(
	SELECT WarehouseUID, ItemUID, PackageUID, Type, SUM(Qty) AS InboundQty 
	FROM WMS_Inventory 
    {where}
	GROUP BY WarehouseUID, ItemUID, PackageUID, Type 
) AS Inventory
		INNER JOIN WMS_Warehouse ON WMS_Warehouse.UID = Inventory.WarehouseUID   
";

            var resultContainer = ActionResultTemplates.Result<IEnumerable<IInventoryViewModel>>();

            try
            {
                var collection = this._Handler.Instance.Query<InventoryViewModel>(query, parameters);

                if (collection?.Count() > 0)
                {
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

        public IActionResult<IEnumerable<IInventoryDetailViewModel>> GetInventoryDetail(Guid itemUID)
        {
            string query =
@"
SELECT  
--DISTINCT
	    WMS_Warehouse.ID AS WarehouseID,
	    WMS_Warehouse.Name AS WarehouseName,
	    WMS_Manifest.ID AS ManifestID, 
	    WMS_BOL.ID AS BolRef, 
	    WMS_Vessel.ID AS VesselRef, 
	    WMS_Inventory.*
FROM WMS_Inventory
			INNER JOIN WMS_Payload ON WMS_Payload.SlotUID=WMS_Inventory.SlotUID AND WMS_Payload.ItemUID=WMS_Inventory.ItemUID
		    INNER JOIN WMS_Warehouse ON WMS_Warehouse.UID = WMS_Inventory.WarehouseUID 
		    INNER JOIN WMS_Vessel ON WMS_Vessel.UID = WMS_Payload.VesselUID
		    INNER JOIN WMS_BOL ON WMS_BOL.UID = WMS_Vessel.BolUID 
		    INNER JOIN WMS_Manifest ON WMS_Manifest.UID = WMS_BOL.ManifestUID    
WHERE   (WMS_Inventory.ItemUID = @ItemUID) 
AND (WMS_Inventory.Status>0) AND (WMS_Payload.Status>0) AND(WMS_Warehouse.Status>0 ) AND(WMS_Vessel.Status>0 )
AND(WMS_BOL.Status>0 ) AND(WMS_Manifest.Status>0 )
AND WMS_Payload.Type=1
";

            var resultContainer = ActionResultTemplates.Result<IEnumerable<IInventoryDetailViewModel>>();

            try
            {
                var collection = this._Handler.Instance.Query<InventoryDetailViewModel>(query, new { ItemUID = itemUID });

                if (collection?.Count() > 0)
                {
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

        public IActionResult<IInventoryModel> GetInventory(Guid warehouseUID, Guid itemUID, Guid packageUID, Guid slotUID)
        {

            var rs = ActionResultTemplates.Result<IInventoryModel>();
            try
            {
                var query = @"SELECT * FROM WMS_INVENTORY WHERE WarehouseUID=@WarehouseUID
                              AND ItemUID=@ItemUID AND PackageUID=@PackageUID AND SlotUID=@SlotUID AND Status>0";
                rs.Content = this._Handler.Instance.Query<InventoryInnerModel>(query, new
                {
                    WarehouseUID = warehouseUID,
                    ItemUID = itemUID,
                    PackageUID = packageUID,
                    SlotUID = slotUID
                }).FirstOrDefault();

                rs.Success = rs.Content != null;
                if (!rs.Success)
                {
                    rs.Success = false;
                    rs.Message = "not find onhand";
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

        public IActionResult<bool> IsItemInSlot(Guid warehouseUID, Guid itemUID, Guid packageUID, Guid slotUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this.GetInventory(warehouseUID, itemUID, packageUID, slotUID).Success;
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

        public IActionResult<IEnumerable<IAvailableInventoryModel>> GeteAvailableInventoryList(IGetAvailableInventoryParameters parameters)
        {
            string query =
@"
    SELECT [WW].Name 'WarehouseName', [WA].Name 'AreaName',[WBin].Name 'BinName',[WS].Name 'SlotName',
[WPL].*,[WV].RefNo 'VesselRefNo',[WB].RefNo 'BolRefNo',[WS].Name 'SlotName',[WP].Name 'PodName'
FROM WMS_Payload AS [WPL]
LEFT JOIN WMS_Pod AS [WP] on [WP].UID=[WPL].PodUID  
INNER JOIN WMS_Vessel AS [WV] ON [WPL].VesselUID=[WV].UID
INNER JOIN WMS_BOL AS [WB] ON [WB].UID=[WV].BolUID
INNER JOIN WMS_Slot AS[WS] ON [WS].UID=[WPL].SlotUID
INNER JOIN WMS_Bin AS [WBin] ON [WBin].UID=[WS].BinUID
INNER JOIN WMS_Area AS [WA] ON [WA].UID=[WBin].AreaUID
INNER JOIN WMS_Warehouse [WW] ON [WA].WarehouseUID=[WW].UID
WHERE [WPL].Status>0 AND [WV].Status>0 AND [WW].Status>0 AND 
      [WB].Status>0 AND [WS].Status>0 AND [WBin].Status>0 AND [WA].Status>0 AND [WP].Status>0
";

            List<string> conditions = new List<string>();
            conditions.Add(" ([WPL].Type=1) ");
            if (parameters != null)
            {
                if (parameters.Warehouse.HasValue && parameters.Warehouse != Guid.Empty)
                {
                    conditions.Add(" ([WA].WarehouseUID=@WarehouseUID) ");
                }
                if (parameters.AreaUID.HasValue && parameters.AreaUID != Guid.Empty)
                {
                    conditions.Add(" ([WA].UID=@AreaUID) ");
                }
                if (parameters.BinUID.HasValue && parameters.BinUID != Guid.Empty)
                {
                    conditions.Add(" ([WBin].UID=@BinUID) ");
                }
                if (parameters.SlotUID.HasValue && parameters.SlotUID != Guid.Empty)
                {
                    conditions.Add(" ([WS].UID=@SlotUID) ");
                }
                if (parameters.ItemUID != Guid.Empty)
                {
                    conditions.Add(" ([WPL].ItemUID=@ItemUID) ");
                }
                if (!string.IsNullOrEmpty(parameters.Option) && (parameters.Option.ToLower() == "vesselrefno" ||
                    parameters.Option.ToLower() == "bolrefno") && !string.IsNullOrEmpty(parameters.OptionText))
                {
                    parameters.OptionText = $"%{parameters.OptionText}%";
                    if ((parameters.Option.ToLower() == "vesselrefno"))
                    {
                        conditions.Add(" ([WV].RefNo LIKE @OptionText) ");

                    }
                    else if (parameters.Option.ToLower() == "bolrefno")
                    {
                        conditions.Add(" ([WB].RefNo LIKE @OptionText) ");
                    }
                }
            }


            query += conditions.Count > 0 ? " AND " + String.Join(" AND ", conditions) : "";

            var resultContainer = ActionResultTemplates.Result<IEnumerable<IAvailableInventoryModel>>();

            try
            {
                var collection = this._Handler.Instance.Query<AvailableInventoryInnerModel>(query, parameters);

                if (collection?.Count() > 0)
                {
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

        public IActionResult<IEnumerable<ICheckOnhandModel>> GetOnhandData(Guid warhouseUID, Guid itemUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ICheckOnhandModel>>();
            try
            {
                var query = @"SELECT [WPL].ItemUID,[WPL].PackageUID,SUM([WPL].Quantity) Qty
                              FROM WMS_Payload AS [WPL]
                              LEFT JOIN WMS_Pod AS [WP] on [WP].UID=[WPL].PodUID  
                              INNER JOIN WMS_Vessel AS [WV] ON [WPL].VesselUID=[WV].UID
                              INNER JOIN WMS_BOL AS [WB] ON [WB].UID=[WV].BolUID
                              INNER JOIN WMS_Slot AS[WS] ON [WS].UID=[WPL].SlotUID
                              INNER JOIN WMS_Bin AS [WBin] ON [WBin].UID=[WS].BinUID
                              INNER JOIN WMS_Area AS [WA] ON [WA].UID=[WBin].AreaUID
                              INNER JOIN WMS_Warehouse [WW] ON [WA].WarehouseUID=[WW].UID
                              WHERE [WPL].Status=@PayloadStatus 
                              AND [WV].Status>0 AND [WW].Status>0 AND [WB].Status>0 AND [WS].Status>0
                              AND [WBin].Status>0 AND [WA].Status>0  AND [WP].Status>0
                              AND [WW].UID=@WarehouseUID AND [WPL].Type=@PayloadType AND [WPL].ItemUID=@itemUID
                              GROUP BY [WPL].ItemUID,[WPL].PackageUID";
                rs.Content = this._Handler.Instance.Query<CheckOnhandInnerModel>(query,
                    new
                    {
                        WarehouseUID = warhouseUID,
                        itemUID = itemUID,
                        PayloadStatus = (int)PayloadStatus.Active,
                        PayloadType = (int)PayloadType.Stock
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

        public IActionResult<IEnumerable<IGetModifyPayloadListModel>> GetModifyPayloadListData(IGetModifyPayloadListParameters parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IGetModifyPayloadListModel>>();
            try
            {
                var query = @"SELECT 
                [WW].UID WarehouseUID,
                [WP].ID PayloadID,
                [WP].UID PayloadUID,
                [WP].ItemUID,
                [WP].PackageUID,
                [WP].Status PayloadStatus,
                [WP].Type PayloadType,
                [WP].Quantity Qty,
                [WA].ID AreaID,
                [WB].ID BinID,
                [WS].ID SlotID,
                [WS].Status SlotStatus,
                [WS].Type SlotType,
                [WS].UID SlotUID,
                [WS].AreaUID,
                [WS].BinUID,
                [Manifest].RefNo ManifestRefNo,
                [Manifest].Name ManifestName,
                [Manifest].Name ManifestTypeName,
                [Manifest].Type ManifestType,
                [WBol].ID BolID,
                [WV].ID VesselID,
                [Pod].UID PodUID
                FROM WMS_Payload AS [WP]
                INNER JOIN WMS_Slot AS [WS] ON [WP].SlotUID=[WS].UID AND [WS].Status >0
                INNER JOIN WMS_Bin AS [WB] ON [WB].UID=[WS].BinUID AND [WB].Status >0
                INNER JOIN WMS_Area AS [WA] ON [WA].UID=[WS].AreaUID AND [WA].Status >0
                INNER JOIN WMS_Warehouse AS [WW] ON [WW].UID=[WA].WarehouseUID AND [WW].Status >0
                LEFT JOIN  WMS_Vessel [WV] ON [WV].UID=[WP].VesselUID AND [WV].Status >0
                LEFT JOIN  WMS_BOL [WBol] ON [WBol].UID=[WV].BolUID AND [WBol].Status >0
                LEFT JOIN WMS_Manifest AS [Manifest] ON [Manifest].UID=[WBol].ManifestUID AND [Manifest].Status >0
                LEFT JOIN WMS_Pod AS [Pod] ON [Pod].UID=[WP].PODUID AND [Pod].Status >0
                WHERE [WP].Status>0 {0}";
                query = string.Format(query, getModifyPayloadListDataCondition(parameters));
                if (parameters.ItemUID.Count() > 2000)
                {

                    var index = 0;
                    List<GetModifyPayloadListModel> rsb = new List<GetModifyPayloadListModel>();
                    var grp = parameters.ItemUID.GroupBy(g => index++ / 2000);
                    foreach (var gitems in grp)
                    {
                        var parm = parameters.Clone<IGetModifyPayloadListParameters>();
                        parm.ItemUID = gitems.ToArray();
                        rsb.AddRange(this._Handler.Instance.Query<GetModifyPayloadListModel>(query, parm));
                    }
                    rs.Content = rsb;
                }
                else
                {
                    rs.Content = this._Handler.Instance.Query<GetModifyPayloadListModel>(query, parameters);
                }
                if (!string.IsNullOrEmpty(parameters.PodBarcode))
                {
                    var belongtoUIDs = rs.Content.Select(x => x.PodUID);
                    var labels = this._LabelRepository.GetLabels(belongtoUIDs.ToArray());
                    if (labels.Content.Count() > 0)
                    {
                        rs.Content = rs.Content.Where(p => labels.Content
                        .Where(l => l.Content.Contains(parameters.PodBarcode))
                        .Any(x => x.BelongToUID == p.PodUID));
                    }
                    else //no label
                    {
                        rs.Content = new List<GetModifyPayloadListModel>();
                    }

                }
                foreach (var item in rs.Content)
                {
                    item.PayloadTypeName = ((PayloadType)item.PayloadType).ToString();
                    item.PayloadStatusName = ((PayloadStatus)item.PayloadStatus).ToString();
                    item.SlotStatusName = ((SlotStatus)item.SlotStatus).ToString();
                    item.SlotTypeName = ((SlotType)item.SlotType).ToString();
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
        public IActionResult<IEnumerable<IInventoryModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IInventoryModel>>();
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
        #region private method 
        private string getModifyPayloadListDataCondition(IGetModifyPayloadListParameters param)
        {
            List<string> condition = new List<string>();
            if (param.WarehouseUID.HasValue)
            {
                condition.Add("([WW].UID=@WarehouseUID)");
            }
            if (param.AreaUID.HasValue)
            {
                condition.Add("([WS].AreaUID=@AreaUID)");
            }
            if (param.BinUID.HasValue)
            {
                condition.Add("([WS].BinUID=@BinUID)");
            }
            if (param.SlotUID.HasValue)
            {
                condition.Add("([WS].UID=@SlotUID)");
            }
            if (!string.IsNullOrEmpty(param.ManifestRefNo))
            {
                condition.Add("([Manifest].RefNo=@ManifestRefNo)");
            }
            if (!string.IsNullOrEmpty(param.ManifestName))
            {
                condition.Add("([Manifest].Name=@ManifestName)");
            }
            if (!string.IsNullOrEmpty(param.ManifestType))
            {
                condition.Add("([Manifest].Type=@ManifestType)");
            }
            if (param.ItemUID != null && param.ItemUID.Count() > 0)
            {
                condition.Add("([WP].ItemUID IN @ItemUID)");
            }
            if (param.PayloadStatus == null || (param.PayloadStatus != null && param.PayloadStatus.Length > 0))
            {
                condition.Add("([WP].Status IN @PayloadStatus)");
            }
            if (param.PayloadType != null && param.PayloadType.Length > 0)
            {
                condition.Add("([WP].Type IN @PayloadType)");
            }
            //else
            //{
            //    condition.Add("([WP].Type = 1)");
            //}
            if (condition.Count() > 0)
            {
                return "AND" + string.Join("AND", condition);
            }

            return string.Empty;
        }

        public IActionResult<int> GetItemUsageStatus(Guid ItemUID)
        {
            var rs = ActionResultTemplates.Result<int>();
            try
            {
                var query = @"
SELECT 
[ItemUID],
SUM([TicketUsageCount]) AS [TicketUsageCount],
SUM([TicketOnHand]) AS [TicketOnHand],
SUM([NonTicketOnHand]) AS [NonTicketOnHand],
SUM([OnHand]) AS [OnHand]
FROM (
	SELECT 		
		[WMS_WorkOrder_Payload].[ItemUID],
		COUNT(*) AS [TicketUsageCount],
		SUM([WMS_WorkOrder_Payload].[Qty]) AS [TicketOnHand],
		0 AS [NonTicketOnHand],
		SUM([WMS_WorkOrder_Payload].[Qty]) AS [OnHand]
		FROM  [WMS_WorkOrder_Payload]
		INNER JOIN [WMS_WorkOrder] ON [WMS_WorkOrder].[UID] = [WMS_WorkOrder_Payload].[WorkOrderUID]
		INNER JOIN [WMS_Manifest] ON [WMS_Manifest].[UID]=[WMS_WorkOrder].[ManifestUID]
	WHERE 
		[WMS_WorkOrder_Payload].[Status]>0 AND [WMS_WorkOrder].[Status]>0 AND [WMS_Manifest].[Status]>0
		AND [WMS_WorkOrder_Payload].[ItemUID] = '{0}'
	GROUP BY [WMS_WorkOrder_Payload].[ItemUID]

	UNION ALL

	SELECT  
		[WMS_PayLoad].[ItemUID],
		0 AS [TicketUsageCount],
		0 AS [TicketOnHand],
		SUM([WMS_PayLoad].[Quantity]) AS [NonTicketOnHand],
		SUM([WMS_PayLoad].[Quantity]) AS [OnHand]
	FROM [WMS_PayLoad] 
	WHERE [VesselUID]='00000000-0000-0000-0000-000000000000' 
		AND [WMS_PayLoad].[Status]>0
		AND [WMS_PayLoad].[ItemUID] = '{0}'
	GROUP BY [ItemUID]
) AS [CheckOnHand] 
GROUP BY [ItemUID]
";
                query = string.Format(query, ItemUID);
                CheckItemUsageStatusInnerModel item_status = this._Handler.Instance.Query<CheckItemUsageStatusInnerModel>(query).FirstOrDefault();
                if (item_status != null)
                {
                    if (item_status.TicketUsageCount.Equals(0))
                    {
                        if (item_status.NonTicketOnHand.Equals(0))
                        {
                            rs.Message = ItemStorageStatus.Noneonahnd_Unused.ToString();
                            rs.Content = (int)ItemStorageStatus.Noneonahnd_Unused;
                        }
                        else
                        {
                            rs.Message = ItemStorageStatus.HadOnahnd_Unused.ToString();
                            rs.Content = (int)ItemStorageStatus.HadOnahnd_Unused;
                        }
                    }
                    else
                    {
                        if (item_status.TicketOnHand.Equals(0))
                        {
                            rs.Message = ItemStorageStatus.Noneonahnd_Used.ToString();
                            rs.Content = (int)ItemStorageStatus.Noneonahnd_Used;
                        }
                        else
                        {
                            rs.Message = ItemStorageStatus.HadOnahnd_Used.ToString();
                            rs.Content = (int)ItemStorageStatus.HadOnahnd_Used;
                        }
                    }
                }
                else
                {
                    rs.Message = ItemStorageStatus.Noneonahnd_Unused.ToString();
                    rs.Content = (int)ItemStorageStatus.Noneonahnd_Unused;
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





        #endregion
    }
}
