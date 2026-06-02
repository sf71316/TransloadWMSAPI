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
    public class VesselRepository<T> : AbstractRepository<T>, IVesselRepository
         where T : class, IVesselModel
    {
        public VesselRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
        public IActionResult<bool> AddVessel(IVesselModel Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(Model);
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
        public IActionResult<bool> BatchAddVessel(IEnumerable<IVesselModel> Collection)
        {

            var rs = ActionResultTemplates.Result<bool>();
            var dt = this.ToDataTable<IVesselModel>(Collection);
            //try
            //{
            var query = @"
INSERT INTO [dbo].[WMS_Vessel]
           ([UID]
           ,[ID]
           ,[Name]
           ,[Type]
           ,[RefNo]
           ,[BolUID]
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
           @Status, 
           @Description,
           @CreatedBy,
           @CreatedOn, 
           @ModifiedBy,
           @ModifiedOn)";
            rs.Content = true;
            var index = 0;
            var grp = Collection.GroupBy(g => index++ / 2000);
            SqlCommand cmd = new SqlCommand(query, this._Handler.Instance.Connection as SqlConnection);
            cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier, 16, "UID");
            cmd.Parameters.Add("@ID", SqlDbType.NVarChar, 100, "ID");
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100, "Name");
            cmd.Parameters.Add("@Type", SqlDbType.Int, 4, "Type");
            cmd.Parameters.Add("@RefNo", SqlDbType.NVarChar, 100, "RefNo");
            cmd.Parameters.Add("@BolUID", SqlDbType.UniqueIdentifier, 16, "BolUID");
            cmd.Parameters.Add("@Status", SqlDbType.Int, 4, "Status");
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 1000, "Description");
            cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50, "CreatedBy");
            cmd.Parameters.Add("@CreatedOn", SqlDbType.DateTime, 8, "CreatedOn");
            cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 50, "ModifiedBy");
            cmd.Parameters.Add("@ModifiedOn", SqlDbType.DateTime, 8, "ModifiedOn");
            rs = this.BatchInsertTable(dt, cmd);

            return rs;
        }
        public IActionResult<bool> ChangeVesselStatus(Guid vesselguid, VesselStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var _manifest = this._Handler.UpdateByDynamicConditions(
                    new { Status = (int)status },
                    new { UID = vesselguid });
                rs.Content =
                rs.Success = _manifest;
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
        public IActionResult<bool> BatchChangeVesselStatus(IEnumerable<Guid> vesselguid, VesselStatus status, string modifiedBy = "")
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = "UPDATE WMS_Vessel SET Status=@Status,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn WHERE UID IN @UID AND Status>0";
                var index = 0;
                var grp = vesselguid.GroupBy(g => index++ / 2000);
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
        public IActionResult<bool> DeleteVessel(IVesselDeleteParamters Parameters)
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
        public IActionResult<bool> DeleteVessel(object Parameters)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(Parameters);
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
        public IActionResult<bool> EditVessel(dynamic Model)
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
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
        public IActionResult<IEnumerable<IVesselModel>> GetList(IVesselSearchParameters Parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IVesselModel>>();
            string query = @"SELECT [WV].* FROM  WMS_Manifest AS [WM]
                            INNER JOIN WMS_BOL AS [WB] ON [WM].UID=[WB].ManifestUID
                            INNER JOIN WMS_Vessel AS [WV]  ON [WV].BOLUID=[WB].UID
                            WHERE [WV].Status>0 AND [WB].Status>0 AND [WM].Status>0 {0} ";
            try
            {
                query = string.Format(query, this.getSearchCondition(Parameters));
                rs.Content = this._Handler.Instance.Query<T>(query, Parameters);
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
        public IActionResult<IVesselModel> GetData(object condition)
        {
            var rs = ActionResultTemplates.Result<IVesselModel>();
            try
            {

                rs.Content = this._Handler.RetrieveByDynamicConditions(condition);
                if (rs.Content.Status > 0)
                {
                    rs.Success = true;
                }
                else
                {
                    rs.Success = false;
                    rs.Content = null;
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
        public IActionResult<IEnumerable<IVesselModel>> GetList(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IVesselModel>>();
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
        private string getSearchCondition(IVesselSearchParameters parameters)
        {
            List<string> condition = new List<string>();
            if (parameters.BolUID != Guid.Empty)
            {
                condition.Add("([WB].UID=@BolUID)");
            }
            if (!string.IsNullOrEmpty(parameters.RefNo))
            {
                condition.Add("([WM].RefNo=@RefNo)");
            }

            return condition.Count > 0 ? " AND " + string.Join("AND", condition) : "";
        }
    }
}
