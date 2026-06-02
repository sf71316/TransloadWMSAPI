using System;

namespace YAEP.WMS.Interfaces
{
    public interface ILocationInfoViewModel
    {
        /// <summary>
        /// Warehouse ID
        /// <para /><see cref="IWarehouseModel.ID"/>
        /// </summary>
        string WarehouseID { get; set; }
        /// <summary>
        /// Warehouse Name
        /// <para /><see cref="IWarehouseModel.Name"/>
        /// </summary>
        string WarehouseName { get; set; }
        /// <summary>
        /// Area UID
        /// <para /><see cref="IAreaModel.UID"/>
        /// </summary>
        Guid AreaUID { get; set; }
        /// <summary>
        /// Area ID
        /// <para /><see cref="IAreaModel.ID"/>
        /// </summary>
        string AreaID { get; set; }
        /// <summary>
        /// Area Name
        /// <para /><see cref="IAreaModel.Name"/>
        /// </summary>
        string AreaName { get; set; }
        /// <summary>
        /// Bin UID
        /// <para /><see cref="IBinModel.UID"/>
        /// </summary>
        Guid BinUID { get; set; }
        /// <summary>
        /// Bin ID
        /// <para /><see cref="IBinModel.ID"/>
        /// </summary>
        string BinID { get; set; }
        /// <summary>
        /// Bin Name
        /// <para /><see cref="IBinModel.Name"/>
        /// </summary>
        string BinName { get; set; }
        /// <summary>
        /// Slot UID
        /// <para /><see cref="ISlotModel.UID"/>
        /// </summary>
        Guid SlotUID { get; set; }
        /// <summary>
        /// Slot ID
        /// <para /><see cref="ISlotModel.ID"/>
        /// </summary>
        string SlotID { get; set; }
        /// <summary>
        /// Slot Name
        /// <para /><see cref="ISlotModel.Name"/>
        /// </summary>
        string SlotName { get; set; } 
        /// <summary>
        /// Slot Volume
        /// <para /><see cref="ISlotModel.VolumeLimit"/>
        /// </summary>
        decimal Volume { get; set; }
        /// <summary>
        /// Slot Weight
        /// <para /><see cref="ISlotModel.WeightLimit"/>
        /// </summary>
        decimal Weight { get; set; }
    }
}