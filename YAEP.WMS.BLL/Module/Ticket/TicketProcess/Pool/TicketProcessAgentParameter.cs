using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class TicketProcessAgentParameter : ITicketProcessAgentParameter
    {
        public ITicketManager TicketManager { get; set; }
        public IInventoryManager InventoryManager { get; set; }
        public ISequenceAgent SequenceAgent { get; set; }
        public IWarehouseManger WarehouseManger { get; set; }
        public IPackageManager PackageManager { get; set; }
        public ILabelRepository LabelRepository { get; set; }
        public IWorkOrderManager WorkOrderManager { get; set; }
        public StatusManageAgent StatusAgent { get; set; }
        public IPackageUomManager PackageUomManager { get; set; }
        public IManifestManger ManifestManager { get; set; }
        public INotificationSenderTaskRepository NotificationSenderTaskRepository { get; set; }
        public ILogInfiltrator LogInfiltrator { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }
        public IAppConfigure AppConfigure { get; set; }
        public IBulkPickManager BulkPickManager { get; set; }
        public ITicketInfoRepository TicketInfoRepository { get; set; }
        public ITicketRepository TicketRepository { get; set; }
        public IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        public IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        public IWorkOrderRepository WorkOrderRepository { get; set; }
        public ITicketInfoAssigneeRelationRepository TicketInfoAssigneeRelationRepository { get; set; }
        public ReplicationManager ReplicationManager { get; set; }
        public ITracingAgent TracingAgent { get; set; }
        public PackageCacheManager PackageCacheManager { get; set; }
        public ITransactionAction TransactionAction { get; set; }
    }
}
