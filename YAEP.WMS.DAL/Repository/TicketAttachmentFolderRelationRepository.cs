using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class TicketAttachmentFolderRelationRepository<T> : AbstractRepository<T>, ITicketAttachmentFolderRelationRepository
          where T : class, ITicketAttachmentFolderRelationModel
    {
        public TicketAttachmentFolderRelationRepository(IRepositoryHandler<T> handler) : base(handler)
        {
        }

        public IActionResult<bool> Add(dynamic model)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.CreateByDynamic(model);
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

        public IActionResult<IEnumerable<ITicketAttachmentFolderRelationModel>> GetAttachmentFolderUID(Guid belongtouid, int belongtotype)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketAttachmentFolderRelationModel>>();
            try
            {
                rs.Content = this._Handler.RetrieveCollectionByDynamicConditions(new { belongtouid = belongtouid, belongtotype = belongtotype }).Where(p => p.Status > 0);
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
