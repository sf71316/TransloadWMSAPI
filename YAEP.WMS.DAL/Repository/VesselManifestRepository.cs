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
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class VesselManifestRepository<T> : AbstractRepository<T>, IVesselManifestRepository
         where T : class, IVesselManifestModel
    {
        public VesselManifestRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> AddVesselManifest(IEnumerable<IVesselManifestModel> collection)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"
                INSERT INTO [dbo].[WMS_Vessel_Manifest]
           (      [UID]
				, [ID]
				, [Name]
				, [Type]
				, [RefNo]
				, [BolUID]
				, [ManifestItemUID]
				, [PartyUID]
				, [VesselUID]
				, [ItemUID]
				, [PackageUID]
				, [Volume]
				, [Weight]
				, [Qty]
				, [Status]
				, [Description]
				, [CreatedBy]
				, [CreatedOn]
				, [ModifiedBy]
				, [ModifiedOn]        )
                      VALUES
                    (
				  @UID
				, @ID
				, @Name
				, @Type
				, @RefNo
				, @BolUID
				, @ManifestItemUID
				, @PartyUID
				, @VesselUID
				, @ItemUID
				, @PackageUID
				, @Volume
				, @Weight
				, @Qty
				, @Status
				, @Description
				, @CreatedBy
				, @CreatedOn
				, @ModifiedBy
				, @ModifiedOn   
		            )
                ";
                rs.Content = this._Handler.Instance.Execute(query, collection) > 0;
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
        public IActionResult<bool> BatchAddVesselManifest(IEnumerable<IVesselManifestModel> Collection)
        {

            var rs = ActionResultTemplates.Result<bool>();
            var dt = this.ToDataTable<IVesselManifestModel>(Collection);
            var query = @"INSERT INTO [dbo].[WMS_Vessel_Manifest]
           ([UID]
           ,[ID]
           ,[Name]
           ,[Type]
           ,[RefNo]
           ,[BolUID]
           ,[ManifestItemUID]
           ,[PartyUID]
           ,[VesselUID]
           ,[ItemUID]
           ,[PackageUID]
           ,[Volume]
           ,[Weight]
           ,[Qty]
           ,[Status]
           ,[Description]
           ,[CreatedBy]
           ,[CreatedOn]
           ,[ModifiedBy]
           ,[ModifiedOn])
     VALUES
           (
           @UID, 
           @ID, 
           @Name, 
           @Type, 
           @RefNo,
           @BolUID, 
           @ManifestItemUID,
           @PartyUID, 
           @VesselUID,
           @ItemUID, 
           @PackageUID, 
           @Volume, 
           @Weight,
           @Qty,
           @Status, 
           @Description,
           @CreatedBy,
           @CreatedOn,
           @ModifiedBy,
           @ModifiedOn)";
            SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
            cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
            cmd.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");
            cmd.Parameters.Add("@RefNo", SqlDbType.NVarChar, 100, "RefNo");
            cmd.Parameters.Add("@BolUID", SqlDbType.UniqueIdentifier, 16, "BolUID");
            cmd.Parameters.Add("@ManifestItemUID", SqlDbType.UniqueIdentifier, 16, "ManifestItemUID");
            cmd.Parameters.Add("@PartyUID", SqlDbType.UniqueIdentifier, 16, "PartyUID");
            cmd.Parameters.Add("@VesselUID", SqlDbType.UniqueIdentifier, 16, "VesselUID");
            cmd.Parameters.Add("@ItemUID", SqlDbType.UniqueIdentifier, 16, "ItemUID");
            cmd.Parameters.Add("@PackageUID", SqlDbType.UniqueIdentifier, 16, "PackageUID");
            cmd.Parameters.Add("@Volume", SqlDbType.Decimal, 17, "Volume");
            cmd.Parameters.Add("@Weight", SqlDbType.Decimal, 17, "Weight");
            cmd.Parameters.Add("@Qty", SqlDbType.Int, 4, "Qty");
            cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000, "Description");
            cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
            cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
            cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
            cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");
            rs = this.BatchInsertTable(dt, cmd);
            return rs;
        }
        public IActionResult<bool> AddVesselManifest(IVesselManifestModel Model)
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

        public IActionResult<bool> ChangeVesselManifestStatus(Guid vesselUID, VesselManifestStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE wvm  SET Status=@Status 
                FROM WMS_Vessel_Manifest AS [WVM]
                INNER JOIN WMS_Vessel AS [WV] ON [WVM].VesselUID=[WV].UID
                WHERE [WV].UID=@vesselUID AND [WV].Status>0 AND [WVM].Status>0";
                var rs1 = this._Handler.Instance.Execute(query,
                    new { Status = (int)status, vesselUID = vesselUID }) > 0;
                rs.Content = rs1;
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
        public IActionResult<bool> BatchChangeVesselManifestStatus(IEnumerable<Guid> vesselUID, VesselManifestStatus status, string modifiedBy = "")
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = @"UPDATE wvm  SET Status=@Status ,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn
                FROM WMS_Vessel_Manifest AS [WVM]
                INNER JOIN WMS_Vessel AS [WV] ON [WVM].VesselUID=[WV].UID
                WHERE [WV].UID IN @UID AND [WV].Status>0 AND [WVM].Status>0";

                var index = 0;
                var grp = vesselUID.GroupBy(g => index++ / 2000);
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
        public IActionResult<bool> ChangeVesselManifestStatusByBOL(Guid bolUID, VesselManifestStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE wvm  SET Status=@Status 
                FROM WMS_Vessel_Manifest AS [WVM]
                INNER JOIN WMS_Vessel AS [WV] ON [WVM].VesselUID=[WV].UID
                INNER JOIN WMS_BOL AS [WB] ON [WV].BolUID=[WB].UID
                WHERE [WB].UID=@bolUID AND [WVM].Status>0 AND [WV].Status>0 AND [WB].Status>0";
                var rs1 = this._Handler.Instance.Execute(query,
                    new { Status = (int)status, bolUID = bolUID }) > 0;
                rs.Content = rs1;
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
        public IActionResult<bool> ChangeVesselManifestStatusByVesselManifestUID(Guid vessemanifestlUID, VesselManifestStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = @"UPDATE wvm  SET Status=@Status 
                FROM WMS_Vessel_Manifest AS [WVM]
                INNER JOIN WMS_Vessel AS [WV] ON [WVM].VesselUID=[WV].UID
                --INNER JOIN WMS_BOL AS [WB] ON [WV].BolUID=[WB].UID
                WHERE [WVM].UID=@vessemanifestlUID AND [WV].Status>0 AND [WVM].Status>0";
                var rs1 = this._Handler.Instance.Execute(query,
                    new { Status = (int)status, vessemanifestlUID = vessemanifestlUID }) > 0;
                rs.Content = rs1;
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
        public IActionResult<bool> DeleteVesselManifest(object Parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(Parameters);
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
        public IActionResult<bool> DeleteVesselManifest(IVesselManifestDeleteParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.Delete(Parameters.UID);
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

        public IActionResult<IEnumerable<IVesselManifestViewModel>> GetList(IVesselManifestSearchParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IVesselManifestViewModel>>();
            try
            {
                var query = @"SELECT [WVM].*,[WM].Type as 'ManifestType'FROM [WMS_Vessel_Manifest] AS [WVM]
                    INNER JOIN [WMS_Vessel] AS [WV] ON [WVM].VesselUID=[WV].UID
                    INNER JOIN [WMS_BOL] AS [WB] ON [WB].UID=[WV].BolUID
                    INNER JOIN [WMS_Manifest] AS[WM] ON [WM].UID=[WB].ManifestUID
                    {0}";
                query = string.Format(query, this.getSearchCondition(Parameters));
                rs.Content = this._Handler.Instance.Query<VesselManifestInnerViewModel>(query, Parameters);
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
        public IActionResult<IEnumerable<IVesselManifestModel>> GetListByBol(IEnumerable<Guid> bolUIDs)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IVesselManifestModel>>();
            try
            {
                var query = @"SELECT [WVM].*,[WM].Type as 'ManifestType'FROM [WMS_Vessel_Manifest] AS [WVM]
                    INNER JOIN [WMS_Vessel] AS [WV] ON [WVM].VesselUID=[WV].UID
                    INNER JOIN [WMS_BOL] AS [WB] ON [WB].UID=[WV].BolUID
                    INNER JOIN [WMS_Manifest] AS[WM] ON [WM].UID=[WB].ManifestUID
                    WHERE [WV].Status>0 AND [WB].Status>0 AND  [WB].UID in @BolUIDs";
                rs.Content = this._Handler.Instance.Query<VesselManifestInnerViewModel>(query, new { BolUIDs = bolUIDs });
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

        public IActionResult<IEnumerable<IVesselManifestModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IVesselManifestModel>>();
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

        public IActionResult<IManifestModel> GetManifestInfo(Guid VesselManifestItemUID)
        {

            var rs = ActionResultTemplates.Result<IManifestModel>();
            try
            {
                var query = @"SELECT DISTINCT [WM].* FROM WMS_Manifest AS [WM] 
			        INNER JOIN WMS_BOL AS [WB] ON [WM].UID=[WB].ManifestUID
			        INNER JOIN WMS_Vessel As [WV] ON [WV].BolUID=[WB].UID
			        INNER JOIN WMS_Vessel_Manifest AS [WVM] ON [WVM].VesselUID=[WV].UID
			        WHERE [WVM].UID=@UID AND [WVM].Status>0 AND [WM].Status>0 AND [WB].Status>0 ";
                rs.Content = this._Handler.Instance.Query<ManifestInnerModel>(query,
                            new { UID = VesselManifestItemUID }).FirstOrDefault();
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

        public IActionResult<dynamic> GetPartyBolInfo(Guid vesselUID)
        {

            var rs = ActionResultTemplates.Result<dynamic>();
            try
            {
                var query = @"SELECT DISTINCT [WM].PartyUID,[WB].UID 'BolUID' FROM WMS_Vessel AS [WV] 
                INNER JOIN WMS_BOL AS [WB] ON [WV].BolUID=[WB].UID
                INNER JOIN WMS_Manifest AS [WM] ON [WM].UID=[WB].ManifestUID
                WHERE [WM].Status>0 AND [WV].Status>0 AND [WB].Status>0
                AND [WV].UID=@VesselUID";
                rs.Content = this._Handler.Instance.Query(query, new { vesselUID = vesselUID }).FirstOrDefault();
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

        public IActionResult<IEnumerable<ICalVesselAddItemInnerModel>> GetVesselAssignItemList(IGetAddItemListparameters parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ICalVesselAddItemInnerModel>>();
            try
            {
                var query = @"
                            
                 SELECT [WVM].ManifestItemUID,[WVM].ItemUID,[WVM].PackageUID,SUM([WVM].Qty) PackageQty FROM
                [WMS_Manifest] as[WM]
                INNER JOIN [WMS_BOL] as [WB] on [WM].UID=[WB].ManifestUID
                INNER JOIN　[WMS_Vessel_Manifest] as [WVM] ON [WB].UID=[WVM].[BolUID]
                {0}
                WHERE  [WM].Status>0 AND [WB].Status>0 AND [WVM].Status>0 {{0}}
                GROUP by [WVM].ManifestItemUID,[WVM].ItemUID,[WVM].PackageUID
                                ";
                if (parameters.vesseluid.HasValue)
                {
                    query = string.Format(query, @"INNER JOIN 
                        (SELECT [WB].ManifestUID,[WV].UID 'VesselUID' FROM [WMS_BOL] AS [WB] 
                        INNER JOIN [WMS_Vessel] AS [WV] ON [WB].UID=[WV].BolUID
                        WHERE [WV].Status>0 AND [WB].Status>0 and [WV].UID=@VesselUID
                        ) T ON T.ManifestUID=[WM].UID");
                }
                else
                {
                    query = string.Format(query, "");

                }
                query = string.Format(query, this.getCondition(parameters));
                rs.Content = this._Handler.Instance.Query<CalVesselAddItemInnerModel>(query,
                            parameters);
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

        public IActionResult<IEnumerable<IVesselManifestItemListViewModel>> GetVesselManifestItemList(IVesselManifestSearchParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IVesselManifestItemListViewModel>>();
            try
            {
                List<IVesselManifestItemListViewModel> Result = new List<IVesselManifestItemListViewModel>();
                var collection = this.GetList(Parameters);
                foreach (var item in collection.Content)
                {
                    var r = new VesselManifestItemListViewInnerModel();
                    r.StatusName = ((VesselManifestStatus)item.Status).ToString();
                    r.ManifestItemUID = item.ManifestItemUID;
                    r.ItemUID = item.ItemUID;
                    r.PackageUID = item.PackageUID;
                    r.ReceiveQty = item.Qty;
                    r.UID = item.UID;
                    Result.Add(r);
                }
                rs.Content = Result;
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
                Condition.Add("([WM].UID= @manifestuid)");
            }
            //else if (Parameters.vesseluid.HasValue)
            //{
            //    Condition.Add("([WVM].VesselUID = @vesseluid)");
            //}
            return Condition.Count > 0 ? " AND " + string.Join("AND", Condition) : "";
        }
        private string getSearchCondition(IVesselManifestSearchParameters parameters)
        {
            List<string> condition = new List<string>();
            if (parameters.VesselUID.HasValue)
            {
                condition.Add("([WVM].VesselUID=@VesselUID)");
            }
            if (parameters.BOLUID.HasValue)
            {
                condition.Add("([WB].UID=@BOLUID)");
            }
            if (parameters.VesselManifestUID.HasValue)
            {
                condition.Add("([WVM].UID=@VesselManifestUID)");
            }
            if (parameters.ManifestItemUID != null && parameters.ManifestItemUID.Length > 0)
            {
                condition.Add("([WVM].ManifestItemUID in @ManifestItemUID)");
            }
            condition.Add("([WVM].Status>0)");
            condition.Add("([WV].Status>0)");
            return condition.Count > 0 ? "WHERE " + string.Join("AND", condition) : "";
        }
    }
}
