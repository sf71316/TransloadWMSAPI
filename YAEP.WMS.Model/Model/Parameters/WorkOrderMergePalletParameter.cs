using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class WorkOrderMergePalletParameter : IWorkOrderMergePalletParameter
    {
        public WorkOrderMergePalletParameter()
        {
            this.Mergefrom = new List<Guid>();
        }
        public IEnumerable<Guid> Mergefrom { get; set; }
        public Guid Mergeto { get; set; }
    }
}
