using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.BLL.Module
{
    internal class ManifestStatusManageAgent : StatusProcessAgent
    {
        internal ManifestStatusManageAgent(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }
        internal IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestStatus manifestStatus)
        {
            return this.Repositorys.ManifestRepository.ChangeManifestStatus(manifestUID, manifestStatus);
        }
        internal IActionResult<bool> ChangeManifestItemStatus(Guid manifestUID, ManifestItemListStatus manifestItemListStatus)
        {
            return this.Repositorys.ManifestItemRepository.ChangeManifestStatus(manifestUID, manifestItemListStatus);
        }
        internal IActionResult<bool> BatchChangeManifestItemStatus(IEnumerable<Guid> manifestitemUIDs, ManifestItemListStatus manifestItemListStatus)
        {
            return this.Repositorys.ManifestItemRepository.BatchChangeManifestStatus(manifestitemUIDs, manifestItemListStatus);
        }
        internal IActionResult<bool> ChangeManifestStatus(Guid manifestUID, ManifestStatus status, ManifestItemListStatus ManifestStatus,
            string modifiedBy = "")
        {

            var rs1 = this.Repositorys.ManifestRepository.ChangeManifestStatus(manifestUID, status, modifiedBy);
            var rs2 = this.Repositorys.ManifestItemRepository.ChangeManifestStatus(manifestUID, ManifestStatus, modifiedBy);
            rs1.Content &= rs2.Content;
            rs1.Success &= rs2.Success;
            return rs1;
        }
        internal IActionResult<bool> ChangeManifestItemStatusByBol(Guid boluid, ManifestItemListStatus ManifestItemStatus)
        {
            return this.Repositorys.ManifestItemRepository.ChangeManifestStatusByBol(boluid, ManifestItemStatus);
        }
        internal IActionResult<bool> RollbackLabel(IEnumerable<Guid> ticketUids)
        {
            return this.Repositorys.LabelManager.RollbackLabel(ticketUids);
        }
    }
}
