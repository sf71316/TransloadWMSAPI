using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class HaveTicketParameters : IHaveTicketParameters
    {
        public Guid[] workOrderPodGuids { get; set; }
        public Guid[] workOrderPayloadGuids { get; set; }
    }
}
