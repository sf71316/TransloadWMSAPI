using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class BulkPickInfoViewModel : IBulkPickInfoViewModel
    {
        public Guid BulkPickUID { get; set; }
        public string BulkPickID { get; set; }
        public string CustomerName { get; set; }
        public string ManifestNo { get; set; }
        public string RefNo { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemNo { get; set; }
        public int? EstQty { get; set; }
        public int? ActQty { get; set; }
        public string FromSlot { get; set; }
        public string ToSlot { get; set; }
        public string TicketInfoID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public Guid ShipViaUID { get; set; }
    }
}
