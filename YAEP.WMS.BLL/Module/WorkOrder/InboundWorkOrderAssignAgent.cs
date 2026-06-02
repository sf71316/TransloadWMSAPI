using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundWorkOrderAssignAgent : AbstractWorkOrderAssignAgent
    {
        public InboundWorkOrderAssignAgent(IWorkOrderAssignAgentParameters Parameters)
            : base(Parameters)
        {

        }
        protected override bool BeforeGenerateCheckData(IAssignedWorkOrderCollection parameters, ref List<string> errors)
        {
            //inbound 數量檢查
            //var groups = parameters.Items.GroupBy(p => p.VesselMainifestUID);
            errors = new List<string>();
            var groups = parameters.Items.GroupBy(p => p.VesselMainifestUID);
            var vesselManifestInfo = this.Managers.VesselManifestRepository.GetList(new { UID = groups.Select(p => p.Key) });
            var workorderPayloadInfo = this.Managers.WorkOrderPayloadRepository
                .GetList(new { VesselManifestUID = groups.Select(p => p.Key) });
            if (vesselManifestInfo.Success)
            {
                foreach (var item in groups)
                {
                    var vf = vesselManifestInfo.Content.FirstOrDefault(p => p.UID == item.Key);
                    var wf = workorderPayloadInfo.Content.Where(p => p.VesselManifestUID == item.Key);
                    if (vf != null)
                    {
                        //manifestItem Package 與 VesselManifest Package 相同
                        var allocatedQty = item.Sum(p => p.ReceivePackageQty) + wf.Sum(p => p.Qty);
                        if (vf.Qty < allocatedQty)
                        {
                            errors.Add(string.Format(Resource.MANIFEST_INBOUND_ALLOCATED_QTY_OVER, vf.Name));
                        }
                    }
                }
                return base.BeforeGenerateCheckData(parameters, ref errors);
            }
            else
            {
                return false;
            }

        }
        protected override IActionResult<WorkOrderResult> InnerExecute(ConcurrentStack<Func<IActionResult<bool>>> Actions, IWorkOrderAssignAgentExecuteParameters parameters)
        {
            this.ActionResult.Success = true;
            return base.InnerExecute(Actions, parameters);
        }
    }
}
