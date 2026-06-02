using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class CalVesselAddItemInnerModel : ICalVesselAddItemInnerModel
    {
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }
        public int PackageQty { get; set; }
        public Guid ManifestItemUID { get; set; }
    }
    internal class CalVesselAssignedItemInnerModel : ICalVesselAssignedItemInnerModel
    {
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }
        public int PackageQty { get; set; }
        public Guid VesselManifestUID { get; set; }
    }
}
