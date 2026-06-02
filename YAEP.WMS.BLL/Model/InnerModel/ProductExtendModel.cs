using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Model
{
    public class ProductExtendModel : IProductExtendModel
    {
        public ProductExtendModel()
        {

        }
        public ProductExtendModel(ProductCacheModel productCache)
        {
            this.UID = productCache.UID;
            this.GroupUID = productCache.GroupUID;
            this.ID = productCache.ID;
            this.Name = productCache.Name;
            this.Status = productCache.Status;
            this.Type = productCache.Type;
            this.Description = productCache.Description;
            this.CreatedBy = productCache.CreatedBy;
            this.CreatedOn = productCache.CreatedOn;
            this.ModifiedBy = productCache.ModifiedBy;
            this.ModifiedOn = productCache.ModifiedOn;
            this.CustomerUID = productCache.CustomerUID.ToString("D");
            this.ActualProduct = productCache.ActualProduct;
            this.BoxQuantity = productCache.BoxQuantity;
            this.PUOM = productCache.PUOM;
            this.CombinedQuantity = productCache.CombinedQuantity;
            this.UPC = productCache.UPC;
            this.EAN = productCache.EAN;
        }
        public Guid UID { get; set; }
        public Guid GroupUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string CustomerUID { get; set; }
        /// <summary>
        /// 是否為虛擬 Item
        /// </summary>
        public bool IsVirtualItem { get => (this.Type == 100); }
        /// <summary>
        /// 實際 Item ID
        /// </summary>
        public string ActualProduct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BoxQuantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PUOM { get; set; }
        public string UPC { get; set; }
        public string EAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CombinedQuantity { get; set; }
    }
}
