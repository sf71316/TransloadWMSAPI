using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.BLL.Model
{
    class PackageTree : IPackageTree
    {
        private readonly IEnumerable<IPackageModel> _Source;
        public PackageTree(IEnumerable<IPackageModel> source)
        {
            this._Source = source;
        }
        public IPackageNode Root { get; set; }

        //public override string ToString()
        //{
        //    return this.ToString();
        //}
    }
}
