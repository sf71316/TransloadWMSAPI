using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class ExternalInboundAutoAssignAgent : AbstractExternalAutoInboundAssignedAgent
    {
        public ExternalInboundAutoAssignAgent(IAutoAssignAgentProviders providers) : base(providers)
        {

        }

        public override InboundAutoAssignedResult Execute(InboundAutoAssignedParameters inboundparameters)
        {

            InboundAutoAssignedResult Result = new InboundAutoAssignedResult();
            ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
            ReceivingPlannerInitParameters receivingPlannerInitParameters = new ReceivingPlannerInitParameters();
            receivingPlannerInitParameters.CustomerUID = inboundparameters.Manifest.PartyUID;
            receivingPlannerInitParameters.WarehouseManger = this.Providers.WarehouseManager;
            receivingPlannerInitParameters.WorkOrderAssignAgentParameters = this.Providers.WorkOrderAssignAgentParameters;
            //ReceivingPlannerParameters param = new ReceivingPlannerParameters();
            //param.LabelMapping = inboundparameters.LabelMapping;
            //param.TransactionScope = this.Providers.TransactionScope;
            //param.VesselManifests = inboundparameters.VesselItems;
            //param.WarehouseUID = inboundparameters.Manifest.WarehouseUID;
            //param.ReceivingRequest = inboundparameters.ReceivingRequest;
            var planner = AbstractReceivingPlanner.GetInstance(receivingPlannerInitParameters);
            var result = planner.Plan( inboundparameters);
            Result.IsComplete = result.Success;
            Result.Message = result.Message;
            return Result;
        }
    }
}
