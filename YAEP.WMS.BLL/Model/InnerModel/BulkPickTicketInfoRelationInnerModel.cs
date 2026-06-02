using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class BulkPickTicketInfoRelationInnerModel : IBulkPickTicketInfoRelationModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public Guid BulkPickUID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public Guid FromSlotUID { get; set; }
        public Guid ToSlotUID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
