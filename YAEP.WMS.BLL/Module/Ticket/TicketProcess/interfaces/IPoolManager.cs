using System;

namespace YAEP.WMS.BLL.Module
{
    public interface IPoolManager
    {
        IPoolModel GetPool(Guid PoolUID);
        bool IsCreated(Guid PoolUID);
        bool RegisterPool(Guid PoolUID);
        bool RemovePool(Guid PoolUID);
    }
}