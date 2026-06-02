using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    public class SlotInfoModel : IHomeAddressRelationAreaModel, IHomeAddressRelationBinModel, IHomeAddressReltationSlotModel
    {
        public Guid AreaUID { get; set; }
        public string AreaName { get; set; }
        public Guid BinUID { get; set; }
        public string BinName { get; set; }
        public Guid SlotUID { get; set; }
        public string SlotName { get; set; }
    }
}
