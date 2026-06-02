using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class TicketGenerateParameter : ITicketGenerateParameter
    {
        public Guid? ManifestUID { get; set; }
        public Guid? BolUID { get; set; }
        public Guid? VesselUID { get; set; }
        public Guid? WorkOrderUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public bool IsBulkPick { get; set; }
        public Guid? ManifestDataUID { get; set; }
        public bool ForceOpen { get; set; }
        public int ManifestType { get; set; }
        public IEnumerable<Guid> BolUIDs { get; set; }
    }
}
