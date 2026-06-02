using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IEditWorkOrderPodParameters
    {
        Guid UID { get; set; }
        string Name { get; set; }
        int? Type { get; set; }
        Guid? ContainerType { get; set; }
        string OperationSuggestion { get; set; }
    }
}
