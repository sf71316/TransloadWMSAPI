using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractReceivingPlanner
    {
        protected IWarehouseManger WarehouseManger { get; set; }
        protected IWorkOrderAssignAgentParameters WorkOrderAssignAgentParameters { get; set; }
        protected Guid CustomerUID { get; set; }
        public AbstractReceivingPlanner(ReceivingPlannerInitParameters parameters)
        {
            this.CustomerUID = parameters.CustomerUID;
            this.WarehouseManger = parameters.WarehouseManger;
            this.WorkOrderAssignAgentParameters = parameters.WorkOrderAssignAgentParameters;
        }
        public abstract IActionResult<bool> Plan(InboundAutoAssignedParameters inboundparameters);
        public static AbstractReceivingPlanner GetInstance(ReceivingPlannerInitParameters parameters)
        {
            return new DefaultReceivingPlanner(parameters);
        }
    }
}
