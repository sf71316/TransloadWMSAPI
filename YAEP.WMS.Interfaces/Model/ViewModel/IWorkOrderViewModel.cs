using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderViewModel : IWorkOrderPodModel
    {

        string Barcode { get; set; }
        string TypeName { get; set; }
        string StatusName { get; set; }
        string LoadingZoneName { get; set; }
        string BinName { get; set; }
        string AreaName { get; set; }
        string SlotName { get; set; }
    }
}
