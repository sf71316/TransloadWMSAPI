using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.DAL.Repository
{
    public class ItemModel : IItemModel
    {
        public Guid UID {get;set;}
        public Guid GroupUID {get;set;}
        public string ID {get;set;}
        public string Name {get;set;}
        public int Status {get;set;}
        public int Type {get;set;}
        public string Description {get;set;}
        public string CreatedBy {get;set;}
        public DateTime? CreatedOn {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime? ModifiedOn {get;set;}
    }
}
