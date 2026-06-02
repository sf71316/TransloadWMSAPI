using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IAllocatedRequest
    {
        bool UsePackingStation { get; set; }
        DateTime ETD { get; set; }
        string RequestBy { get; set; }
        Guid WarehouseUID { get; set; }
        Guid CustomerUID { get; set; }
        string RefNo { get; set; }
        string CustomerPartyName { get; set; }
        string ReceiverUrl { get; set; }
        string ReceiverSecret { get; set; }
        string ShipToAddress { get; set; }
        string ShipToZip { get; set; }
        string ShipToCity { get; set; }
        string ShipToState { get; set; }
        string ShipToCountry { get; set; }
        int OrderType { get; set; }
        bool PassPackageVersion { get; set; }
        AllocateType AllocateMode { get; set; }
        IList<IAllocatedItemRequest> Items { get; set; }
    }
    public interface IAllocatedItemRequest
    {
        IList<IItemModel> VirtualItems { get; set; }
        PackingStationCarrier? Carrier { get; set; }
        Guid ManifestItemUID { get; set; }
        Guid VesselManifestUID { get; set; }
        Guid ShipPackageUID { get; set; }
        Guid ShipViaUID { get; set; }
        int PalletID { get; set; }
        string PalletBarcode { get; set; }
        int Line_No { get; set; }
        int ComponentType { get; set; }
        string ItemNo { get; set; }
        string ParentItemNo { get; set; }
        string VirtualItemParentItemNo { get; set; }
        int Qty { get; set; }
        bool UseMiniPackage { get; set; }
        int ShipViaNo { get; set; }
        bool IsHasVirtualItem { get; set; }
        int AllocatedOnhandType { get; set; }
        Guid? ItemGroupUID { get; set; }
    }
}
