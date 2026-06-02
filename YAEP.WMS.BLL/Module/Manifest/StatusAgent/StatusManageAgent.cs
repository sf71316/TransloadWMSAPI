using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class StatusManageAgent
    {
        internal ManifestStatusManageAgent Manifest { get; set; }
        internal BolStatusManageAgent Bol { get; set; }
        internal VesselStatusManageAgent Vessel { get; set; }
        internal WorkOrderStatusManageAgent WorkOrder { get; set; }
        internal TicketStatusManageAgent Ticket { get; set; }
        internal IStatusManageAgentParamters DataModules { get; set; }
        public StatusManageAgent(IStatusManageAgentParamters paramters)
        {
            Manifest = new ManifestStatusManageAgent(paramters);
            Bol = new BolStatusManageAgent(paramters);
            Vessel = new VesselStatusManageAgent(paramters);
            WorkOrder = new WorkOrderStatusManageAgent(paramters);
            Ticket = new TicketStatusManageAgent(paramters);
            DataModules = paramters;
        }
    }
    internal abstract class StatusProcessAgent
    {
        protected IStatusManageAgentParamters Repositorys { get; set; }
        public StatusProcessAgent(IStatusManageAgentParamters paramters)
        {
            Repositorys = paramters;
        }
    }


}
