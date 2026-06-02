using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Model
{
    internal class ReceivingExternalModel
    {
        public string Name { get; set; }
        public string BOLNO { get; set; }
        public string MARKS { get; set; }
        public string VendorID { get; set; }
        public DateTime ETD { get; set; }
        public string ExportCompany { get; set; }
        public string SONO { get; set; }
    }
}
