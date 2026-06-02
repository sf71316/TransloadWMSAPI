using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Slot")]
    [DbTable("WMS_Slot")]
    public class SlotModel : ISlotModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        [ExplicitKey]
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid? AreaUID { get; set; }
        public Guid? BinUID { get; set; }
        public decimal? VolumeLimit { get; set; }
        public decimal? WeightLimit { get; set; }
        public int Status { get; set; }
        public int AllocatedSequence { get; set; } = 1;
        public int StorageSequence { get; set; } = 1;
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsDefaultLoadingZone { get; set; }
    }
}
