using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class ParentTicketRelation : TicketInnerModel, ITicketProcessParentViewModel
    {
        public Guid TicketInfoUID { get; set; }
        public int TicketInfoStatus { get; set; }
    }
}
