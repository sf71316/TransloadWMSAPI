using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class TicketListInnerModel : ITicketListViewModel
    {
        public TicketListInnerModel()
        {
            this.Parent = new List<ITicketParentViewModel>();
        }
        public Guid TicketUID { get; set; }
        public string ID { get; set; }
        public string BolRefNo { get; set; }
        public string VesselRefNo { get; set; }
        public int TicketType { get; set; }
        public Guid PartyUID { get; set; }
        public string PartyName { get; set; }
        public int TicketStatus { get; set; }
        public string TicketStatusName { get; set; }
        public string TicketTypeName { get; set; }
        //public int? ParentTicketStatus { get; set; }
        //public string ParentTicketStatusName { get; set; }
        public IEnumerable<ITicketParentViewModel> Parent { get; set; }
        public string Description { get; set; }
        public Guid ContainerType { get; set; }
        public string ContainerTypeName { get; set; }
        public string OperationInstruction { get; set; }
        public string OperationSuggestion { get; set; }
        public string BolNo { get; set; }
        public string VesselNo { get; set; }
        public IEnumerable<string> FromSlots { get; set; }
        public string BulkPickNo { get; set; }
    }
    internal class TicketParentViewInnerModel : ITicketParentViewModel
    {
        public string ParentTicketStatusName { get; set; }
        public int? ParentTicketStatus { get; set; }
        public Guid ParentTicketUID { get; set; }
        public Guid TicketUID { get; set; }
        public bool IsLock { get; set; }
        public string ParentTicketID { get; set; }
    }
}
