using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class DeleteManifestRequest : IDeleteManifestRequest
    {
        public string RefNo { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public string RequestBy { get; set; }
        public bool IgnoreCheckManifest { get; set; }
        public bool ForceDelete { get; set; }
    }
}
