using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Item.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundTicketViewParser : AbstractTicketViewParser
    {
        public InboundTicketViewParser(ILabelRepository labelRepository,
            ProductCacheManager itemManager, PackageCacheManager packageManager, IPackageUomManager uomManager, IWarehouseAgent warehouseAgent)
            : base(labelRepository, itemManager, packageManager, uomManager, warehouseAgent)
        {
        }
        public override IEnumerable<dynamic> Parser(IEnumerable<ITicketInfoListViewModel> collection)
        {
            return this.CustomParser(collection);
        }
        internal override IEnumerable<ITicketViewGroupItem> CustomParser(IEnumerable<ITicketInfoListViewModel> collection)
        {

            //get package name
            foreach (var item in collection)
            {
                if (item.OriginalPackage.HasValue
                    && item.OriginalPackage != Guid.Empty)
                {
                    var pkg = this.PackageCacheManager.GetPackage(item.OriginalPackage.Value);
                    if (pkg!=null)
                        item.OriginalPackageName = pkg.Name;
                }
                if (item.TargetPackage.HasValue && item.TargetPackage != Guid.Empty)
                {
                    var pkg = this.PackageCacheManager.GetPackage(item.TargetPackage.Value);
                    if (pkg != null)
                    {
                        var tpkg = this.PackageCacheManager.GetUOM(pkg.UOM);
                        item.TargetPackageName = pkg.Name;
                        if (tpkg!=null)
                            item.TargetUOMName = tpkg.Name;
                    }
                }
                var pitem = this.ItemCacheManager.GetItem(item.ItemUID);
                if (pitem != null)
                    item.ItemID = pitem.ID;
                item.TicketTypeName = ((TicketCategory)item.TicketType).ToString();
            }
            //group data
            //1.split Workorder (New Pod)
            var _1_group = collection.Where(p => !p.IsPodExist && p.StorageType == 1);
            //2.split workorder+Item (Item,Box) & (in exist pod)
            var _2_group = collection.Where(p => (!p.IsPodExist && p.StorageType != 1));
            var _1_collection = _1_group.GroupBy(grp => new
            {
                PodName = grp.PodName,
                WorkOrderPodUID = grp.WorkOrderPodUID
            }).Select(g => new TicketViewGroupItem
            {
                PodName = g.Key.PodName,
                OperationInstruction = g.First().OperationInstruction,
                OperationSuggestion = g.First().OperationSuggestion,
                WorkOrderPodUID = g.Key.WorkOrderPodUID,
                StorageType = g.First().StorageType,
                WorkOrderPodStatus = ProcessPodStatus(g),
                WorkOrderPodStatusName = ProcessPodStatus(g).ToString(),
                PodBarcode = g.First().PodBarcode,
                UOM = WMSAPIParameters.PALLET_UOM_KEYNAME,//TODO 暫時寫死
                Group = g.Select(p => new TicketViewGroup { ItemID = p.ItemID, UOM = p.TargetUOMName, Qty = p.EstQty }),
                Items = g.ToList()
            });
            var _2_collection = _2_group.GroupBy(grp => new
            {
                PodName = grp.PodName,
                WorkOrderPodUID = grp.WorkOrderPodUID,
                Item = grp.ItemUID,
                UOM = grp.TargetUOMName
            }).Select(g => new TicketViewGroupItem
            {
                PodName = g.Key.PodName,
                OperationInstruction = g.First().OperationInstruction,
                OperationSuggestion = g.First().OperationSuggestion,
                WorkOrderPodUID = g.Key.WorkOrderPodUID,
                UOM = g.Key.UOM,
                StorageType = g.First().StorageType,
                WorkOrderPodStatus = ProcessPodStatus(g),
                WorkOrderPodStatusName = ProcessPodStatus(g).ToString(),
                PodBarcode = g.First().PodBarcode,
                Group = g.Select(p => new TicketViewGroup { ItemID = p.ItemID, UOM = p.TargetUOMName, Qty = p.EstQty }),
                Items = g.ToList()
            });
            _1_collection = _1_collection.Concat(_2_collection).OrderBy(x => x.Items.FirstOrDefault().ItemID);
            return _1_collection;
        }

        private WorkOrderPodProcessStatus ProcessPodStatus(IGrouping<object, ITicketInfoListViewModel> g)
        {
            if (g.All(x => x.TicketInfoStatus == (int)TicketInfoStatus.Open))
            {
                return WorkOrderPodProcessStatus.UnProcess;
            }
            else if (g.Any(p => p.TicketInfoStatus < (int)TicketInfoStatus.Glitch))
            {
                return WorkOrderPodProcessStatus.Processing;
            }
            else
            {
                return WorkOrderPodProcessStatus.Processed;
            }

        }
    }
}
