using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketAssignedListViewModel : ITicketInfoCommonViewModel
    {
        Guid WorkOrderPodUID { get; set; }
        Guid TicketUID { get; set; }
        Guid TicketInfoUID { get; set; }
        Guid WarehouseGroupUID { get; set; }
        string TicketNo { get; set; }
        int ServiceType { get; set; }
        int ManifestType { get; set; }
        string ServiceTypeName { get; set; }
        TicketInfoStatus Status { get; set; }
        Guid ItemUID { get; set; }
        string ItemID { get; set; }
        int EstQty { get; set; }
        int ActQty { get; set; }
        int ShtQty { get; set; }
        int SavQty { get; set; }
        string OriginalSlotName { get; set; }
        string TargetSlotName { get; set; }
        string AssignedGroup { get; set; }
        string VesselName { get; set; }
        Guid VesselUID { get; set; }
        ILocation OriginalLocation { get; set; }
        ILocation TargetLocation { get; set; }
    }
}
