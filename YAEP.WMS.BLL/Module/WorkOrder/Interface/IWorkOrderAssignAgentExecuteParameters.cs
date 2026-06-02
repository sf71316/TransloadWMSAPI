using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    public interface IWorkOrderAssignAgentExecuteParameters
    {
        List<IWorkOrderPayloadModel> WorkOrderPayload { get; set; }
        List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
        dynamic WorkOrderPod { get; set; }
        dynamic WorkOrder { get; set; }
        bool PassCheck { get; set; }
    }
}
