using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class HomeAddressRelationListModel : IHomeAddressRelationListModel
    {
        public Guid UID { get; set; }
        public string WarehouseName { get; set; }
        public string AreaName { get; set; }
        public Guid AreaUID { get; set; }
        public string BinName { get; set; }
        public Guid BinUID { get; set; }
        public Guid SlotUID { get; set; }
        public string SlotName { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public int? OutboundType { get; set; }
        public string OutboundTypeName { get; set; }
        public int Priority { get; set; }
    }
}
