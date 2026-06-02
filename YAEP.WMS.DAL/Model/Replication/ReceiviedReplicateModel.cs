using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    public class ReceiviedReplicateModel : IReceiviedReplicateModel
    {
        public int LocationID { get; set; }
        public int? WarehouseID { get; set; }
        public Guid UID { get; set; }
        public string Barcode { get; set; }
        public int ActQty { get; set; }
        public int Qty { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemName { get; set; }
        public Guid PackageUID { get; set; }
        public string OriginalSlotName { get; set; }
        public string LandingZoneSlotName { get; set; }
        public string ExternalOrderNo { get; set; }
        public Guid PartyUID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public Guid ItemGroupUID { get; set; }
    }
}
