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
    public abstract class AbstractTicketViewParser
    {
        protected ILabelRepository LabelRepository { get; set; }
        internal ProductCacheManager ItemCacheManager { get; set; }
        internal PackageCacheManager PackageCacheManager { get; set; }
        //protected IPackageUomManager UOMManager { get; set; }
        protected IWarehouseAgent WarehouseAgent { get; set; }

        internal AbstractTicketViewParser(ILabelRepository labelRepository, ProductCacheManager itemManager,
            PackageCacheManager packageManager, IPackageUomManager uomManger, IWarehouseAgent warehouseAgent)
        {
            this.LabelRepository = labelRepository;
            this.ItemCacheManager = itemManager;
            this.PackageCacheManager = packageManager;
            //this.UOMManager = uomManger;
            this.WarehouseAgent = warehouseAgent;
        }
        internal virtual IEnumerable<ITicketViewGroupItem> CustomParser(IEnumerable<ITicketInfoListViewModel> collection)
        {
            //get label 
            //var _allLabel = collection.Select(p => p.PodUID).ToList();
            //_allLabel.AddRange(collection.Select(p => p.PayloadUID));
            //get package name
            foreach (var item in collection)
            {
                if (item.OriginalPackage.HasValue && item.OriginalPackage != Guid.Empty)
                {
                    var pkg = this.PackageCacheManager.GetPackage(item.OriginalPackage.Value);

                    if (pkg != null)
                    {
                        var tpkg = this.PackageCacheManager.GetUOM(pkg.UOM);
                        item.OriginalPackageName = tpkg.Name;
                    }
                }
                if (item.TargetPackage.HasValue && item.TargetPackage != Guid.Empty)
                {
                    var pkg = this.PackageCacheManager.GetPackage(item.TargetPackage.Value);
                    if (pkg != null)
                    {
                        var tpkg = this.PackageCacheManager.GetUOM(pkg.UOM);
                        if (tpkg != null)
                        {
                            item.TargetUOMName =
                            item.TargetPackageName = tpkg.Name;
                        }
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
                WorkOrderPodUID = g.Key.WorkOrderPodUID,
                OperationInstruction = g.First().OperationInstruction,
                OperationSuggestion = g.First().OperationSuggestion,
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
                WorkOrderPodUID = g.Key.WorkOrderPodUID,
                UOM = g.Key.UOM,
                OperationInstruction = g.First().OperationInstruction,
                OperationSuggestion = g.First().OperationSuggestion,
                Group = g.Select(p => new TicketViewGroup { ItemID = p.ItemID, UOM = p.TargetUOMName, Qty = p.EstQty }),
                Items = g.ToList()
            });
            _1_collection = _1_collection.Concat(_2_collection).OrderBy(x => x.Items.FirstOrDefault().ItemID);
            return _1_collection;
        }
        public virtual IEnumerable<dynamic> Parser(IEnumerable<ITicketInfoListViewModel> collection)
        {

            //get package name
            foreach (var item in collection)
            {
                if (item.OriginalPackage.HasValue && item.OriginalPackage != Guid.Empty)
                {
                    var pkg = this.PackageCacheManager.GetPackage(item.OriginalPackage.Value);
                    if (pkg != null)
                        item.OriginalPackageName = pkg.Name;
                }
                if (item.TargetPackage.HasValue && item.TargetPackage != Guid.Empty)
                {
                    var pkg = this.PackageCacheManager.GetPackage(item.TargetPackage.Value);
                    if (pkg != null)
                        item.TargetPackageName = pkg.Name;
                    if (string.IsNullOrEmpty(item.TargetUOMName))
                    {
                        var tpkg = this.PackageCacheManager.GetUOM(pkg.UOM);
                        item.TargetPackageName = pkg.Name;
                        if (tpkg != null)
                            item.TargetUOMName = tpkg.Name;
                    }
                }
                var pitem = this.ItemCacheManager.GetItem(item.ItemUID);
                if (pitem != null)
                    item.ItemID = pitem.ID;
                item.TicketTypeName = item.TicketType.ToString();

            }
            return collection.OrderBy(p => p.ItemID);
        }
        internal static AbstractTicketViewParser GetParser(TicketCategory serviceItem,
            ILabelRepository labelRepository, ProductCacheManager itemManager,
            PackageCacheManager packageManager, IPackageUomManager packageUomManager, IWarehouseAgent warehouseAgent)
        {
            switch (serviceItem)
            {
                case TicketCategory.Receive:
                    return new InboundTicketViewParser(labelRepository, itemManager, packageManager, packageUomManager, warehouseAgent);
                case TicketCategory.Outbound:
                    return new OutboundTicketViewParser(labelRepository, itemManager, packageManager, packageUomManager, warehouseAgent);
                case TicketCategory.Move:
                case TicketCategory.BulkPick:
                    return new MoveTicketInfoViewParser(labelRepository, itemManager, packageManager, packageUomManager, warehouseAgent);
                case TicketCategory.InventoryCounting:
                    return new InventoryCountingTicketVieweParser(labelRepository, itemManager, packageManager, packageUomManager, warehouseAgent);

            }
            return null;
        }
    }
}
