using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AssignedWorkOrderInnerCollection : IAssignedWorkOrderCollection
    {
        public AssignedWorkOrderInnerCollection()
        {

        }
        public Guid VesselUID { get; set; }
        public Guid? LoadingZoneSlotUID { get; set; }
        public int StorageMethod { get; set; }
        public Guid? PodUID { get; set; }
        public ManifestType ServiceType { get; set; }
        public IList<IAssignedWorkOrderPayload> Items { get; set; } = new List<IAssignedWorkOrderPayload>();
        public string OperationSuggestion { get; set; }
        public string ExternalBarcode { get; set; }
    }
    internal class AssignedWorkOrderPayloadInnerModel : IAssignedWorkOrderPayload
    {
        public AssignedWorkOrderPayloadInnerModel()
        {

        }

        public string Name { get; set; }
        public Guid? SlotUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid VesselMainifestUID { get; set; }
        public Guid? PayloadUID { get; set; }
        public int ReceivePackageQty { get; set; }
        public Guid ReceivePackageUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public PayloadType PayloadType { get; set; }
        public WorkOrderPayloadType WorkorderPayloadType { get; set; }
        public List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
    }
}
