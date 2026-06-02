using System;

namespace YAEP.WMS.Interfaces
{
    public interface IDeallocatedRequest
    {
        string RequestBy { get; set; }
        Guid? BolUID { get; set; }
        Guid CustomerUID { get; set; }
        Guid WarehouseUID { get; set; }
        string RefNo { get; set; }
        string BolNo { get; set; }
    }
}