using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class GetAddItemListParameters : IGetAddItemListparameters
    {
        public Guid? manifestuid { get; set; }
        public Guid? vesseluid { get; set; }
    }
}
