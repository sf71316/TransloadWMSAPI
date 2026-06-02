using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class BulkPikcAssignedWorkOrderPayload : IBulkPikcAssignedWorkOrderPayload
    {
        public Guid TargetSlotUID { get; set; }
        public string Name { get; set; }
        public Guid? SlotUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid VesselMainifestUID { get; set; }
        public Guid? PayloadUID { get; set; }
        public int ReceivePackageQty { get; set; }
        public Guid ReceivePackageUID { get; set; }
        public IEnumerable<Guid> OriginalPayloadUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public IEnumerable<Guid> OriginalWorkOrderPayloadUID { get; set; }
        public PayloadType PayloadType { get; set; }
        public WorkOrderPayloadType WorkorderPayloadType { get; set; }
        public List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
    }
}
