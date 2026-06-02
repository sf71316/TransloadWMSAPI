using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderPodModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int Type { get; set; }
        Guid? ContainerType { get; set; }
        Guid WorkOrderUID { get; set; }
        Guid? PodUID { get; set; }
        Guid BarcodeUID { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        int Status { get; set; }
        decimal? Volume { get; set; }
        decimal? Weight { get; set; }
        string OperationSuggestion { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}
