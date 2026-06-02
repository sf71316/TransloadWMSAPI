using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.LittleBird.WMS;
using YAEP.LittleBird.WMS.Models;
using YAEP.Utilities;
using YAEP.WMS.Constant;

namespace YAEP.WMS.BLL.Module
{
    internal class ReplenishmentSyncer : AbstractSyncer<SyncReplenishmentModel>
    {
        public override IActionResult<bool> Sync(IEnumerable<SyncReplenishmentModel> data, string replicateionKey)
        {

            var rs = ActionResultTemplates.Result<bool>();

            var littleBird = new SyncWMSReplenishmentClient();
            rs.Success = rs.Content = littleBird.Build().Channel("Varys.CapWMS")
                                                .Content($"{replicateionKey}|{DateTime.Now.ToString("yyyyMMddHHmmssfff")}")
                                                .Mode(200)
                                                .Owner(Agent)
                                                .Data(data)
                                                .Tweet();

            return rs;

        }
    }
}
