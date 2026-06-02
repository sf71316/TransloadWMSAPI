using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class WorkOrderPodParameter : IWorkOrderPodParameter
    {
        public Guid? UID { get; set; }
        public int Type { get; set; }
        public Guid VesselUID { get; set; }
        public Guid? PodUID { get; set; }
        public Guid? BarcodeUID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Volume { get; set; }
        public string Name { get; set; }
        public Guid? ContainerType { get; set; }
        public string OperationSuggestion { get; set; }
    }
}
