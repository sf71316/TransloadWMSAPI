using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Manifest_Item_List")]
    [DbTable("WMS_Manifest_Item_List")]
    public class ManifestItemListModel : IManifestItemListModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        [ExplicitKey]
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid ManifestUID { get; set; }
        public Guid PackageUID { get; set; }
        public Guid ItemUID { get; set; }
        public int? PackageQty { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Weight { get; set; }
        public ManifestItemListStatus Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public string PackageName { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public string StatusName { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public string ItemID { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public string ItemName { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public string ItemDescription { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public string VersionName { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public int OnhandType { get; set; }
    }
}
