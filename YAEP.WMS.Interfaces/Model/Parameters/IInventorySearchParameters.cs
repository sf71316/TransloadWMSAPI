using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IInventorySearchParameters
    {
        /// <summary>
        /// Item UID array
        /// </summary>
        IEnumerable<Guid> PHierarchy { get; set; }
        IEnumerable<string> CHierarchy { get; set; }
        Guid? CustomerUID { get; set; }
        Guid? WarehouseUID { get; set; }
        //Guid? AreaUID { get; set; }
        //Guid? BinUID { get; set; }
        //Guid? SlotUID { get; set; }
    }
}
