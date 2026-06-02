using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class WarehouseComponentParameters : IWarehouseComponentParameters
    {
        public Guid? ConditionUID { get; set; }
        public string Name { get; set; }
        public Guid? WarehouseUID { get; set; }
        public bool UnAssigned { get; set; }
    }
}
