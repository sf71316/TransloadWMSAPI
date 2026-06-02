using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ICheckBolExistResponse
    {
        IEnumerable<ICheckBolExistItemResponse> Items { get; set; }
    }
    public interface ICheckBolExistItemResponse
    {
        string BolRefNo { get; set; }
        bool IsExist { get; set; }
    }
}
