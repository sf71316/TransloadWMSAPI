using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class GetAvailableInventoryRequest : IGetAvailableInventoryParameters
    {
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public Guid? SlotUID { get; set; }
        public Guid ItemUID { get; set; }
        public string Option { get; set; }
        public string OptionText { get; set; }
        public Guid? Warehouse { get; set; }
    }
}