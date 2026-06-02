using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class CheckItemUsageStatusInnerModel : ICheckOnhandModel
    {
        public Guid ItemUID { get; set; }
        public int Qty { get; set; }
        public Guid PackageUID { get; set; }
        
        public int TicketUsageCount { get; set; }
        public int TicketOnHand { get; set; }
        public int NonTicketOnHand { get; set; }
        public int OnHand { get; set; }
    }
}
