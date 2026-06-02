using System;
using System.Collections.Generic;
using YAEP.Core.Party.Interfaces.Models;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    public interface IAutoAssignProcessArgs
    {
        IManifestModel Manifest { get; }
        IBolModel BOL { get; }
        IVesselModel Vessel { get; }
        IPartyModel Party { get; }
        IEnumerable<IPackageModel> Packages { get; }
    }
}
