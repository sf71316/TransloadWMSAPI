using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class TicketProcessAgent : AbstractProcessAgent
    {
        AbstractProcessModule _ProcessModule;
        public TicketProcessAgent(ITicketProcessAgentParameter parameter)
            : base(parameter)
        {
           
        }
        public override Guid AgentID => new Guid("bd83deff-e614-4e63-b1ad-5b03fd6117f3");
        public override IActionResult<bool> Process(IEnumerable<IUploadTicketDataParameter> parameter,
            NotifySenderConfig sendInfo = null)
        {
            //RETEST 重新修改Ticket 物件呼叫方式
            //確認上傳的Ticket 都是同一種Service Type
            if (parameter.GroupBy(p => p.ServiceItem).Count() == 1)
            {
                //if (transactionScopeAgent != null)
                //    this._Parameter.TransactionScopeAgent = transactionScopeAgent;
                _ProcessModule =
                   AbstractProcessModule.GetProcessModule(parameter.First().ServiceItem, this._Parameter, this.logInfiltrator);
                if (_ProcessModule != null)
                {
                    return _ProcessModule.Execute(parameter, sendInfo);
                }
            }
            return null;

        }
        public override ConcurrentQueue<Func<IActionResult<bool>>> CompleteUnexecutedMethod()
        {
            return _ProcessModule.CompleteUnexecutedMethod;
        }
        internal override ConcurrentStack<Func<IActionResult<bool>>> CheckStatus(Guid ticketUID)
        {

            var _ProcessModule =
                    AbstractProcessModule.GetProcessModule(TicketType.Outbound, this._Parameter, this.logInfiltrator);
            return _ProcessModule.CheckDataStatus(ticketUID);
        }
    }
}
