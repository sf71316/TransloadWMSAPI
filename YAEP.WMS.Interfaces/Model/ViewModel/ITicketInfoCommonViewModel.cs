using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ITicketInfoCommonViewModel
    {
        Guid? TargetSlotUID { get; set; }
        Guid? TargetPackage { get; set; }
        Guid? OriginalPackage { get; set; }
        Guid? OriginalSlotUID { get; set; }
        Guid SourceLoadingZoneSlotUID { get; set; }
        Guid SourcePackageUID { get; set; }
        Guid SourceSlotUID { get; set; }
        Guid PayloadPackageUID { get; set; }
        int MappingType { get; set; }
        string OriginalPackageName { get; set; }
        string TargetPackageName { get; set; }
        string TargetUOMName { get; set; }
    }
}
