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
    internal abstract class AbstractAutoAssignWorkOrderExecutor : IAutoAssignWorkOrderExecutor
    {
        protected readonly IAutoAssignAgentProviders Providers;
        protected readonly ProductUtility ProductManager;
        public AbstractAutoAssignWorkOrderExecutor(IAutoAssignAgentProviders providers)
        {
            this.Providers = providers;
            this.ProductManager = new ProductUtility();
        }

        public abstract IActionResult<bool> Execute(IAutoAssignProcessArgs e);
        public abstract IActionResult<List<Func<IActionResult<bool>>>> ExecuteReturnAction(IAutoAssignProcessArgs e);
    }
}
