using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model.Parameters
{
    internal class ManifestItemListDeleteInnerParameters : IManifestItemListDeleteParameters
    {
        public Guid[] UID { get; set; }
    }
}
