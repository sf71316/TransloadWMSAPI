using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    public class PodInSlotInnerModel : IGetPodInSlotParameters
    {
        public Guid? PodUID { get; set; }
        public string PodNo { get; set; }
    }
}
