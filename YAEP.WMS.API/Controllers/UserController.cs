using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using YAEP.Identities.Constants;
using YAEP.Identities.DI;
using YAEP.Identities.Interfaces.Models;
using YAEP.Identities.Models;
using YAEP.WMS.Api.Code;
using YAEP.WMS.API.Code;
using YAEP.WMS.API.Models;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Controllers.Api.Attributes;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// 
    /// </summary>
    [Authentication]
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [RoutePrefix("api/User")]
    public class UserController : AbstractApiController
    {
        /// <summary>
        /// 
        /// </summary>
        public UserController()
        {
            this._IdentityFactory = new Lazy<IdentityFactory>(() => FactoryUtils.GetIdentityFactory(base.GetAuthenticationInfo()));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetCurrentUser")]
        public IHttpActionResult GetCurrentUser()
        {
            var authInfo = base.GetAuthenticationInfo();

            var factory = this.GetIdentityFactory();
            var manager = factory.CreateUserManager();

            var userResult = manager.GetUser(authInfo.UID);

            if (userResult.Success)
            {
                var user = userResult.Content;
                var r = new
                {
                    name = $"{user.FirstName} {user.LastName}",
                    avatar = "./static/icon3_128.8e492104.ico",
                    userid = user.Account,
                    email = user.Email,
                    phone = user.CellPhone,
                    title = user.Description,
                    notifyCount = 12,
                    signature = "Coooo~",
                    group = "CAP Barbell - TWDC",
                    country = "Anywhere",
                    address = "Anywhere",
                    geographic = new
                    {
                        province = new
                        {
                            label = "Taiwan",
                            key = "330000",
                        },
                        city = new
                        {
                            label = "Taichung",
                            key = "330100",
                        },
                    },
                    tags = new object[] {
                       new  {
                           key ="0",
                           label ="好喝",
                       },
                       new  {
                           key ="1",
                           label ="臉紅紅",
                       },
                       new  {
                           key ="2",
                           label ="飲料",
                       },
                       new  {
                           key ="3",
                           label ="馬上臉紅紅",
                       },
                       new  {
                           key ="4",
                           label ="果汁",
                       },
                        new  {
                           key ="5",
                           label ="It's good to drink",
                       },
                  },
                };

                return this.Json(r);
                //var actionResult = this.GetSuccessResult(r);
                //return this.Json(actionResult);
            }

            return this.Json(new { });
            //return this.GetFailureResult(-1, "Not Found.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetCurrentUserGroupKeys")]
        public IHttpActionResult GetCurrentUserGroupKeys()
        {
            var authInfo = base.GetAuthenticationInfo();

            var factory = this.GetIdentityFactory();
            var manager = factory.CreateGroupManager();

            var result = manager.GetGroupKeysByUser(authInfo.UID);
            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }

            return this.GetFailureResult(-1, "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetCurrentUserGroups")]
        public IHttpActionResult GetCurrentUserGroups()
        {
            var authInfo = base.GetAuthenticationInfo();

            var factory = this.GetIdentityFactory();
            var manager = factory.CreateGroupManager();

            var result = manager.GetGroupsByUser(authInfo.UID);
            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }

            return this.GetFailureResult(-1, "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetCurrentUserRoles")]
        public IHttpActionResult GetCurrentUserRoles()
        {
            var authInfo = base.GetAuthenticationInfo();

            var factory = this.GetIdentityFactory();
            var manager = factory.CreateUserManager();

            var result = manager.GetUserRoles(authInfo.UID);
            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }

            return this.GetFailureResult(-1, "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetAllUserRoles")]
        public IHttpActionResult GetAllUserRoles()
        {
            var authInfo = base.GetAuthenticationInfo();

            var factory = this.GetIdentityFactory();
            var manager = factory.CreateUserManager();

            var result = manager.GetAllUserRoles();
            if (result.Success)
            {
                var group = result.Content?.GroupBy(o => new { o.UserUID, o.UserAccount, o.UserEmail });

                if (group?.Count() > 0)
                {
                    var r = group.Select(g =>
                    new
                    {
                        g.Key.UserUID,
                        g.Key.UserAccount,
                        g.Key.UserEmail,
                        Roles = g.Select(o => new
                        {
                            o.ID,
                            o.Name,
                            o.Description,
                            o.Sort,
                            o.Type,
                        }).ToArray(),
                    });

                    var aresult = this.GetSuccessResult(r);
                    return this.Json(aresult);
                }

                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }

            return this.GetFailureResult(-1, "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetUserMenu")]
        public IHttpActionResult GetUserMenu()
        {
            string webSiteId = "WMS";

            var factory = this.GetIdentityFactory();
            var manager = factory.CreateResourceManager();
            var webSite = manager.GetWebSite(webSiteId).Content;

            if (webSite == null)
            {
                return this.GetFailureResult(-1, "Not Found.");
            }

            var authInfo = base.GetAuthenticationInfo();
            var permissions = authInfo.GetUserResourcePermissions();

            var webSitePermission = permissions.FirstOrDefault(o => o.ResourceType == ResourceTypes.WebSite && o.ResourceName.Equals(webSiteId, StringComparison.OrdinalIgnoreCase));

            if (webSitePermission == null || !((webSitePermission.Permission & Permissions.Enable) == Permissions.Enable))
            {
                return this.GetFailureResult(-1, "No Permission. (Web Site)");
            }

            var sitemapTree = manager.GetSiteMapTree(webSite.UID).Content;

            if (sitemapTree == null)
            {
                return this.Json(new object[] { });
            }

            var list = new List<object>();
            list.AddRange(this.getAppMenu(sitemapTree, permissions));

            return this.Json(list);
        }


        private IEnumerable<object> getAppMenu(ISiteMapTree tree, IEnumerable<IUserResourcePermissionModel> permissions)
        {
            var list = new List<object>();

            // default 
            //list.Add(new
            //{
            //    path = "/",
            //    redirect = "/log-view",
            //    exact = true
            //});

            var sorted = tree.Children.OrderBy(o => o.Sort);
            foreach (var node in sorted)
            {
                var o = this.getAppMenuItem(node, permissions);
                if (o == null)
                {
                    continue;
                }
                list.Add(o);
            }

            return list;
        }
        private object getAppMenuItem(ISiteMapNode node, IEnumerable<IUserResourcePermissionModel> permissions)
        {
            if (this.checkResourcePermission(node, permissions) && node.IsDisplay)
            {
                var list = new List<object>();

                if (node.Children?.Count() > 0)
                {
                    var sorted = node.Children.OrderBy(o => o.Sort);
                    foreach (var cnode in sorted)
                    {
                        var c = this.getAppMenuItem(cnode, permissions);
                        if (c != null)
                        {
                            list.Add(c);
                        }
                    }
                }

                IDictionary<string, object> expando = new System.Dynamic.ExpandoObject();
                expando["name"] = node.LocaleId ?? String.Empty;
                expando["path"] = node.Path ?? String.Empty;
                if (!String.IsNullOrWhiteSpace(node.Icon))
                {
                    expando["icon"] = node.Icon ?? String.Empty;
                }
                if (list.Count() > 0)
                {
                    expando["children"] = list.ToArray();
                }
                expando["authority"] = new string[] { "admin", "user" };

                return expando;
            }

            return null;
        }
        private bool checkResourcePermission(ISiteMapNode node, IEnumerable<IUserResourcePermissionModel> permissions)
        {
            var p = permissions.FirstOrDefault(o => o.ResourceType == ResourceTypes.SiteMap && o.ResourceName.Equals(node.ID, StringComparison.OrdinalIgnoreCase));

            if (p == null || !((p.Permission & Permissions.Enable) == Permissions.Enable))
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CheckPermission")]
        public IHttpActionResult CheckPagePermissions(string path)
        {
            var trueResult = base.GetSuccessResult(true);
            return base.Json(trueResult);

            string webSiteId = "WMS";

            try
            {
                var factory = this.GetIdentityFactory();
                var manager = factory.CreateResourceManager();
                var webSite = manager.GetWebSite(webSiteId).Content;
                path = path.GetFilterXSSstring();
                if (webSite == null)
                {
                    return this.GetFailureResult(-1, "Not Found.");
                }

                var authInfo = base.GetAuthenticationInfo();
                var permissions = authInfo.GetUserResourcePermissions();

                var webSitePermission = permissions.FirstOrDefault(o => o.ResourceType == ResourceTypes.WebSite && o.ResourceName.Equals(webSiteId, StringComparison.OrdinalIgnoreCase));

                if (webSitePermission == null || !((webSitePermission.Permission & Permissions.Enable) == Permissions.Enable))
                {
                    return this.GetFailureResult(-1, "No Permission. (Web Site)");
                }

                var sitemapTree = manager.GetSiteMapTree(webSite.UID).Content;

                if (sitemapTree != null)
                {
                    bool hasPermission = sitemapTree.CheckSitemapPermission(authInfo, o => o.Path.Equals(path, StringComparison.OrdinalIgnoreCase), Permissions.Enable);
                    var apiResult = base.GetSuccessResult(hasPermission);
                    return base.Json(apiResult);
                }
            }
            catch (Exception ex)
            {
                return this.GetFailureResult(-1, ex.Message);
            }

            return this.GetFailureResult(-1, "No Permission.");
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="user">user model</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddUser")]
        public IHttpActionResult AddUser([FromBody]UserModel user)
        {
            return this.createUser(user);
        }

        /// <summary>
        /// Create User & Relationship to groups
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddUserWithGroups")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult AddUserWithGroups([FromBody]AddUserWithGroupsRequestModel user)
        {
            return this.createUser(user);
        }

        private IHttpActionResult createUser(IUserModel user)
        {
            if (user == null)
            {
                return base.GetFailureResult(-1, "incorrect parameters.");
            }
            if (String.IsNullOrWhiteSpace(user.Account))
            {
                return base.GetFailureResult(-1, $"incorrect parameters. ({nameof(user.Account)})");
            }
            if (String.IsNullOrWhiteSpace(user.Password))
            {
                return base.GetFailureResult(-1, $"incorrect parameters. ({nameof(user.Password)})");
            }


            try
            {
                var factory = this.GetIdentityFactory();
                var manager = factory.CreateUserManager();
                user = this.AntiXSSEncode(user);

                YAEP.Interfaces.IActionResult<bool> result = null;

                user.Description = user.Password;
                user.Password = Utilities.Utility.MD5Encrypt(user.Password);

                if (user is AddUserWithGroupsRequestModel)
                {
                    result = manager.CreateUser(user, (user as AddUserWithGroupsRequestModel)?.GroupUID ?? new Guid[] { });
                }
                else
                {
                    result = manager.CreateUser(user);
                }

                if (result?.Success ?? false)
                {
                    // refresh cache
                    DrKnowAll.RefreshUser(user.UID);

                    var apiResult = base.GetSuccessResult(user);
                    return base.Json(apiResult);
                }
                else
                {
                    return base.GetFailureResult(-1, result.Message);
                }
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }

        #region Factories

        private readonly Lazy<IdentityFactory> _IdentityFactory;

        private IdentityFactory GetIdentityFactory()
        {
            return this._IdentityFactory.Value;
        }

        #endregion
    }

}
