using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    public class PickAllRequest : IPickAllRequest
    {
        public string RefNo { get; set; }
        public int RequestFunction { get; set; }
        public int ChangeStatus { get; set; }
        public IEnumerable<Guid> ItemRefUID { get; set; }
        public string RequestBy { get; set; }
    }
    public class PickItemRequest : IPickItemRequest
    {
        public string RefNo { get; set; }
        public int ChangeStatus { get; set; }
        public IEnumerable<Guid> ItemRefUID { get; set; }
        public string RequestBy { get; set; }
        public int RequestFunction { get; set; }
    }
}