using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISetWorkOrderPodBarcodeParameters
    {
        Guid BarcodeUID { get; set; }
        Guid PodUID { get; set; }
    }
}
