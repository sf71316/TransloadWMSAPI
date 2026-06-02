using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class LocationMappingInnerModel : ILocationMapping
    {
        public Guid UID { get; set; }
        public string Name { get; set; }
        public Guid WarehouseUID { get; set; }
        public string LocationID { get; set; }
        public string WarehouseID { get; set; }
    }
}
