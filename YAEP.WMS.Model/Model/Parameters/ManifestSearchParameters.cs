using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class ManifestSearchParameters : IManifestSearchParameters
    {
        public Guid?[] PHierarchy { get; set; }
        public Guid?[] CHierarchy { get; set; }
        public Guid?[] SCMS { get; set; }
        public Guid?[] Warehouse { get; set; }
        public string Option { get; set; }
        public string manifestid { get; set; }
        public string OptionText { get; set; }
        public ManifestType? Type { get; set; }
        public Guid?[] CustomerList { get; set; }
        public Guid? Customer { get; set; }
        public string manifestref { get; set; }
        public string manifestname { get; set; }
    }
}
