using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGetBulkPickInfoParameters
    {
        /// <summary>
        /// 
        /// </summary>
        Guid? BulkPickUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid? BulkPickInfoUID { get; set; }
    } 
}
