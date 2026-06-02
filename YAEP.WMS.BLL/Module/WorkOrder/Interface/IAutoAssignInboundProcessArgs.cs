using System;
using System.Collections.Generic;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    public interface IAutoAssignInboundProcessArgs : IAutoAssignProcessArgs
    {
        IEnumerable<ISlotUsageInfoModel> SlotUsageInfos { get; }
        IEnumerable<IUnAssignedListViewModel> VesselManifests { get; }
    }
}
