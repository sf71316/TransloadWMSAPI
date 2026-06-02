using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    public class TicketLabelInnerModel : ITicketLabelViewModel
    {
        public int BarcodeType { get; set; }
        public int BelongToType { get; set; }
        public Guid BelongToUID { get; set; }
        public Guid AttachmentUID { get; set; }
        public string Barcode { get; set; }
        public string BarcodeTypeName { get; set; }
        public string StatusName { get; set; }
        public int Status { get; set; }
        public int AddQty { get; set; }
    }
}
