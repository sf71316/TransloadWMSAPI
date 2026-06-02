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

namespace YAEP.WMS.BLL.Module
{
    internal class MoveRollbackProcesser : RollbackProcesserAbstract
    {
        public MoveRollbackProcesser(IStatusManageAgentParamters paramters) : base(paramters)
        {

        }
        public override IActionResult<bool> Execute(Guid warehouseUID,
            IEnumerable<ITicketModel> ticketModels)
        {
            var Result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                //outbound 因為slot就定還原，如果Ticket 完成就不需要move ticket 
                if (ticketModels.First().ManifestType == (int)ManifestType.Inbound ||
                    (ticketModels.First().ManifestType == (int)ManifestType.Outbound &&
                    ticketModels.First().Status < (int)TicketStatus.Glitch))
                {
                    //rollback ticket status
                    Result.Add(this._parameters.TicketRepository
                        .UpdateTicketStatus(ticketModels.Select(x => x.UID), TicketStatus.Open));
                    ////rollback ticketinfo qty & status
                    Result.Add(this._parameters.TicketInfoRepository
                        .RollbackTicketInfo(ticketModels.Select(x => x.UID),
                        TicketInfoStatus.Open));
                    rs.Content = rs.Success = Result.All(x => x.Success);
                    if (!rs.Success)
                    {
                        rs.Message = string.Join(",", Result.Select(x => x.Message));
                    }
                }
                else
                {
                    rs.Content = true;
                    rs.Success = true;
                    rs.Message = "don't roll back";
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
