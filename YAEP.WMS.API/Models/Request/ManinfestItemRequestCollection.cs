using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class ManinfestItemRequestCollection
    {
        public ManinfestItemRequestCollection()
        {
            this.Data = new List<ManinfestItemRequestModel>();
        }
        public IEnumerable<ManinfestItemRequestModel> Data { get; set; }
    }
}