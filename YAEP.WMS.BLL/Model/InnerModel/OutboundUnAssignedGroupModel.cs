using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class OutboundUnAssignedGroupModel : IOutboundUnAssignedGroupModel
    {
        public Guid UID { get; set; }
        public Guid VesselManifestUID { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public Guid PickPackageUID { get; set; }
        public string PickPackagename { get; set; }
        public int PickQty { get; set; }
        public int AllocatedQty { get; set; }
        public int FreeQty { get; set; }
        public IEnumerable<IOutboundAllocatedItemModel> Items { get; set; }
    }
}
