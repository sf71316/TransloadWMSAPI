using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class AllocateQueue : QueueManger<bool>
    {
        public AllocateQueue(ITracingAgent tracingAgent) : base(tracingAgent)
        {

        }
    }
}
