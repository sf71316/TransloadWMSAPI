using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.BLL.Extension;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Constant;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class PackageCacheManager
    {
        IPackageManager _PackageManager;
        ConcurrentDictionary<Guid, IPackageNode> _MiniPackageCollection;
        ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> _PkgversionCollection;
        ConcurrentDictionary<Guid, IPackageModel> _PkgCollection;
        ConcurrentBag<IPackageTree> _PkgTreeSet;
        ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> _ItemPkgTrees;
        List<IPackageViewModel> _PackageCache;
        List<IPackageVersionModel> _PackageVersionsCache;
        Func<List<IPackageViewModel>> _packageCacheMethod;
        Func<List<IPackageVersionModel>> _packageVersinCacheMethod;
        Func<List<IPackageUomModel>> _packageUomCacheMethod;
        ILogInfiltrator _Logger { get; set; }
        IEnumerable<IPackageUomModel> _PackageUomCache;
        public PackageCacheManager(
            Func<List<IPackageViewModel>> packageCacheMethod,
            Func<List<IPackageVersionModel>> packageVersionCacheMethod,
            Func<List<IPackageUomModel>> packageUomCacheMethod,
            ConcurrentDictionary<Guid, IPackageNode> _miniPackageCollection, ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> _pkgversionCollection,
            ConcurrentDictionary<Guid, IPackageModel> _pkgCollection, ConcurrentBag<IPackageTree> _pkgTreeSet,
            ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> _itemPkgTrees,
        IPackageManager packageManager, IPackageUomManager packageUomManager, ILogInfiltrator log = null)
        {
            _PackageManager = packageManager;
            _MiniPackageCollection = new ConcurrentDictionary<Guid, IPackageNode>();
            _PkgversionCollection = new ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>>();
            _PkgCollection = new ConcurrentDictionary<Guid, IPackageModel>();
            _PkgTreeSet = new ConcurrentBag<IPackageTree>();
            _ItemPkgTrees = new ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>>();
            _Logger = log;
            //_PackageUomModels = this._PackageUomManager.GetPackageUomList();
            this._packageCacheMethod = packageCacheMethod;
            this._packageUomCacheMethod = packageUomCacheMethod;
            this._packageVersinCacheMethod = packageVersionCacheMethod;
            LoadCache();
        }
        public void InitPackageManager(ConcurrentDictionary<Guid, IPackageNode> _miniPackageCollection, ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> _pkgversionCollection,
            ConcurrentDictionary<Guid, IPackageModel> _pkgCollection, ConcurrentBag<IPackageTree> _pkgTreeSet,
            ConcurrentDictionary<Guid, IEnumerable<IPackageViewModel>> _itemPkgTrees, IPackageManager packageManager, IPackageUomManager packageUomManager)
        {
            //  _PackageManager = packageManager;
            _MiniPackageCollection = _miniPackageCollection;
            _PkgversionCollection = _pkgversionCollection;
            _PkgCollection = _pkgCollection;
            _PkgTreeSet = _pkgTreeSet;
            _ItemPkgTrees = _itemPkgTrees;

        }
        public void LoadCache()
        {

            this._PackageCache = _packageCacheMethod.Invoke();
            this._PackageUomCache = _packageUomCacheMethod.Invoke();
            this._PackageVersionsCache = _packageVersinCacheMethod.Invoke();
        }
        public IEnumerable<IPackageViewModel> GetPackagesByItems(IEnumerable<Guid> itemUIDs)
        {
            var packages = this._PackageCache.Where(p => itemUIDs.Contains(p.ItemUID));
            return packages;
        }
        public IEnumerable<IPackageViewModel> GetPackagesByItem(Guid itemUID)
        {
            var packages = this._PackageCache.Where(p => p.ItemUID == itemUID);

            if (packages.Count() == 0)
            {
                this.WriteLog($"Get cache data error {itemUID}", "GetPackagesByItem", "Warn");
                //this.WriteLog($"Package get data error {packages.Message} {packages.InnerException.Message}", "", "");
            }
            return packages;
        }
        public IEnumerable<IPackageViewModel> GetPackagesByVersion(Guid VersionUID)
        {
            //var pkg = _PkgversionCollection.FirstOrDefault(p => p.Key == VersionUID);
            var _pkgver = this._PackageCache.Where(p => p.VersionUID == VersionUID);
            return _pkgver;

        }
        public IPackageNode GetMinPackage(Guid PackageUID)
        {
            var pkg = _MiniPackageCollection.FirstOrDefault(p => p.Key == PackageUID);
            if (pkg.Equals(default(KeyValuePair<Guid, IPackageNode>))) //找不到對應最小包裝
            {
                var _mpkg = this.GetPackageTree(PackageUID).MinPackage();
                _MiniPackageCollection.TryAdd(PackageUID, _mpkg);
                return _mpkg;
            }
            else
            {
                return pkg.Value;
            }
        }
        public IPackageNode GetMinPackage(IEnumerable<IPackageViewModel> packages)
        {
            var _tree = packages.GetTree();
            var _mpkg = _tree.MinPackage();
            if (!_PkgTreeSet.Any(p => p.CompareTree(_tree)))
            {
                _PkgTreeSet.Add(_tree);
            }
            foreach (var item in packages)
            {
                if (!_MiniPackageCollection.Any(p => p.Key == item.UID))
                {
                    _MiniPackageCollection.TryAdd(item.UID, _mpkg);
                }
            }
            return _mpkg;
        }
        public IPackageModel GetPackage(Guid PackageUID)
        {
            var pkg = _PkgCollection.FirstOrDefault(p => p.Key == PackageUID);
            if (pkg.Equals(default(KeyValuePair<Guid, IPackageModel>)))
            {

                var _pkg = this._PackageCache.FirstOrDefault(p => p.UID == PackageUID);
                return _pkg;
            }
            else
            {
                return pkg.Value;
            }
        }
        public IPackageTree GetPackageTree(Guid packageUID)
        {
            var pkg = this.GetPackage(packageUID);
            var pkgs = this.GetPackagesByVersion(pkg.VersionUID);

            return pkgs.GetTree();
        }
        public IEnumerable<IPackageNode> GetScc14barcde(IPackageNode pkgNode, List<IPackageNode> results = null)
        {
            if (pkgNode != null)
            {
                var stack = new Stack<IPackageNode>(pkgNode.Children);
                if (results == null)
                    results = new List<IPackageNode>();
                if (!string.IsNullOrEmpty(pkgNode.SCC14))
                {
                    results.Add(pkgNode);
                }
                while (stack.Any())
                {
                    var next = stack.Pop();
                    GetScc14barcde(next, results);
                }
            }
            return results;
        }
        public IPackageNode GetPUOMbarcde(IPackageNode pkgNode, string barcode, List<IPackageNode> results = null)
        {
            var puompkg = GetPUOMbarcde(pkgNode, results);
            return puompkg.FirstOrDefault(p => p.PUOM == barcode);
        }
        public IEnumerable<IPackageNode> GetPUOMbarcde(IPackageNode pkgNode, List<IPackageNode> results = null)
        {
            if (pkgNode != null)
            {
                var stack = new Stack<IPackageNode>(pkgNode.Children);
                if (results == null)
                    results = new List<IPackageNode>();
                if (!string.IsNullOrEmpty(pkgNode.PUOM))
                {
                    results.Add(pkgNode);
                }
                while (stack.Any())
                {
                    var next = stack.Pop();
                    GetPUOMbarcde(next, results);
                }
            }
            return results;
        }
        public IPackageNode FindPkgSCC14barcode(IPackageNode pkgNode, string content)
        {
            if (pkgNode.SCC14 == content)
            {
                return pkgNode;
            }
            else
            {
                foreach (var item in pkgNode.Children)
                {
                    return FindPkgSCC14barcode(item, content);
                }
                return null;
            }
        }
        //public bool UseVirtualItem(IEnumerable<IPackageViewModel> packages)
        //{
        //    //是否有Set 包裝
        //    var rs1 = packages.Any(p => p.ParentUOM.HasValue ?
        //                            GetUOM(p.ParentUOM.Value).Name == WMSAPIParameters.SET_UOM_KEYNAME
        //                            : false);
        //    //判斷剩下有包裝的Parent 是Set
        //    var rs2 = packages.Where(x => x.ParentUomName == WMSAPIParameters.SET_UOM_KEYNAME 
        //    && x.UomName.Contains(WMSAPIParameters.BOX_UOM_KEYNAME)).Count() > 0;
        //    return rs1 && rs2;
        //}
        public IActionResult<int> GetReceivePackageUomQuantity(
            Guid packageUID, Guid receivePackageUID, int quantity, IEnumerable<IPackageViewModel> packages = null)
        {
            if (packages != null)
            {
                var tree = packages.GetTree();
                if (!_PkgTreeSet.Any(p => p.CompareTree(tree)))
                {
                    _PkgTreeSet.Add(tree);
                }
            }
            var _tree = _PkgTreeSet.FirstOrDefault(p =>
            p.CompareTreeByPackages(packageUID, receivePackageUID));
            if (_tree == null)
            {
                var pkgTree = this.GetPackageTree(packageUID);
                return this._PackageManager
                      .GetReceivePackageUomQuantity(pkgTree, packageUID,
                      receivePackageUID, quantity);
            }
            else
            {
                return this._PackageManager
                        .GetReceivePackageUomQuantity(_tree, packageUID,
                        receivePackageUID, quantity);
            }

        }
        public Guid? GetUomUniqueFromName(string name)
        {
            var uom = GetUomList().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (uom != null)
            {
                return uom.UID;
            }
            return null;
        }
        public IPackageUomModel GetUOM(Guid uomUID)
        {
            if (_PackageUomCache == null)
            {
                GetUomList();
            }
            return _PackageUomCache.FirstOrDefault(p => p.UID == uomUID);
        }
        public IEnumerable<Guid> GetMaxUomUnique()
        {
            var uom = GetUomList().Where(p =>
            WMSAPIParameters.MAX_PACKAGE_UOM.Any(x => x.Equals(p.Name, StringComparison.OrdinalIgnoreCase)));
            if (uom != null)
            {
                return uom.Select(p => p.UID);
            }
            return null;
        }
        public IEnumerable<Guid> GetMinUomUnique()
        {
            var uom = GetUomList().Where(p =>
            WMSAPIParameters.MIN_PACKAGE_UOM.Any(x => x.Equals(p.Name, StringComparison.OrdinalIgnoreCase)));
            if (uom != null)
            {
                return uom.Select(p => p.UID);
            }
            return null;
        }

        public int? FindPackageDepthIndex(Guid packageUID)
        {
            int depth = 0;
            var tree = this.GetPackageTree(packageUID);
            return InnerFindPackageDepthIndex(tree.Root, packageUID, depth);
        }
        private int? InnerFindPackageDepthIndex(IPackageNode node, Guid packageUID, int depth)
        {
            if (node.UID == packageUID)
            {
                return depth;
            }
            else
            {
                if (node != GetPackageTree(packageUID).MinPackage())
                {
                    depth++;
                    //平行包裝可能會算錯
                    foreach (var item in node.Children)
                    {
                        return InnerFindPackageDepthIndex(item, packageUID, depth);
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
        }
        private IEnumerable<IPackageUomModel> GetUomList()
        {
            return _PackageUomCache;
        }
        private void WriteLog(string message, string type, string level)
        {
            if (_Logger != null)
            {
                this._Logger.Log(message, type, "", level, (int)YAEP.Constants.BelongToTypes.Package, application: WMSAPIParameters.CONNECT_LOG_NAME);
            }
        }
        //private void AddMiniPackageCache(Guid PackageUID)
        //{
        //    var pkg = _MiniPackageCollection.FirstOrDefault(p => p.Key == PackageUID);
        //    if (pkg.Equals(default(KeyValuePair<Guid, IPackageNode>))) //找不到對應最小包裝
        //    {
        //        var _mpkg = this.GetPackageTree(PackageUID).MinPackage(this._PackageUomManager);
        //        _MiniPackageCollection.Add(PackageUID, _mpkg);
        //    }
        //}
    }
}
