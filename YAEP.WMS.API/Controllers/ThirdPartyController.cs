using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Identities.Constants;
using YAEP.Interfaces;
using YAEP.SSO.DI;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Cache.Redis;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;


namespace YAEP.WMS.API.Controllers
{
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    public class ThirdPartyController : AbstractApiController
    {
        [HttpGet]
        [ActionName("GetWarehouseList")]
        public IHttpActionResult GetWarehouseList()
        {
            InitDIRoot();
            using (var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager)
            {
                return this.Ok(_instance.GetThirdPartyWarehouseNameList());
            }
        }
        [HttpPost]
        [ActionName("GetProducts")]
        public IHttpActionResult GetProducts(ProductThirdPartySearchRequestModel orequestModel)
        {
            InitDIRoot();
            if (!orequestModel.CustomerUID.HasValue || (orequestModel.CustomerUID.HasValue && orequestModel.CustomerUID.Value == Guid.Empty))
            {
                return base.GetFailureResult(-1, "Must have CustomerUID");

            }

            var colletion = DrKnowAll.GetProduct().Where(o =>
            {
                bool b = true;

                var itemUIDs = (orequestModel?.ItemUID ?? new Guid[] { });
                if (itemUIDs?.Count() > 0)
                {
                    b = itemUIDs.Any(itemUID => itemUID == o.UID);
                }
                else
                {
                    var itemIDs = orequestModel.ItemIDs.Where(x => !string.IsNullOrEmpty(x));
                    //b = o.ID.StartsWith(requestModel.ItemID.Trim(), StringComparison.OrdinalIgnoreCase);
                    b = itemIDs.Any(x => o.ID.StartsWith(x, StringComparison.OrdinalIgnoreCase));
                    if (b)
                    {
                        if (orequestModel.CustomerUID.HasValue)
                        {
                            b = o.CustomerUID == orequestModel.CustomerUID.Value;
                        }
                    }

                }

                return b;
            });

            return this.Ok(colletion);
        }
    }
}