using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.Interfaces.Model
{
    public interface IProductExtendModel : IItemModel
    {
        Guid UID { get; set; }
        Guid GroupUID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        int Status { get; set; }
        int Type { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
        string CustomerUID { get; set; }
        /// <summary>
        /// 是否為虛擬 Item
        /// </summary>
        bool IsVirtualItem { get; }
        /// <summary>
        /// 實際 Item ID
        /// </summary>
        string ActualProduct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int BoxQuantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string PUOM { get; set; }
        string UPC { get; set; }
        string EAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        int CombinedQuantity { get; set; }
    }
}
