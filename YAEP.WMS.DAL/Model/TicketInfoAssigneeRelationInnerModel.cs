using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    public class TicketInfoAssigneeRelationInnerModel : ITicketInfoAssigneeRelationModel
    {
        public Guid UID { get; set; }
        public Guid GroupUID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
