using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class UnAssignedListViewInnerModel : IUnAssignedListViewModel
    {
        public UnAssignedListViewInnerModel()
        {
            this.UID = Guid.NewGuid();
            this.EstimateQtyList = new List<IPackageEstimateQtyList>();
        }
        public Guid UID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid VesselMainifestUID { get; set; }
        public string ItemID { get; set; }
        public Guid PackPackageUID { get; set; }
        public string PackPackageName { get; set; }
        public Guid ReceivePackageUID { get; set; }
        public string ReceivePackageName { get; set; }
        public int ReceivePackageQty { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
        public int FreeQty { get; set; }
        public int AllocatedQty { get; set; }
        public ICollection<IPackageEstimateQtyList> EstimateQtyList { get; set; }
        public int PackageQty { get; set; }
        public int ManifestType { get; set; }
    }
}
