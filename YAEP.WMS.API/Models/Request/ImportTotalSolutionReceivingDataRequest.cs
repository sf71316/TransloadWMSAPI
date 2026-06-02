using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Models.Request
{
    public class ImportTotalSolutionReceivingDataRequest
    {
        public ImportTotalSolutionReceivingDataRequest()
        {
            this.data = new List<ImportTSReceivingDataRequestModel>().ToArray();
        }
        public IEnumerable<ImportTSReceivingDataRequestModel> data { get; set; }
    }
}