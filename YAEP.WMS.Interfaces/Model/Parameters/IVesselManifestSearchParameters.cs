using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselManifestSearchParameters
    {
        Guid? VesselUID { get; set; }
        Guid? BOLUID { get; set; }
        Guid? VesselManifestUID { get; set; }
        Guid[] ManifestItemUID { get; set; }
    }
}
