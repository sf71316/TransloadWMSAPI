using System;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IInventoryModel
    {
        /// <summary>
        /// 
        /// </summary>
        Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid WarehouseUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid SlotUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid PackageUID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Guid ItemUID { get; set; }
        /// Ãþ«¬
        /// <para /> Regular : 100 (Default) 
        /// <para /> Sav : 200
        /// </summary>
        int Type { get; set; }
        int Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int Qty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DateTime? CreatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ModifiedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DateTime? ModifiedOn { get; set; }
    }


}
