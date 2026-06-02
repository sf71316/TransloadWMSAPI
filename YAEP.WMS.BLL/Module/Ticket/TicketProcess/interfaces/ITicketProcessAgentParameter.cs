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
    internal interface ITicketProcessAgentParameter
    {
        ITransactionAction TransactionAction { get; set; }
        IAuthenticationProvider AuthenticationProvider { get; set; }
        ILogInfiltrator LogInfiltrator { get; set; }
        ITicketManager TicketManager { get; set; }
        IManifestManger ManifestManager { get; set; }
        IInventoryManager InventoryManager { get; set; }
        IWarehouseManger WarehouseManger { get; set; }
        IBulkPickManager BulkPickManager { get; set; }
        IWorkOrderManager WorkOrderManager { get; set; }
        ISequenceAgent SequenceAgent { get; set; }
        IPackageManager PackageManager { get; set; }
        IPackageUomManager PackageUomManager { get; set; }
        ILabelRepository LabelRepository { get; set; }
        StatusManageAgent StatusAgent { get; set; }
        INotificationSenderTaskRepository NotificationSenderTaskRepository { get; set; }
        //ITransacationScope TransactionScopeAgent { get; set; }
        IAppConfigure AppConfigure { get; set; }
        ITicketInfoRepository TicketInfoRepository { get; set; }
        ITicketInfoAssigneeRelationRepository TicketInfoAssigneeRelationRepository { get; set; }
        ITicketRepository TicketRepository { get; set; }
        IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        IWorkOrderPodRepository WorkOrderPodRepository { get; set; }
        IWorkOrderRepository WorkOrderRepository { get; set; }
        ITracingAgent TracingAgent { get; set; }
        ReplicationManager ReplicationManager { get; set; }
        PackageCacheManager PackageCacheManager { get; set; }
    }
}
