using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class VesselInnerModel : IVesselModel
    {
        public Guid UID {get;set;}
        public string ID {get;set;}
        public string Name {get;set;}
        public int? Type {get;set;}
        public string RefNo {get;set;}
        public Guid BolUID {get;set;}
        public int Status {get;set;}
        public string StatusName {get;set;}
        public string Description {get;set;}
        public string CreatedBy {get;set;}
        public DateTime? CreatedOn {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime? ModifiedOn {get;set;}
    }
}
