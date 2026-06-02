using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IManifestSearchParameters
    {
        Guid?[] PHierarchy { get; set; }
        Guid?[] CHierarchy { get; set; }
        Guid?[] SCMS { get; set; }
        Guid?[] CustomerList { get; set; }
        Guid? Customer { get; set; }
        Guid?[] Warehouse { get; set; }
        ManifestType? Type { get; set; }
        string Option { get; set; }
        string OptionText { get; set; }
        string manifestref { get; set; }
        string manifestname { get; set; }
        string manifestid { get; set; }
    }
}
