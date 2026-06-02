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
    public class SlotRepository<T> : AbstractRepository<T>, ISlotRepository where T : class, ISlotModel
    {
        private readonly IAuthenticationProvider _AuthenticationProvider;
        public SlotRepository(IRepositoryHandler<T> handler, IAuthenticationProvider authenticationInfoProvider) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;
            this._AuthenticationProvider = authenticationInfoProvider;
        }

        public IActionResult<IEnumerable<ISlotViewModel>> GetSlotList(Guid? warehouseUID, Guid? areaUID, Guid? binUID)
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<ISlotViewModel>>();

            string query =
@"
SELECT 
		    WMS_Warehouse.ID AS WarehouseID, 
		    WMS_Warehouse.Name AS WarehouseName, 
		    WMS_Area.ID AS AreaID, 
		    WMS_Area.Name AS AreaName, 
		    WMS_Bin.ID AS BinID, 
		    WMS_Bin.Name AS BinName, 
            WMS_Slot.* 
FROM WMS_Slot
            INNER JOIN WMS_Warehouse ON WMS_Warehouse.UID = WMS_Slot.WarehouseUID  
            LEFT OUTER JOIN WMS_Area ON WMS_Area.UID = WMS_Slot.AreaUID
			LEFT OUTER JOIN WMS_Bin ON WMS_Bin.UID = WMS_Slot.BinUID
";

            try
            {
                var parameters = new { Status = (int)SlotStatus.Inactive, WarehouseStatus = (int)WarehouseStatus.Active, WarehouseUID = warehouseUID, AreaUID = areaUID, BinUID = binUID };

                List<string> conditions = new List<string>();
                conditions.Add(" (WMS_Slot.Status > @Status) ");
                conditions.Add(" (WMS_Warehouse.Status = @WarehouseStatus) ");

                if (warehouseUID.HasValue)
                {
                    conditions.Add(" (WMS_Slot.WarehouseUID = @WarehouseUID) ");
                }
                if (areaUID.HasValue)
                {
                    conditions.Add(" (WMS_Slot.AreaUID = @AreaUID) ");
                }
                if (binUID.HasValue)
                {
                    conditions.Add(" (WMS_Slot.BinUID = @BinUID) ");
                }

                var collection = this._Handler.Instance.Query<SlotViewModel>(query + (conditions.Count > 0 ? $" WHERE {String.Join(" AND ", conditions)} " : ""), parameters);

                if (collection != null && collection.Count() > 0)
                {
                    foreach (var item in collection)
                    {
                        item.StatusName = ((SlotStatus)item.Status).ToString();
                    }
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
        public IActionResult<bool> SetSlotMappingToBin(Guid? areaUID, Guid slotUID, Guid? binUID)
        {
            var resultContainer = ActionResultTemplates.Result<bool>();

            try
            {
                bool success = this._Handler.UpdateByDynamicConditions(
                            new { AreaUID = areaUID, BinUID = binUID, ModifiedBy = this._AuthenticationProvider?.GetAuthenticationInfo()?.Account, ModifiedOn = DateTime.UtcNow },
                            new { UID = slotUID }
                 );

                if (success)
                {
                    resultContainer.Success = true;
                    resultContainer.Content = true;
                }
                else
                {
                    resultContainer.Message = "Fail to set mapping";
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

        public IActionResult<bool> AddSlot(ISlotModel Model)
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
                rs.TypeCode = 500;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<bool> DeleteSlot(Guid[] UID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.Delete(UID);
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

        public IActionResult<bool> EditSlot(ISlotModel Model)
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
                rs.TypeCode = 500;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<IEnumerable<ISlotModel>> GetList(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ISlotModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > 0); ;
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

        public IActionResult<IEnumerable<IComponentViewModel>> GetSlotNameList(IWarehouseComponentParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IComponentViewModel>>();
            try
            {
                var query = $"SELECT UID, ID, Name FROM WMS_Slot {this.getCondition(parameters)}";
                rs.Content = this._Handler.Instance.Query<ComponentViewModel>(query, parameters);
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

        private string getCondition(IWarehouseComponentParameters Parameters)
        {
            List<string> Condition = new List<string>();
            if (Parameters.ConditionUID.HasValue)
            {
                Condition.Add("(WMS_Slot.BinUID=@ConditionUID)");
            }
            if (!string.IsNullOrEmpty(Parameters.Name))
            {
                Condition.Add("(WMS_Slot.Name LIKE @Name)");
                Parameters.Name = "%" + Parameters.Name + "%";
            }
            if (Parameters.WarehouseUID.HasValue)
            {
                Condition.Add("(WMS_Slot.WarehouseUID =@WarehouseUID)");

            }
            if (Parameters.UnAssigned)
            {
                Condition.Add("(WMS_Slot.AreaUID IS NULL AND WMS_Slot.BinUID IS NULL)");

            }
            Condition.Add("(Status>0)");
            return Condition.Count > 0 ? "WHERE " + string.Join(" AND ", Condition) : "";
        }

        public IActionResult<IEnumerable<ILocation>> GetLocations(Guid[] slotUIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ILocation>>();
            try
            {
                var query = @"SELECT 
                            [WS].Name 'SlotName',[WS].UID 'SlotUID',[WS].ID 'SlotID',
                            [WB].Name 'BinName',[WB].UID 'BinUID',[WB].ID 'BinID',
                            [WA].Name 'AreaName',[WA].UID 'AreaUID',[WA].ID 'AreaID'
                            FROM WMS_Slot AS [WS] 
                            INNER JOIN WMS_Bin AS [WB] ON [WS].BinUID=[WB].UID AND [WB].Status>0
                            INNER JOIN WMS_Area AS [WA] ON [WB].AreaUID=[WA].UID AND WA.Status>0
                            WHERE [WS].Status>0 AND [WS].UID in @SlotUIDs";
                rs.Content = this._Handler.Instance.Query<TicketLocationInnerModel>(query, new { SlotUIDs = slotUIDs });
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

        public IActionResult<IEnumerable<ISlotMappingLocation>> GetSlotMappingList(IEnumerable<Guid> slotlist)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ISlotMappingLocation>>();
            try
            {
                var query = @"SELECT Slot.UID,Slot.Name SlotName,mapping.WarehouseID,mapping.LocationID FROM WMS_Slot(nolock) Slot 
                INNER JOIN WMS_LocationMapping(nolock) mapping ON Slot.WarehouseUID=mapping.WarehouseUID
                WHERE Slot.UID IN @slotlist";
                rs.Content = this._Handler.Instance.Query<SlotMappingLocationModel>(query, new { slotlist = slotlist });
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

        public IActionResult<IEnumerable<ISlotSearchViewModel>> GetSearchSlotList(Guid? warehouseUID, string slotid)
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<ISlotSearchViewModel>>();

            string query =
@"
            SELECT 
		    WMS_Warehouse.ID AS WarehouseID, 
		    WMS_Warehouse.Name AS WarehouseName, 
		    WMS_Area.ID AS AreaID, 
		    WMS_Area.Name AS AreaName, 
		    WMS_Bin.ID AS BinID, 
		    WMS_Bin.Name AS BinName, 
            WMS_Slot.* 
            FROM WMS_Slot
            INNER JOIN WMS_Warehouse ON WMS_Warehouse.UID = WMS_Slot.WarehouseUID  
            LEFT OUTER JOIN WMS_Area ON WMS_Area.UID = WMS_Slot.AreaUID
			LEFT OUTER JOIN WMS_Bin ON WMS_Bin.UID = WMS_Slot.BinUID
";

            try
            {
                var parameters = new
                {
                    Status = (int)SlotStatus.Inactive,
                    WarehouseStatus = (int)WarehouseStatus.Active,
                    WarehouseUID = warehouseUID,
                    slotid = slotid
                };

                List<string> conditions = new List<string>();
                conditions.Add(" (WMS_Slot.Status > @Status) ");
                conditions.Add(" (WMS_Warehouse.Status = @WarehouseStatus) ");

                if (warehouseUID.HasValue)
                {
                    conditions.Add(" (WMS_Slot.WarehouseUID = @WarehouseUID) ");
                }
                conditions.Add($"(WMS_Slot.ID Like '%{slotid}%' )");

                query = query + (conditions.Count > 0 ? $" WHERE {String.Join(" AND ", conditions)} " : "");
                var collection = this._Handler.Instance.Query<SlotSearchViewModel>(query, parameters);

                if (collection != null && collection.Count() > 0)
                {
                    foreach (var item in collection)
                    {
                        item.SlotStatusName = ((SlotStatus)item.Status).ToString();
                        item.SlotTypeName = ((SlotType)item.Type).ToString();
                    }
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
    }
}
