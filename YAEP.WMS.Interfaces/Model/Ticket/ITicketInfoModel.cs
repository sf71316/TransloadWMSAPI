using System;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketInfoModel
    {
        Guid UID { get; set; }

        Guid TicketUID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int Type { get; set; }
        Guid? WorkOrderPodUID { get; set; }
        Guid? WorkOrderPayloadUID { get; set; }
        // Guid ItemUID { get; set; }
        int EstQty { get; set; }
        int ActQty { get; set; }
        int ShtQty { get; set; }
        int SavQty { get; set; }
        // Guid? OriginalPackage { get; set; }
        //  Guid? TargetPackage { get; set; }
        //  Guid? OriginalSlotUID { get; set; }
        //  Guid? TargetSlotUID { get; set; }
        string OperationInstruction { get; set; }
        string OperationSuggestion { get; set; }
        int Status { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}