using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderPodRepository
    {
        IActionResult<bool> AddWorkOrderPod(dynamic Model);
        IActionResult<bool> AddWorkOrderPod(IEnumerable<dynamic> entitys);

        IActionResult<bool> ChangeStatus(Guid workorderPodUID, WorkOrderPodStatus status);

        IActionResult<bool> ChangeStatusByWorkOrder(Guid workorderUID, WorkOrderPodStatus status);

        IActionResult<bool> DeleteWorkOrderPod(object parameters);
        IActionResult<bool> DeleteWorkOrderPod(IEnumerable<Guid> workorderPodUID);


        IActionResult<bool> EditWorkOrderPod(dynamic conditon, dynamic Model);

        IActionResult<IWorkOrderPodModel> GetWorkOrderPod(object condition);
        IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(object condition);
        IActionResult<IEnumerable<IWorkOrderPodViewModel>> GetWorkOrderPod(Guid VesselUID);

        IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(Guid workorderUID);

        IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodList(IEnumerable<Guid> workorderUID);

        IActionResult<IEnumerable<IWorkOrderPodModel>> GetWorkOrderPodListByVessel(IEnumerable<Guid> VesselUID);

        IActionResult<bool> MergePod(IWorkOrderMergePalletParameter parameter);

        IActionResult<bool> WorkOrderPodIsExist(Guid WorkOrderPodUID);

        IActionResult<bool> BatchChangeStatus(IEnumerable<Guid> workorderPodUID, WorkOrderPodStatus status, string modifiedBy = "");
    }
}
