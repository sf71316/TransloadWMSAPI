using System.Collections;

namespace YAEP.WMS.BLL.Module
{
    public interface IPoolModel
    {
        int Count { get; }
        bool IsPoolFull { get; }
        bool IsReadOnly { get; }
        bool IsSynchronized { get; }
        int MaxProcessCount { get; set; }
        object SyncRoot { get; }
        void Add(IProcessModel item);
        void Clear();
        bool Contains(IProcessModel item);
        bool Contains(object id);
        void CopyTo(IProcessModel[] array, int arrayIndex);
        IEnumerator GetEnumerator();
        bool Remove(IProcessModel item);
    }
}