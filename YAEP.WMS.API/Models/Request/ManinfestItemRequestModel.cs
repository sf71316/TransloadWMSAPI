using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class ManinfestItemRequestContainer
    {
        public ManinfestItemRequestModel[] Items { get; set; }
    }
    public class ManinfestItemRequestModel
    {
        public Guid ManifestUID { get; set; }
        public Guid? UID { get; set; }
        public Guid ItemUID { get; set; }
        public Guid PackageUID { get; set; }
        public int PackageQty { get; set; }
    }

}