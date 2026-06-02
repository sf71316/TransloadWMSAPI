using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces.Models;

namespace YAEP.WMS.DAL.Repository
{
    internal class ItemPropertiesModel : IItemPropertiesModel
    {
        public Guid UID {get;set;}
        public Guid ItemUID {get;set;}
        public string Name {get;set;}
        public string Value {get;set;}
        public int DataType {get;set;}
    }
}
