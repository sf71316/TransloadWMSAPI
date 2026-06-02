using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class HomeAddressRelationBinModel : IHomeAddressRelationBinModel
    {
        public Guid BinUID { get; set; }
        public string BinName { get; set; }
    }
}
