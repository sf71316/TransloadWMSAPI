using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class BulkPickManifestViewModel : IBulkPickManifestViewModel
    {
        public Guid TicketInfoUID { get; set; }
        public string TicketInfoID { get; set; }
        public Guid ManifestUID { get; set; }
        public string ManifestNo { get; set; }
        public Guid ManifestItemListUID { get; set; }
        public string ManifestItemListID { get; set; }
        public string CustomerPartyName { get; set; }
        public string RefNo { get; set; }
        public string ItemNo { get; set; }
        public decimal EstQty { get; set; }
        public string FromSlot { get; set; }
        public string ToSlot { get; set; }
        public string ShipVia { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid ShipViaUID { get; set; }
    }
}
