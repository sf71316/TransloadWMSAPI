using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{

    internal class TicketGeneratoreDataModel : ITicketGeneratoreDataModel
    {
        public Guid WorkOrderUID { get; set; }
        public Guid WorkOrderPodUID { get; set; }
        public string Name { get; set; }
        public Guid PodUID { get; set; }
        public int StoreageMethod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid PayloadUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }
        public Guid LoadingZoneSlotUID { get; set; }
        public Guid OriginalPackageUID { get; set; }
        public int Qty { get; set; }
        public int OriginalQty { get; set; }
        public Guid WorkOrderPayloadUID { get; set; }
        public ITicketLabelViewModel[] Labels { get; set; }
        public string OperationSuggestion { get; set; }
        public Guid? PodBarcodeUID { get; set; }
    }

}
