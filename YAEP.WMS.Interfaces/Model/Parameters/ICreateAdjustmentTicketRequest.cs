using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICreateAdjustmentTicketRequest
    {
        Guid WarehouseUID { get; set; }
        Guid PayloadUID { get; set; }
        Guid ItemUID { get; set; }
        Guid ModifyPackageUID { get; set; }
        int ModifyQty { get; set; }
        Guid ModifySlotUID { get; set; }
        int ActionType { get; set; }
        int AdjustType { get; set; }
        bool isNew { get; set; }
    }
}
