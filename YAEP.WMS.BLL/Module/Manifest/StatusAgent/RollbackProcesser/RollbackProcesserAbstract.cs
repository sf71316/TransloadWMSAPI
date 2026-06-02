using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class RollbackProcesserAbstract
    {
        protected IStatusManageAgentParamters _parameters;
        public RollbackProcesserAbstract(IStatusManageAgentParamters paramters)
        {
            _parameters = paramters;
        }
        public abstract IActionResult<bool> Execute(Guid warehouseUID, IEnumerable<ITicketModel> ticketModels);
        public static RollbackProcesserAbstract GetInstance(IStatusManageAgentParamters paramters, TicketType ticketServiceType)
        {
            switch (ticketServiceType)
            {
                case TicketType.Receiving:
                    return new InboundRollbackProcesser(paramters);
                case TicketType.Outbound:
                    return new OutboundRollbackProcesser(paramters);
                case TicketType.Move:
                    return new MoveRollbackProcesser(paramters);
                case TicketType.InventoryCounting:
                default:
                    return null;
            }
        }
    }
}
