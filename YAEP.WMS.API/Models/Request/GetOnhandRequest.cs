using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class GetOnhandRequest : IGetOnhandRequest
    {
        public GetOnhandRequest()
        {
            this.Items = new List<GetOnhandItem>().ToArray();
        }
        public Guid WarehouseUID { get; set; }
        public Guid CustomerUID { get; set; }
        public string ReceiverUrl { get; set; }
        public string ReceiverSecret { get; set; }
        public IList<IGetOnhandItem> Items { get; set; }
    }
    public class GetOnhandItem : IGetOnhandItem
    {
        public string ItemNo { get; set; }
        public Guid ItemUID { get; set; }
        public int Onhand { get; set; }
    }
}