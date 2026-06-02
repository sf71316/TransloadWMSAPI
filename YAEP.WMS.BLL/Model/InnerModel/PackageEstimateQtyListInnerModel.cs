using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class PackageEstimateQtyListInnerModel : IPackageEstimateQtyList
    {
        public Guid PackPackageUID { get; set; }
        public string PackPackageName { get; set; }
        public int TTLQty { get; set; }
        public int AllocatedQty { get; set; }
        public int FreeQty { get; set; }
    }
}
