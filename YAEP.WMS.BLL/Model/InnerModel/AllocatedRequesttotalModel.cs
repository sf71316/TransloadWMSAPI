using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class AllocatedRequesttotalModel
    {
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }
        public int Qty { get; set; }
        public int OnhandType { get; set; }
        public IEnumerable<IVesselManifestModel> VesselManifestCollection { get; set; }

    }
}
