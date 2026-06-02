using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.BLL.Extension;
using System.Transactions;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IBolManager
    {
        public IActionResult<bool> DeleteBolAPI(IBolDeleteParameters parameters)
        {
            var maninfestInfos = this.Repository.GetDataFromBOL(parameters.UID);
            if (!maninfestInfos.Content.All(p => (int)p.Status < (int)ManifestStatus.Complete)
                || maninfestInfos.Content.Count() == 0)
            {
                var rs = ActionResultTemplates.OK();
                rs.Success = false;
                rs.Message = string.Format(Resource.MANIFEST_STATUS_FAILURE, ManifestStatus.Open.ToString());
                return rs;
            }
            using (var db = this.DbEntities.DbAdapter)
            {
                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                {
                    var rs = this.DeleteBol(parameters);
                    if (rs.Success)
                    {
                        db.Commit();
                    }
                    else
                    {
                        db.Rollback();
                    }
                    return rs;
                }
            }
        }
        public IActionResult<bool> DeleteBol(IBolDeleteParameters Parameters)
        {

            var rs = ActionResultTemplates.Result<bool>();
            var vminfo = this.VesselRepository.GetList(new { BOLUID = Parameters.UID });
            var wkinfo = this.WorkOrderRepository.GetList(new { VesselUID = vminfo.Content.Select(p => p.UID) });

            try
            {
                VesselDeleteInnerParamters param = new VesselDeleteInnerParamters();
                param.UID = vminfo.Content.Select(p => p.UID).ToArray();

                IActionResult<bool> rs1 = ActionResultTemplates.Result<bool>();
                IActionResult<bool> rs2 = ActionResultTemplates.Result<bool>();
                rs1 = this.DeleteVessel(param);
                if (rs1.Success)
                    rs2 = this.BolRepository.DeleteBol(Parameters);
                if (rs1.Success && rs2.Success)
                {
                    rs.Success = true;

                }
                else
                {
                    rs.Success = false;
                    rs.Message = rs1.Message + "," + rs2.Message;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }

            return rs;
        }

        public IActionResult<IBolViewModel> GetBol(object condition)
        {
            return this.BolRepository.GetBol(condition);
        }
        public IActionResult<bool> AddBol(IBolModel Model)
        {
            var _seq = this.SequenceAgent.GetBOLSeqence(Model.ManifestUID);
            Model.ID = _seq;
            return this.BolRepository.AddBol(Model);
        }
        public IActionResult<bool> EditBol(dynamic Model)
        {
            return this.BolRepository.EditBol(Model);
        }
        public IActionResult<IEnumerable<IBolViewModel>> GetBolList(IBolSearchParameters Parameters)
        {
            var collection = this.BolRepository.GetList(Parameters);
            if (collection.Success)
            {
                foreach (var item in collection.Content)
                {
                    item.StatusName = item.Status.ToString();
                }
            }
            return collection;
        }
        public IActionResult<IEnumerable<IBolModel>> GetBolList(object condition)
        {
            var collection = this.BolRepository.GetList(condition);
            return collection;
        }
        public IActionResult<bool> ChangeBolStatus(Guid bolUID, BolStatus status)
        {
            return this.BolRepository.ChangeBolStatus(bolUID, status);
        }
        public IActionResult<IBolModel> ApproveBol(Guid boluid)
        {
            return this.StatusCenter.ProcessBOL(boluid);
        }
        public IActionResult<bool> ForceApproveBol(Guid boluid, Guid warehouseUID, int manifestType)
        {
            return this.StatusCenter.ForceApproveBol(boluid, warehouseUID, manifestType);
        }
        public IActionResult<bool> BatchForceApproveBol(IEnumerable<Guid> boluids, Guid warehouseUID, int manifestType)
        {
            return this.StatusCenter.BatchForceApproveBol(boluids, warehouseUID, manifestType);
        }
        public IActionResult<IBolModel> RejectBol(Guid boluid)
        {
            var accountInfo = this.AuthProvider.GetAuthenticationInfo();
            return this.StatusCenter.RejectBOL(boluid, accountInfo.Account);
        }
        public IActionResult<bool> CheckHaveUnassignedTicket(Guid boluid)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var rs2 = this.TicketRepository.GetTicketByBol(boluid);
                rs.Content = rs2.Content.All(p => p.Status == (int)TicketStatus.Assigned);
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        public IActionResult<IEnumerable<string>> GetAllVesslWorkPayload(Guid bolUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<string>>();
            try
            {
                List<string> unCompleteList = new List<string>();
                VesselManifestSearchInnerParameters param = new VesselManifestSearchInnerParameters();
                param.BOLUID = bolUID;
                var vidata = this.VesselManifestRepository.GetList(param);
                if (vidata.Success && vidata.Content.Count() > 0)
                {
                    var pdata = this.WorkOrderPayloadRepository.GetList(new { VesselManifestUID = vidata.Content.Select(p => p.UID) });
                    var itemInfos = this.ProductCacheManager.GetItems(vidata.Content.Select(p => p.ItemUID));
                    foreach (var item in vidata.Content)
                    {
                        //總計Vessel Manifest 總數

                        //var vpkg = this.PackageManager.GetPackageTree(item.PackageUID);
                        var itemInfo = itemInfos.FirstOrDefault(p => p.UID == item.ItemUID);
                        var vptree = this.PackageCacheManager.GetMinPackage(item.PackageUID);
                        var ttlQty = this.PackageCacheManager.GetReceivePackageUomQuantity(item.PackageUID, vptree.UID, item.Qty).Content;
                        //計算被assigned 總數
                        var assignedpl = pdata.Content.Where(p => p.VesselManifestUID == item.UID);
                        var assignedTtlQty = assignedpl.Sum(s =>
                        {
                            //var apkg = this.PackageManager.GetPackageTree(s.PackageUID);
                            var aptree = this.PackageCacheManager.GetMinPackage(s.PackageUID);
                            return this.PackageCacheManager.GetReceivePackageUomQuantity(s.PackageUID, aptree.UID, s.Qty).Content;
                        });
                        if (ttlQty - assignedTtlQty != 0)
                        {
                            unCompleteList.Add(string.Format("Item#:{0} TTLQty:{1} -> TTLAssignedQty {2}", itemInfo.Name, ttlQty, assignedTtlQty));
                        }
                    }
                    rs.Content = unCompleteList;
                }
                else
                {
                    rs.Success = false;
                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_VESSEL;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }


    }
}
