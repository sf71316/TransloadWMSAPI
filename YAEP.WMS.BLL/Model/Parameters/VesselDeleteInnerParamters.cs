using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class VesselDeleteInnerParamters : IVesselDeleteParamters
    {
        public Guid[] UID { get; set; }
    }
}
