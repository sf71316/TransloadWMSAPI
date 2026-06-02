using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class SlotUsageInfoInnerModel : ISlotUsageInfoModel
    {
        public Guid SlotUID { get; set; }
        public string SlotID { get; set; }
        public decimal VolumeLimit { get; set; }
        public decimal WeightLimit { get; set; }
        public decimal Volume { get; set; }
        public decimal Weight { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public int StorageSequence { get; set; }
        public int AllocatedSequence { get; set; }
    }
}
