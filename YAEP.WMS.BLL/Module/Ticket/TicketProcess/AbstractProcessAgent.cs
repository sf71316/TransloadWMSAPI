using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal abstract class AbstractProcessAgent
    {
        //protected ITicketManager _ticketManager;
        //protected IInventoryManager _inventoryManager;
        //protected IManifestAgent _manifestAgent;
        //protected ISequenceAgent _sequenceAgent;
        //protected IWarehouseManger _warehouseManger;
        //protected IPackageManager _packageManager;
        protected ITicketProcessAgentParameter _Parameter;
        protected static Lazy<AbstractProcessAgent> _Agent;
        protected ILogInfiltrator logInfiltrator;
        private static readonly object _Padlock = new object();
        public AbstractProcessAgent(ITicketProcessAgentParameter Parameter)
        {
            logInfiltrator = Parameter.LogInfiltrator;
            //_ticketManager = Parameter.TicketManager;
            //_manifestAgent = Parameter.ManifestAgent;
            //_inventoryManager = Parameter.InventoryManager;
            //_warehouseManger = Parameter.WarehouseManger;
            //_sequenceAgent = Parameter.SequenceAgent;
            //_packageManager = Parameter.PackageManager;
            this._Parameter = Parameter;
        }
        public static AbstractProcessAgent GetAgent(ProcessKind kind, ITicketProcessAgentParameter Parameter)
        {
            // 目前並不允許多個TicketProcessAgent 同時使用
            if (kind == ProcessKind.TicketProcess)
            {
                //if (_Agent == null)
                //{
                //    _Agent = new Lazy<AbstractProcessAgent>(
                //        (() => new TicketProcessAgent(Parameter)),
                //        LazyThreadSafetyMode.ExecutionAndPublication);

                //}
                return new TicketProcessAgent(Parameter);
            }
            return _Agent.Value;
        }
        public abstract Guid AgentID { get; }


        public abstract IActionResult<bool> Process(IEnumerable<IUploadTicketDataParameter> parameter,
            NotifySenderConfig SendInfo = null);

        internal abstract ConcurrentStack<Func<IActionResult<bool>>> CheckStatus(Guid ticketUID);
        public abstract ConcurrentQueue<Func<IActionResult<bool>>> CompleteUnexecutedMethod();
    }
}
