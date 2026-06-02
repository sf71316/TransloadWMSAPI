using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class GetBulkPickInfoParameters : IGetBulkPickInfoParameters
    {
        public Guid? BulkPickUID { get; set; }
        public Guid? BulkPickInfoUID { get; set; } 
    }
}
