using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class LabelInnerModel : ILabelModel
    {
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public LabelType Type { get; set; }
        public int Status { get; set; }
        public LabelBelongType BelongToType { get; set; }
        public Guid BelongToUID { get; set; }
        public Guid FileUID { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
