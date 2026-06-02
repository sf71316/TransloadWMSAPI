using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class BulkPickWorkOrderPayloadRelationModel : IBulkPickWorkOrderPayloadRelationModel
    {
        public Guid UID { get; set; }
        public Guid BulkPickWorkOrderPayloadUID { get; set; }
        public Guid OriginalWorkOrderPayloadUID { get; set; }
        public int Status { get; set; }
    }
}
