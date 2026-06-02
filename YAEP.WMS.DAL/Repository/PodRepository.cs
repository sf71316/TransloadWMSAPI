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
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class PodRepository<T> : AbstractRepository<T>, IPodRepository where T : class, IPodModel
    {
        public PodRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }

        public IActionResult<bool> AddPod(IPodModel Model)
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

        public IActionResult<bool> ChangePodStauts(Guid poduid, PodStatus status)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.UpdateByDynamicConditions(new { Status = (int)status }, new { UID = poduid });
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

        public IActionResult<bool> DeletePod(Guid PodUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.Delete(PodUID);
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
        public IActionResult<bool> DeletePodFromDb(object condition)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteFromDatabaseByDynamicConditions(condition) > 0;
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

        public IActionResult<IEnumerable<IPodModel>> GetPod(Guid[] PodUIDs)
        {

            List<IPodModel> payloadModels = new List<IPodModel>();
            var rs = ActionResultTemplates.Result<IEnumerable<IPodModel>>();
            var index = 0;
            var grp = PodUIDs.GroupBy(g => index++ / 2000);
            var query = "SELECT * FROM WMS_Pod WHERE Status >@Status AND UID IN @UID ";

            foreach (var items in grp)
            {
                payloadModels.AddRange(this._Handler.Instance.Query<PodInnerModel>(query,
                    new { UID = items, Status = (int)PayloadStatus.Inactive }));
            }
            rs.Success = true;
            rs.Content = payloadModels;

            return rs;
        }
        public IActionResult<bool> UnPack(IEnumerable<Guid> PodUIDs)
        {

            var rs = ActionResultTemplates.Result<bool>();

            rs.Content = true;
            var query = "UPDATE WMS_Pod SET IsPack=0 WHERE UID in @UID";
            var index = 0;
            var grp = PodUIDs.GroupBy(g => index++ / 2000);
            foreach (var items in grp)
            {
                rs.Content &= this._Handler.Instance.Execute(query, new { UID = items }) > 0;
            }
            rs.Success = rs.Content;

            return rs;
        }
        public IActionResult<bool> UnPack(Guid PodUID)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_Pod SET IsPack=0 WHERE UID=@UID";
                rs.Content = this._Handler.Instance.Execute(query, new { UID = PodUID }) > 0;
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

        public IActionResult<bool> UpdatePod(IPodModel Model)
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
    }
}
