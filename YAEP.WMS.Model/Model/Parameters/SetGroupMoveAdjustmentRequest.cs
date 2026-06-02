using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class SetGroupMoveAdjustmentRequest : ISetGroupMoveAdjustmentRequest
    {
        public Guid TargetWarehouseUID { get; set; }
        public Guid TargetSlotUID { get; set; }
        public Guid TargetItemUID { get; set; }
        public Guid TargetPackageUID { get; set; }
        public int TargetQty { get; set; }
        public int PayloadType { get; set; }
        public List<Guid> SourcePayloadUIDList { get; set; }
        
    }
}
