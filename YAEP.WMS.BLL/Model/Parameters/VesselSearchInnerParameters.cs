using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class VesselSearchInnerParameters : IVesselSearchParameters
    {
        public Guid BolUID { get; set; }
        public string RefNo { get; set; }
    }
}
