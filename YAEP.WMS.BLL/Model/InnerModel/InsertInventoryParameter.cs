using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class InsertInventoryParameter : IInsertInventoryParameter
    {
       
        public Guid WarehouseUID {get;set;}
        public Guid ItemUID {get;set;}
        public Guid TargetPackageUID {get;set;}
        public Guid SlotUID {get;set;}
        public InventoryType Type {get;set;}
        public int Qty {get;set;}
        public bool UseMiniPackage {get;set;}
    }
}
