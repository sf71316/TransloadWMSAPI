using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum LabelType
    {
        Pallet_Self = 101,
        Pallet_Other = 102,
        Pallet_ShippingLabel = 103,
        Pallet_OrginalTracking = 104,
        Box_Self = 201,
        Box_UPC = 202,
        Box_EAN = 203,
        Box_SCC14 = 204,
        Item_Self = 301,
        Item_UPC = 302,
        Item_EAN = 303,
        Item_PUOM = 304,
        Item_ProductID = 305,
        Location_Slot = 404,
        Location_Bin = 403,
        Location_Area = 402,
        Location_Warehouse = 401,
    }
}
