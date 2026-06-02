using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.BLL.Model
{
    class PackageNode : IPackageNode
    {
        public Guid? ParentUOM { get; set; }
        public List<IPackageNode> Children { get; set; } = new List<IPackageNode>();
        public int Level { get; internal set; }
        public IPackageNode Parent { get; internal set; }

        #region IPackageModel Members

        public Guid UID { get; set; }
        public Guid? ParentUID { get; set; }
        public Guid ItemUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public Guid VersionUID { get; set; }
        public Guid UOM { get; set; }
        public int Quantity { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Length { get; set; }
        public decimal GrossWeight { get; set; }
        public Guid? ImageUID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string SCC14 { get; set; }
        public decimal? WidthCM  { get; set; }
        public decimal? HeightCM  { get; set; }
        public decimal? LengthCM  { get; set; }
        public decimal? GrossWeightKG  { get; set; }
        public string PUOM { get; set; }

        #endregion
    }
}
