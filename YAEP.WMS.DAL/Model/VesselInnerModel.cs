using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
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
        public string SealNo {get;set;}
        public int? ContainerSize {get;set;}
        public int? LoadingType {get;set;}
        public int? StackableType {get;set;}
        public DateTime? ArrivalDate {get;set;}
        public decimal? Weight {get;set;}
        public decimal? Volume {get;set;}
        public string StatusName {get;set;}
        public string Description {get;set;}
        public string CreatedBy {get;set;}
        public DateTime? CreatedOn {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime? ModifiedOn {get;set;}
    }
}
