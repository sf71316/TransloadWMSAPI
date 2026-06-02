using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ReplicationlogModel : IReplicationlogModel
    {
        public Guid UID { get; set; }
        public Guid ItemUID { get; set; }
        public int Action { get; set; }
        public int Operate { get; set; }
        public int Quantity { get; set; }
        public string OriginalData { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public Guid ReplicateUID { get; set; }
        public Guid? BelongToUID { get; set; }
    }
}
