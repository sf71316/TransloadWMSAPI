using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.BLL.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageSyncModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PackageSyncEntity Package { get; set; } = new PackageSyncEntity();

        public override string ToString()
        {
            return $"{this.ProductId}: {this.Package.ToString()}";
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class PackageSyncEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Length { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// gross weight
        /// </summary>
        public decimal? GrossWeight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SCC14 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PUOM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PackageSyncEntity> Children { get; set; } = new List<PackageSyncEntity>();

        public override string ToString()
        {
            string childrenText = "";
            if (this.Children.Count() > 0)
            {
                childrenText = $"/ [ {String.Join(", ", this.Children.Select(o => o.ToString()))} ]";
            }
            return $"{this.Name}({this.UOM}) {childrenText}";
        }
    }
}