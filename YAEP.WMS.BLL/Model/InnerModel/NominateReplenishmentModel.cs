using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public class NominateReplenishmentModel : INominateReplenishmentModel
    {
        public NominateReplenishmentModel() { }

        public Guid? WarehouseUID { get; set; }
        public IEnumerable<Guid> WorkOrderPayloadUIDList { get; set; }
    }
}