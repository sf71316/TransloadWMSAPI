using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class BatchUploadTicketDataParameter : IBatchUploadTicketDataParameter
    {
        public BatchUploadTicketDataParameter()
        {
            this.Items = new List<ITicketInfoParameter>();
        }
        public TicketType ServiceItem { get; set; }
        public MoveTicketCommand TicketInfoCommand { get; set; }
        public IEnumerable<ITicketInfoParameter> Items { get; set; }
    }
}
