using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISequenceModel
    {
        Guid UID { get; set; }
        string BelongToUID { get; set; }
        string BelongToTag { get; set; }
        int SequenceValue { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedOn { get; set; }
    }
}
