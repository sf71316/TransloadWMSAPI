using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class ShipMethodRepository<T> : AbstractRepository<T>, IShipMethodRepository where T : class, IShipMethodModel
    {
        private readonly IAuthenticationProvider _AuthenticationProvider;
        public ShipMethodRepository(IRepositoryHandler<T> handler, IAuthenticationProvider authenticationInfoProvider) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;
            this._AuthenticationProvider = authenticationInfoProvider;
        }

        public IActionResult<IEnumerable<IShipMethodModel>> GetList(object condition)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IShipMethodModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(condition);
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
    }
}
