using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces.Model;

namespace YAEP.WMS.BLL.Model
{
    public class ProductPackageExtendModel : IProductPackageExtendModel
    {
        public ProductPackageExtendModel()
        {

        }

        public ProductPackageExtendModel(ProductExtendModel product_model)
        {
            this.UID = product_model.UID;
            //this.ID = product_model.ID;
            this.Name = product_model.Name;
            this.UPC = product_model.UPC;
            //this.Status = product_model.Status;
            //this.Type = product_model.Type;
            this.Description = product_model.Description;
            //this.CreatedBy = product_model.CreatedBy;
            //this.CreatedOn = product_model.CreatedOn;
            //this.ModifiedBy = product_model.ModifiedBy;
            //this.ModifiedOn = product_model.ModifiedOn;
            this.CustomerUID = product_model.CustomerUID;
            this.GroupUID = product_model.GroupUID;
            //this.ActualProduct = product_model.ActualProduct;
            //this.BoxQuantity = product_model.BoxQuantity;
            //this.PUOM = product_model.PUOM;
            //this.CombinedQuantity = product_model.CombinedQuantity;
            //this.UPC = product_model.UPC;
            //this.EAN = product_model.EAN;
        }

        public Guid UID { get; set; }
        //public String ID { get; set; }
        public String Name { get; set; }
        public String UPC { get; set; }
        //public int Status { get; set; }
        //public int Type { get; set; }
        public String CustomerUID { get; set; }
        public Guid GroupUID { get; set; }
        public String Description { get; set; }

        public IEnumerable<PackageExtendModel> Packages { get; set; }

    }

    public class PackageExtendModel : IPackageExtendModel
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VersionId { get; set; }

    }
}
