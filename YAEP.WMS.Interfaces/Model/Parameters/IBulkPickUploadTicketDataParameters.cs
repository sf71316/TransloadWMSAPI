using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IBulkPickUploadTicketDataParameters
    {
        MoveTicketCommand Command { get; set; }
        Guid BulkPickInfoUID { get; set; }
        int ActQty { get; set; }
        int ShtQty { get; set; }
        int SavQty { get; set; }
        IEnumerable<string> Barcode { get; set; }
        ScanType ScanType { get; set; }
    }
}
