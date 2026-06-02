using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class HomeAddressSlotSsageInfoModel : ISlotUsageInfoModel
    {
        public HomeAddressSlotSsageInfoModel(ISlotUsageInfoModel slotUsageInfoModel)
        {
            this.SlotUID = slotUsageInfoModel.SlotUID;
            this.SlotID = slotUsageInfoModel.SlotID;
            this.SlotID = slotUsageInfoModel.SlotID;
            this.VolumeLimit = slotUsageInfoModel.VolumeLimit;
            this.WeightLimit = slotUsageInfoModel.WeightLimit;
            this.Volume = slotUsageInfoModel.Volume;
            this.Weight = slotUsageInfoModel.Weight;
            this.WarehouseUID = slotUsageInfoModel.WarehouseUID;
            this.AreaUID = slotUsageInfoModel.AreaUID;
            this.BinUID = slotUsageInfoModel.BinUID;
            this.StorageSequence = slotUsageInfoModel.StorageSequence;
            this.AllocatedSequence = slotUsageInfoModel.AllocatedSequence;
        }
        public Guid ItemUID { get; set; }
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
