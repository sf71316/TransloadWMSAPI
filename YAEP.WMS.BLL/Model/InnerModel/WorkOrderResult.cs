using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class WorkOrderResult
    {
        public WorkOrderResult()
        {
            this.WorkorderAssignedResults = new List<WorkorderAssignedResult>();
        }
        public Guid WorkOrderUID { get; set; }
        public dynamic Workorder { get; set; }
        public WorkOrderPodInnerModel WorkorderPod { get; set; }
        public List<IWorkOrderPayloadModel> WorkOrderPayload { get; set; }
        public List<ILabelModel> PodLabels { get; set; }
        public ConcurrentStack<Func<IActionResult<bool>>> GeneratePayloadFunc { get; set; }

        public List<WorkorderAssignedResult> WorkorderAssignedResults { get; set; }
        public AllocatedExecutorResult AllocatedExecutorResult { get; set; }

    }
    internal class WorkorderAssignedResult
    {
        public Guid VesselManifestUID { get; set; }
        public Guid WorkorderPayloadUID { get; set; }
        public Guid WorkorderPodUID { get; set; }
        public int Quantity { get; set; }

    }
}
