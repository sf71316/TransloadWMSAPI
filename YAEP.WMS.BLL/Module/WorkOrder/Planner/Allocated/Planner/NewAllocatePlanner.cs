using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class NewAllocatePlanner : AbstractAllocatePlanner, IAllocatePlanner
    {
        public NewAllocatePlanner(AllocatedPlannerInitParameters allocatedPlannerInitParameters)
            : base(allocatedPlannerInitParameters)
        {

        }
        /// <summary>
        /// 提供內部系統自動配貨規劃清單
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IEnumerable<AllocatedPlannerResult> PlanByWMS(IEnumerable<AllocatedPlannerInnerParameter> parameters,
            bool passPackageVersion, bool isChinaWarehouse)
        {
            var result = new List<AllocatedPlannerResult>();
            var vesselManifestinfos = this._VesselManager.GetVesselManifest(
                new { UID = parameters.Select(p => p.VesselManifestUID) });
            var mInfo = this._VesselManager.GetManifestInfo(vesselManifestinfos.Content.FirstOrDefault().UID);

            //TODO WMS UI Order Type 使用Truckload
            if (mInfo.Content != null)
            {
                return this.ProcessPlanByWMS(mInfo.Content.WarehouseUID, vesselManifestinfos.Content, passPackageVersion, isChinaWarehouse);
            }
            else
            {
                return new List<AllocatedPlannerResult>();
            }
        }
        /// <summary>
        /// 提供外部系統要Allocated配貨規劃清單
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IEnumerable<AllocatedPlannerResult> ExternalOrderPlanByWMS(Guid warehouseUID,
            IEnumerable<IVesselManifestModel> parameters, bool passPackageVersion, bool isChinaWarehouse)
        {
            return this.ProcessPlanByWMS(warehouseUID, parameters, passPackageVersion, isChinaWarehouse);
        }

    }
}
