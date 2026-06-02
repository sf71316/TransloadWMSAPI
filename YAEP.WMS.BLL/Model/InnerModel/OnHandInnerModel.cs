using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public class OnHandInnerModel
    {
        public OnHandInnerModel()
        {

        }

        public Guid ItemUID { get; set; }
        public Guid? ItemGroupUID { get; set; }
        public int TotalEachQty { get; set; }
    }
}
