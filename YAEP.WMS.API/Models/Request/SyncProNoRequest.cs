using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    public class SyncProNoRequest : ISyncProNoRequest
    {
        public SyncProNoRequest()
        {
            this.Items = new List<SyncProItem>().ToArray();
        }
        public string RequestBy { get; set; }
        public IList<ISyncProNoItem> Items { get; set; }
    }
    public class SyncProItem : ISyncProNoItem
    {
        public string Syspon { get; set; }
        public string ProNo { get; set; }
        public Guid ShipViaRefUID { get; set; }
    }
}