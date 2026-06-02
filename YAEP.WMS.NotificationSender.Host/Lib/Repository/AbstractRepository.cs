using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Dapper.Contrib;
using YAEP.Data.ORM.Templates;

namespace YAEP.WMS.NotificationSender.Host.Lib
{
    internal abstract class AbstractRepository<T> : GenericRepositoryHandler<T>, IGenericRepository<T> where T : class
    {
        public AbstractRepository() : base(new DbEntities(new DefaultConnectionSettings()),
            null, new GenericModelDescriptor<T>())
        {

        }
        public bool Delete(object condition)
        {
            return this.Maintain.DeleteByDynamicConditions(condition);
        }
        public abstract T GetData(object condition);

        public abstract IEnumerable<T> GetList(object condition);

        public bool Insert(object entity)
        {
            return this.Maintain.CreateByDynamic(entity);
        }

        public bool Update(object entity, object condition)
        {
            return this.Maintain.UpdateByDynamicConditions(entity, condition);
        }
    }
}
