using System;
using System.Collections.Generic;
using System.Linq;
using YAEP.Common.Constants;
using YAEP.Common.DI;
using YAEP.Common.Interfaces.Models;
using YAEP.Identities.Constants;
using YAEP.Identities.Interfaces.Models;
using YAEP.Interfaces;
using YAEP.Utilities;

namespace YAEP.WMS.Api.Code.Cache
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
        public static IEnumerable<IGroupModel> GetGroup()
        {
            var enumType = DrKnowAllKeys.Group;

            var knowledge = Instance.Recollect<IGroupModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetIdentityFactory().CreateGroupManager();
                var parameters = GetIdentityFactory().CreateGroupSearchParameters();
                var dataResult = manager.GetGroups(parameters);
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// 取得群組物件
        /// </summary>
        /// <param name="groupUID"></param>
        /// <returns></returns>
        public static IGroupModel GetGroup(Guid groupUID)
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
        public static IEnumerable<IGroupModel> GetGroup(IEnumerable<Guid> groupUID)
        {
            if (groupUID?.Count() == 0)
            {
                return null;
            }

            return GetGroup().Where(o => groupUID.Any(g => g == o.UID));
        }
        /// <summary>
        /// 取得所有使用者物件集合
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IUserModel> GetUser()
        {
            var enumType = DrKnowAllKeys.User;

            var knowledge = Instance.Recollect<IUserModel>(enumType.ToString());

            if (!knowledge.HasData)
            {
                var manager = GetIdentityFactory().CreateUserManager();
                var parameters = GetIdentityFactory().CreateUserSearchParameters();
                var dataResult = manager.GetUsers(parameters);
                if (dataResult.Success)
                {
                    knowledge.SetData(dataResult.Content);
                }
                Instance.Remember(enumType.ToString(), knowledge);
            }

            return knowledge.GetData();
        }
        /// <summary>
        /// 取得所有使用者物件
        /// </summary>
        /// <param name="userUID"></param>
        /// <returns></returns>
        public static IUserModel GetUser(Guid userUID)
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
        public static IEnumerable<IUserModel> GetUser(IEnumerable<Guid> userUID)
        {
            if (userUID?.Count() == 0)
            {
                return null;
            }

            return GetUser().Where(o => userUID.Any(g => g == o.UID));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userUID"></param>
        public static void RefreshUser(Guid userUID)
        {
            var enumType = DrKnowAllKeys.User;

            var collection = GetUser();

            var userCached = collection.Where(o => o.UID == userUID);

            var manager = GetIdentityFactory().CreateUserManager();
            var dataResult = manager.GetUser(userUID);

            var userInDB = dataResult.Content;
            if (userInDB?.Status == (int)UserStatus.Void)
            {
                userInDB = null;
            }

            var cacheData = new List<IUserModel>(collection);

            if (userInDB == null)
            {
                cacheData.RemoveAll(o => o.UID == userUID);
            }
            else
            {

                if (userCached == null)
                {
                    // add 
                    cacheData.Add(userInDB);
                }
                else
                {
                    // update 
                    cacheData.RemoveAll(o => o.UID == userUID);
                    cacheData.Add(userInDB);
                }
            }

            var knowledge = Instance.Recollect<IUserModel>(enumType.ToString());
            knowledge.SetData(cacheData);
            Instance.Remember(enumType.ToString(), knowledge);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public static void RecordUserResourcePermissions(IAuthenticationInfo info)
        {
            if (info == null && info.UID == Guid.Empty)
            {
                return;
            }
            string key = $"USER_RESOURCE_PERMISSIONS:{info.UID}";

            var permissions = getUserResourcePermissions(info); 
            var knowledge = Instance.Recollect<IUserResourcePermissionModel>(key);
            knowledge.SetData(permissions);
            Instance.Remember(key, knowledge);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static IEnumerable<IUserResourcePermissionModel> GetUserResourcePermissions(IAuthenticationInfo info)
        {
            if (info == null && info.UID == Guid.Empty)
            {
                return null;
            }

            string key = $"USER_RESOURCE_PERMISSIONS:{info.UID}";

            var knowledge = Instance.Recollect<IUserResourcePermissionModel>(key);

            if (!knowledge.HasData)
            {
                var permissions = getUserResourcePermissions(info);
                knowledge.SetData(permissions);
                Instance.Remember(key, knowledge);
            }

            return knowledge.GetData();
        }

        private static IEnumerable<IUserResourcePermissionModel> getUserResourcePermissions(IAuthenticationInfo info)
        {
            var factory = FactoryUtils.GetIdentityFactory(info);
            var manager = factory.CreateUserManager();
            var result = manager.GetUserResourcePermissions(info.UID);

            if (result.Success)
            {
                return result.Content;
            }

            return null;
        }

    }

}