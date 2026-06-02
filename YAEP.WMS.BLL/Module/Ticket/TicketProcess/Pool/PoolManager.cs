using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Module
{
    public class PoolManager : IPoolManager
    {
        static Lazy<Dictionary<Guid, IPoolModel>> _PublicPool = new Lazy<Dictionary<Guid, IPoolModel>>();
        static Lazy<IPoolManager> _PoolManager = new Lazy<IPoolManager>(() => new PoolManager());
        public static IPoolManager GetPoolManager()
        {
            return _PoolManager.Value;
        }
        public bool RegisterPool(Guid PoolUID)
        {
            if (_PublicPool.Value.ContainsKey(PoolUID))
            {
                return false;
            }
            else
            {
                ConcurrentBag<IProcessModel> _pool = new ConcurrentBag<IProcessModel>();
                IPoolModel _model = new PoolModel(_pool);
                _PublicPool.Value.Add(PoolUID, _model);
                return true;
            }
        }
        public bool IsCreated(Guid PoolUID)
        {
            return _PublicPool.Value.ContainsKey(PoolUID);
        }
        public IPoolModel GetPool(Guid PoolUID)
        {
            if (_PublicPool.Value.ContainsKey(PoolUID))
            {
                IPoolModel _model = null;
                if (_PublicPool.Value.TryGetValue(PoolUID, out _model))
                {
                    return _model;
                }
            }
            return null;
        }
        public bool RemovePool(Guid PoolUID)
        {
            return _PublicPool.Value.Remove(PoolUID);
        }
    }
}
