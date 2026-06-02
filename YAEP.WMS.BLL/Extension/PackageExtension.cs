using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Constant;

namespace YAEP.WMS.BLL.Extension
{
    public static class PackageExtension
    {
        const string PKG_UOM_DATA_CACHE_KEY = "PKG_UOM_DATA_CACHE";
        //static IPackageUomManager _PackageUomManager;
        static IEnumerable<IPackageUomModel> _PackageUomModels;

        public static IPackageNode MinPackage(this IPackageTree tree)
        {
            //_PackageUomManager = packageUomManager;
            //_PackageUomModels = GetUomlist();
            if (tree != null)
            {
                return GetMinNextPackage(tree.Root);
            }
            else
                return null;
        }
        public static IEnumerable<Guid> GetAllPackageUID(this IPackageNode node)
        {
            if (node.Children.Count == 0)
                yield return node.UID;
            else
            {
                yield return node.UID;
                foreach (var subItem in node.Children)
                {
                    var collection = GetAllPackageUID(subItem);
                    foreach (var item in collection)
                    {
                        yield return item;
                    }

                }

            }
        }
        public static IPackageTree GetTree(this IEnumerable<IPackageModel> source)
        {
            var tree = new PackageTree(source);

            var rootPackage = source.FirstOrDefault(o => !o.ParentUID.HasValue);

            if (rootPackage == null)
            {
                return null;
            }

            var root = source.GetPackageNode(rootPackage, 1);

            tree.Root = root;

            return tree;
        }
        public static bool CompareTree(this IPackageTree tree, IPackageTree tree2)
        {
            var _t1 = tree.Root.GetAllPackageUID();
            var _t2 = tree2.Root.GetAllPackageUID();
            return _t1.Intersect(_t2).Count() == _t1.Count();
        }
        public static bool CompareTreeByPackages(this IPackageTree tree, params Guid[] packages)
        {
            var _t1 = tree.Root.GetAllPackageUID();
            return _t1.Intersect(packages).Count() == packages.GroupBy(g => g).Count();
        }

        private static IPackageNode GetMinNextPackage(IPackageNode node, List<IPackageNode> _CachePackageNode = null)
        {
            if (_CachePackageNode == null)
            {
                _CachePackageNode = new List<IPackageNode>();
            }
            var eachUOM = GetUomlist().FirstOrDefault(x => x.Name.Equals(WMSAPIParameters.EACH_UOM_KEYNAME));
            if (node.Children != null && node.Children.Count > 0)
            {

                var minUomUIDs = GetUomlist()
                    .Where(p => WMSAPIParameters.MIN_PACKAGE_UOM.Any(x => x.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.UID);
                //2019/8/3 版本
                //var minUom = node.Children.FirstOrDefault(p =>
                //    minUomUIDs.Any(x => x.Equals(p.UOM)));
                if (minUomUIDs.Any(x => x == node.UOM))
                {
                    //如果不是Each 再往最下層則繼續往下找直到找到Each
                    if (node.UOM != eachUOM.UID && node.Children.Count > 0)
                    {
                        _CachePackageNode.Add(node);
                        return GetMinNextPackage(node.Children.First(), _CachePackageNode);
                    }
                    else
                    {
                        return node;
                    }
                }

                return GetMinNextPackage(node.Children.First(), _CachePackageNode);
            }
            else
            {
                if (node.UOM != eachUOM.UID && _CachePackageNode.Count > 0)
                {
                    return _CachePackageNode.FirstOrDefault();
                }
                else
                {
                    return node;
                }
            }
        }
        public static IPackageNode Find(this IPackageTree tree, Guid packageUID)
        {
            return tree.Root.Find(o => o.UID == packageUID);
        }
        public static IPackageNode Find(this IPackageTree tree, Func<IPackageNode, bool> predicate)
        {
            return tree.Root.Find(predicate);
        }
        private static IPackageNode Find(this IPackageNode node, Func<IPackageNode, bool> predicate)
        {
            if (predicate(node))
            {
                return node;
            }

            foreach (var item in node.Children)
            {
                var found = item.Find(predicate);

                if (found == null)
                {
                    continue;
                }

                return found;
            }

            return null;
        }
        private static IPackageNode GetPackageNode(this IEnumerable<IPackageModel> source, IPackageModel package, int level, IPackageNode parentNode = null)
        {
            var node = new PackageNode()
            {
                UID = package.UID,
                ItemUID = package.ItemUID,
                VersionUID = package.VersionUID,
                UOM = package.UOM,
                ID = package.ID,
                Name = package.Name,
                Quantity = package.Quantity,
                Width = package.Width,
                Height = package.Height,
                Length = package.Length,
                GrossWeight = package.GrossWeight,
                ImageUID = package.ImageUID,
                CreatedBy = package.CreatedBy,
                CreatedOn = package.CreatedOn,
                ModifiedBy = package.ModifiedBy,
                ModifiedOn = package.ModifiedOn,
                // Package Node
                Level = level,
                Parent = parentNode,
                ParentUID = package.ParentUID,
                ParentUOM = parentNode?.UOM,
                SCC14 = package.SCC14,
                PUOM = package.PUOM
            };

            var children = source.Where(o => o.ParentUID == package.UID);

            var childrenNodes = new List<IPackageNode>();

            foreach (var item in children)
            {
                var cnode = source.GetPackageNode(item, level + 1, node);
                childrenNodes.Add(cnode);
            }

            node.Children = childrenNodes;

            return node;
        }
        private static void ResetUomlist()
        {
            ObjectCache _Cache = MemoryCache.Default;
            _PackageUomModels = _Cache[PKG_UOM_DATA_CACHE_KEY] as List<IPackageUomModel>;
        }
        private static IEnumerable<IPackageUomModel> GetUomlist()
        {
            if (_PackageUomModels == null)
            {
                ResetUomlist();
            }
            return _PackageUomModels;
        }
    }
}
