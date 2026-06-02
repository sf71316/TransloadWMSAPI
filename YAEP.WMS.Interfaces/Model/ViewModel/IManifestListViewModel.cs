using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IManifestListViewModel
    {
        Guid UID { get; set; }
        string ManifestNo { get; set; }
        string ManifestName { get; set; }
        string CustNo { get; set; }
        ManifestType Type { get; set; }
        string TypeName { get; set; }
        ManifestStatus Status { get; set; }
        string StatusName { get; set; }
        string RefNo { get; set; }
        Guid WarehouseUID { get; set; }
        Guid PartyUID { get; set; }
    }
}
