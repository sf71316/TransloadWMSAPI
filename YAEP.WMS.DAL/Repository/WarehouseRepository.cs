using General.Data.SQLConditionConverter.Interfaces;
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
    public class WarehouseRepository<T> : AbstractRepository<T>, IWarehouseRepository where T : class, IWarehouseModel
    {
        public WarehouseRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> Add(IWarehouseModel warehouse)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(warehouse);
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

        public IActionResult<bool> Delete(IWarehouseDeleteParameters parameters)
        {
            return this.Delete(parameters.UID);
        }

        public IActionResult<bool> Update(IWarehouseModel warehouse)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(warehouse, new { UID = warehouse.UID });
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

        public IActionResult<IEnumerable<IComponentViewModel>> GetWarehouseNameList()
        {
            string query = "SELECT UID, ID, Name FROM WMS_Warehouse WHERE Status > @Status";

            var rs = ActionResultTemplates.Result<IEnumerable<IComponentViewModel>>();
            try
            {
                rs.Content = this._Handler.Instance.Query<ComponentViewModel>(query, new { Status = (int)WarehouseStatus.Inactive });
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.InnerException = ex;
                rs.TypeCode = 500;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<Guid> GetPodInSlot(IGetPodInSlotParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<Guid>();
            try
            {
                var query = @"SELECT [WPL].SlotUID FROM [WMS_Pod] as [WP]
                               INNER JOIN [WMS_Payload] as [WPL] on  [WP].UID=[WPL].PodUID
                                {0}";
                query = string.Format(query, this.getPodInSlotCondition(Parameters));
                var guidString = this._Handler.Instance.ExecuteScalar(query, Parameters).ToString();
                rs.Content = new Guid(guidString);
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = 500;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<bool> PodIsExist(Guid PodUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"SELECT [WP].UID FROM [WMS_Pod] as [WP]
                              WHERE [WP].UID=@PodUID AND Status>0";
                rs.Content = this._Handler.Instance.Query<dynamic>(query, new { PodUID = PodUID }).Count() > 0;
                rs.Success = rs.Content;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = 500;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<IWarehouseModel> GetWarehouse(Guid warehouseUID)
        {
            var resultContainer = ActionResultTemplates.Result<IWarehouseModel>();

            try
            {
                var collection = this._Handler.RetrieveCollectionByDynamicConditions(new { UID = warehouseUID, Status = (int)WarehouseStatus.Active });

                if (collection.Count() == 0)
                {
                    resultContainer.Message = "Not Found.";
                }
                else
                {
                    resultContainer.Success = true;
                    resultContainer.Content = collection.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resultContainer.Message = "Error";
                resultContainer.InnerException = ex;
                resultContainer.TypeCode = 500;
                this.OnExpcetion(ex);
            }

            return resultContainer;
        }

        public IActionResult<IEnumerable<IWarehouseModel>> GetWarehouseList()
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<IWarehouseModel>>();

            try
            {
                var collection = this._Handler.RetrieveCollectionByDynamicConditions(new { Status = (int)WarehouseStatus.Active });

                if (collection.Count() == 0)
                {
                    resultContainer.Message = "Not Found.";
                }
                else
                {
                    resultContainer.Success = true;
                    resultContainer.Content = collection;
                }
            }
            catch (Exception ex)
            {
                resultContainer.Message = "Error";
                resultContainer.InnerException = ex;
                resultContainer.TypeCode = 500;
                this.OnExpcetion(ex);
            }

            return resultContainer;
        }

        public IActionResult<IEnumerable<ILocationInfoViewModel>> GetLocationInfoList(Guid? warehouseUID, Guid? areaUID, Guid? binUID, Guid? slotUID)
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<ILocationInfoViewModel>>();

            string query =
@"
SELECT 
		    WMS_Warehouse.ID AS WarehouseID, 
		    WMS_Warehouse.Name AS WarehouseName, 
		    WMS_Area.UID AS AreaUID, 
		    WMS_Area.ID AS AreaID, 
		    WMS_Area.Name AS AreaName, 
		    WMS_Bin.UID AS BinUID, 
		    WMS_Bin.ID AS BinID, 
		    WMS_Bin.Name AS BinName, 
		    WMS_Slot.ID AS SlotID, 
		    WMS_Slot.Name AS SlotName, 
		    WMS_Slot.UID AS SlotUID,
		    WMS_Slot.VolumeLimit AS Volume,
		    WMS_Slot.WeightLimit AS Weight
FROM WMS_Bin
			INNER JOIN WMS_Warehouse ON WMS_Warehouse.UID = WMS_Bin.WarehouseUID  
			INNER JOIN WMS_Area ON WMS_Area.UID = WMS_Bin.AreaUID
			LEFT OUTER JOIN WMS_Slot ON WMS_Bin.UID = WMS_Slot.BinUID
";
            //TODO 需調整用什麼狀態的Slot
            try
            {
                var parameters = new
                {
                    SlotStatus = new int[] {
                    (int)SlotStatus.InAndOut,
                    (int)SlotStatus.In,
                    (int)SlotStatus.Out,
                    (int)SlotStatus.Neitherof,
                    },
                    BinStatus = (int)BinStatus.Active,
                    AreaStatus = (int)AreaStatus.Active,
                    WarehouseStatus = (int)WarehouseStatus.Active,
                    WarehouseUID = warehouseUID,
                    AreaUID = areaUID,
                    BinUID = binUID,
                    SlotUID = slotUID
                };

                List<string> conditions = new List<string>();
                conditions.Add(" (WMS_Slot.Status in @SlotStatus) ");
                conditions.Add(" (WMS_Bin.Status = @BinStatus) ");
                conditions.Add(" (WMS_Area.Status = @AreaStatus) ");
                conditions.Add(" (WMS_Warehouse.Status = @WarehouseStatus) ");

                if (warehouseUID.HasValue)
                {
                    conditions.Add(" (WMS_Bin.WarehouseUID = @WarehouseUID) ");
                }
                if (areaUID.HasValue)
                {
                    conditions.Add(" (WMS_Bin.AreaUID = @AreaUID) ");
                }
                if (binUID.HasValue)
                {
                    conditions.Add(" (WMS_Bin.BinUID = @BinUID) ");
                }
                if (slotUID.HasValue)
                {
                    conditions.Add(" (WMS_Slot.UID = @SlotUID) ");
                }

                query += (conditions.Count > 0 ? $" WHERE {String.Join(" AND ", conditions)} " : "");

                var collection = this._Handler.Instance.Query<LocationInfoViewModel>(query, parameters);

                if (collection != null && collection.Count() > 0)
                {
                    resultContainer.Success = true;
                    resultContainer.Content = collection;
                }
                else
                {
                    resultContainer.Message = "Not Found";
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

        protected string getPodInSlotCondition(IGetPodInSlotParameters parameters)
        {
            List<string> condition = new List<string>();
            if (parameters.PodUID.HasValue)
            {
                condition.Add("([WP].UID=@PodUID)");
            }
            if (!string.IsNullOrEmpty(parameters.PodNo))
            {
                condition.Add("([WP].Name=@PodNo)");
            }
            condition.Add("([WP].Status>0)");
            return condition.Count > 0 ? "WHERE " + string.Join("AND", condition) : "";
        }

        public IActionResult<IEnumerable<IPodSelectListModel>> GetPodSelectList(Guid wuid)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPodSelectListModel>>();
            try
            {
                var query = @"SELECT
                              [WP].UID 'PodUID',
							  [WP].Name 'PodName',
                              [WB].RefNo 'BolRefNo',
                              [WV].RefNo 'VesselRefNo',
							  [WW].Name 'WarehouseName',
                              [WA].Name 'AreaName',
                              [Bin].Name 'BinName',
                              [WS].Name 'SlotName',
                              [WP].VolumeLimit,
                              [WP].WeightLimit,
                              SUM([WPL].VolumeLimit) 'TTLUsedVolume',
                              SUM([WPL].WeightLimit) 'TTLUsedWeight'
                              FROM
                              [WMS_Pod] AS [WP] 
                              INNER JOIN [WMS_Payload] AS [WPL] ON [WPL].PodUID=[WP].UID
                              INNER JOIN [WMS_Vessel] AS [WV] ON [WV].UID=[WPL].VesselUID
                              INNER JOIN [WMS_BOL] AS [WB] ON [WB].UID=[WV].BolUID
                              INNER JOIN [WMS_Slot] AS [WS] ON [WS].UID=[WPL].SlotUID
                              INNER JOIN [WMS_Bin] AS [Bin] ON [Bin].UID=[WS].BinUID
                              INNER JOIN [WMS_Area] AS [WA] ON [WA].UID=[Bin].AreaUID
							  INNER JOIN [WMS_Warehouse] AS [WW] ON WW.UID=[WA].WarehouseUID
                              WHERE [WA].WarehouseUID=@WarehouseUID AND [WPL].Status>0 AND [WP].Status>0
                                AND [WV].Status>0 AND [WB].Status>0 AND [WS].Status>0 AND [Bin].Status>0
                                AND [WA].Status>0 AND [WW].Status>0
                              GROUP BY
                              [WP].UID ,
							  [WP].Name,
                              [WB].RefNo ,
                              [WV].RefNo ,
                              [WA].Name ,
                              [Bin].Name ,
                              [WS].Name ,
                              [WP].VolumeLimit,
                              [WP].WeightLimit,
							  [WW].Name";
                rs.Content = this._Handler.Instance.Query<PodSelectListInnerModel>(query, new { WarehouseUID = wuid }); ;
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

        public IActionResult<IEnumerable<ILoadingZoneSelectModel>> GetLoadingZoneList(Guid value)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ILoadingZoneSelectModel>>();
            try
            {
                var query = @"SELECT 
            [WS].UID ,
            [WA].Name 'AreaName',
            [Bin].Name 'BinName',
            [WS].Name 'SlotName',
            [WS].Type 'SlotType',       
            [WS].IsDefaultLoadingZone 'IsDefaultLoadingZone'
            FROM WMS_Slot [WS] 
            INNER JOIN WMS_Bin [Bin] ON [WS].BinUID=[Bin].UID
            INNER JOIN WMS_Area [WA] ON [WA].UID=[Bin].AreaUID
            WHERE [WS].Type IN @SlotType AND [WS].Status>0 AND [Bin].Status>0 AND [WA].Status>0 AND [WA].WarehouseUID=@WarehouseUID
            Order by [WS].IsDefaultLoadingZone DESC";

                rs.Content = this._Handler.Instance.Query<LoadingZoneSelectInnerModel>(query, new
                {
                    WarehouseUID = value,
                    SlotType = new int[] { (int)SlotType.OutboundTemp, (int)SlotType.InboundTemp, (int)SlotType.PackingArea }
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

        public IActionResult<IEnumerable<ILocationItemViewModel>> GetAvailableInventoryList(
            IGetAvailableInventoryDataInnerListParameters request)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ILocationItemViewModel>>();
            try
            {
                var query = @"
                     SELECT [WW].Name 'WarehouseName', [WA].Name 'AreaName',[WBin].Name 'BinName',[WS].Name 'SlotName'
	                ,[WV].RefNo 'VesselRefNo',[WB].RefNo 'BolRefNo',[WS].ID 'SlotId',[WS].Name 'SlotName',[WP].Name 'PodName',
                    [WPL].UID,[WPL].ID,[WPL].Name,[WPL].Type,[WPL].PodUID,[WPL].SlotUID,[WPL].VesselUID,[WPL].ItemUID,
					[WPL].Quantity,[WPL].PackageUID 'OriginalPackageUID',[WPL].VolumeLimit,[WPL].WeightLimit,[WPL].Status,
					[WPL].Description
                    FROM WMS_Payload AS [WPL]
                    LEFT JOIN WMS_Pod AS [WP] on [WP].UID=[WPL].PodUID  AND [WP].Status>0
                    LEFT JOIN WMS_Vessel AS [WV] ON [WPL].VesselUID=[WV].UID  AND [WV].Status>0 
                    LEFT JOIN WMS_BOL AS [WB] ON [WB].UID=[WV].BolUID   AND [WB].Status>0
                    INNER JOIN WMS_Slot AS[WS] ON [WS].UID=[WPL].SlotUID AND [WS].Status>0
                    INNER JOIN WMS_Bin AS [WBin] ON [WBin].UID=[WS].BinUID AND [WBin].Status>0
                    INNER JOIN WMS_Area AS [WA] ON [WA].UID=[WBin].AreaUID AND [WA].Status>0
                    INNER JOIN WMS_Warehouse [WW] ON [WA].WarehouseUID=[WW].UID AND [WW].Status>0
                    WHERE [WPL].Quantity>0 AND [WPL].Status=@PayloadStatus 
                     {0}
                    ORDER BY [WA].AllocatedSequence
                ";
                query = string.Format(query, this.getLocationItemListCondition(request));
                rs.Content = this._Handler.Instance.Query<LocationItemViewInnerModel>(query, new
                {
                    PayloadStatus = (int)PayloadStatus.Active,
                    //PayloadType = (int)PayloadType.Stock,
                    WarehouseUID = request.WarehouseUID,
                    AreaUID = request.AreaUID,
                    BinUID = request.BinUID,
                    SlotUID = request.SlotUID,
                    OptionValue = request.OptionValue,
                    SlotStatuses = request.SlotStatuses
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
        protected string getLocationItemListCondition(IGetAvailableInventoryDataInnerListParameters parameters)
        {
            List<string> condition = new List<string>();
            if (parameters.Items.Count > 0)
            {
                List<string> itemcollections = new List<string>();
                foreach (var item in parameters.Items)
                {
                    if (item.Value != null && item.Value.Count() > 0)
                    {
                        itemcollections.Add($"([WPL].ItemUID IN({string.Join(",", item.Value.Select(x => $"'{x}'"))}) AND [WPL].Type={(int)item.Key})");
                    }
                }
                if (itemcollections.Count > 0)
                {
                    condition.Add($"({String.Join("OR", itemcollections)})");
                    //condition.Add("([WPL].ItemUID IN @ItemUID)");
                }

            }
            if (parameters.WarehouseUID.HasValue)
            {
                condition.Add("([WW].UID=@WarehouseUID)");
            }
            if (parameters.AreaUID.HasValue)
            {
                condition.Add("([WA].UID=@AreaUID)");
            }
            if (parameters.BinUID.HasValue)
            {
                condition.Add("([WBin].UID=@BinUID)");
            }
            if (parameters.SlotUID.HasValue)
            {
                condition.Add("([WS].UID=@SlotUID)");
            }
            if (parameters.SlotStatuses != null)
            {
                condition.Add("([WS].Status IN @SlotStatuses)");
            }
            if (!string.IsNullOrEmpty(parameters.OptionText))
            {
                if (parameters.OptionText.ToLower() == "bolrefno")
                {
                    condition.Add("([WB].RefNo Like @OptionValue)");
                    parameters.OptionValue = "%" + parameters.OptionValue + "%";
                }
                else if (parameters.OptionText.ToLower() == "vesselrefno")
                {
                    condition.Add("([WV].RefNo Like @OptionValue)");
                    parameters.OptionValue = "%" + parameters.OptionValue + "%";
                }
            }


            return condition.Count > 0 ? " AND " + string.Join("AND", condition) : "";
        }

        public IActionResult<IEnumerable<ISlotModel>> GetDefaultLoadingZone(Guid warehouseUID, SlotType slotType)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ISlotModel>>();
            try
            {
                var query = @"SELECT [WS].* from WMS_Warehouse AS [WW] 
                INNER JOIN WMS_Area AS [WA] ON [WW].UID=[WA].WarehouseUID
                INNER JOIN WMS_Bin AS [WB] ON [WA].UID=[WB].AreaUID AND [WB].Status>0
                INNER JOIN WMS_Slot AS [WS] ON [WS].BinUID=[WB].UID  
                WHERE [WA].Status>0 AND [WS].Status>0 AND [WS].Type=@Type AND [WW].UID=@UID";
                rs.Content = this._Handler.Instance.Query<SlotInnerModel>(query, new { UID = warehouseUID, Type = slotType });
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
        public IActionResult<IEnumerable<ISlotModel>> GetSlotByType(Guid warehouseUID, IEnumerable<SlotType> slotTypes)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ISlotModel>>();
            try
            {
                var query = @"SELECT [WS].* from WMS_Warehouse AS [WW] 
                INNER JOIN WMS_Area AS [WA] ON [WW].UID=[WA].WarehouseUID
                INNER JOIN WMS_Bin AS [WB] ON [WA].UID=[WB].AreaUID AND [WB].Status>0
                INNER JOIN WMS_Slot AS [WS] ON [WS].BinUID=[WB].UID  
                WHERE [WA].Status>0 AND [WS].Status>0 AND [WS].Type in @Type AND [WW].UID=@UID";
                rs.Content = this._Handler.Instance.Query<SlotInnerModel>(query, new { UID = warehouseUID, Type = slotTypes });
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
        public IActionResult<IEnumerable<ISlotUsageInfoModel>> GetSlotUsageInfo(
            Guid warehouseUID, IEnumerable<SlotType> slotTypes, IEnumerable<SlotStatus> slotStatuses,
            ManifestType manifestType)
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<ISlotUsageInfoModel>>();
            try
            {
                var query =
@"SELECT 
                SlotUsage.*, 
		        Slot.ID AS SlotID, Slot.VolumeLimit, Slot.WeightLimit, 
		        Slot.WarehouseUID, Slot.AreaUID, Slot.BinUID, 
                Slot.StorageSequence, Slot.AllocatedSequence
FROM (
	SELECT SlotUID,SUM(ISNULL(Volume, 0)) AS Volume, SUM(ISNULL([Weight], 0)) AS [Weight]
	FROM
	(
		(
--目前已存在的庫存資訊
			SELECT  
					Slot.UID AS SlotUID, 
					Payload.VolumeLimit AS Volume, Payload.WeightLimit AS [Weight]
			FROM WMS_Slot AS Slot
            --INNER JOIN WMS_Bin Bin ON Slot.BinUID=Bin.UID AND Bin.Status>0
            --INNER JOIN WMS_Area Area ON Slot.AreaUID=Area.UID AND Area.Status>0
			LEFT OUTER JOIN WMS_Payload AS Payload ON (Payload.SlotUID = Slot.UID) 
				 AND (Payload.[Type] IN @PayloadType) AND (Payload.[Status] = @PayloadStatus)
			WHERE (Slot.Type IN @SlotType) AND (Slot.[Status] IN @SlotStatus)
					    AND (Slot.WarehouseUID = @WarehouseUID) 
		)
		UNION ALL
--預備要入庫的庫存資訊
			SELECT SlotUID, Volume, [Weight]
			FROM 
			(
				SELECT  
						Slot.UID AS SlotUID, 
						ISNULL(Manifest.Type, 0) AS [ManifestType], 
						WorkPayload.Volume, 
						WorkPayload.[Weight]
					FROM WMS_Slot AS Slot
                        --經測試WMS_BIN/WMS_Area 註解掉會產生平行處理
                        INNER JOIN WMS_Bin Bin ON Slot.BinUID=Bin.UID AND Bin.Status>0
                        INNER JOIN WMS_Area Area ON Slot.AreaUID=Area.UID AND Area.Status>0
						LEFT OUTER JOIN WMS_WorkOrder_Payload AS WorkPayload ON WorkPayload.SlotUID = Slot.UID 
						 AND (WorkPayload.[Status] IN @WorkPayloadStatus) 
						LEFT OUTER JOIN WMS_WorkOrder AS WorkOrder ON WorkOrder.UID = WorkPayload.WorkOrderUID 
                        AND WorkOrder.Status>0
						LEFT OUTER JOIN WMS_Manifest AS Manifest ON (Manifest.UID = WorkOrder.ManifestUID) 
                        AND Manifest.Status>0
				WHERE (Slot.Type IN @SlotType) AND (Slot.[Status] IN @SlotStatus)
						    AND (Slot.WarehouseUID = @WarehouseUID) 
                    AND Manifest.Type = @ManifestType AND Manifest.Status in (400)
			) AS T2
			WHERE (T2.[ManifestType] = @ManifestType)
	) AS Tbl 
	GROUP BY SlotUID
) AS SlotUsage
		INNER JOIN WMS_Slot AS Slot ON Slot.UID = SlotUsage.SlotUID  AND Slot.Status>0
                 AND  (Slot.Type IN @SlotType) AND (Slot.[Status] IN @SlotStatus)
option(RECOMPILE,MAXDOP 1)
";
                //TODO 需調整用什麼狀態的Slot

                resultContainer.Content = this._Handler.Instance.Query<SlotUsageInfoInnerModel>(query, new
                {
                    WarehouseUID = warehouseUID,
                    SlotStatus = slotStatuses.Select(x => (int)x),
                    SlotType = slotTypes.Select(x => (int)x),
                    ManifestType = (int)manifestType,
                    PayloadType = new int[] { (int)PayloadType.Stock, (int)PayloadType.Allocated },
                    PayloadStatus = (int)PayloadStatus.Active,
                    WorkPayloadStatus = new int[] {
                        (int)WorkOrderPayloadStatus.WaitingForProcessing,
                        (int)WorkOrderPayloadStatus.Processing,
                    },
                });

                resultContainer.Success = true;
            }
            catch (Exception ex)
            {
                resultContainer.Message = ex.Message;
                resultContainer.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                resultContainer.InnerException = ex;
            }

            return resultContainer;
        }

        public IActionResult<IEnumerable<IPayloadLocationModel>> GetLocations(IEnumerable<Guid> payloadUIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IPayloadLocationModel>>();
            try
            {
                var query = @"SELECT 
                            [WS].Name 'SlotName',[WS].UID 'SlotUID',[WS].ID 'SlotID',
                            [WB].Name 'BinName',[WB].UID 'BinUID',[WB].ID 'BinID',
                            [WA].Name 'AreaName',[WA].UID 'AreaUID',[WA].ID 'AreaID'
							,[WP].UID  PayloadUID
                            FROM WMS_Slot AS [WS] 
                            INNER JOIN WMS_Bin AS [WB] ON [WS].BinUID=[WB].UID AND [WB].Status>0
                            INNER JOIN WMS_Area AS [WA] ON [WB].AreaUID=[WA].UID  AND WA.Status>0
							INNER JOIN WMS_Payload [WP] ON [WS].UID=[WP].SlotUID
							WHERE [WS].Status>0 AND [WP].UID IN ({0})";
                query = string.Format(query, string.Join(",", payloadUIDs.Select(p => "'" + p + "'")));
                rs.Content = this._Handler.Instance.Query<PayloadLocationInnerModel>(query);
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

        public IActionResult<IEnumerable<IWarehouseModel>> GetWarehouseList(IQueryConditionExtractor conditionExtractor)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IWarehouseModel>>();
            try
            {
                
                var query = @"SELECT *  FROM [WMS_Warehouse]  WHERE {0}";
                query = string.Format(query, conditionExtractor.Translate());
                var param = conditionExtractor.Parameters.ConvertDapperParameters();
                if (param.ParameterNames.Count() > 0)
                {
                    rs.Content = this._Handler.Instance.Query<WarehouseInnerModel>(query, param);
                    rs.Success = true;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = "must have parameter";
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
    }
}
