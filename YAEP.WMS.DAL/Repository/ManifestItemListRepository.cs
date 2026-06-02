using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public class ManifestItemListRepository<T> : AbstractRepository<T>, IManifestItemListRepository where T : class, IManifestItemListModel
    {
        public ManifestItemListRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> Add(IEnumerable<IManifestItemListModel> Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            var dt = this.ToDataTable<IManifestItemListModel>(Model);
            //try
            //{
            rs.Content = true;
            var query = @"INSERT INTO [dbo].[WMS_Manifest_Item_List]
           ([UID]
           ,[ID]
           ,[Name]
           ,[Type]
           ,[ManifestUID]
           ,[ItemUID]
           ,[PackageUID]
           ,[PackageQty]
           ,[Volume]
           ,[Weight]
           ,[Status]
           ,[Description]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
     VALUES
           (@UID
           ,@ID
           ,@Name
           ,@Type
           ,@ManifestUID
           ,@ItemUID
           ,@PackageUID
           ,@PackageQty
           ,@Volume
           ,@Weight
           ,@Status
           ,@Description
           ,@CreatedBy
           ,@CreatedOn
           ,@ModifiedBy
           ,@ModifiedOn)";

            SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
            cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
            cmd.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");
            cmd.Parameters.Add("@ManifestUID", SqlDbType.UniqueIdentifier, 16, "ManifestUID");
            cmd.Parameters.Add("@ItemUID", SqlDbType.UniqueIdentifier, 16, "ItemUID");
            cmd.Parameters.Add("@PackageUID", SqlDbType.UniqueIdentifier, 16, "PackageUID");
            cmd.Parameters.Add("@PackageQty", SqlDbType.Int, 4, "PackageQty");
            cmd.Parameters.Add("@Volume", SqlDbType.Decimal, 17, "Volume");
            cmd.Parameters.Add("@Weight", SqlDbType.Decimal, 17, "Weight");
            cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000, "Description");
            cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
            cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
            cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
            cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");
            rs = this.BatchInsertTable(dt, cmd);
            return rs;
        }
        public IActionResult<bool> Delete(object parameters)
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
        public IActionResult<bool> Delete(IManifestItemListDeleteParameters parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(new { UID = parameters.UID });
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

        public IActionResult<bool> Update(IManifestItemListModel Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(Model, new { UID = Model.UID });
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

        public IActionResult<IEnumerable<IManifestItemListModel>> GetManifestItemList(Guid Manifestuid)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IManifestItemListModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollection("ManifestUID", Manifestuid).Where(p => p.Status > 0);
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
        IActionResult<IEnumerable<ICalVesselAddItemInnerModel>> IManifestItemListRepository.GetManifestItemListByGroupItem(IGetAddItemListparameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ICalVesselAddItemInnerModel>>();
            try
            {
                var query = @"SELECT [WMIL].UID ManifestItemUID,[WMIL].ItemUID,[WMIL].PackageUID,[WMIL].PackageQty    FROM
                            [WMS_Manifest] as [WM]
                            INNER JOIN [WMS_Manifest_Item_List] as [WMIL] ON [WM].UID=[WMIL].ManifestUID
                            INNER JOIN [WMS_BOL] as [WB] ON [WB].ManifestUID=[WM].UID
                            INNER JOIN [WMS_Vessel] as [WV] ON [WV].BolUID=[WB].UID
                            WHERE [WMIL].Status>0 AND [WM].Status>0 AND [WB].Status>0 AND [WV].Status>0 {0}
                            GROUP BY [WMIL].UID ,[WMIL].ItemUID,[WMIL].PackageUID,[WMIL].PackageQty 
                            ";
                query = string.Format(query, this.getCondition(parameters));
                rs.Content = this._Handler.Instance.Query<CalVesselAddItemInnerModel>(query, parameters);
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
        private string getCondition(IGetAddItemListparameters Parameters)
        {
            List<string> Condition = new List<string>();
            if (Parameters.manifestuid.HasValue)
            {
                Condition.Add("([WM].UID =@manifestuid)");
            }
            else if (Parameters.vesseluid.HasValue)
            {
                Condition.Add("([WV].UID = @vesseluid)");
            }
            return Condition.Count > 0 ? " AND " + string.Join("AND", Condition) : "";
        }
        public IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestItemListStatus status, string modifiedBy = "")
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE WMS_Manifest_Item_List SET Status=@Status ,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn
                              WHERE ManifestUID=@ManifestUID AND Status>@InactiveStatus";
                var _manifest = this._Handler.Instance.Execute(query, new
                {
                    Status = status,
                    ManifestUID = manifestUID,
                    modifiedBy = modifiedBy,
                    ModifiedOn = DateTime.UtcNow,
                    InactiveStatus = ManifestItemListStatus.Void
                });
                rs.Content = _manifest > 0;
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
        public IActionResult<bool> BatchChangeManifestStatus(IEnumerable<Guid> manifestitemUID, ManifestItemListStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = @"UPDATE WMS_Manifest_Item_List SET Status=@Status 
                              WHERE UID in @manifestitemUID AND Status>@InactiveStatus";
                var index = 0;
                var grp = manifestitemUID.GroupBy(g => index++ / 1000);
                foreach (var items in grp)
                {
                    rs.Content &= this._Handler.Instance.Execute(query, new
                    {
                        Status = status,
                        manifestitemUID = items,
                        InactiveStatus = ManifestItemListStatus.Void
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
        public IActionResult<bool> ChangeManifestStatusByBol(Guid BolUID, ManifestItemListStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"
                UPDATE ManifestItem SET Status=@Status FROM WMS_BOL AS [BOL] 
                INNER JOIN WMS_Vessel AS [Vessel] ON [BOL].UID=[Vessel].BolUID
                INNER JOIN WMS_Vessel_Manifest AS [VesselManifest] ON [Vessel].UID=[VesselManifest].VesselUID
                INNER JOIN WMS_Manifest_Item_List AS [ManifestItem] ON [VesselManifest].ManifestItemUID=[ManifestItem].UID
                WHERE [BOL].Status>0 AND [Vessel].Status>0 AND [VesselManifest].Status>0 AND [ManifestItem].Status>0
                AND [BOL].UID=@BolUID";
                var result = this._Handler.Instance.Execute(query, new { BolUID = BolUID, Status = status }) > 0;
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

        public IActionResult<int> GetManifestItemListByPackageQty(Guid packageUID)
        {

            var rs = ActionResultTemplates.Result<int>();
            try
            {
                var query = @"SELECT SUM(ISNULL(PackageQty,0)) from WMS_Manifest_Item_List
                              WHERE PackageUID=@packageuid and Status>0";
                var rs2 = this._Handler.Instance.QueryFirst<int?>(query, new { packageuid = packageUID });
                rs.Content = rs2.HasValue ? rs2.Value : 0;
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

        public IActionResult<IManifestItemListModel> GetManifestItemInfo(object condition)
        {

            var rs = ActionResultTemplates.Result<IManifestItemListModel>();
            try
            {
                rs.Content = this._Handler.RetrieveByDynamicConditions(condition);
                if (rs.Content != null && rs.Content.Status > 0)
                {
                    rs.Success = true;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = "not find manifest item info data";
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

        public IActionResult<IEnumerable<ICheckManifestItemStatusResultModel>> GetCheckManifestItemStatusResult(Guid manifestUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ICheckManifestItemStatusResultModel>>();
            try
            {
                var query = @"SELECT [WM].Status ManifestStatus,[WMIL].Status ManifestItemStatus
                ,[WB].UID BolUID,[WVM].VesselUID,
                [WMIL].UID ManifestItemUID,[WVM].ItemUID,[WVM].Qty CompleteQty,[WVM].PackageUID CompletePackageUID,
                [WMIL].PackageQty OriginalQty,[WMIL].PackageUID OriginalPackageUID 
                FROM WMS_BOL AS [WB]
                INNER JOIN WMS_Vessel AS [WV] ON [WB].UID=[WV].BolUID 
                INNER JOIN WMS_Vessel_Manifest AS [WVM] ON [WVM].VesselUID=[WV].UID
                INNER JOIN WMS_Manifest_Item_List AS [WMIL] ON [WVM].ManifestItemUID=[WMIL].UID
                INNER JOIN WMS_Manifest AS [WM] ON [WM].UID=[WMIL].ManifestUID
                WHERE [WB].Status>0 AND [WV].Status>0 AND [WVM].Status>0 AND [WMIL].Status>0  AND [WM].UID=@ManifestUID
                ORDER BY [WMIL].UID";
                rs.Content = this._Handler.Instance.Query<CheckManifestItemStatusResultModel>(query, new { manifestUID = manifestUID });
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
