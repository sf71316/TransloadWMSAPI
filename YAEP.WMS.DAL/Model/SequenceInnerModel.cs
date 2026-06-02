using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class SequenceInnerModel : ISequenceModel
    {
        public Guid UID { get; set; }
        public string BelongToUID { get; set; }
        public string BelongToTag { get; set; }
        public int SequenceValue { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
