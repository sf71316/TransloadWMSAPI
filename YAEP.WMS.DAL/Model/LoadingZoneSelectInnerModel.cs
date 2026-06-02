using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class LoadingZoneSelectInnerModel : ILoadingZoneSelectModel
    {
        public Guid UID { get; set; }
        public string AreaName { get; set; }
        public string BinName { get; set; }
        public string SlotName { get; set; }
        public bool IsDefaultLoadingZone { get; set; }
        public int SlotType { get; set; }
    }
}
