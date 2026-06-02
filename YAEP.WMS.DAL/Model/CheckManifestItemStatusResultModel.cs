using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class CheckManifestItemStatusResultModel : ICheckManifestItemStatusResultModel
    {
        public int ManifestStatus { get; set; }
        public int ManifestItemStatus { get; set; }
        public Guid BolUID { get; set; }
        public Guid VesselUID { get; set; }
        public Guid ManifestItemUID { get; set; }
        public Guid ItemUID { get; set; }
        public int CompleteQty { get; set; }
        public Guid CompletePackageUID { get; set; }
        public int OriginalQty { get; set; }
        public Guid OriginalPackageUID { get; set; }
    }
}
