using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IReceiviedReplicateModel
    {
        Guid PartyUID { get; set; }
        int LocationID { get; set; }
        int? WarehouseID { get; set; }
        Guid UID { get; set; }
        string Barcode { get; set; }
        string ExternalOrderNo { get; set; }
        int Qty { get; set; }
        int ActQty { get; set; }
        Guid ItemUID { get; set; }
        Guid TicketInfoUID { get; set; }
        Guid ItemGroupUID { get; set; }
        string ItemName { get; set; }
        Guid PackageUID { get; set; }
        string OriginalSlotName { get; set; }
        string LandingZoneSlotName { get; set; }
    }
}
