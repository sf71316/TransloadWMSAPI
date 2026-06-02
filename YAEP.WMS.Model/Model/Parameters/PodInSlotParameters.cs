using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class PodInSlotParameters : IGetPodInSlotParameters
    {
        public Guid? PodUID { get; set; }
        public string PodNo { get; set; }
    }
}
