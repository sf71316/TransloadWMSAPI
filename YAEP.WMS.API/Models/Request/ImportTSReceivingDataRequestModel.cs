using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class ImportTSReceivingDataRequestModel : IImportTSReceivingDataRequestModel
    {
        public string Key { get; set; }
        public Guid ItemUID { get; set; }
        public Guid SlotUID { get; set; }
        public int Qty { get; set; }
        public string ReceivingBarcode { get; set; }
        public Guid WarehouseUID { get; set; }
        public string Description { get; set; }
    }
}