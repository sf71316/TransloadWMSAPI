using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class PayloadTransactionLogViewModel : IPayloadTransactionLogViewModel
    {
        public PayloadTransactionLogViewModel()
        {
            this.UniqueKey = Guid.NewGuid();
        }
        public string WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string TypeName { get; set; }
        public string TargetAreaName { get; set; }
        public string TargetBinName { get; set; }
        public string TargetSlotName { get; set; }
        public string PackageName { get; set; }
        public string ItemID { get; set; }
        public Guid UID { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public Guid WorkOrderPayloadUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid PayloadUID { get; set; }
        public int QtyBeforeTX { get; set; }
        public int QtyAfterTX { get; set; }
        public Guid? OriginalSlotUID { get; set; }
        public Guid? OriginalPackage { get; set; }
        public Guid? TargetSlotUID { get; set; }
        public Guid TargetPackage { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ReceivedVesselRefNo { get; set; }
        public string VesselNo { get; set; }
        public string BolNo { get; set; }
        public string TicketID { get; set; }
        public Guid? TicketInfoUID { get; set; }
        public string OriginalAreaName { get; set; }
        public string OriginalBinName { get; set; }
        public string OriginalSlotName { get; set; }
        public Guid UniqueKey { get; set; }
        public Guid UOMUID { get; set; }
        public string UOM { get; set; }
        public DateTime? PayloadModifiedOn { get; set; }
        public string RefNo { get; set; }
        public int? PayloadType { get; set; }
        public string PayloadTypeName { get; set; }
    }
}
