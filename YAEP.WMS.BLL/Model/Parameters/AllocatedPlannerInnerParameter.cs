using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Model.Parameters
{
    internal class AllocatedPlannerInnerParameter
    {
        public Guid VesselManifestUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }

    }
}
