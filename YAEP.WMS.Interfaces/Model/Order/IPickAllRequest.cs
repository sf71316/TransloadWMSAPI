using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IPickAllRequest
    {
        int RequestFunction { get; set; }
        string RequestBy { get; set; }
        string RefNo { get; set; }
        int ChangeStatus { get; set; }
        IEnumerable<Guid> ItemRefUID { get; set; }
    }
    public interface IPickAllResponse : ICommonResponse
    {
        int ErrorCode { get; set; }
    }
    public interface IPickItemRequest : IPickAllRequest
    {

    }
    public interface IPickItemResponse : ICommonResponse
    {

    }
}
