using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.BLL.Module
{
    internal class VesselStatusManageAgent : StatusProcessAgent
    {
        public VesselStatusManageAgent(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }
        internal IActionResult<bool> ChangeVesselStatus(Guid VesselUID, VesselStatus status, VesselManifestStatus vesselManifestStatus)
        {
            var rs1 = this.Repositorys.VesselRepository.ChangeVesselStatus(VesselUID, status);
            var rs2 = this.Repositorys.VesselManifestRepository.ChangeVesselManifestStatus(VesselUID, vesselManifestStatus);
            rs1.Content &= rs2.Content;
            rs1.Success &= rs2.Success;
            return rs1;
        }
        internal IActionResult<bool> BatchChangeVesselStatus(IEnumerable<Guid> vesselUID, VesselStatus vesselStatus,string modifiedBy)
        {
            return this.Repositorys.VesselRepository.BatchChangeVesselStatus(vesselUID, vesselStatus, modifiedBy);
        }
        internal IActionResult<bool> BatchChangeVesselManiestStatus(IEnumerable<Guid> vesselUID, VesselManifestStatus vesselmanifestStatus,string modifiedBy)
        {
            return this.Repositorys.VesselManifestRepository.BatchChangeVesselManifestStatus(vesselUID, vesselmanifestStatus, modifiedBy);
        }
        internal IActionResult<bool> ChangeVesselByBol(Guid BolUID, VesselStatus vesselStatus)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var Result = new List<IActionResult<bool>>();
                var parameters = new VesselSearchInnerParameters();
                parameters.BolUID = BolUID;
                var vessels = this.Repositorys.VesselRepository.GetList(parameters);
                foreach (var vesselModel in vessels.Content)
                {
                    Result.Add(this.Repositorys.VesselRepository.ChangeVesselStatus(vesselModel.UID, vesselStatus));
                }
                if (!Result.All(p => p.Success))
                {
                    rs.Success = false;
                    rs.Message = string.Join(",", Result.Where(x => !x.Success).Select(s => s.Message));
                }
                else
                {
                    rs.Success = true;
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
        //RETEST ChangeVesselManifestStatus 分離出ChangeVesselManifestStatusByBOL 方法
        internal IActionResult<bool> ChangeVesselManifestStatus(Guid vesseluid, VesselManifestStatus vesselManifestStatus)
        {
            return this.Repositorys.VesselManifestRepository.ChangeVesselManifestStatus(vesseluid, vesselManifestStatus);
        }
        //RETEST
        internal IActionResult<bool> ChangeVesselManifestStatusByBOL(Guid boluid, VesselManifestStatus vesselManifestStatus)
        {
            return this.Repositorys.VesselManifestRepository.ChangeVesselManifestStatusByBOL(boluid, vesselManifestStatus);
        }
    }
}
