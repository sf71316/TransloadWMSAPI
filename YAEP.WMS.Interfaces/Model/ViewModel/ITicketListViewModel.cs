using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketListViewModel
    {
        Guid TicketUID { get; set; }
        int TicketStatus { get; set; }
        string BolNo { get; set; }
        string VesselNo { get; set; }
        //int? ParentTicketStatus { get; set; }
        string TicketStatusName { get; set; }
        //string ParentTicketStatusName { get; set; }
        string ID { get; set; }
        string BolRefNo { get; set; }
        string VesselRefNo { get; set; }
        string BulkPickNo { get; set; }
        int TicketType { get; set; }
        string TicketTypeName { get; set; }
        Guid PartyUID { get; set; }
        string PartyName { get; set; }
        string Description { get; set; }
        Guid ContainerType { get; set; }
        string ContainerTypeName { get; set; }
        string OperationInstruction { get; set; }
        string OperationSuggestion { get; set; }
        IEnumerable<ITicketParentViewModel> Parent { get; set; }
        IEnumerable<string> FromSlots { get; set; }


    }


}
