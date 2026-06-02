using System;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketInfoAssigneeRelationModel
    {
        Guid UID { get; set; }
        Guid GroupUID { get; set; }
        Guid TicketInfoUID { get; set; }
        int Status { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}