using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.BLL.Module
{

    internal class TicketGeneralOperationalModule
    {
        private PackageCacheManager _PackageManager;
       // private IPackageUomManager _PackageUomManager;
        public TicketGeneralOperationalModule(PackageCacheManager pManager, IPackageUomManager puManager)
        {
            this._PackageManager = pManager;
        //    this._PackageUomManager = puManager;
        }
        public LabelType[] PackageforUOM(Guid packageUID)
        {
            var pkgInfo = this._PackageManager.GetPackage(packageUID);
            //var uomInfo = this._PackageUomManager.GetPackageUom(pkgInfo.UOM);
            var uomInfo = this._PackageManager.GetUOM(pkgInfo.UOM);
            if (uomInfo.Name.Equals(WMSAPIParameters.BOX_UOM_KEYNAME, StringComparison.OrdinalIgnoreCase))
            {
                return new LabelType[] {
                    LabelType.Box_EAN,LabelType.Box_Self,LabelType.Box_UPC,LabelType.Box_SCC14
                };
            }
            else if (WMSAPIParameters.MIN_PACKAGE_UOM.Any(x => uomInfo.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
            {
                return new LabelType[] {
                   LabelType.Box_EAN,LabelType.Box_Self,LabelType.Box_UPC,
                    LabelType.Item_EAN,LabelType.Item_Self,LabelType.Item_UPC,LabelType.Box_SCC14,LabelType.Item_PUOM
                };
            }
            else
            {
                return new LabelType[] {
                    LabelType.Pallet_Self
                };
            }
        }
    }
}
