using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketInfoListViewModel : ITicketInfoCommonViewModel
    {
        Guid UID { get; set; }
        Guid ContainerType { get; set; }
        string ContainerTypeName { get; set; }
        int StorageType { get; set; }
        string TicketID { get; set; }
        int TicketType { get; set; }
        int ManifestType { get; set; }
        string TicketTypeName { get; set; }
        int TicketInfoStatus { get; set; }
        string TicketInfoStatusName { get; set; }
        string Description { get; set; }
        Guid PodUID { get; set; }
        Guid PayloadUID { get; set; }
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        int EstQty { get; set; }
        int ActQty { get; set; }
        int ShtQty { get; set; }
        int SavQty { get; set; }
        Guid WorkOrderPodUID { get; set; }
        Guid WarehouseUID { get; set; }
        string PodName { get; set; }
        string PodBarcode { get; set; }
        List<ITicketLabelViewModel> Labels { get; set; }
        bool IsPodExist { get; set; }
        string OperationInstruction { get; set; }
        string OperationSuggestion { get; set; }
        ILocation OriginalLocation { get; set; }
        ILocation TargetLocation { get; set; }
    }
}
