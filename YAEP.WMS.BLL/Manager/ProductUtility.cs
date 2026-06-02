using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces.Models;

namespace YAEP.WMS.BLL
{
    /// <summary>
    /// 內部有關Product 的處理
    /// </summary>
    internal class ProductUtility
    {
        internal decimal CalculateCUFT(IPackageModel package, int qty)
        {
            //1英呎=12英吋
            decimal volume = (package.Length / 12 * package.Width / 12 * package.Height / 12) * qty;


            decimal cuft = Math.Round(volume, 4, MidpointRounding.AwayFromZero);
            return cuft;
        }
        internal decimal CalculateCBM(IPackageModel package, int qty)
        {
            var cuft = CalculateCUFT(package, qty);
            return Math.Round(cuft / 35.315m, 4, MidpointRounding.AwayFromZero);
        }
        internal decimal CaculateTTLWeight(IPackageModel pkg, int qty)
        {
            return pkg.GrossWeight * qty;
        }
    }
}
