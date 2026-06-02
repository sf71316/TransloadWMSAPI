using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICloneLabelModel
    {
        string CreatedBy { get; set; }
        Guid SourceBelongToUID { get; set; }
        Guid TargetBelongToUID { get; set; }
    }
}
