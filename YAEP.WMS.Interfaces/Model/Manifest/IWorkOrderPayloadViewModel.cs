using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderPayloadViewModel : IWorkOrderPayloadModel
    {
        
        string PodName { get; set; }
        string AreaName { get; set; }
        string BinName { get; set; }
        string SlotName { get; set; }
        string PackageName { get; set; }
        string ItemID { get; set; }
        string StatusName { get; set; }

        IEnumerable<ITicketLabelViewModel> Labels { get; set; }
    }
}
