using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AssignedMoveWorkOrderCollection : AssignedOutboundWorkOrderCollection
    {

    }
    internal class AssignedMoveWorkOrderPayload : AssignedOutboundWorkOrderPayload
    {

    }
    internal class AssignedOutboundWorkOrderCollection : IAssignedOutboundWorkOrderCollection
    {
        public AssignedOutboundWorkOrderCollection()
        {
            this.Items = new List<AssignedOutboundWorkOrderPayload>().ToArray();
        }
        public Guid VesselUID { get; set; }
        public string PalletBarcode { get; set; }
        public ManifestType ServiceType { get; set; }
        public IList<IAssignedOutboundWorkOrderPayload> Items { get; set; }
        public Guid? LoadingZoneSlotUID { get; set; }
    }

    internal class AssignedOutboundWorkOrderPayload : IAssignedOutboundWorkOrderPayload
    {
        public AssignedOutboundWorkOrderPayload()
        {
            this.OnhandPayloadItems = new List<ILocationItemViewModel>();
        }
        //public string Name { get; set; }
        //public Guid? SlotUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid VesselMainifestUID { get; set; }
        public Guid? PayloadUID { get; set; }
        public int AllocatedQty { get; set; }
        public Guid? SlotUID { get; set; }
        public Guid PickPackageUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public AllocateType AllocateType { get; set; }
        public List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
    }
}
