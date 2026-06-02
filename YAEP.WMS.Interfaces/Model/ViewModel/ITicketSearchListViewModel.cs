using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketSearchListViewModel : ITicketInfoCommonViewModel
    {
        Guid TicketUID { get; set; }
        int TicketStatus { get; set; }
        string TicketStatusName { get; set; }
        string TicketNo { get; set; }
        Guid ManifestUID { get; set; }
        string ManifestNo { get; set; }
        string BOLRefNo { get; set; }
        string VesselRefNo { get; set; }
        Guid BolUID { get; set; }
        string BolNo { get; set; }
        string VesselNo { get; set; }
        int TicketType { get; set; }
        string TicketTypeName { get; set; }
        Guid? PartyUID { get; set; }
        string OperationInstruction { get; set; }
        string OperationSuggestion { get; set; }
        string TicketSequence { get; set; }
        DateTime? DeliveryDate { get; set; }
        DateTime? ETA { get; set; }
        DateTime? RevETA { get; set; }
    }
}