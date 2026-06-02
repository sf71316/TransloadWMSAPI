using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IUnAssignedListViewModel
    {
        int ManifestType { get; set; }
        Guid UID { get; set; }
        Guid ItemUID { get; set; }
        Guid VesselMainifestUID { get; set; }
        string ItemID { get; set; }
        Guid PackPackageUID { get; set; }
        string PackPackageName { get; set; }
        int PackageQty { get; set; }
        Guid ReceivePackageUID { get; set; }
        string ReceivePackageName { get; set; }
        int ReceivePackageQty { get; set; }
        int FreeQty { get; set; }
        int AllocatedQty { get; set; }
        decimal Volume { get; set; }
        decimal Weight { get; set; }
        ICollection<IPackageEstimateQtyList> EstimateQtyList { get; set; }
    }
}
