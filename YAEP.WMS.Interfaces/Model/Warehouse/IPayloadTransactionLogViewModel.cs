using System;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IPayloadTransactionLogViewModel : IPayloadTransactionLogModel
    {
        /// <summary>
        /// Warehouse ID
        /// <para /><see cref="IWarehouseModel.ID"/>
        /// </summary>
        string WarehouseID { get; set; }
        string UOM { get; set; }
        Guid UniqueKey { get; set; }
        Guid UOMUID { get; set; }
        /// <summary>
        /// Warehouse Name
        /// <para /><see cref="IWarehouseModel.Name"/>
        /// </summary>
        string WarehouseName { get; set; }
        /// <summary>
        /// Area Name
        /// <para /><see cref="IAreaModel.Name"/>
        /// </summary>
        string TargetAreaName { get; set; }
        /// <summary>
        /// Bin Name
        /// <para /><see cref="IBinModel.Name"/>
        /// </summary>
        string TargetBinName { get; set; }
        /// <summary>
        /// Slot Name
        /// <para /><see cref="ISlotModel.Name"/>
        /// </summary>
        string TargetSlotName { get; set; }
        string OriginalAreaName { get; set; }
        /// <summary>
        /// Bin Name
        /// <para /><see cref="IBinModel.Name"/>
        /// </summary>
        string OriginalBinName { get; set; }
        /// <summary>
        /// Slot Name
        /// <para /><see cref="ISlotModel.Name"/>
        /// </summary>
        string OriginalSlotName { get; set; }
        string ReceivedVesselRefNo { get; set; }
        string VesselNo { get; set; }
        string BolNo { get; set; }
        string RefNo { get; set; }
        string TicketID { get; set; }
        /// <summary>
        /// Package Name 
        /// </summary>
        string PackageName { get; set; }
        /// <summary>
        /// Package Name 
        /// </summary>
        string ItemID { get; set; }
        /// <summary>
        /// Customer ID
        /// </summary>
        string CustomerID { get; set; }
        /// <summary>
        /// Customer Name
        /// </summary>
        string CustomerName { get; set; }
        /// <summary>
        /// Type String
        /// <para /><see cref="PayloadTransactionLogTypes"/>
        /// </summary>
        string TypeName { get; set; }
        DateTime? PayloadModifiedOn { get; set; }
        int? PayloadType { get; set; }
        string PayloadTypeName { get; set; }
    }
}