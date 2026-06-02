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
    public class SequenceRepository<T> : AbstractRepository<T>, ISequenceRepository where T : class, ISequenceModel
    {
        int _Init_sequence_value = 1;
        private readonly IAuthenticationProvider _AuthenticationProvider;
        public SequenceRepository(IRepositoryHandler<T> handler, IAuthenticationProvider authenticationInfoProvider) : base(handler)
        {
            this._Handler.IsAutoHandleError = false;
            this._AuthenticationProvider = authenticationInfoProvider;
        }

        public IActionResult<ISequenceModel> GetSeqeuce(string belongtoUID, string belongtoTag)
        {

            var rs = ActionResultTemplates.Result<ISequenceModel>();

            rs.Content = new SequenceInnerModel();
            var result = this._Handler.Instance.Query("SP_Getsequence", new
            {
                belongtoUID = belongtoUID,
                belongtoTag = belongtoTag,
                User = this._AuthenticationProvider.GetAuthenticationInfo().Account
            }, commandType: System.Data.CommandType.StoredProcedure);
            rs.Content.BelongToTag = belongtoTag;
            rs.Content.BelongToUID = belongtoUID;
            rs.Content.SequenceValue = result.FirstOrDefault().Seq;
            rs.Success = true;

            return rs;
        }

        public IActionResult<List<ISequenceModel>> GetSeqeuceByBatch(string belongtoUID, string belongtoTag, int BatchCount)
        {
            var rs = ActionResultTemplates.Result<List<ISequenceModel>>();
            //try
            //{
            rs.Content = new List<ISequenceModel>();
            var result = this._Handler.Instance.Query("SP_Getsequence", new
            {
                belongtoUID = belongtoUID,
                belongtoTag = belongtoTag,
                User = this._AuthenticationProvider.GetAuthenticationInfo().Account,
                BatchCount = BatchCount
            }, commandType: System.Data.CommandType.StoredProcedure);
            foreach (var item in result)
            {
                var seq = new SequenceInnerModel();
                seq.BelongToTag = belongtoTag;
                seq.BelongToUID = belongtoUID;
                seq.SequenceValue = item.Seq;
                rs.Content.Add(seq);
            }

            rs.Success = true;
            //}
            //catch (Exception ex)
            //{
            //    rs.Message = ex.Message;
            //    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
            //    rs.Success = false;
            //    rs.InnerException = ex;
            //    this.OnExpcetion(ex);
            //}
            return rs;
        }

        //public IActionResult<ISequenceModel> Find(Guid belongtoUID, string belongtoTag)
        //{
        //    var rs = ActionResultTemplates.Result<ISequenceModel>();
        //    try
        //    {
        //        //get sequence SequenceInnerModel
        //        var _sequence = this._Handler.RetrieveByDynamicConditions(new
        //        {
        //            BelongToUID = belongtoUID,
        //            BelongToTag = belongtoTag
        //        });
        //        if (_sequence == null)
        //        {
        //            SequenceInnerModel _model = new SequenceInnerModel();
        //            _model.BelongToTag = belongtoTag;
        //            _model.BelongToUID = belongtoUID;
        //            _model.SequenceValue = _Init_sequence_value;
        //            rs.Content = _model;

        //        }
        //        else
        //        {
        //            rs.Content = _sequence;
        //        }
        //        rs.Success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        rs.Message = ex.Message;
        //        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
        //        rs.Success = false;
        //        rs.InnerException = ex;
        //    }
        //    return rs;

        //}

        //public IActionResult<bool> UpdateSeqeuce(Guid belongtoUID, string belongtoTag)
        //{
        //    //update  new sequence

        //    var rs = ActionResultTemplates.Result<bool>();
        //    try
        //    {
        //        var _sequence = this._Handler.RetrieveByDynamicConditions(new
        //        {
        //            BelongToUID = belongtoUID,
        //            BelongToTag = belongtoTag
        //        });
        //        if (_sequence != null)
        //        {
        //            _sequence.SequenceValue++;
        //            this._Handler.Update(_sequence);
        //        }
        //        else
        //        {
        //            SequenceInnerModel _model = new SequenceInnerModel();
        //            _model.UID = Guid.NewGuid();
        //            _model.BelongToTag = belongtoTag;
        //            _model.BelongToUID = belongtoUID;
        //            _model.SequenceValue = _Init_sequence_value+1;
        //            rs.Content = this._Handler.CreateByDynamic(_model);
        //            rs.Success = rs.Content;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        rs.Message = ex.Message;
        //        rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
        //        rs.Success = false;
        //        rs.InnerException = ex;
        //    }
        //    return rs;
        //}
    }
}
