using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    public class PBSCItemPackagingModel : IPBSCItemPackagingModel
    {
        public PBSCItemPackagingModel()
        {
            Item = new PBSCItemModel();
            MultiBoxItem = new List<PBSCVirtualItem>().ToArray();
            Packages = new List<PBSCPackagingModel>().ToArray();
        }
        public IPBSCItemModel Item { get; set; }
        public IList<IPBSCVirtualItem> MultiBoxItem { get; set; }
        public IList<IPBSCPackagingModel> Packages { get; set; }
    }
}
