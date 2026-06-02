using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Attachment.ClientAPI;
using YAEP.WMS.BLL.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public class AttachmentFileViewModel : IAttachmentFileViewModel
    {
        public AttachmentFileViewModel(IAttachmentFile file)
        {
            this.BelongToType = file.BelongToType;
            this.BelongToUID = file.BelongToUID;
            this.ContentType = file.ContentType;
            this.Description = file.Description;
            this.FileExtension = file.FileExtension;
            this.FileName = file.FileName;
            this.FileUID = file.FileUID;
            this.FolderUID = file.FolderUID;
            this.TypeUID = file.TypeUID;
            this.UID = file.UID;
            this.CreatedOn = file.CreatedOn;
        }
        public string TypeName { get; set; }
        public int BelongToType { get; set; }
        public Guid BelongToUID { get; set; }
        public string ContentType { get; set; }
        public string Description { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public Guid FileUID { get; set; }
        public Guid FolderUID { get; set; }

        public Guid? TypeUID { get; set; }
        public Guid UID { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
