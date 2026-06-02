using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IReplicationlogModel
    {
        Guid UID { get; set; }
        Guid? BelongToUID { get; set; }
        Guid ReplicateUID { get; set; }
        Guid ItemUID { get; set; }
        int Operate { get; set; }
        int Action { get; set; }
        int Quantity { get; set; }
        string OriginalData { get; set; }
        bool IsComplete { get; set; }
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; }
    }
}
