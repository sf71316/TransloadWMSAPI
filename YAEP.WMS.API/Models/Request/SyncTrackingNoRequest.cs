using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models
{
    public class SyncTrackingNoRequest : ISyncTrackingNoRequest
    {
        public SyncTrackingNoRequest()
        {
            this.Packages = new List<SyncTrackingNoItem>().ToArray();
        }
        public IEnumerable<ISyncTrackingNoItem> Packages { get; set; }
    }
    public class SyncTrackingNoItem : ISyncTrackingNoItem
    {
        public string Syspon { get; set; }
        public string TrackingNo { get; set; }
        public Guid PalletRefUID { get; set; }
    }


}