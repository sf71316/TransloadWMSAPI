using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace YAEP.WMS.Interfaces
{
    public interface IImportInboundParameter
    {
        Guid CustomerUID { get; set; }
        Guid WarehouseUID { get; set; }
        HttpPostedFile File { get; set; }
    }
}
