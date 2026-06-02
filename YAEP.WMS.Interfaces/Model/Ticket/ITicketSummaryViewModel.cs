using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketSummaryViewModel
    {
        int TicketType { get; set; }
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        Guid PackageUID { get; set; }
        string PackageName { get; set; }
        int EstQty { get; set; }
        int ActQty { get; set; }
        int ShtQty { get; set; }
        int SavQty { get; set; }
    }
}
