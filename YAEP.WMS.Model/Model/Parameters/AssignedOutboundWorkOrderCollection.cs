using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class AssignedOutboundWorkOrderCollection : IAssignedOutboundWorkOrderCollection
    {
        public AssignedOutboundWorkOrderCollection()
        {
            this.Items = new List<AssignedOutboundWorkOrderPayload>().ToArray();
        }
        public Guid VesselUID { get; set; }
        public ManifestType ServiceType { get; set; }
        public IList<IAssignedOutboundWorkOrderPayload> Items { get; set; }
        public Guid? LoadingZoneSlotUID { get; set; }
        public string PalletBarcode { get; set; }
    }

    public class AssignedOutboundWorkOrderPayload : IAssignedOutboundWorkOrderPayload
    {
        //public string Name { get; set; }
        public Guid? SlotUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid VesselMainifestUID { get; set; }
        public Guid? PayloadUID { get; set; }
        public int AllocatedQty { get; set; }
        public Guid PickPackageUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public AllocateType AllocateType { get; set; }
        public List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
    }
}
