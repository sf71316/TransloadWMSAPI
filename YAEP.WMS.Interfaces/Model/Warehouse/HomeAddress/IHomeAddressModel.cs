using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IHomeAddressRelationModel
    {
        Guid UID { get; set; }
        Guid ItemCategoryUID { get; set; }
        Guid ItemUID { get; set; }
        Guid SlotUID { get; set; }
        int Type { get; set; }
        int OutboundType { get; set; }
        int Sequence { get; set; }
        int Status { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedOn { get; set; }
    }
}
