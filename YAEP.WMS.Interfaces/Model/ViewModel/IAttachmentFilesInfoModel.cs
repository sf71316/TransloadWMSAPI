using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAttachmentFilesInfoModel
    {
        int BelongToType { get; set; }
        Guid BelongToUID { get; set; }
        string ContentType { get; set; }
        string Description { get; set; }
        string FileExtension { get; set; }
        string FileName { get; set; }
        Guid FileUID { get; set; }
        Guid FolderUID { get; set; }
        Guid UID { get; set; }
        string Path { get; set; }
        string Name { get; set; }
        string FileBase64 { get; set; }
    }
}
