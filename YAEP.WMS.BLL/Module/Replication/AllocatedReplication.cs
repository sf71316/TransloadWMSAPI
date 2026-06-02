using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.LittleBird.CapPBSC;
using YAEP.LittleBird.CapPBSC.Models;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class AllocatedReplication : AbstractReplication<WmsAllocatedModel>
    {
        public AllocatedReplication(ITracingAgent traceAgent) : base(traceAgent)
        {

        }
        public override IActionResult<bool> Sync(IEnumerable<WmsAllocatedModel> data, string replicateionKey)
        {
            var rs = ActionResultTemplates.Result<bool>();
            Policy.Create().Retry(RETRY_COUNT, 2000, (obj, args) =>
            {
                this.TraceAgent.Trace("sync allocate data exception", args);
            }).Execute(() =>
            {
                
                var littleBird = new SyncWmsAllocatedClient();
                this.TraceAgent.Trace("Begin sync allocated data", DateTime.Now, replicateionKey);
                rs.Success = rs.Content = littleBird.Build().Channel("Varys.CapPBSC.Test")
                                                      .Content($"{replicateionKey}|{DateTime.Now.ToString("yyyyMMddHHmmssfff")}")
                                                      .Owner(Agent)
                                                      .Data(data)
                                                      .Tweet();
                this.TraceAgent.Trace("End sync allocated data", data, rs);

                return rs;

            });

            return rs;
        }
    }
}
