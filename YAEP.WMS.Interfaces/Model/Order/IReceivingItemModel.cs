using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.Interfaces
{
    public interface IReceivingItemModel
    {
        string Barcode { get; set; }
        Guid UID { get; set; }
        /// <summary>
        /// Product ID
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <summary>
        /// 
        /// </summary>
        string PackageUOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int PackageQty { get; set; }

        bool UseMinUOM { get; set; }
        int InventoryRatio { get; set; }
        string ExternalData { get; set; }

        Guid ContainerUID { get; set; }
        Guid? ItemGroupUID { get; set; }
        IList<Guid> ItemUID { get; set; }
        IList<IItemModel> VirtualItems { get; set; }
    }
}
