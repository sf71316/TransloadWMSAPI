using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class ReceivingResponse : IReceivingResponse
    {
        public ReceivingResponse()
        {
            this.Results = new List<IReceivingResponseItem>();
        }
        public bool IsComplete { get; set; }
        public string Message { get; set; }
        public List<IReceivingResponseItem> Results { get; set; }
    }
    internal class ReceivingItemResult : IReceivingResponseItem
    {
        public Guid WorkorderPayloadUID { get; set; }
        public string PO { get; set; }
        public string VendorID { get; set; }
        public string Barcode { get; set; }
    }
}
