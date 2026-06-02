using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Model
{
    internal class AttachmentResultModel
    {
        public string success { get; set; }
        public Guid[] uids { get; set; }
        public string errormessage { get; set; }
        public string ExceptionMessage { get; set; }
    }
    internal class AttachmentFailureResultModel
    {
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
