using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;

namespace YAEP.WMS.API.Models
{
    public class ReceivingRequest : IReceivingRequest
    {
        public ReceivingRequest()
        {
            this.Container = new List<InboundContainerModel>().ToArray();
            this.ImportItems = new List<PBSCItemPackagingModel>().ToArray();
        }
        public string RefNo { get; set; }
        public Guid WarehouseUID { get; set; }
        public IList<IReceivingContainer> Container { get; set; }
        public int ReceivingType { get; set; }
        public string ReceiverUrl { get; set; }
        public string ReceiverSecret { get; set; }
        public string CustomerPartyName { get; set; }
        public Guid CustomerUID { get; set; }
        public IList<IPBSCItemPackagingModel> ImportItems { get; set; }
        public bool IsTransferOrder { get; set; }

        public void Init()
        {
            this.Container.ToList().ForEach(p => p.UID = Guid.NewGuid());
            this.Container.ToList().ForEach(p => p.Items.ToList().ForEach(y => y.UID = Guid.NewGuid()));
            foreach (var c in Container)
            {
                foreach (var item in c.Items)
                {
                    item.ContainerUID = c.UID;
                }
            }
        }
    }
    public class InboundItemModel : IReceivingItemModel
    {
        public InboundItemModel()
        {
            this.ItemUID = new List<Guid>();
        }
        public string Name { get; set; }
        public string PackageUOM { get; set; }
        public int PackageQty { get; set; }
        public bool UseMinUOM { get; set; }
        public Guid UID { get; set; }
        public int InventoryRatio { get; set; }
        public string ExternalData { get; set; }
        public Guid ContainerUID { get; set; }
        public string Barcode { get; set; }
        public IList<IItemModel> VirtualItems { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public IList<Guid> ItemUID { get; set; }
    }
    public class InboundContainerModel : IReceivingContainer
    {
        public InboundContainerModel()
        {
            Items = new List<InboundItemModel>().ToArray();
            ManifestItemUID = new List<Guid>();
        }
        public string ExternalData { get; set; }
        public string PackageUOM { get; set; }

        public IList<IReceivingItemModel> Items { get; set; }
        public Guid UID { get; set; }
        public IList<Guid> ManifestItemUID { get; set; }
        public Guid VesselManifestUID { get; set; }
    }
}