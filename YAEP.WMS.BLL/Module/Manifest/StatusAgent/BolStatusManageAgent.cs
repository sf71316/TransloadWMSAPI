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
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class BolStatusManageAgent : StatusProcessAgent
    {
        internal BolStatusManageAgent(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }
        internal IActionResult<bool> ChangeBolStatus(Guid bolUID, BolStatus bolStatus)
        {
            return this.Repositorys.BolRepository.ChangeBolStatus(bolUID, bolStatus);
        }
        internal IActionResult<bool> ChangeBolStatus(IEnumerable<Guid> bolUID, BolStatus bolStatus,string modifiedBy)
        {
            return this.Repositorys.BolRepository.BatchChangeBolStatus(bolUID, bolStatus);
        }
        internal IActionResult<bool> ChangeBolStatusByManifest(Guid manifestUID, BolStatus bolStatus)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var Result = new List<IActionResult<bool>>();
                var parameters = new BolSearchInnerParameters();
                parameters.ManifestUID = manifestUID;
                var bols = this.Repositorys.BolRepository.GetList(parameters);
                foreach (var bolModel in bols.Content)
                {
                    Result.Add(this.Repositorys.BolRepository.ChangeBolStatus(bolModel.UID, bolStatus));
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

        public IActionResult<IEnumerable<string>> CheckVesselAssignedComplete(Guid bolUID)
        {
            return this.Repositorys.BolManager.GetAllVesslWorkPayload(bolUID);
        }
    }
}
