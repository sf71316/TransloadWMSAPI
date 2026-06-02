using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.BLL.Model
{
    public class ItemPropertiesModel : IItemPropertiesModel
    {
        public ItemPropertiesModel() { }
        
        public Guid UID { get; set; }
        public Guid ItemUID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int DataType { get; set; }

        //public override string ToString();
    }
}
