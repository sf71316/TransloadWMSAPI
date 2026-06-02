using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractExternalAutoOutboundAssignedAgent
    {
        protected readonly IAutoAssignAgentProviders Providers;

        public AbstractExternalAutoOutboundAssignedAgent(IAutoAssignAgentProviders providers)
        {
            this.Providers = providers;
        }
        /// <summary>
        /// 進行訂單Allocated
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract OutboundAutoAssignedResult Execute(OutboundAutoAssignedParameters parameters);
    }
}
