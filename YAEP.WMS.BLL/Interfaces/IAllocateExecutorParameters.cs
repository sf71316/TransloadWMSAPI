using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Interfaces
{
    internal interface IAllocateExecutorParameters
    {
        IInventoryManager InventoryManager { get; set; }
        ISequenceAgent SequenceAgent { get; set; }
        ILabelManager LabelManager { get; set; }
        ProductUtility ProductUtility { get; set; }
        PackageCacheManager PackageMappingCache { get; set; }
        IWorkOrderPayloadRepository WorkOrderPayloadRepository { get; set; }
        ITracingAgent TracingAgent { get; set; }
    }
}
