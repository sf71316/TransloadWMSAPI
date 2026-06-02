using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces.Model.InnerModel;

namespace YAEP.WMS.DAL.Model
{
    internal class TransloadSwappedAllocatedModel : ITransloadSwappedAllocatedModel
    {
        public Guid UID {get;set;}
        public Guid PayloadUID {get;set;}
        public Guid PodUID {get;set;}
        public Guid BarcodeUID {get;set;}
        public Guid ReceivingWorkorderPayloadUID {get;set;}
        public string Barcode { get; set; }
    }
}
