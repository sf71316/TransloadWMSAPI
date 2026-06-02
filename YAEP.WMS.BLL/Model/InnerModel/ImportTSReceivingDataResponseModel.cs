using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ImportTSReceivingDataResponseModel : IImportTSReceivingDataResponseModel, IImportTSReceivingDataRequestModel
    {
        public string Key { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public int Qty { get; set; }
        public string ReceivingBarcode { get; set; }
        public string Description { get; set; }
    }
}
