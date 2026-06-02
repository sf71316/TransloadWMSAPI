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
    internal abstract class AbstractAutoAssignTicketExecutor : IAutoAssignTicketExecutor
    {
        protected readonly IAutoAssignAgentProviders Providers;
        public AbstractAutoAssignTicketExecutor(IAutoAssignAgentProviders providers)
        {
            this.Providers = providers;
        }
        public abstract IActionResult<bool> Execute(IAutoAssignProcessArgs e);
    }
}
