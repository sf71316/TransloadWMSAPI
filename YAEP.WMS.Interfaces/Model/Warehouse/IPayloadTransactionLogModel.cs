using System;

namespace YAEP.WMS.Interfaces
{
    public interface IPayloadTransactionLogModel
    {
        Guid UID { get; set; }
        Guid WorkOrderPodUID { get; set; }
        Guid WorkOrderPayloadUID { get; set; }
        Guid WarehouseUID { get; set; }
        Guid ItemUID { get; set; }
        Guid PayloadUID { get; set; }
        int QtyBeforeTX { get; set; }
        int QtyAfterTX { get; set; }
        Guid? OriginalSlotUID { get; set; }
        Guid? OriginalPackage { get; set; }
        Guid? TargetSlotUID { get; set; }
        Guid? TicketInfoUID { get; set; }
        Guid TargetPackage { get; set; }
        int Type { get; set; }
        int Status { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}