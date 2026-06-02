using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IReceivingRequest
    {
        /// <summary>
        /// <para /> PO: Booking#
        /// <para /> SO: Order#
        /// </summary>
        string RefNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid WarehouseUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string CustomerPartyName { get; set; }
        Guid CustomerUID { get; set; }
        int ReceivingType { get; set; }

        /// <summary>
        /// Item
        /// </summary>
        string ReceiverUrl { get; set; }
        string ReceiverSecret { get; set; }
        bool IsTransferOrder { get; set; }
        IList<IReceivingContainer> Container { get; set; }
        IList<IPBSCItemPackagingModel> ImportItems { get; set; }
        void Init();
    }
}
