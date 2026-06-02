using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class CretaeAdjustmentTicket : ICreateAdjustmentTicketRequest
    {
        public Guid WarehouseUID { get; set; }
        public Guid PayloadUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid ModifyPackageUID { get; set; }
        public int ModifyQty { get; set; }
        public Guid ModifySlotUID { get; set; }
        public int ActionType { get; set; }
        public bool isNew { get; set; }
        public int AdjustType { get; set; }
    }
}
