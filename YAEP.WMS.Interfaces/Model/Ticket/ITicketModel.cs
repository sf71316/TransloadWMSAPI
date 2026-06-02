using System;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketModel
    {
        Guid UID { get; set; }
        Guid WarehouseUID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int? Type { get; set; }
        int ManifestType { get; set; }
        long TicketSequence { get; set; }
        Guid WorkOrderUID { get; set; }
        Guid ServiceItemUID { get; set; }
        int Status { get; set; }
        string Description { get; set; }
        string OperationInstruction { get; set; }
        string OperationSuggestion { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}