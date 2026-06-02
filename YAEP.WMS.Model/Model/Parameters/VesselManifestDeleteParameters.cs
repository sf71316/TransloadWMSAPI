using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class VesselManifestDeleteParameters : IVesselManifestDeleteParameters
    {
        public Guid[] UID { get; set; }
    }
}
