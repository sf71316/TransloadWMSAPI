using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class CheckPodBarcodeInfoParameters : ICheckPodBarcodeInfoParameters
    {
        public IEnumerable<Guid> ItemUID { get; set; }
        public IEnumerable<Guid> SlotUID { get; set; }
        public int LabelType { get; set; }
        public IEnumerable<Guid> BelongToUID { get; set; }
        public IEnumerable<Guid> WarehouseUID { get; set; }
    }
}
