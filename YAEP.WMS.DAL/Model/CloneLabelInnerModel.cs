using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class CloneLabelInnerModel : ICloneLabelModel
    {
        public string CreatedBy { get; set; }
        public Guid SourceBelongToUID { get; set; }
        public Guid TargetBelongToUID { get; set; }
    }
}
