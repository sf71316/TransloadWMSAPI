using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IPayloadChangeModel
    {
        Guid PayloadUID { get; set; }
        Guid? ChangepayloadPackageUID { get; set; }
        int? ChangepayloadPackageQty { get; set; }
        decimal VolumeLimit { get; set; }
        decimal WeightLimit { get; set; }
        string ModifiedBy { get; set; }
        int ChangeQty { get; set; }
        bool IsClear { get; set; }
    }
}
