using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISetGroupMoveAdjustmentRequest
    {
        Guid TargetWarehouseUID { get; set; }
        Guid TargetSlotUID { get; set; }
        Guid TargetItemUID { get; set; }
        Guid TargetPackageUID { get; set; }
        int TargetQty { get; set; }
        int PayloadType { get; set; }
        List<Guid> SourcePayloadUIDList { get; set; }
    }
}
