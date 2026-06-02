using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    public class IGetLocationItemListRequest : IGetAvailableInventoryListParameters
    {
        public Guid? WarehouseUID { get; set; }
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public Guid? SlotUID { get; set; }
        public string OptionText { get; set; }
        public string OptionValue { get; set; }
        public Guid PackageUID { get; set; }
        public Guid? VesselManifestUID { get; set; }
        public Guid ItemUID { get; set; }
    }
}