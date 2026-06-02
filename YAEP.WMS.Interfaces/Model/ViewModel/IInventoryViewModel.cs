using System;

namespace YAEP.WMS.Interfaces
{
    public interface IInventoryViewModel : IInventoryModel
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
        /// Customer ID 
        /// </summary>
        string CustomerID { get; set; }
        /// <summary>
        /// Customer Name 
        /// </summary>
        string CustomerName { get; set; } 
        /// <summary>
        /// 類型名稱
        /// <para /> Regular : 100 (Default) 
        /// <para /> Sav : 200
        /// </summary>
        string TypeName {get;set;}
        /// <summary>
        /// 
        /// </summary>
        string UOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int InboundQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int OutboundQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int Onhand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string PackageTree { get; set; } 

    }
}