using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public class WorkOrderAssignAgentExecuteParameters : IWorkOrderAssignAgentExecuteParameters
    {
        public WorkOrderAssignAgentExecuteParameters()
        {
            this.OnhandPayloadItems = new List<ILocationItemViewModel>();
        }
        public List<IWorkOrderPayloadModel> WorkOrderPayload { get; set; }
        public dynamic WorkOrderPod { get; set; }
        public dynamic WorkOrder { get; set; }
        public bool PassCheck { get; set; }
        public List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
    }
}
