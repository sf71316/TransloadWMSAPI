using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public class ManifestItemInnerModel : IManifestItemListModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid ManifestUID { get; set; }
        public Guid PackageUID { get; set; }
        public string PackageName { get; set; }
        public string VersionName { get; set; }
        public Guid ItemUID { get; set; }
        public string ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public int? PackageQty { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Weight { get; set; }
        public ManifestItemListStatus Status { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int OnhandType { get; set; }
    }
}
