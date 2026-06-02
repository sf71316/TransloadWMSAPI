using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickSaveModel
    {
        Guid TicketInfoUID { get; set; } 
        Guid OriginalSlotUID { get; set; }
        Guid TargetSlotUID { get; set; } 
    }


}
