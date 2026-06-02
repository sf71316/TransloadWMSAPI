using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{

    public interface IImportTSReceivingDataRequestModel
    {
        string Key { get; set; }
        Guid ItemUID { get; set; }
        Guid SlotUID { get; set; }
        Guid WarehouseUID { get; set; }
        int Qty { get; set; }
        string ReceivingBarcode { get; set; }
        string Description { get; set; }

    }
    public interface IImportTSReceivingDataResponseModel
    {
        string Key { get; set; }
        bool Success { get; set; }
        string Message { get; set; }
    }
}
