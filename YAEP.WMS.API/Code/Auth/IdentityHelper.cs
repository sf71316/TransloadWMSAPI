using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Identities.Constants;
using YAEP.Identities.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.Api.Code
{
    /// <summary>
    /// 
    /// </summary>
    public static class IdentityHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="groupType"></param>
        /// <returns></returns>
        public static IEnumerable<Guid> GetGroupKeys(this IAuthenticationInfo info, GroupTypes? groupType = null)
        {
            if (info == null && info.UID == Guid.Empty)
            {
                return null;
            }

            var factory = FactoryUtils.GetIdentityFactory(info);
            var manager = factory.CreateGroupManager();
            var result = manager.GetGroupKeysByUser(info.UID, groupType);

            if (result.Success)
            {
                return result.Content;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static IEnumerable<IGroupModel> GetGroups(this IAuthenticationInfo info)
        {
            if (info == null && info.UID == Guid.Empty)
            {
                return null;
            }

            var factory = FactoryUtils.GetIdentityFactory(info);
            var manager = factory.CreateGroupManager();
            var result = manager.GetGroupsByUser(info.UID);

            if (result.Success)
            {
                return result.Content;
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="groupUID"></param>
        /// <returns></returns>
        public static IEnumerable<IUserModel> GetGroupUsers(this IAuthenticationInfo info, Guid groupUID)
        {
            if (info == null && info.UID == Guid.Empty)
            {
                return null;
            }

            if (groupUID == Guid.Empty)
            {
                return null;
            }

            var factory = FactoryUtils.GetIdentityFactory(info);
            var manager = factory.CreateGroupManager();
            var result = manager.GetUsersByGroup(groupUID);

            if (result.Success)
            {
                return result.Content;
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static IEnumerable<IUserResourcePermissionModel> GetUserResourcePermissions(this IAuthenticationInfo info)
        {
            return DrKnowAll.GetUserResourcePermissions(info); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="resourceUID"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public static IUserResourcePermissionModel GetPermission(this IAuthenticationInfo info, Guid resourceUID)
        {
            var permissions = info.GetUserResourcePermissions();

            var permission = permissions.FirstOrDefault(o => o.ResourceUID == resourceUID);

            return permission;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="resourceType"></param>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public static IUserResourcePermissionModel GetPermission(this IAuthenticationInfo info, string resourceId, ResourceTypes resourceType)
        {
            var permissions = info.GetUserResourcePermissions();

            var permission = permissions.FirstOrDefault(o => o.ResourceType == resourceType && o.ResourceName.Equals(resourceId, StringComparison.OrdinalIgnoreCase));

            return permission;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="resourceUID"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool HasPermission(this IAuthenticationInfo info, Guid resourceUID, Permissions permissions)
        {
            var p = info.GetPermission(resourceUID);

            if (p == null)
            {
                return false;
            }

            return ((p.Permission & permissions) == permissions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="resourceId"></param>
        /// <param name="resourceType"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool HasPermission(this IAuthenticationInfo info, string resourceId, ResourceTypes resourceType, Permissions permissions)
        {
            var p = info.GetPermission(resourceId, resourceType);

            if (p == null)
            {
                return false;
            }

            return ((p.Permission & permissions) == permissions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="info"></param>
        /// <param name="predicate"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool CheckSitemapPermission(this ISiteMapTree tree, IAuthenticationInfo info, Predicate<ISiteMapNode> predicate, Permissions permissions)
        {
            if (predicate != null)
            {
                var node = tree.Find(n => predicate(n));
                if (node != null)
                {
                    return info.HasPermission(node.UID, permissions);
                }
            }

            return false;
        }
        private static ISiteMapNode Find(this ISiteMapTree tree, Func<ISiteMapNode, bool> predicate)
        {
            if (tree.Children?.Count() > 0)
            {
                foreach (var node in tree.Children)
                {
                    var found = node.Find(predicate);

                    if (found == null)
                    {
                        continue;
                    }

                    return found;
                }
            }

            return null;
        }
        private static ISiteMapNode Find(this ISiteMapNode node, Func<ISiteMapNode, bool> predicate)
        {
            if (predicate(node))
            {
                return node;
            }

            foreach (var cnode in node.Children)
            {
                var found = cnode.Find(predicate);

                if (found == null)
                {
                    continue;
                }

                return found;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static int ActionEnableView(this IAuthenticationInfo info, string controllerName, string actionName)
        {
            return info.ActionEnablePermissions(controllerName: controllerName, actionName: actionName, permissions: Permissions.Read);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static int ActionEnableAdd(this IAuthenticationInfo info, string controllerName, string actionName)
        {
            return info.ActionEnablePermissions(controllerName: controllerName, actionName: actionName, permissions: Permissions.Add);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static int ActionEnableEdit(this IAuthenticationInfo info, string controllerName, string actionName)
        {
            return info.ActionEnablePermissions(controllerName: controllerName, actionName: actionName, permissions: Permissions.Edit);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static int ActionEnableDelete(this IAuthenticationInfo info, string controllerName, string actionName)
        {
            return info.ActionEnablePermissions(controllerName: controllerName, actionName: actionName, permissions: Permissions.Delete);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static int ActionEnablePermissions(this IAuthenticationInfo info, string controllerName, string actionName, Permissions permissions)
        {
            var p = info.GetUserResourcePermissions();

            return p.CheckActionPermission(controllerName: controllerName, actionName: actionName, permission: permissions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public static int ControllerEnable(this IAuthenticationInfo info, string controllerName)
        {
            var p = info.GetUserResourcePermissions();

            return p.CheckControllerPermission(controllerName, Permissions.Enable);
        }

        private static int CheckControllerPermission(this IEnumerable<IUserResourcePermissionModel> p, string controllerName, Permissions permission)
        {
            if (p?.Count() > 0)
            {
                var ctrl = p.FirstOrDefault(o => o.ResourceType == ResourceTypes.WebApiController && o.ResourceName.Equals(controllerName, StringComparison.OrdinalIgnoreCase));
                if (ctrl == null)
                {
                    return -1;
                }
                if (!((ctrl.Permission & permission) == permission))
                {
                    return -1;
                }

                return 1;
            }

            return 0;
        }
        private static int CheckActionPermission(this IEnumerable<IUserResourcePermissionModel> p, string actionName, Permissions permission)
        {
            if (p?.Count() > 0)
            {
                var act = p.FirstOrDefault(o => o.ResourceType == ResourceTypes.WebApiMethod && o.ResourceName.Equals(actionName, StringComparison.OrdinalIgnoreCase));
                if (act == null)
                {
                    return -2;
                }
                if (!((act.Permission & permission) == permission))
                {
                    return -1;
                }

                return 1;
            }

            return 0;
        }
        private static int CheckActionPermission(this IEnumerable<IUserResourcePermissionModel> p, string controllerName, string actionName, Permissions permission)
        {
            if (p?.Count() > 0)
            {
                var ctrlr = p.CheckControllerPermission(controllerName: controllerName, permission: Permissions.Enable);

                if (ctrlr != 1)
                {
                    return ctrlr;
                }

                var actr = p.CheckActionPermission(actionName, permission);
                if (actr != 1)
                {
                    return actr;
                }

                return 1;
            }

            return 0;
        }
    }
}