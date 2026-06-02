using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderPodParameter
    {
        Guid? UID { get; set; }
        int Type { get; set; }
        Guid VesselUID { get; set; }
        Guid? PodUID { get; set; }
        Guid? BarcodeUID { get; set; }
        Guid? ContainerType { get; set; }
        string OperationSuggestion { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        decimal Volume { get; set; }
        string Name { get; set; }
    }
}
