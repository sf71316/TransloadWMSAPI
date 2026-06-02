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
    public class BinRepository<T> : AbstractRepository<T>, IBinRepository where T : class, IBinModel
    {
        private readonly IAuthenticationProvider _AuthenticationProvider;
        public BinRepository(IRepositoryHandler<T> handler, IAuthenticationProvider authenticationInfoProvider) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;
            this._AuthenticationProvider = authenticationInfoProvider;
        }

        public IActionResult<bool> SetBinMappingToArea(Guid areaUID, Guid binUID)
        {
            var resultContainer = ActionResultTemplates.Result<bool>();

            try
            {
                bool success = this._Handler.UpdateByDynamicConditions(
                            new { AreaUID = areaUID, ModifiedBy = this._AuthenticationProvider?.GetAuthenticationInfo()?.Account, ModifiedOn = DateTime.UtcNow },
                            new { UID = binUID }
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

        public IActionResult<bool> AddBin(IBinModel model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(model);
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

        public IActionResult<bool> DeleteBin(Guid[] UID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                foreach (var item in UID)
                {
                    rs.Content &= this._Handler.DeleteByDynamicConditions(new { UID = UID });
                }
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

        public IActionResult<bool> EditBin(IBinModel Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(Model,
                new { UID = Model.UID });
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

        public IActionResult<IEnumerable<IBinModel>> GetList(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IBinModel>>();
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

        public IActionResult<IEnumerable<IBinViewModel>> GetBinList(Guid? warehouseUID, Guid? areaUID)
        {
            var resultContainer = ActionResultTemplates.Result<IEnumerable<IBinViewModel>>();

            string query =
        @"
SELECT 
		    WMS_Warehouse.ID AS WarehouseID, 
		    WMS_Warehouse.Name AS WarehouseName, 
		    WMS_Area.ID AS AreaID, 
		    WMS_Area.Name AS AreaName, 
            WMS_Bin.* 
FROM WMS_Bin
            INNER JOIN WMS_Warehouse ON WMS_Bin.WarehouseUID = WMS_Warehouse.UID
            LEFT OUTER JOIN WMS_Area ON WMS_Area.UID = WMS_Bin.AreaUID
";

            try
            {
                var parameters = new { Status = (int)BinStatus.Active, WarehouseStatus = (int)WarehouseStatus.Active, WarehouseUID = warehouseUID, AreaUID = areaUID };

                List<string> conditions = new List<string>();
                conditions.Add(" (WMS_Bin.Status = @Status) ");
                conditions.Add(" (WMS_Warehouse.Status = @WarehouseStatus) ");

                if (warehouseUID.HasValue)
                {
                    conditions.Add(" (WMS_Bin.WarehouseUID = @WarehouseUID) ");
                }
                if (areaUID.HasValue)
                {
                    conditions.Add(" (WMS_Bin.AreaUID = @AreaUID) ");
                }

                var collection = this._Handler.Instance.Query<BinViewModel>(query + (conditions.Count > 0 ? $" WHERE {String.Join(" AND ", conditions)} " : ""), parameters);

                if (collection != null && collection.Count() > 0)
                {
                    foreach (var item in collection)
                    {
                        item.StatusName = ((BinStatus)item.Status).ToString();
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
        public IActionResult<IEnumerable<IComponentViewModel>> GetBinNameList(IWarehouseComponentParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IComponentViewModel>>();
            try
            {
                var query = $"SELECT UID, ID, Name FROM WMS_BIN {this.getCondition(parameters)}";
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

        private string getCondition(IWarehouseComponentParameters Parameters)
        {
            List<string> Condition = new List<string>();
            if (Parameters.ConditionUID != null)
            {
                Condition.Add("(WMS_BIN.AreaUID=@ConditionUID)");
            }
            if (!string.IsNullOrEmpty(Parameters.Name))
            {
                Condition.Add("(WMS_BIN.Name LIKE @Name)");
                Parameters.Name = "%" + Parameters.Name + "%";
            }
            if (Parameters.WarehouseUID != null)
            {
                Condition.Add("(WMS_BIN.WarehouseUID =@WarehouseUID)");
            }
            if (Parameters.UnAssigned)
            {
                Condition.Add("(WMS_BIN.AreaUID IS NULL )");
            }
            Condition.Add("(Status>0)");
            return Condition.Count > 0 ? "WHERE " + string.Join(" AND ", Condition) : "";
        }
    }
}
