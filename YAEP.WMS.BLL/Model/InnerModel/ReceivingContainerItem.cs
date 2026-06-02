using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ReceivingContainerItem : IReceivingItemModel
    {
        public ReceivingContainerItem()
        {
            this.ItemUID = new List<Guid>();
        }
        public string Barcode { get; set; }
        public Guid UID { get; set; }
        public string Name { get; set; }
        public string PackageUOM { get; set; }
        public int PackageQty { get; set; }
        public bool UseMinUOM { get; set; }
        public int InventoryRatio { get; set; }
        public string ExternalData { get; set; }
        public Guid ContainerUID { get; set; }
        public IList<IItemModel> VirtualItems { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public IList<Guid> ItemUID { get; set; }
    }
}
