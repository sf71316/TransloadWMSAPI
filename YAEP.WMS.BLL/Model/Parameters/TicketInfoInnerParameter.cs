using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class TicketInfoInnerParameter : ITicketInfoParameter
    {
        public TicketInfoInnerParameter()
        {
            this.Barcode = new List<IUploadTicketBarcode>().ToArray();
        }
        public Guid TicketInfoUID { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public ScanType ScanType { get; set; }
        public IEnumerable<IUploadTicketBarcode> Barcode { get; set; }
        public int? ActQty { get; set; }
        public int? ShtQty { get; set; }
        public int? SavQty { get; set; }
        public int? Status { get; set; }
        public bool IsPodScan { get; set; }
        public bool IsAllPass { get; set; }
        public bool IsAllShortage { get; set; }
    }
}
