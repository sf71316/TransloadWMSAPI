using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class ImportInboundParameter : IImportInboundParameter
    {
        public HttpPostedFile File { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid WarehouseUID { get; set; }
    }
}
