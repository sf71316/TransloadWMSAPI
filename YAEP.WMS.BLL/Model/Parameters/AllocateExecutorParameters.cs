using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AllocateExecutorParameters : IAllocateExecutorParameters
    {
        public IInventoryManager InventoryManager { get; set; }
        public ISequenceAgent SequenceAgent { get; set; }
        public ProductUtility ProductUtility { get; set; }
        public IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        public ILabelManager LabelManager { get; set; }
        public PackageCacheManager PackageMappingCache { get; set; }
        public ITracingAgent TracingAgent { get; set; }
    }
}
