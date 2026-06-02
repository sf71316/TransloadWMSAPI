using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public class TicketSummaryViewInnerModel : ITicketSummaryViewModel
    {
        public int TicketType { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public Guid PackageUID { get; set; }
        public string PackageName { get; set; }
        public int EstQty { get; set; }
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
    }
}
