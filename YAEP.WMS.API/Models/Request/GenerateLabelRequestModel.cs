using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class GenerateLabelRequestModel : IGenerateLabelRequest
    {
        public BarcodeMethod BarcodeMethod { get; set; }
        public BarcodeKind BarcodeKind { get; set; }
        //public BarcodeType BarcodeType { get; set; }
        public Guid BelongToUID { get; set; }
        public HttpPostedFile File { get; set; }
        public int GenerateQty { get; set; }
        public int BelongToType { get; set; }
    }
}