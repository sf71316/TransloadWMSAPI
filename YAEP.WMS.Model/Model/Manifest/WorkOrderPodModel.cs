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
    [Table("WMS_WorkOrder_Pod")]
    [DbTable("WMS_WorkOrder_Pod")]
    public class WorkOrderPodModel : IWorkOrderPodModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        [ExplicitKey]
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid WorkOrderUID { get; set; }
        public Guid? PodUID { get; set; }
        public Guid BarcodeUID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Status { get; set; }
        public decimal? Volume { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public decimal? Weight { get; set; }
        public Guid? ContainerType { get; set; }
        public string OperationSuggestion { get; set; }
    }
}
