using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IOutboundUnAssignedGroupModel
    {
        Guid UID { get; set; }
        Guid VesselManifestUID { get; set; }
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        string PickPackagename { get; set; }
        Guid PickPackageUID { get; set; }
        int PickQty { get; set; }
        int FreeQty { get; set; }
        int AllocatedQty { get; set; }
        IEnumerable<IOutboundAllocatedItemModel> Items { get; set; }
    }
}
