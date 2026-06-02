using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class TicketInfoListInnerViewModel : ITicketInfoListViewModel
    {
        public Guid UID { get; set; }
        public int StorageType { get; set; }
        public string TicketID { get; set; }
        public int TicketType { get; set; }
        public int ManifestType { get; set; }
        public string TicketTypeName { get; set; }
        public int TicketInfoStatus { get; set; }
        public string TicketInfoStatusName { get; set; }
        public string Description { get; set; }
        public Guid PodUID { get; set; }
        public Guid PayloadUID { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public int EstQty { get; set; }
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public string PodName { get; set; }
        public List<ITicketLabelViewModel> Labels { get; set; }
        public bool IsPodExist { get; set; }
        public ILocation OriginalLocation { get; set; }
        public ILocation TargetLocation { get; set; }
        public Guid? TargetSlotUID { get; set; }
        public Guid? TargetPackage { get; set; }
        public Guid? OriginalPackage { get; set; }
        public Guid? OriginalSlotUID { get; set; }
        public Guid SourceLoadingZoneSlotUID { get; set; }
        public Guid SourcePackageUID { get; set; }
        public Guid SourceSlotUID { get; set; }
        public Guid PayloadPackageUID { get; set; }
        public int MappingType { get; set; }
        public string OriginalPackageName { get; set; }
        public string TargetPackageName { get; set; }
        public string TargetUOMName { get; set; }
        public Guid ContainerType { get; set; }
        public string ContainerTypeName { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public string PodBarcode { get; set; }
        public Guid WarehouseUID { get; set; }
    }
}
