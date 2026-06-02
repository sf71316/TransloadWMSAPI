using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces.Model.ViewModel
{
    public interface IGlobalLockRelationModel
    {
        string RefNo { get; set; }
        Guid BolUID { get; set; }
        Guid ManifestUID { get; set; }
        Guid TicketInfoUID { get; set; }
    }
}
