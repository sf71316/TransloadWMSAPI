using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class RollbackTicketParameters : IRollbackTicketParameters
    {
        public Guid? ManifestUID { get; set; }
        public Guid? BolUID { get; set; }
        public string ModifiedBy { get; set; }
    }
}
