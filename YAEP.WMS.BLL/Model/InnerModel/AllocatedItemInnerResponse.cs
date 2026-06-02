using System;
using System.Collections.Generic;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AllocatedItemInnerResponse : IAllocatedItemResponse
    {
        public AllocatedItemInnerResponse()
        {
        }

        public AllocatedItemInnerResponse(IAllocatedItemRequest request)
        {
            this.ManifestItemUID = request.ManifestItemUID;
            this.VesselManifestUID = request.VesselManifestUID;
            this.ShipPackageUID = request.ShipPackageUID;
            this.ShipViaUID = request.ShipViaUID;
            this.PalletID = request.PalletID;
            this.Line_No = request.Line_No;
            this.ComponentType = request.ComponentType;
            this.ItemNo = request.ItemNo;
            this.ParentItemNo = request.ParentItemNo;
            this.Qty = request.Qty;
            this.UseMiniPackage = request.UseMiniPackage;
            this.Carrier = request.Carrier;
            this.ShipViaNo = request.ShipViaNo;
            this.VirtualItemParentItemNo = request.VirtualItemParentItemNo;
            this.ItemGroupUID = request.ItemGroupUID;
            this.AllocatedOnhandType = request.AllocatedOnhandType;
            this.OnhandType = request.AllocatedOnhandType;
            // this.PackageVersionUID = request.PackageVersionUID;
        }
        public Guid ReceivingWorkorderPodUID { get; set; }
        public bool IsComplete { get; set; }
        public int ShortageQty { get; set; }
        public int OnhandType { get; set; }
        public int Onhand { get; set; }
        public string Location { get; set; }
        public Guid ItemRefUID { get; set; }
        public Guid PalletRefUID { get; set; }
        public Guid ManifestItemUID { get; set; }
        public Guid VesselManifestUID { get; set; }
        public Guid ShipPackageUID { get; set; }
        public Guid ShipViaUID { get; set; }
        public int PalletID { get; set; }
        public int Line_No { get; set; }
        public int ComponentType { get; set; }
        public string ItemNo { get; set; }
        public string ParentItemNo { get; set; }
        public int Qty { get; set; }
        public bool UseMiniPackage { get; set; }
        public Guid PackageVersionUID { get; set; }
        public Guid ShipViaRefUID { get; set; }
        public Guid ProcessItemUID { get; set; }
        public string PalletBarcode { get; set; }
        public PackingStationCarrier? Carrier { get; set; }
        public int ShipViaNo { get; set; }
        public IList<IItemModel> VirtualItems { get; set; }
        public string VirtualItemParentItemNo { get; set; }
        public bool IsHasVirtualItem { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public int AllocatedOnhandType { get; set; }
    }
}