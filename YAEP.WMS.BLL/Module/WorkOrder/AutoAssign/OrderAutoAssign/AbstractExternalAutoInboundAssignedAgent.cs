using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractExternalAutoInboundAssignedAgent
    {
        protected readonly IAutoAssignAgentProviders Providers;

        public AbstractExternalAutoInboundAssignedAgent(IAutoAssignAgentProviders providers)
        {
            this.Providers = providers;
        }
        public abstract InboundAutoAssignedResult Execute(InboundAutoAssignedParameters parameters);
    }
}
