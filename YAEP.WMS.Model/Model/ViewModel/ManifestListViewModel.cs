using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class ManifestListViewModel : IManifestListViewModel
    {
        public Guid UID { get; set; }
        public string ManifestNo { get; set; }
        public string ManifestName { get; set; }
        public string CustNo { get; set; }
        public ManifestType Type { get; set; }
        public ManifestStatus Status { get; set; }
        public string StatusName { get; set; }
        public string RefNo { get; set; }
        public string TypeName { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid PartyUID { get; set; }
    }
}
