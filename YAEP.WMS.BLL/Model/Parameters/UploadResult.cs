using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL
{
    internal class UploadResult
    {
        public bool success { get; set; }
        public Guid[] uids { get; set; }
        public string errormessage { get; set; }
    }
}
