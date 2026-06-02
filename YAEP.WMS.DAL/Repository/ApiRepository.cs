using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class ApiRepository<T> : AbstractRepository<T>, IApiRepository where T : class, IApiModel
    {
        public ApiRepository(IRepositoryHandler<T> handler) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;

        }
    }
}
