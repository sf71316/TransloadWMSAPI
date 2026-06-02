using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IDeallocatedParameters
    {
        Guid PayloadUID { get; set; }
        int RecoveryPayloadType { get; set; }
    }
}
