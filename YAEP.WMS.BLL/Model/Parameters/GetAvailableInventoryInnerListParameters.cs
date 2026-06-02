using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class GetAvailableInventoryInnerListParameters : IGetAvailableInventoryDataInnerListParameters
    {
        public GetAvailableInventoryInnerListParameters()
        {
            this.Items = new Dictionary<int, IEnumerable<Guid>>();

        }
        public Guid? WarehouseUID { get; set; }
        //public Guid[] ItemUID { get; set; }
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public Guid? SlotUID { get; set; }
        public string OptionText { get; set; }
        public string OptionValue { get; set; }
        public bool IsincludeReceivingQty { get; set; }
        public SlotStatus[] SlotStatuses { get; set; }
        public Dictionary<int, IEnumerable<Guid>> Items { get; set; }
    }
}
