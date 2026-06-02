using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class PickallViewModel : IPickallViewModel
    {
        public Guid TicketInfoUID { get; set; }
        public int TicketInfoType { get; set; }
        public Guid TicketUID { get; set; }
        public Guid WorkOrderPayloadUID { get; set; }
        public Guid PodUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid PackageUID { get; set; }
        public int Quantity { get; set; }
        public Guid PayloadUID { get; set; }
        public int PayloadType { get; set; }
        public int OriginalPayloadType { get; set; }
    }
}
