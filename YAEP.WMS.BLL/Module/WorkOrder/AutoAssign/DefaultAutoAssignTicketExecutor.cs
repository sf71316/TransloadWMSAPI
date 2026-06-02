using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.Package.Interfaces.Models;
using YAEP.Package.Constants;
using YAEP.WMS.Constant.Enums;
using YAEP.Core.Party.Interfaces.Models;

namespace YAEP.WMS.BLL.Module
{
    internal class DefaultAutoAssignTicketExecutor : AbstractAutoAssignTicketExecutor
    {
        public DefaultAutoAssignTicketExecutor(IAutoAssignAgentProviders providers) : base(providers)
        {

        }
        public override IActionResult<bool> Execute(IAutoAssignProcessArgs e)
        {
            var result = ActionResultTemplates.Result();

            var manifestType = (ManifestType)e.Manifest.Type;
            var vesselUID = e.Vessel.UID;
            var unassignedCountResult = this.Providers.VesselManager.GetUnassignedVesslManifestCount(vesselUID);

            // 如果全部 Manifest Item 都 Assigned, 則開 Ticket
            if (unassignedCountResult.Success)
            {
                if (unassignedCountResult.Content == 0)
                {
                    var ticketParameters = new TicketGenerateInnerParameter()
                    {
                        VesselUID = vesselUID,
                    };
                    var parameters = new TicketGeneratorParameters();
                    parameters.PackageManager = this.Providers.PackageCacheManager;
                    parameters.PackageUomManager = this.Providers.PackageUomManager;
                    parameters.LabelRepository = this.Providers.LabelRepository;
                    parameters.TicketInfoRepository = this.Providers.TicketInfoRepository;
                    parameters.TicketRelationRepository = this.Providers.TicketRelationRepository;
                    parameters.TicketRepository = this.Providers.TicketRepository;
                    parameters.SequenceAgent = this.Providers.WorkOrderAssignAgentParameters.SequenceAgent;
                    parameters.WorkOrderManager = this.Providers.WorkOrderAssignAgentParameters.WorkOrderManager;
                    parameters.TracingAgent = this.Providers.TracingAgent;
                    var ticketGenerator = AbstractTicketGenerator.GetInstance(manifestType, parameters);
                    var resultGenerateTicket = ticketGenerator.Execute(ticketParameters);
                    if (resultGenerateTicket.Success)
                    {
                        result.Success = true;
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = Resource.MANIFEST_GENERATE_TICKET_ERROR;
                    }
                }
                else
                {
                    // 沒有所有Item皆建立Work Pod, 無法建立Ticket
                    result.Success = true;
                }
            }
            else
            {
                result.Message = unassignedCountResult.Message;
            }

            return result;
        }
    }
}
