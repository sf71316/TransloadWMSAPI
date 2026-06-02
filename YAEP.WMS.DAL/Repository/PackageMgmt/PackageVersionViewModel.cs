using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.DAL.Repository
{
    public class PackageVersionViewModel : IPackageVersionViewModel
    {
        public Guid PackageUID {get;set;}
        public Guid UID {get;set;}
        public Guid ItemUID {get;set;}
        public string VersionId {get;set;}
        public int Status {get;set;}
        public string CreatedBy {get;set;}
        public DateTime? CreatedOn {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime? ModifiedOn {get;set;}
        public long SerialNumber {get;set;}
    }
}
