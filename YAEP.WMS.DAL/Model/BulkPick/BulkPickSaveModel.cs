using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class BulkPickSaveModel : IBulkPickSaveModel
    {
        public Guid TicketInfoUID { get; set; }
        public Guid OriginalSlotUID { get; set; }
        public Guid TargetSlotUID { get; set; }
    }
}
