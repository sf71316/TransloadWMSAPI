using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class WorkOrderInnerModel : IWorkOrderModel
    {
        public Guid UID {get;set;}
        public string ID {get;set;}
        public string Name {get;set;}
        public Guid ManifestUID {get;set;}
        public Guid VesselUID {get;set;}
        public int Status {get;set;}
        public int Type {get;set;}
        public string CreatedBy {get;set;}
        public DateTime? CreatedOn {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime? ModifiedOn {get;set;}
    }
}
