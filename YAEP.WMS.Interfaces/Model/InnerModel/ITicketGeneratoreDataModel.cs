using System;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketGeneratoreDataModel
    {
        Guid? PodBarcodeUID { get; set; }
        DateTime EndDate { get; set; }
        Guid ItemUID { get; set; }
        string Name { get; set; }
        Guid PackageUID { get; set; }
        Guid OriginalPackageUID { get; set; }
        Guid LoadingZoneSlotUID { get; set; }
        Guid PayloadUID { get; set; }
        Guid PodUID { get; set; }
        int Qty { get; set; }
        int OriginalQty { get; set; }
        Guid SlotUID { get; set; }
        DateTime StartDate { get; set; }
        int StoreageMethod { get; set; }
        Guid WorkOrderPodUID { get; set; }
        Guid WorkOrderPayloadUID { get; set; }
        Guid WorkOrderUID { get; set; }
        string OperationSuggestion { get; set; }
        ITicketLabelViewModel[] Labels { get; set; }
    }
}