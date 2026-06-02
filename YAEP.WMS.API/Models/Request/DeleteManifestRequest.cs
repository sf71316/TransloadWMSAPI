using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class DeleteManifestRequest : IDeleteManifestRequest
    {
        public string RefNo { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public string RequestBy { get; set; }
        public bool IgnoreCheckManifest { get; set; }
        public bool ForceDelete { get; set; }
    }
}