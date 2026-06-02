using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IGetPodInSlotParameters
    {
        Guid? PodUID { get; set; }
        string PodNo { get; set; }
    }
}
