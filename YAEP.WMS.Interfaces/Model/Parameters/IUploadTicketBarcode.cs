using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IUploadTicketBarcode
    {
        string Barcode { get; set; }
        int ScanQty { get; set; }
    }
}
