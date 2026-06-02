using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.LittleBird.CapPBSC;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractReplication<T> where T : class, new()
    {
        protected const int RETRY_COUNT = 3;
        protected ITracingAgent TraceAgent { get; set; }
        public AbstractReplication(ITracingAgent traceAgent)
        {
            TraceAgent = traceAgent;
        }
        public string Agent { get; set; } = "admin";
        public abstract IActionResult<bool> Sync(IEnumerable<T> data, string replicateionKey);
    }
}
