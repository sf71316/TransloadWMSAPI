using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class CheckItemRequestModel
    {
        public CheckItemRequestModel()
        {
            this.itemNames = new List<string>();
        }
        public Guid customerUID { get; set; }
        public IEnumerable<string> itemNames { get; set; }
    }
    public class CheckSlotRequestModel
    {
        public CheckSlotRequestModel()
        {
            this.slotNames = new List<string>();
        }
        public Guid warehouseUID { get; set; }
        public IEnumerable<string> slotNames { get; set; }
    }
}