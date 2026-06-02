using System;
using System.Collections.Generic;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    public partial class PBSCItemPackagingModel : IPBSCItemPackagingModel
    {
        public PBSCItemPackagingModel()
        {
            Packages = new List<PBSCPackagingModel>().ToArray();
        }
        public PBSCItemPackagingModel(
            PBSCItemModel item,
            List<PBSCVirtualItem> mutiboxes,
            List<PBSCPackagingModel> packages)
        {
            Item = (item != null ? item : new PBSCItemModel());
            MultiBoxItem = (mutiboxes != null ? mutiboxes.ToArray() : new List<PBSCVirtualItem>().ToArray());
            Packages = (packages != null ? packages.ToArray() : new List<PBSCPackagingModel>().ToArray());
        }

        public IPBSCItemModel Item { get; set; }
        public IList<IPBSCVirtualItem> MultiBoxItem { get; set; }
        public IList<IPBSCPackagingModel> Packages { get; set; }
    }
}
