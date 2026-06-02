using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAEP.WMS.Interfaces
{
    public interface IDeleteManifestRequest
    {
        bool IgnoreCheckManifest { get; set; }
        bool ForceDelete { get; set; }
        string RequestBy { get; set; }
        string RefNo { get; set; }
        Guid CustomerUID { get; set; }
        Guid WarehouseUID { get; set; }
    }
}
