using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class TicketAssignedListInnerViewModel : ITicketAssignedListViewModel
    {
        public Guid TicketUID { get; set; }
        public Guid TicketInfoUID { get; set; }
        public Guid WarehouseGroupUID { get; set; }
        public string TicketNo { get; set; }
        public int ServiceType { get; set; }
        public int ManifestType { get; set; }
        public string ServiceTypeName { get; set; }
        public TicketInfoStatus Status { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public int EstQty { get; set; }
        public int ActQty { get; set; }
        public int ShtQty { get; set; }
        public int SavQty { get; set; }
        public string OriginalSlotName { get; set; }
        public string TargetSlotName { get; set; }
        public string AssignedGroup { get; set; }
        public string VesselName { get; set; }
        public Guid VesselUID { get; set; }
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
        public Guid WorkOrderPodUID { get; set; }
    }
}
