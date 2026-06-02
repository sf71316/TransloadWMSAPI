using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.BLL.Module
{
    internal class WorkOrderStatusManageAgent : StatusProcessAgent
    {
        public WorkOrderStatusManageAgent(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }
        public IActionResult<bool> ChangeAllWorkOrderStatus(Guid workOrderUID, WorkOrderStatus workOrderStatus,
            WorkOrderPodStatus workOrderPodStatus, WorkOrderPayloadStatus workOrderPayloadStatus)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var Result = new List<IActionResult<bool>>();
                var wops = this.Repositorys.WorkOrderPodRepository.GetWorkOrderPodList(workOrderUID);
                var wopls = this.Repositorys.WorkOrderPayloadRepository.GetList(new { workOrderUID = workOrderUID });
                Result.Add(this.Repositorys.WorkOrderRepository.ChangeStatus(workOrderUID, workOrderStatus));
                Result.Add(this.Repositorys.WorkOrderPodRepository.ChangeStatusByWorkOrder(workOrderUID, workOrderPodStatus));
                Result.Add(this.Repositorys.WorkOrderPayloadRepository.ChangeStatusByWorkOrder(workOrderUID, workOrderPayloadStatus));
                if (!Result.All(p => p.Success))
                {
                    rs.Message = string.Join(",", Result.Select(x => x.Message));
                }
                else
                {
                    rs.Success = true;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;



        }
        public IActionResult<bool> ChangeWorkOrderPodStatus(Guid workOrderPodUID, WorkOrderPodStatus workOrderPodStatus)
        {

            return this.Repositorys.WorkOrderPodRepository.ChangeStatus(workOrderPodUID, workOrderPodStatus);

        }
        public IActionResult<bool> ChangeWorkOrderPodStatus(IEnumerable<Guid> workOrderPodUID, WorkOrderPodStatus workOrderPodStatus, string modifiedBy)
        {

            return this.Repositorys.WorkOrderPodRepository.BatchChangeStatus(workOrderPodUID, workOrderPodStatus, modifiedBy);

        }
        public IActionResult<bool> ChangeWorkOrderPayloadStatus(Guid workOrderPayUID, WorkOrderPayloadStatus workOrderPayloadStatus)
        {

            return this.Repositorys.WorkOrderPayloadRepository.ChangeStatus(workOrderPayUID, workOrderPayloadStatus);

        }
        public IActionResult<bool> ChangeWorkOrderPayloadStatus(IEnumerable<Guid> workOrderPayUID, WorkOrderPayloadStatus workOrderPayloadStatus, string modifiedBy)
        {
            return this.Repositorys.WorkOrderPayloadRepository.BatchChangeStatus(workOrderPayUID, workOrderPayloadStatus, modifiedBy);

        }
        public IActionResult<bool> ChangeWorkOrderStatus(Guid workOrderUID, WorkOrderStatus workOrderStatus)
        {

            return this.Repositorys.WorkOrderRepository.ChangeStatus(workOrderUID, workOrderStatus);

        }
        public IActionResult<bool> ChangeWorkOrderStatus(IEnumerable<Guid> workOrderUID, WorkOrderStatus workOrderStatus, string modifiedBy)
        {

            return this.Repositorys.WorkOrderRepository.BatchChangeStatus(workOrderUID, workOrderStatus, modifiedBy);

        }
        public IActionResult<bool> ChangeAllWorkOrderStatus(IEnumerable<Guid> workOrderUID,
            WorkOrderStatus workOrderStatus, WorkOrderPodStatus workOrderPodStatus, WorkOrderPayloadStatus workOrderPayloadStatus)
        {

            return this.Repositorys.WorkOrderRepository.ChangeAllWorkOrderStatus(workOrderUID, workOrderStatus,
               workOrderPodStatus, workOrderPayloadStatus);

        }
    }
}
