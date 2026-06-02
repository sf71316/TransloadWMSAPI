using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class AreaRepository<T> : AbstractRepository<T>, IAreaRepository where T : class, IAreaModel
    {
        public AreaRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> AddArea(IAreaModel Model)
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

        public IActionResult<bool> DeleteArea(Guid[] UID)
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

        public IActionResult<bool> EditArea(IAreaModel Model)
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

        public IActionResult<IEnumerable<IAreaViewModel>> GetAreaList(Guid? warehouseUID, Guid? areaUID)
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<IAreaViewModel>>();

            string query =
@"
            SELECT	
		    WMS_Warehouse.ID AS WarehouseID, 
		    WMS_Warehouse.Name AS WarehouseName,  
            WMS_Area.* 
            FROM	[WMS_Area]
			INNER JOIN WMS_Warehouse ON WMS_Area.WarehouseUID = WMS_Warehouse.[UID]
";

            try
            {
                var parameters = new
                {
                    Status = (int)AreaStatus.Active,
                    WarehouseStatus = (int)WarehouseStatus.Active,
                    WarehouseUID = warehouseUID,
                    areaUID = areaUID
                };

                List<string> conditions = new List<string>();
                conditions.Add(" (WMS_Area.Status = @Status) ");
                conditions.Add(" (WMS_Warehouse.Status = @WarehouseStatus) ");

                if (warehouseUID.HasValue)
                {
                    conditions.Add(" (WMS_Area.WarehouseUID = @WarehouseUID) ");
                }
                if (areaUID.HasValue)
                {
                    conditions.Add(" (WMS_Area.UID = @areaUID) ");
                }

                var collection = this._Handler.Instance.Query<AreaViewModel>(query +
                    (conditions.Count > 0 ? $" WHERE {String.Join(" AND ", conditions)} " : ""), parameters);

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

        public IActionResult<IEnumerable<IComponentViewModel>> GetAreaNameList(IWarehouseComponentParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IComponentViewModel>>();
            try
            {
                var query = $"SELECT UID, ID, Name FROM WMS_AREA {this.getCondition(parameters)}";
                rs.Content = this._Handler.Instance.Query<ComponentViewModel>(query, parameters);
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = 500;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<IEnumerable<IAreaModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IAreaModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > 0);
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

        private string getCondition(IWarehouseComponentParameters Parameters)
        {
            List<string> Condition = new List<string>();
            if (Parameters.ConditionUID.HasValue)
            {
                Condition.Add("(WMS_AREA.WarehouseUID=@ConditionUID)");
            }
            if (!string.IsNullOrEmpty(Parameters.Name))
            {
                Condition.Add("(WMS_AREA.Name LIKE @Name)");
                Parameters.Name = "%" + Parameters.Name + "%";
            }
            if (Parameters.WarehouseUID.HasValue)
            {
                Condition.Add("(WMS_AREA.WarehouseUID =@WarehouseUID)");

            }
            Condition.Add("(Status>0)");
            return Condition.Count > 0 ? "WHERE " + string.Join("AND", Condition) : "";
        }
    }
}
