using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class DeallocatedPayloadInfoModel : IDeallocatedPayloadInfoModel
    {
        public DeallocatedPayloadInfoModel()
        {
            this.AllocatedPayload = new List<IPayloadModel>();
            this.OriginalPayload = new List<IPayloadModel>();
        }
        public List<IPayloadModel> AllocatedPayload { get; set; }
        public List<IPayloadModel> OriginalPayload { get; set; }
    }
}
