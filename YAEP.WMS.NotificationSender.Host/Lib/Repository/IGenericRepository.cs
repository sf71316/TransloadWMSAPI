using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    public interface IGenericRepository<T> where T : class
    {
        bool Insert(object entity);
        bool Update(object entity, object condition);
        bool Delete(object condition);
        IEnumerable<T> GetList(object condition);
        T GetData(object condition);
    }
}
