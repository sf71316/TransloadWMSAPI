using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;
using YAEP.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class TicketGeneratorParameters
    {
        public ITicketRepository TicketRepository { get; set; }
        public ITicketRelationRepository TicketRelationRepository { get; set; }
        public ITicketInfoRepository TicketInfoRepository { get; set; }
        public IWorkOrderManager WorkOrderManager { get; set; }
        public PackageCacheManager PackageManager { get; set; }
        public IPackageUomManager PackageUomManager { get; set; }
        public ISequenceAgent SequenceAgent { get; set; }
        public ILabelRepository LabelRepository { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }
        public ITracingAgent TracingAgent { get; set; }
    }
}
