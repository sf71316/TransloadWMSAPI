using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class TicketStatusManageAgent : StatusProcessAgent
    {
        public TicketStatusManageAgent(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }
        public IActionResult<bool> ChangeTicketStatus(Guid ticketUID, TicketStatus ticketStatus, string modifiedBy)
        {
            return this.Repositorys.TicketRepository.UpdateTicketStatus(new Guid[] { ticketUID }, ticketStatus, modifiedBy);
        }
        public IActionResult<bool> ChangeTicketStatus(IEnumerable<Guid> ticketUID, TicketStatus ticketStatus, string modifiedBy)
        {
            return this.Repositorys.TicketRepository.UpdateTicketStatus(ticketUID, ticketStatus, modifiedBy);
        }
        public IActionResult<bool> ChangeTicketInfoStatus(Guid ticketInfoUID, TicketInfoStatus ticketInfoStatus)
        {
            //RETEST 重新測試
            return this.Repositorys.TicketInfoRepository.UpdateTicketInfoStatus(new Guid[] { ticketInfoUID }, ticketInfoStatus);
        }
        public IActionResult<bool> RollbackTicket(Guid warehouseUID, IEnumerable<ITicketModel> ticketUIDs)
        {
            //RETEST 測試

            var Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var ticketGrps = ticketUIDs.GroupBy(g => g.Type);
                foreach (var ticketModels in ticketGrps)
                {
                    //判斷不同Service Type 有不同處理方式
                    // Inbound Ticket->扣除onhand,刪paylaod (因payloaduid 會跟原本資料一樣要真刪)
                    // Outbound Ticket->加回onhand payload status->Active
                    var processer = RollbackProcesserAbstract
                                    .GetInstance(this.Repositorys, (TicketType)ticketModels.Key.Value);
                    if (processer != null)
                        Result.Add(processer.Execute(warehouseUID, ticketModels));
                }



                rs.Success = Result.All(p => p.Success);
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
        public IActionResult<bool> ChangeAllTicketStatus(Guid WorkOrderUID, TicketStatus ticketStatus,
            TicketInfoStatus ticketInfoStatus)
        {

            var ticketResult = this.Repositorys.TicketRepository.GetList(new { WorkOrderUID = WorkOrderUID });
            return this.PvtChangeAllTicketStatus(ticketResult, ticketStatus, ticketInfoStatus);
        }

        public IActionResult<bool> ChangeAllTicketStatus(Guid[] ticketUID, TicketStatus ticketStatus,
            TicketInfoStatus ticketInfoStatus)
        {

            var ticketResult = this.Repositorys.TicketRepository.GetList(new { UID = ticketUID });
            return this.PvtChangeAllTicketStatus(ticketResult, ticketStatus, ticketInfoStatus);
        }
        private IActionResult<bool> PvtChangeAllTicketStatus(IActionResult<IEnumerable<ITicketModel>> ticketResult, TicketStatus ticketStatus,
           TicketInfoStatus ticketInfoStatus)
        {
            var Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                if (ticketResult.Success && ticketResult.Content != null && ticketResult.Content.Count() > 0)
                {
                    //var ticketInfoResult = this.Repositorys.TicketInfoRepository.GetList(
                    //    new
                    //    {
                    //        TicketUID = ticketResult.Content.Select(p => p.UID)
                    //    });
                    Result.Add(this.Repositorys.TicketRepository.UpdateTicketStatus(ticketResult.Content.Select(p => p.UID), ticketStatus));
                    Result.Add(this.Repositorys.TicketInfoRepository
                        .UpdateTicketInfoStatusByTicket(ticketResult.Content.Select(p => p.UID), ticketInfoStatus));
                    //foreach (var item in ticketResult.Content)
                    //{
                    //    Result.Add(this.Repositorys.TicketRepository.UpdateTicketStatus(item.UID, ticketStatus));
                    //    Result.Add(this.Repositorys.TicketInfoRepository.UpdateTicketInfoStatus(item.UID, ticketInfoStatus));
                    //}
                    if (!Result.All(p => p.Success))
                    {
                        rs.Message = string.Join(",", Result.Select(x => x.Message));
                    }
                    else
                    {
                        rs.Success = true;
                    }
                }
                else
                {
                    rs.Message = Resource.MANIFEST_WORKORDER_NOT_FIND_WORKORDER;
                    rs.Success = false;
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
    }
}
