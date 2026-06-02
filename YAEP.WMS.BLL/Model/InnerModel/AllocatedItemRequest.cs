using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AllocatedItemRequest : IAllocatedItemRequest
    {
        public IList<IItemModel> VirtualItems {get;set;}
        public PackingStationCarrier? Carrier {get;set;}
        public Guid ManifestItemUID {get;set;}
        public Guid VesselManifestUID {get;set;}
        public Guid ShipPackageUID {get;set;}
        public Guid ShipViaUID {get;set;}
        public int PalletID {get;set;}
        public string PalletBarcode {get;set;}
        public int Line_No {get;set;}
        public int ComponentType {get;set;}
        public string ItemNo {get;set;}
        public string ParentItemNo {get;set;}
        public int Qty {get;set;}
        public bool UseMiniPackage {get;set;}
        public int ShipViaNo {get;set;}
        public string VirtualItemParentItemNo { get; set; }
        public bool IsHasVirtualItem { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public int AllocatedOnhandType { get; set; }
    }
}
