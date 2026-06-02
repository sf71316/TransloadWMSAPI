using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Model
{
    internal class GetModifyPayloadPackageItem : IGetModifyPayloadPackageItem
    {
        public string VersionID {get;set;}
        public string ItemName {get;set;}
        public string PackageName {get;set;}
        public Guid PackageUID {get;set;}
        public Guid ItemUID {get;set;}
    }
}
