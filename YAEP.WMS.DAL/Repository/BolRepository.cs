using Dapper;
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
    public class BolRepository<T> : AbstractRepository<T>, IBolRepository where T : class, IBolModel
    {
        public BolRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> AddBol(IBolModel Model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            //try
            //{
            rs.Content = this._Handler.CreateByDynamic(Model);
            rs.Success = rs.Content;
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
        public IActionResult<bool> BatchChangeBolStatus(IEnumerable<Guid> bolUID, BolStatus status, string modifiedBy = "")
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = true;
                var query = "UPDATE WMS_BOL SET Status=@Status,ModifiedBy=@modifiedBy,ModifiedOn=@ModifiedOn WHERE UID IN @UID AND Status>0";
                var index = 0;
                var grp = bolUID.GroupBy(g => index++ / 2000);
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
        public IActionResult<bool> ChangeBolStatus(Guid bolUID, BolStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var _rs = this._Handler.UpdateByDynamicConditions(
                    new { Status = (int)status },
                    new { UID = bolUID });
                rs.Content = _rs;
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
        public IActionResult<bool> ChangeBolStatus(object condition, BolStatus status)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var _rs = this._Handler.UpdateByDynamicConditions(
                    new { Status = (int)status },
                    condition);
                rs.Content = _rs;
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
        public IActionResult<bool> DeleteBol(object condition)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(condition);
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

        public IActionResult<bool> EditBol(dynamic Model)
        {
            this._Handler.IsAllUpdate = false;

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

        public IActionResult<IBolViewModel> GetBol(object condition)
        {
            var rs = ActionResultTemplates.Result<IBolViewModel>();
            var query = @"SELECT * FROM WMS_BOL WHERE Status>0 AND UID=@UID";

            try
            {
                rs.Content = this._Handler.Instance.QueryFirst<BolViewInnerModel>(query, condition);
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

        public IActionResult<IEnumerable<IBolViewModel>> GetList(IBolSearchParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IBolViewModel>>();
            var query = @"SELECT * FROM WMS_BOL {0}";

            try
            {
                var param = new DynamicParameters();
                query = string.Format(query, this.getSearchCondition(parameters, param));
                rs.Content = this._Handler.Instance.Query<BolViewInnerModel>(query, param);
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
        public IActionResult<IEnumerable<string>> GetBolRefNo(IEnumerable<string> refNos)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<string>>();
            var query = @"SELECT RefNo FROM WMS_BOL WHERE Name IN @Refno AND Status>0";


            rs.Content = this._Handler.Instance.Query<string>(query, new
            {
                Refno = refNos.Select(x => x.ToNvarchar())
            });
            rs.Success = true;

            return rs;
        }
        public IActionResult<IEnumerable<IBolModel>> GetList(object condition)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IBolModel>>();
            //try
            //{
            rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition).Where(p => p.Status > 0);
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
        private string getSearchCondition(IBolSearchParameters Parameters, DynamicParameters param)
        {
            List<string> Condition = new List<string>();
            if (Parameters.ManifestUID != null)
            {
                Condition.Add("(ManifestUID=@ManifestUID )");
                param.Add("ManifestUID", Parameters.ManifestUID);
            }
            if (Parameters.RefNo != null)
            {
                Condition.Add("(RefNo IN @RefNo )");
                param.Add("RefNo", Parameters.RefNo.Select(x => x.ToNvarchar()));
            }
            Condition.Add("(Status>0)");
            return Condition.Count > 0 ? "WHERE " + string.Join("AND", Condition) : "";
        }
    }
}
