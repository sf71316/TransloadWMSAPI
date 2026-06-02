using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model.Parameters
{
    internal class VesselManifestSearchInnerParameters : IVesselManifestSearchParameters
    {
        public Guid? VesselUID { get; set; }
        public Guid? VesselManifestUID { get; set; }
        public Guid? BOLUID { get; set; }
        public Guid[] ManifestItemUID { get; set; }
    }
}
