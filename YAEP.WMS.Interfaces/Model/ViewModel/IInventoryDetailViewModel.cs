using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInventoryDetailViewModel : IInventoryModel
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
        /// Manifest ID
        /// <para /><see cref="IManifestModel.ID"/>
        /// </summary>
        string ManifestID { get; set; }
        /// <summary>
        /// BOL ID 
        /// <para /><see cref="IBolModel.ID"/>
        /// </summary>
        string BolRef { get; set; }
        /// <summary>
        /// Vessel ID
        /// <para /><see cref="IVesselModel.ID"/>
        /// </summary>
        string VesselRef { get; set; }
        /// <summary>
        /// Item #
        /// </summary>
        string ItemID { get; set; }
        /// <summary>
        /// Package UOM name
        /// </summary>
        string Package { get; set; }
    }
}