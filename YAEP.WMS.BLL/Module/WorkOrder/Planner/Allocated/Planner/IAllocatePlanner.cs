using System;
using System.Collections.Generic;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.BLL.Model.Parameters;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal interface IAllocatePlanner
    {
        IEnumerable<AllocatedPlannerResult> ExternalOrderPlanByWMS(Guid warehouseUID, IEnumerable<IVesselManifestModel> parameters,
            bool passPackageVersion, bool isChinaWarehouse);
        IEnumerable<AllocatedPlannerResult> PlanByWMS(IEnumerable<AllocatedPlannerInnerParameter> parameters
            , bool passPackageVersion, bool isChinaWarehouse);
    }
}