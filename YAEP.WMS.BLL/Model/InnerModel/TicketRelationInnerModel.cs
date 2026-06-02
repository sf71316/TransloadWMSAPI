using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class TicketRelationInnerModel : ITicketRelationModel
    {
        public Guid UID { get; set; }
        public Guid ParentUID { get; set; }
        public Guid TicketUID { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
