using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_TicketAttachmentFolderRelation")]
    [DbTable("WMS_TicketAttachmentFolderRelation")]
    public class TicketAttachmentFolderRelationModel : ITicketAttachmentFolderRelationModel
    {
        [ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }
        public Guid BelongToUID { get; set; }
        public int BelongToType { get; set; }
        public Guid AttachmentFolderUID { get; set; }
        public int Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
