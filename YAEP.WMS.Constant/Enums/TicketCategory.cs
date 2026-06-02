using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum TicketCategory
    {
        /// <summary>
        /// Inbound
        /// </summary>
        Receive = 100,
        /// <summary>
        /// Outbound
        /// </summary>
        Outbound = 200,
        /// <summary>
        /// Move
        /// </summary>
        Move = 300,
        /// <summary>
        /// Invenoty Counting
        /// </summary>
        InventoryCounting = 400,
        BulkPick = 301
    }
}
