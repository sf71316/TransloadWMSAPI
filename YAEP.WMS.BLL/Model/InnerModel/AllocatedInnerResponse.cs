using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AllocatedInnerResponse : IAllocatedResponse
    {
        public AllocatedInnerResponse()
        {
            this.Results = new List<IAllocatedItemResponse>();
        }

        public List<IAllocatedItemResponse> Results { get; set; }
        public bool IsComplete { get; set; }
        public string Message { get; set; }
    }
}