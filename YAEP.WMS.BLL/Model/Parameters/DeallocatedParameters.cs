using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL
{
    internal class DeallocatedParameters : IDeallocatedParameters
    {
        public Guid PayloadUID { get; set; }
        public int RecoveryPayloadType { get; set; }
    }
}
