using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Attachment.ClientAPI;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AttachmentFilesInfoInnerModel : IAttachmentFilesInfoModel
    {
        public AttachmentFilesInfoInnerModel(AttachmentFilesInfoModel model)
        {
            this.BelongToType = model.BelongToType;
            this.BelongToUID = model.BelongToUID;
            this.ContentType = model.ContentType;
            this.Description = model.Description;
            this.FileExtension = model.FileExtension;
            this.FileName = model.FileName;
            this.FileUID = model.FileUID;
            this.FolderUID = model.FolderUID;
            this.UID = model.UID;
            this.FileBase64 = model.FileBase64;
        }
        public int BelongToType { get; set; }
        public Guid BelongToUID { get; set; }
        public string ContentType { get; set; }
        public string Description { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public Guid FileUID { get; set; }
        public Guid FolderUID { get; set; }
        public Guid UID { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string FileBase64 { get; set; }
    }
}
