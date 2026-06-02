using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class GetAvailableInventoryListInnerParameters : IGetAvailableInventoryListParameters
    {
        public Guid? VesselManifestUID { get; set; }
        public Guid? WarehouseUID { get; set; }
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public Guid? SlotUID { get; set; }
        public string OptionText { get; set; }
        public string OptionValue { get; set; }
        public Guid PackageUID { get; set; }
        public Guid ItemUID { get; set; }
    }
}
