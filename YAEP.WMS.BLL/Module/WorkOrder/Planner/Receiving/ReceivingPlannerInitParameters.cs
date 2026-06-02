using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class ReceivingPlannerInitParameters
    {
        public Guid CustomerUID { get; set; }
        public IWarehouseManger WarehouseManger { get; set; }
        public IWorkOrderAssignAgentParameters WorkOrderAssignAgentParameters { get; set; }
    }
}
