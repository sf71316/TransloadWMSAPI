using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IVesselAddItemListVewModel
    {
        Guid ItemUID { get; set; }
        Guid ManifestItemUID { get; set; }
        string ItemID { get; set; }
        string PackPackageName { get; set; }
        Guid PackPackageUID { get; set; }
        //string ReceivePackageName { get; set; }
        Guid ReceivePackage { get; set; }
        int ReceiveQty { get; set; }
        int PackageQty { get; set; }
        int FreeQty { get; set; }
        int AllocatedQty { get; set; }
        decimal Volume { get; set; }
        decimal Weight { get; set; }
        ICollection<IPackageEstimateQtyList> EstimateQtyList { get; set; }
    }
    public interface IPackageEstimateQtyList
    {
        Guid PackPackageUID { get; set; }
        string PackPackageName { get; set; }
        int TTLQty { get; set; }
        int AllocatedQty { get; set; }
        int FreeQty { get; set; }
    }
}
