using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    public class VesselManifestItemListViewInnerModel : IVesselManifestItemListViewModel
    {
        public Guid UID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }
        public int ReceiveQty { get; set; }
        public string ItemName { get; set; }
        public string ItemID { get; set; }
        public string PackageName { get; set; }
        public string StatusName { get; set; }
        public Guid ManifestItemUID { get; set; }
    }
}
