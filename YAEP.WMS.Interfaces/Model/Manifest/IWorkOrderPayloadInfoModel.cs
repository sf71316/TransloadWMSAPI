using System;

namespace YAEP.WMS.Interfaces
{
    public interface IWorkOrderPayloadInfoModel : IWorkOrderPayloadModel
    {
        DateTime? BOL_ETA_D { get; set; }
    }
}