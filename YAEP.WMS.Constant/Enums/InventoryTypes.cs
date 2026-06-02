using System;

namespace YAEP.WMS.Constant.Enums
{
    /// <summary>
    /// 類型
    /// <para /> Regular : 100 (Default) 
    /// <para /> Sav : 200
    /// </summary>
    public enum InventoryType
    {
        Stock = 1,
        Salvage = 5,
        Shrinkage = 6,
        AsIs = 7,
        Sample = 8,
        Sakana = 9
    }
}
