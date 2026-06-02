using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model;

namespace YAEP.WMS.BLL.Module
{
    internal class InventoryCountingWorkOrderAssignAgent : AbstractWorkOrderAssignAgent
    {
        public InventoryCountingWorkOrderAssignAgent(IWorkOrderAssignAgentParameters Parameters)
            : base(Parameters)
        {

        }
        protected override IActionResult<WorkOrderResult> InnerExecute(ConcurrentStack<Func<IActionResult<bool>>> Actions, IWorkOrderAssignAgentExecuteParameters parameters)
        {
            this.ActionResult.Success = true;
            return base.InnerExecute(Actions, parameters);
        }
    }
}
