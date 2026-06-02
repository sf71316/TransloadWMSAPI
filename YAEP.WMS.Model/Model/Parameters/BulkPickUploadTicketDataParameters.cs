using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class BulkPickUploadTicketDataParameters : IBulkPickUploadTicketDataParameters
    {
        public MoveTicketCommand Command { get; set; }
        public Guid BulkPickInfoUID { get; set; } 
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
        public IEnumerable<string> Barcode { get; set; }
        public ScanType ScanType { get; set; }
    }
}
