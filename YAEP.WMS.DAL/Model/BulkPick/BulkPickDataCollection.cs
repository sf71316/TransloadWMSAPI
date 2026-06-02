using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class BulkPickDataCollection : IBulkPickDataCollection
    {
        public BulkPickDataCollection()
        {
            this.WorderPayloadCollection = new List<IWorkOrderPayloadModel>();
            this.TicketInfoCollection = new List<IBulkPickTicketInfoModel>();
        }
        public IEnumerable<IWorkOrderPayloadModel> WorderPayloadCollection { get; set; }
        public IEnumerable<IBulkPickTicketInfoModel> TicketInfoCollection { get; set; }
    }
}
