using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.BLL.Model
{
    public class ItemCategoryInnerParameterize : IItemCategoryParameterize
    {
        public ItemCategoryInnerParameterize()
        {
        }
        public Guid? UID { get; set; }
        public Guid? GroupUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; }
    }
}
