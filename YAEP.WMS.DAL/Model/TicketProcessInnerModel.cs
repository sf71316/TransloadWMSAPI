using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class TicketProcessInnerModel : ITicketProcessModel
    {
        public Guid UID { get; set; }
        public Guid TicketUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int PayloadType { get; set; }
        public Guid? WorkOrderPodUID { get; set; }
        public Guid? WorkOrderPayloadUID { get; set; }
        public Guid ItemUID { get; set; }
        public int EstQty { get; set; }
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
        public Guid? OriginalPackage { get; set; }
        public Guid? TargetPackage { get; set; }
        public Guid? OriginalSlotUID { get; set; }
        public Guid? TargetSlotUID { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public IEnumerable<ILabelModel> Barcodes { get; set; }
        public Guid PayloadUID { get; set; }
        public Guid PodUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public int StorageType { get; set; }
        public Guid VesselUID { get; set; }
        public int ManifestType { get; set; }
        public Guid SourceLoadingZoneSlotUID { get; set; }
        public Guid SourcePackageUID { get; set; }
        public Guid SourceSlotUID { get; set; }
        public Guid PayloadPackageUID { get; set; }
        public int MappingType { get; set; }
        public string OriginalPackageName { get; set; }
        public string TargetPackageName { get; set; }
        public string TargetUOMName { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public int OriginalQty { get; set; }
        public string RefNo { get; set; }
        public IEnumerable<ITicketModel> ParentTickets { get; set; }
        public Guid PartyUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public IEnumerable<ITicketProcessParentTicketInfoModel> InboundPartentTicketInfos { get; set; }
        public int PayloadQty { get; set; }
        public int? OriginalPayloadType { get; set; }
    }
}
