using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant.Enums
{
    public enum AllocatedComponentType
    {
        Regular = 0,
        BOM = 1,
        ProStyle = 2,
        NotFindProduct = 100,
        Duplicateitem = 200,
        NotFindPackage = 300,
        DataExist = 400,
        NotStock = 500,
    }
}
