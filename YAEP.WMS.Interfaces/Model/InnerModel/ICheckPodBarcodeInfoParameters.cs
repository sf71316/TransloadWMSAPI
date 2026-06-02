using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICheckPodBarcodeInfoParameters
    {
        IEnumerable<Guid> BelongToUID { get; set; }
        IEnumerable<Guid> ItemUID { get; set; }
        IEnumerable<Guid> SlotUID { get; set; }
        IEnumerable<Guid> WarehouseUID { get; set; }
        int LabelType { get; set; }
    }
}
