using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
     public interface IPBSCItemModel : IItemData
    {
        string CustomerID { get; set; }
        /// <summary>
        /// 
        /// </summary>
         Guid CustomerUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
         Guid GroupUID { get; set; }
    }
}
