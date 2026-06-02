using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class ManifestInnerModel : IManifestModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid PartyUID { get; set; }
        public string RefNo { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Weight { get; set; }
        public ManifestStatus Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
