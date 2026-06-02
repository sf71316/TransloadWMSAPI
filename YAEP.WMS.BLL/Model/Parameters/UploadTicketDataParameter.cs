using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class UploadTicketDataParameter : IUploadTicketDataParameter
    {
        public ITicketInfoParameter Item { get; set; }
        public MoveTicketCommand TicketInfoCommand { get; set; }
        public TicketType ServiceItem { get; set; }
    }
    internal class UploadTicketBarcode : IUploadTicketBarcode
    {
        public string Barcode { get; set; }
        public int ScanQty { get; set; }
    }


}
