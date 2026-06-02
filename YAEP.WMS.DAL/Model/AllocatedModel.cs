using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class AllocatedModel : IAllocatedModel
    {
        public Guid WarehouseUID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }
        public int Quantity { get; set; }
        public int OriginalPayloadType { get; set; }
    }
}
