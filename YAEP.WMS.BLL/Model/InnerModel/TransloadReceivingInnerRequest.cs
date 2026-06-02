using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    /// <summary>
    /// Transload 收貨在 BLL 內部組裝的 <see cref="IReceivingRequest"/>。
    /// 僅供 inbound auto-assign planner 讀取 Container.Items 的 Barcode / PackageQty / ItemUID（規畫 WorkOrder/Pod/Payload 用）。
    /// </summary>
    internal class TransloadReceivingInnerRequest : IReceivingRequest
    {
        public TransloadReceivingInnerRequest()
        {
            this.Container = new List<IReceivingContainer>();
            this.ImportItems = new List<IPBSCItemPackagingModel>();
        }
        public string RefNo { get; set; }
        public Guid WarehouseUID { get; set; }
        public string CustomerPartyName { get; set; }
        public Guid CustomerUID { get; set; }
        public int ReceivingType { get; set; }
        public string ReceiverUrl { get; set; }
        public string ReceiverSecret { get; set; }
        public bool IsTransferOrder { get; set; }
        public IList<IReceivingContainer> Container { get; set; }
        public IList<IPBSCItemPackagingModel> ImportItems { get; set; }
        public void Init() { }
    }
}
