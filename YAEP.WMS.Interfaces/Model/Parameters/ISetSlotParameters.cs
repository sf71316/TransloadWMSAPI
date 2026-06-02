using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISetSlotParameters
    {
        Guid? WorkOrderPodUID { get; set; }
        Guid? WorkOrderPayloadUID { get; set; }
        Guid SlotUID { get; set; }
    }
}
