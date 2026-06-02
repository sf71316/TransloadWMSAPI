using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces.Model.ViewModel;

namespace YAEP.WMS.DAL.Model
{
    internal class GlobalLockRelationInnerModel : IGlobalLockRelationModel
    {
        public string RefNo { get; set; }
        public Guid BolUID { get; set; }
        public Guid ManifestUID { get; set; }
        public Guid TicketInfoUID { get; set; }
    }
}
