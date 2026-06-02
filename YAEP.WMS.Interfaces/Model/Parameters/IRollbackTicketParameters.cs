using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IRollbackTicketParameters
    {
        Guid? ManifestUID { get; set; }
        Guid? BolUID { get; set; }
        string ModifiedBy { get; set; }
    }
}
