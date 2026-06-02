using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IPodRepository
    {

        IActionResult<bool> AddPod(IPodModel Model);
        IActionResult<bool> UpdatePod(IPodModel Model);
        IActionResult<bool> DeletePod(Guid PodUID);
        IActionResult<bool> DeletePodFromDb(object condition);
        IActionResult<bool> UnPack(Guid PodUID);
        IActionResult<bool> UnPack(IEnumerable<Guid> PodUIDs);
        IActionResult<IEnumerable<IPodModel>> GetPod(Guid[] PodUIDs);
        IActionResult<bool> ChangePodStauts(Guid poduid, PodStatus status);

    }
}
