using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class GetOnhandResponse : IGetOnhandResponse
    {
        public IEnumerable<IGetOnhandItem> Items { get; set; }
    }
    internal class GetOnhandItem : IGetOnhandItem
    {
        public string ItemNo { get; set; }
        public Guid ItemUID { get; set; }
        public int Onhand { get; set; }
    }
}
