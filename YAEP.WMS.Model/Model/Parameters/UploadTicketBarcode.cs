using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable]
    public class UploadTicketBarcode : IUploadTicketBarcode
    {
        public string Barcode { get; set; }
        public int ScanQty { get; set; }
    }
}
