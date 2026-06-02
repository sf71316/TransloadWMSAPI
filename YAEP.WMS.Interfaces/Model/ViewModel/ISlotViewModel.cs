using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISlotViewModel : ISlotModel
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
        /// Bin ID
        /// <para /><see cref="IBinModel.ID"/>
        /// </summary>
        string BinID { get; set; }
        /// <summary>
        /// Bin Name
        /// <para /><see cref="IBinModel.Name"/>
        /// </summary>
        string BinName { get; set; }
        string StatusName { get; set; }
    }
}