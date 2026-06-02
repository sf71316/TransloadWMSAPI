using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketGenerateParameter
    {
        Guid WarehouseUID { get; set; }
        Guid? ManifestUID { get; set; }
        Guid? BolUID { get; set; }
        IEnumerable<Guid> BolUIDs { get; set; }
        Guid? VesselUID { get; set; }
        Guid? WorkOrderUID { get; set; }
        Guid? ManifestDataUID { get; set; }
        bool ForceOpen { get; set; }
        bool IsBulkPick { get; set; }
        int ManifestType { get; set; }
    }
}
