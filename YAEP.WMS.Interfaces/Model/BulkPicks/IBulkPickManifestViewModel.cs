using System;
using System.Collections.Generic;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBulkPickManifestViewModel
    {
        Guid TicketInfoUID { get; set; }
        string TicketInfoID { get; set; } 
        Guid ManifestUID { get; set; }
        string ManifestNo { get; set; }
        Guid ManifestItemListUID { get; set; }
        string ManifestItemListID { get; set; }
        string CustomerPartyName { get; set; }
        string RefNo { get; set; }
        string ItemNo { get; set; }
        decimal EstQty { get; set; }
        string FromSlot { get; set; }
        string ToSlot { get; set; }
        string ShipVia { get; set; }
        Guid CustomerUID { get; set; }
        Guid ItemUID { get; set; }
        Guid ShipViaUID { get; set; }
    }


}
