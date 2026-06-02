using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class TicketListSearchModel : ITicketSearchListViewModel
    {
        public TicketListSearchModel()
        {
        }

        public Guid TicketUID { get; set; }
        public int TicketStatus { get; set; }
        public string TicketStatusName { get; set; }
        public string TicketNo { get; set; }
        public Guid ManifestUID { get; set; }
        public string ManifestNo { get; set; }
        public string BOLRefNo { get; set; }
        public string VesselRefNo { get; set; }
        public Guid BolUID { get; set; }
        public string BolNo { get; set; }
        public string VesselNo { get; set; }
        public int TicketType { get; set; }
        public string TicketTypeName { get; set; }
        public Guid? PartyUID { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public string TicketSequence { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? RevETA { get; set; }
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
    }
}