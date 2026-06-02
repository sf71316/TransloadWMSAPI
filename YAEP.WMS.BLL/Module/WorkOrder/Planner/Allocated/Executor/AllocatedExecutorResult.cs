using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class AllocatedExecutorResult
    {
        public AllocatedExecutorResult()
        {
            this.Payloads = new List<IPayloadModel>();
            this.CloneLabels = new List<ICloneLabelModel>();
        }
        public List<IPayloadModel> Payloads { get; set; }
        public List<ICloneLabelModel> CloneLabels { get; set; }
    }
}
