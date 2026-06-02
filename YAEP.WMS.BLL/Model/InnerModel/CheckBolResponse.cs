using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class CheckBolResponse : CommonResponse, ICheckBolExistResponse
    {
        public IEnumerable<ICheckBolExistItemResponse> Items { get; set; }
    }
    internal class CheckBolItemResponse : ICheckBolExistItemResponse
    {
        public string BolRefNo { get; set; }
        public bool IsExist { get; set; }
    }
}
