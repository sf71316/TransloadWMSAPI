using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AllocatedPlannerResult
    {
        public AllocatedPlannerResult()
        {
            Items = new List<AllocatedItem>();
            OnhandPayloadItems = new List<ILocationItemViewModel>();
        }
        public Guid VesselManifestUID { get; set; }
        public Guid VesselUID { get; set; }
        public IEnumerable<IVesselManifestModel> VesselManifestCollection { get; set; }
        public Guid ItemUID { get; set; }
        public bool IsComplete { get; set; }
        public int ShortageQty { get; set; }
        public int Onhand { get; set; }
        public List<AllocatedItem> Items { get; set; }
        public List<ILocationItemViewModel> OnhandPayloadItems { get; set; }
    }
    internal class AllocatedItem
    {
        public Guid PayloadUID { get; set; }
        public int AllocatedQty { get; set; }
        public Guid AllocatedPackageUID { get; set; }
        public AllocateType AllocateType { get; set; } = AllocateType.GeneralAllocate;
    }
}
