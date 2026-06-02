using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Interfaces
{
    internal interface IWMSReplicateExtModel
    {
        Guid ReplicateKey { get; set; }
        Guid ReplicateUID { get; set; }
    }
}
