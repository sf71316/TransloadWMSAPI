using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Identities.Constants;
using YAEP.Interfaces;
using YAEP.WMS.Cache.Models;
using YAEP.WMS.Cache.Redis.Controllers;

namespace YAEP.WMS.Cache.Redis
{
    /*
    *  Identities (Group / User / Role) 
    */
    public static partial class DrKnowAll
    {
        /// <summary>
        /// 取得所有群組物件集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<GroupCacheModel> GetGroup()
        {
            var controller = new GroupRedisController();
            var allGroup = controller.RetrieveAll();

            if ((allGroup?.Count() ?? 0) == 0)
            {
                var factory = FactoryUtils.GetIdentityFactoryInstance();
                var manager = factory.CreateGroupManager();
                var parameters = factory.CreateGroupSearchParameters();
                var data = manager.GetGroups(parameters)?.Content;

                if ((data?.Count() ?? 0) > 0)
                {
                    allGroup = data.Select(o => new GroupCacheModel()
                    {
                        UID = o.UID,
                        ID = o.ID,
                        ParentUID = o.ParentUID,
                        Name = o.Name,
                        Description = o.Description,
                        Abbrev = o.Abbrev,
                        Type = o.Type,
                        Status = o.Status,
                        Sort = o.Sort,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        ModifiedBy = o.ModifiedBy,
                        ModifiedOn = o.ModifiedOn,
                    }).ToArray();

                    controller.Create(allGroup);
                }
            }

            return allGroup;
        }
        /// <summary>
        /// 取得群組物件
        /// </summary>
        /// <param name="groupUID"></param>
        /// <returns></returns>
        public static GroupCacheModel GetGroup(Guid groupUID)
        {
            if (groupUID == Guid.Empty)
            {
                return null;
            }

            return GetGroup().FirstOrDefault(o => o.UID == groupUID);
        }
        /// <summary>
        /// 取得群組物件集合
        /// </summary>
        /// <param name="groupUID"></param>
        /// <returns></returns>
        public static IEnumerable<GroupCacheModel> GetGroup(IEnumerable<Guid> groupUID)
        {
            if (groupUID?.Count() == 0)
            {
                return null;
            }

            return GetGroup().Where(o => groupUID.Any(g => g == o.UID)).ToArray();
        }
        /// <summary>
        /// 取得所有使用者物件集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<UserCacheModel> GetUser()
        {
            var controller = new UserRedisController();
            var allUser = controller.RetrieveAll();

            if ((allUser?.Count() ?? 0) == 0)
            {
                var factory = FactoryUtils.GetIdentityFactoryInstance();
                var manager = factory.CreateUserManager();
                var parameters = factory.CreateUserSearchParameters();
                var data = manager.GetUsers(parameters)?.Content;

                if ((data?.Count() ?? 0) > 0)
                {
                    allUser = data.Select(o => new UserCacheModel()
                    {
                        UID = o.UID,
                        ID = o.ID,
                        Account = o.Account,
                        Email = o.Email,
                        Description = o.Description,
                        Skype = o.Skype,
                        FirstName = o.FirstName,
                        LastName = o.LastName,
                        Telephone = o.Telephone,
                        Fax = o.Fax,
                        CellPhone = o.CellPhone,
                        Theme = o.Theme,
                        Country = o.Country,
                        State = o.State,
                        DefaultRoleUID = o.DefaultRoleUID,
                        Type = o.Type,
                        Status = o.Status,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        ModifiedBy = o.ModifiedBy,
                        ModifiedOn = o.ModifiedOn,
                    }).ToArray();

                    controller.Create(allUser);
                }
            }

            return allUser;
        }
        /// <summary>
        /// 取得所有使用者物件
        /// </summary>
        /// <param name="userUID"></param>
        /// <returns></returns>
        public static UserCacheModel GetUser(Guid userUID)
        {
            if (userUID == Guid.Empty)
            {
                return null;
            }

            return GetUser().FirstOrDefault(o => o.UID == userUID);
        }
        /// <summary>
        /// userUID
        /// </summary>
        /// <param name="userUID"></param>
        /// <returns></returns>
        public static IEnumerable<UserCacheModel> GetUser(IEnumerable<Guid> userUID)
        {
            if (userUID?.Count() == 0)
            {
                return null;
            }

            return GetUser().Where(o => userUID.Any(g => g == o.UID)).ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userUID"></param>
        public static void RefreshUser(Guid userUID)
        {
            if (userUID == Guid.Empty)
            {
                return;
            }

            var factory = FactoryUtils.GetIdentityFactoryInstance();
            var manager = factory.CreateUserManager();
            var data = manager.GetUser(userUID)?.Content;

            var controller = new UserRedisController();
            if (data != null && data.Status != (int)UserStatus.Void)
            {
                var cacheData = new UserCacheModel()
                {
                    UID = data.UID,
                    ID = data.ID,
                    Account = data.Account,
                    Email = data.Email,
                    Description = data.Description,
                    Skype = data.Skype,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Telephone = data.Telephone,
                    Fax = data.Fax,
                    CellPhone = data.CellPhone,
                    Theme = data.Theme,
                    Country = data.Country,
                    State = data.State,
                    DefaultRoleUID = data.DefaultRoleUID,
                    Type = data.Type,
                    Status = data.Status,
                    CreatedBy = data.CreatedBy,
                    CreatedOn = data.CreatedOn,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedOn = data.ModifiedOn,
                };
                controller.Replace(userUID, cacheData);
            }
            else
            {
                controller.Delete(userUID);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public static void RecordUserResourcePermissions(IAuthenticationInfo info)
        {
            if ((info?.UID ?? Guid.Empty) == Guid.Empty)
            {
                return;
            }

            var permissions = getUserResourcePermissions(info);

            var controller = new UserResourcePermissionRedisController();
            controller.Replace(info.UID, permissions);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static IEnumerable<UserResourcePermissionCacheModel> GetUserResourcePermissions(IAuthenticationInfo info)
        {
            if ((info?.UID ?? Guid.Empty) == Guid.Empty)
            {
                return null;
            }

            var controller = new UserResourcePermissionRedisController();
            var permissions = controller.GetByUser(info.UID);

            if ((permissions?.Count() ?? 0) == 0)
            {
                permissions = getUserResourcePermissions(info);

                if ((permissions?.Count() ?? 0) > 0)
                {
                    controller.Create(permissions);
                }
            }

            return permissions;
        }

        private static IEnumerable<UserResourcePermissionCacheModel> getUserResourcePermissions(IAuthenticationInfo info)
        {
            var factory = FactoryUtils.GetIdentityFactoryInstance(info);
            var manager = factory.CreateUserManager();
            var data = manager.GetUserResourcePermissions(info.UID)?.Content;

            if (data != null)
            {
                return data.Select(o => new UserResourcePermissionCacheModel()
                {
                    UserUID = o.UserUID,
                    UserAccount = o.UserAccount,
                    ResourceUID = o.ResourceUID,
                    ResourceID = o.ResourceID,
                    ResourceName = o.ResourceName,
                    ResourceType = o.ResourceType,
                    Permission = o.Permission,
                }).ToArray();
            }

            return null;
        }

    }

}