using System; 
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class GetBulkPickInfoParameters : IGetBulkPickInfoParameters
    {
        public Guid? BulkPickUID { get; set; }
        public Guid? BulkPickInfoUID { get; set; } 
    }
}
