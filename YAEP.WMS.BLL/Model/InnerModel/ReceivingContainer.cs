using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ReceivingContainer : IReceivingContainer
    {
        public ReceivingContainer()
        {
            this.Items = new List<IReceivingItemModel>();
            ManifestItemUID = new List<Guid>();
        }
        public Guid UID {get;set;}
        public string ExternalData {get;set;}
        public string PackageUOM {get;set;}
        public IList<Guid> ManifestItemUID {get;set;}
        public Guid VesselManifestUID {get;set;}

        public IList<IReceivingItemModel> Items { get; set; }
    }
}
