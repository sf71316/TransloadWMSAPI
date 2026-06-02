using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.BLL.Model
{
    internal class ItemInnerParameterize : IItemParameterize
    {
        public ItemInnerParameterize()
        {
            this.ListOfItemUID = new List<Guid>();
            this.ListOfGroupUID = new List<Guid>();
            this.ListOfItemID = new List<string>();
            this.ItemProperties = new List<IItemPropertySearchModel>();
        }
        public Guid? UID { get; set; }
        public Guid? GroupUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; }
        public List<Guid> ItemCategories { get; set; }
        public List<IItemPropertySearchModel> ItemProperties { get; set; }
        public List<Guid> ListOfItemUID { get; set; }
        public List<Guid> ListOfGroupUID { get; set; }
        public List<string> ListOfItemID { get; set; }
    }
}
