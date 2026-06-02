using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ServiceItemProcessModuleParameters
    {
        public ISequenceAgent SequenceAgent { get; set; }
        public PackageCacheManager PackageManager { get; set; }
        public IPackageUomManager PackageUomManager { get; set; }
        public IInstructionBuilder InstructionBuilder { get; set; }
        public IAuthenticationProvider AuthenticationProvider { get; set; }
    }
}
