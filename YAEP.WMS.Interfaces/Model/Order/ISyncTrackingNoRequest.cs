using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISyncTrackingNoRequest
    {
        IEnumerable<ISyncTrackingNoItem> Packages { get; set; }
    }
    public interface ISyncTrackingNoItem
    {
        string Syspon { get; set; }
        string TrackingNo { get; set; }
        Guid PalletRefUID { get; set; }
    }
}
