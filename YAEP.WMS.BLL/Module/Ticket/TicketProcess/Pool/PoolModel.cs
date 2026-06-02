using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Module
{
    public class PoolModel : ICollection<IProcessModel>, IPoolModel
    {
        ConcurrentBag<IProcessModel> _Pool;
        int _MaxProcessCount = 0;
        public PoolModel(ConcurrentBag<IProcessModel> pool)
        {
            this._Pool = pool;
        }
        public int MaxProcessCount
        {
            get
            {
                return _MaxProcessCount;
            }
            set
            {
                _MaxProcessCount = value;
            }
        }
        public bool IsPoolFull
        {
            get
            {
                if (this._MaxProcessCount == 0)
                    return false;
                else
                    return this._Pool.Count == _MaxProcessCount;
            }
        }
        public int Count => this._Pool.Count;
        public object SyncRoot
        {
            get
            {
                return new object();
            }
        }
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;
        public void Add(IProcessModel item)
        {
            if (this._MaxProcessCount == 0)
                this._Pool.Add(item);
            else
            {
                if (this._Pool.Count <= _MaxProcessCount)
                {
                    this._Pool.Add(item);
                }
            }
        }
        public void Clear()
        {
            this._Pool = new ConcurrentBag<IProcessModel>();
        }
        public bool Contains(IProcessModel item)
        {
            return this._Pool.Contains(item);
        }
        public bool Contains(object id)
        {
            return this._Pool.Any(p => p.Equal(id));
        }

        public void CopyTo(IProcessModel[] array, int arrayIndex)
        {
            this._Pool.CopyTo(array, arrayIndex);
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(IProcessModel item)
        {
            try
            {
                while (this._Pool.Count > 0)
                {
                    IProcessModel result;
                    this._Pool.TryTake(out result);

                    if (result.Equals(item))
                    {
                        break;
                    }
                    this._Pool.Add(result);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Remove(Guid dataID)
        {
            try
            {
                IProcessModel result = this._Pool.FirstOrDefault(p => p.Equal(dataID));
                this._Pool.TryTake(out result);
                return true;
            }
            catch
            {
                return false;
            }
        }
        IEnumerator<IProcessModel> IEnumerable<IProcessModel>.GetEnumerator()
        {
            return this._Pool.GetEnumerator();
        }
    }
}
