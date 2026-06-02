using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class HomelocationAreaModel : IHomeAddressRelationAreaModel
    {
        public Guid AreaUID { get; set; }
        public string AreaName { get; set; }
    }
}
