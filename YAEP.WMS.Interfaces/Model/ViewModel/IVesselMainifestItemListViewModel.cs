using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselManifestItemListViewModel
    {
        Guid UID { get; set; }
        Guid ItemUID { get; set; }
        Guid PackageUID { get; set; }
        Guid ManifestItemUID { get; set; }
        int ReceiveQty { get; set; }
        string ItemName { get; set; }
        string ItemID { get; set; }
        string PackageName { get; set; }
        string StatusName { get; set; }
    }
}
