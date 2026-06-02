using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model.InnerModel
{
    internal class PayloadChangeModel : IPayloadChangeModel
    {
        public PayloadChangeModel()
        {

        }
        public PayloadChangeModel(IPayloadModel payloadModel)
        {
            this.PayloadUID = payloadModel.UID;
            this.VolumeLimit = payloadModel.VolumeLimit;
            this.WeightLimit = payloadModel.WeightLimit;
            this.ChangeQty = payloadModel.Quantity;
            this.ModifiedBy = payloadModel.ModifiedBy;
        }
        public Guid PayloadUID { get; set; }
        public decimal VolumeLimit { get; set; }
        public decimal WeightLimit { get; set; }
        public string ModifiedBy { get; set; }
        public int ChangeQty { get; set; }
        public bool IsClear { get; set; }
        public Guid? ChangepayloadPackageUID { get; set; }
        public int? ChangepayloadPackageQty { get; set; }
    }
}
