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
using General.Data.ColumnMapper;
using General.Data.SQLConditionConverter;
using General.Data.SQLConditionConverter.Interfaces;
using YAEP.WMS.DAL.Extension;

namespace YAEP.WMS.DAL.Repository
{
    public class ManifestRepository<T> : AbstractRepository<T>, IManifestRepository where T : class, IManifestModel
    {
        public ManifestRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> Add(IManifestModel Model)
        {
            var rs = ActionResultTemplates.Result<bool>();

            rs.Content = this._Handler.CreateByDynamic(Model);
            rs.Success = true;


            return rs;
        }

        public IActionResult<bool> Delete(IManifestDeleteParameters Parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(new { UID = Parameters.UID });
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
        public IActionResult<bool> Update(dynamic Model)
        {
            this._Handler.IsAllUpdate = false;
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var condition = new { UID = Model.UID };
                rs.Content = this._Handler.UpdateByDynamicConditions(Model, condition);
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

        private string getCondition(IManifestSearchParameters Parameters)
        {
            List<string> Condition = new List<string>();
            if (Parameters.CustomerList != null && Parameters.CustomerList.Length > 0 && Parameters.CustomerList.All(p => p != Guid.Empty))
            {
                Condition.Add("([Party].UID IN @CustomerList)");
            }
            if (Parameters.Customer != null)
            {
                Condition.Add("([Party].UID = @Customer)");
            }
            if (Parameters.Warehouse != null && Parameters.Warehouse.Length > 0 && Parameters.Warehouse.All(p => p != Guid.Empty))
            {
                Condition.Add("([Warehouse].UID IN @Warehouse)");
            }
            if (Parameters.Type.HasValue)
            {
                Condition.Add("([Manifest].Type=@Type)");
            }
            if (Parameters.Option.ToLower() == "vesselref")
            {
                Condition.Add("([Vessel].RefNo LIKE @OptionText)");
                Parameters.OptionText = "%" + Parameters.OptionText + "%";
            }
            else if (Parameters.Option.ToLower() == "vesselname")
            {
                Condition.Add("([Vessel].Name LIKE @OptionText)");
                Parameters.OptionText = "%" + Parameters.OptionText + "%";
            }
            else if (Parameters.Option.ToLower() == "bolref")
            {
                Condition.Add("([Bol].RefNo LIKE @OptionText)");
                Parameters.OptionText = "%" + Parameters.OptionText + "%";
            }
            else if (Parameters.Option.ToLower() == "bolname")
            {
                Condition.Add("([Bol].Name LIKE @OptionText)");
                Parameters.OptionText = "%" + Parameters.OptionText + "%";
            }
            if (!string.IsNullOrEmpty(Parameters.manifestid))
            {
                Condition.Add("([Manifest].ID LIKE @manifestid)");
                Parameters.manifestid = "%" + Parameters.manifestid + "%";
            }
            if (!string.IsNullOrEmpty(Parameters.manifestname))
            {
                Condition.Add("([Manifest].Name LIKE @manifestname)");
                Parameters.manifestname = "%" + Parameters.manifestname + "%";
            }
            if (!string.IsNullOrEmpty(Parameters.manifestref))
            {
                Condition.Add("([Manifest].RefNo LIKE @manifestref)");
                Parameters.manifestref = "%" + Parameters.manifestref + "%";
            }
            if (Parameters.PHierarchy?.Count() > 0)
            {
                Condition.Add("(ManifestItem.ItemUID IN @PHierarchy)");
            }
            Condition.Add("([Manifest].Status  >0)");
            //Condition.Add("([Bol].Status  >0 OR [Bol].UID IS NULL)");
            //Condition.Add("([Vessel].Status  >0 OR [Vessel].UID IS NULL)");
            return Condition.Count > 0 ? "WHERE " + string.Join("AND", Condition) : "";
        }

        public IActionResult<IManifestModel> GetInfo(Guid ManifestUID)
        {
            var rs = ActionResultTemplates.Result<IManifestModel>();
            try
            {
                rs.Content = this._Handler.Retrieve(ManifestUID);
                if (rs.Content.Status == (int)ManifestStatus.Void)
                {
                    rs.Content = null;
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
        public IActionResult<IEnumerable<IManifestModel>> GetList(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IManifestModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > (int)ManifestStatus.Void);
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
                throw ex;
            }
            return rs;
        }
        public IActionResult<IEnumerable<IManifestModel>> GetListBySQLConverter(IQueryConditionExtractor conditionExtractor)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IManifestModel>>();
            try
            {
                var query = @"SELECT *  FROM [WMS_Manifest]  WHERE {0}";
                query = string.Format(query, conditionExtractor.Translate());
                var param = conditionExtractor.Parameters.ConvertDapperParameters();
                if (param.ParameterNames.Count() > 0)
                {
                    rs.Content = this._Handler.Instance.Query<ManifestInnerModel>(query, param);
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
                throw ex;
            }
            return rs;
        }
        public IActionResult<IEnumerable<ILocationMapping>> GetWarehouseMapping()
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ILocationMapping>>();
            var query = @"select * from WMS_LocationMapping";
            rs.Content = this._Handler.Instance.Query<LocationMappingInnerModel>(query, null);
            rs.Success = true;
            return rs;
        }
        public IActionResult<IEnumerable<R>> GetManifestList<R>(IManifestSearchParameters Parameters)
            where R : class, IManifestListViewModel
        {
            var rs = ActionResultTemplates.Result<IEnumerable<R>>();
            var query = @"SELECT [Manifest].PartyUID,[Manifest].WarehouseUID,[Manifest].UID,[Manifest].Name ManifestName,[Manifest].ID ManifestNo ,[Manifest].Type,[Manifest].Status,[Party].Name CustNo,
                          [Manifest].RefNo
                          FROM [WMS_Manifest] AS [Manifest]
                          INNER JOIN [WMS_Warehouse] AS [Warehouse] ON  [Manifest].WarehouseUID=[Warehouse].UID
                          LEFT JOIN [WMS_Manifest_Item_List] AS [ManifestItem] ON [Manifest].UID=[ManifestItem].ManifestUID AND [ManifestItem].Status>0                  
                          LEFT JOIN [WMS_Bol] AS [Bol] ON [Bol].ManifestUID=[Manifest].UID AND [Bol].Status  >0
                          LEFT JOIN [WMS_Vessel] AS [Vessel] ON [Vessel].BolUID=[Bol].UID AND [Vessel].Status  >0 
						  INNER JOIN [YAEP_Party] AS [Party] ON [Party].UID=[Manifest].PartyUID
						  INNER JOIN [YAEP_Party_Type_Relation] AS [PartyRelation] ON [PartyRelation].PartyUID=[Manifest].PartyUID and [PartyRelation].Status>0
						  INNER JOIN [YAEP_Party_Type] AS [PartyType] on [PartyType].UID=[PartyRelation].TypeUID 
						  {0}
						  Group by [Manifest].ID,[Manifest].UID,[Manifest].Name ,[Manifest].Type,[Manifest].Status,[Party].Name,[Manifest].RefNo,[Manifest].PartyUID,
                                    [Manifest].WarehouseUID,[Manifest].Createdon
                          ORDER BY [Manifest].Createdon DESC";
            try
            {
                query = string.Format(query, this.getCondition(Parameters));
                rs.Content = this._Handler.Instance.Query<R>(
                query, Parameters);
                foreach (var item in rs.Content)
                {
                    item.StatusName = item.Status.ToString();
                    item.TypeName = item.Type.ToString();
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

        public IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestStatus status, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            //try
            //{
            var _manifest = this._Handler.UpdateByDynamicConditions(
                new { Status = (int)status, ModifiedBy = modifiedBy, ModifiedOn = DateTime.UtcNow },
                new { UID = manifestUID });
            rs.Content = _manifest;
            rs.Success = true;
            //}
            //catch (Exception ex)
            //{
            //    rs.Message = ex.Message;
            //    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
            //    rs.Success = false;
            //    rs.InnerException = ex;
            //    this.OnExpcetion(ex);
            //}
            return rs;
        }

        public IActionResult<IManifestModel> GetData(object condition)
        {
            var rs = ActionResultTemplates.Result<IManifestModel>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > (int)ManifestStatus.Void).FirstOrDefault();
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
                throw ex;
            }
            return rs;
        }

        public IActionResult<IEnumerable<IManifestModel>> GetDataFromBOL(IEnumerable<Guid> bolUIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IManifestModel>>();
            try
            {
                var query = @"SELECT WM.* FROM WMS_Manifest [WM]
                              INNER JOIN WMS_BOL AS [BOL] ON [WM].UID=[BOL].ManifestUID AND [WM].Status>0
                              WHERE [BOL].Status>0 AND [BOL].UID in @bolUIDs";
                rs.Content = this._Handler.Instance.Query<ManifestInnerModel>(query, new { bolUIDs = bolUIDs });
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
