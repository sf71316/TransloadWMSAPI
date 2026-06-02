using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class OutboundAutoAssignedResult
    {
        public OutboundAutoAssignedResult()
        {
            this.Response = new AllocatedInnerResponse();
        }
        public IAllocatedResponse Response { get; set; }
        public Guid WorkOrderUID { get; set; }
        //public ConcurrentStack<Func<IActionResult<bool>>> Funcs { get; set; }
    }
    internal class InboundAutoAssignedResult
    {
        public InboundAutoAssignedResult()
        {
        }
        public bool IsComplete { get; set; }
        public string Message { get; set; }
        public Guid WorkOrderUID { get; set; }
        //public ConcurrentStack<Func<IActionResult<bool>>> Funcs { get; set; }
    }
}
