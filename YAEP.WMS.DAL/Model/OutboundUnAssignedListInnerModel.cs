using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class OutboundUnAssignedListInnerModel : IOutboundUnAssignedListModel
    {
        public OutboundUnAssignedListInnerModel()
        {
        }
        public Guid UID { get; set; }
        public Guid SlotUID { get; set; }
        public string SlotName { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public Guid PickPackageUID { get; set; }
        public string PickPackageName { get; set; }
        public string PayloadID { get; set; }
        public int PickQty { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
        public string BinName { get; set; }
        public string AreaName { get; set; }
        public Guid VesselManifestUID { get; set; }
        public int AllocatedQty { get; set; }
        public int FreeQty { get; set; }
    }
    internal class OutboundUnAssignedItemInnerModel : IOutboundAllocatedItemModel
    {
        public Guid UID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid VesselManifestUID { get; set; }
        public string SlotName { get; set; }
        public string BinName { get; set; }
        public string AreaName { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public Guid PickPackageUID { get; set; }
        public string PickPackageName { get; set; }
        public string PayloadID { get; set; }
        public int PickQty { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
    }
}
