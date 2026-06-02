using System;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ILabelModel
    {
        Guid UID { get; set; }
        string ID { get; set; }
        string Name { get; set; }
        LabelType Type { get; set; }
        int Status { get; set; }
        LabelBelongType BelongToType { get; set; }
        Guid BelongToUID { get; set; }
        Guid FileUID { get; set; }
        string Content { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
    }
}