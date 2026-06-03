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
    [Table("WMS_Vessel")]
    [DbTable("WMS_Vessel")]
    public class VesselModel : IVesselModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        [ExplicitKey]
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public string RefNo { get; set; }
        public Guid BolUID { get; set; }
        public int Status { get; set; }
        // Transload 容器實體屬性（對應 WMS_Vessel 新欄，皆可空，依名稱自動對欄位）
        public string SealNo { get; set; }
        public int? ContainerSize { get; set; }
        public int? LoadingType { get; set; }
        public int? StackableType { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public string StatusName { get; set; }
    }
}
