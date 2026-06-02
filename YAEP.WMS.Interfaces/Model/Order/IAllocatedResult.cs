using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAEP.WMS.Interfaces
{
    public interface IAllocatedResult
    {
        bool IsComplete { get; set; }
        int ErrorCode { get; set; }
        int SurplusQty { get; set; }
        int ShipQty { get; set; }
        int OrderQty { get; set; }
        int LineNo { get; set; }
        int Onhand { get; set; }
        string Prod_id { get; set; }
    }
}
