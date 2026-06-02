using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.WMS.Api.Code;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.API.Models.Response;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Model;
using YAEP.WMS.Cache.Redis;

namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// Bulk Pick API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/BulkPick")]
    public class BulkPickController : AbstractApiController
    {
        /// <summary>
        /// 查詢  Bulk Pick 資料列表
        /// </summary>
        /// <param name="requestModel">The model of search parameters.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetBulkPickList")]
        public IHttpActionResult GetBulkPickList([FromBody]BulkPickSearchRequestModel requestModel)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().BulkPickManager;
            var parameters = this.DIContainer.ManifestFactory.GenerateModel<IBulkPickSearchParameters>();

            parameters.ID = requestModel.BulkPickNo;
            parameters.Name = requestModel.Name;
            parameters.PartyName = requestModel.Customer;
            parameters.CustomerPartyName = requestModel.CustomerPartyName;

            if ((requestModel.Status?.Length ?? 0) > 0)
            {
                parameters.Status.AddRange(requestModel.Status);
            }


            var result = manager.GetBulkPickList(parameters);
            if (result.Success)
            {
                var collection = result.Content?.Select(o =>
                {
                    string assignedBy = "";

                    var responseModel = new BulkPickResponseModel()
                    {
                        BulkPickUID = o.UID,
                        BulkPickNo = o.ID,
                        CustomerName = o.PartyName,
                        Status = o.Status,
                        StatusName = (YAEP.Utilities.EnumerableData.Parse<BulkPickStatus>(o.Status).ToString()),
                        AssignedBy = assignedBy,
                        AssignedTime = null,
                        TicketID=o.TicketNo,
                        TicketUID= o.TicketUID,                        
                    };

                    return responseModel;
                });

                var apiResult = this.GetSuccessResult(collection);
                return this.Json(apiResult);
            }
            else
            {
                var apiResult = this.GetFailureResult(-1, result.Message);
                return apiResult;
            }

        }

        /// <summary>
        /// 刪除  Bulk Pick 資料列表
        /// </summary>
        /// <param name="uid"> </param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("Delete")]
        public IHttpActionResult Delete([FromUri]Guid[] uid)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().BulkPickManager;
 

            var result = manager.DeleteBulkPick(uid);
            if (result.Success)
            {
                var apiResult = this.GetSuccessResult();
                return this.Json(apiResult);
            }
            else
            {
                var apiResult = this.GetFailureResult(-1, result.Message);
                return apiResult;
            }

        }


        /// <summary>
        /// 儲存BulkPick資料
        /// </summary>
        /// <param name="requestModel">request data</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Save")]
        public IHttpActionResult Save([FromBody]BulkPickSaveModel requestModel)
        {
            if ((requestModel.TicketInfoUID?.Count() ?? 0) == 0)
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(requestModel.TicketInfoUID)})");
            }
            if (String.IsNullOrWhiteSpace(requestModel.CustomerName))
            {
                return base.GetFailureResult(-1, $"{Resource.COMMON_INCORRECT_PARAMETERS}({nameof(requestModel.CustomerName)})");
            }

            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().BulkPickManager;

            var result = manager.SaveBulkPick(requestModel.TicketInfoUID, requestModel.CustomerName);

            if (result.Success)
            {
                var apiResult = this.GetSuccessResult();
                return this.Json(apiResult);
            }
            else
            {
                string message = result.Message;

                var apiResult = this.GetFailureResult(-1, message);
                return apiResult;
            }

        }

        /// <summary>
        /// 批次附加Worker
        /// </summary>
        /// <param name="requestModel">request data</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("BatchAddWorker")]
        public IHttpActionResult BatchAddWorker([FromBody]BulkPickBathAddWorkModel requestModel)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().BulkPickManager;

            var result = manager.BatchAddWorker(bulkPickUID: requestModel.BulkPickUID,
                                                                groupUID: requestModel.GroupUID);
            if (result.Success)
            {
                var apiResult = this.GetSuccessResult();
                return this.Json(apiResult);
            }
            else
            {
                string message = result.Message;

                if (result.InnerException is System.Data.SqlClient.SqlException)
                {
                    if ((result.InnerException as System.Data.SqlClient.SqlException)?.Number == 2627)
                    {
                        message = "Some workers are repeatedly assigned.";
                    }
                }

                var apiResult = this.GetFailureResult(-1, message);
                return apiResult;
            }

        }


        /// <summary>
        /// Manifest 選取列表
        /// </summary>
        /// <param name="requestModel">The model of search parameters.</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetManifestList")]
        public IHttpActionResult GetManifestList([FromBody]BulkPickManifestSearchRequestModel requestModel)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().BulkPickManager;
            var parameters = this.DIContainer.ManifestFactory.GenerateModel<IBulkPickManifestSearchParameters>();

            if (requestModel == null)
            {
                return this.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            // customer condition
            var customer = DrKnowAll.GetCustomer().FirstOrDefault(o => o.Name.Equals(requestModel.Customer, StringComparison.OrdinalIgnoreCase));
            parameters.CustomerUID = customer?.UID ?? Guid.Empty;

            parameters.RefNo = requestModel.RefNo;
            parameters.Name = requestModel.Name;
            parameters.StartDate = requestModel.StartDate;
            parameters.EndDate = requestModel.EndDate;
            parameters.OptionText = requestModel.OptionText;
            parameters.OptionValue = requestModel.OptionValue;

            var result = manager.GetManifestList(parameters);
            if (result.Success)
            {
                var customers = DrKnowAll.GetCustomer().ToList();
                var products = DrKnowAll.GetProduct().ToList();
                var collection = result.Content?.Select(o =>
                {
                    var responseModel = new BulkPickManifestResponseModel()
                    {
                        TicketInfoUID = o.TicketInfoUID,
                        TicketInfoID = o.TicketInfoID,
                        ManifestUID = o.ManifestUID,
                        ManifestNo = o.ManifestNo,
                        ManifestItemListUID = o.ManifestItemListUID,
                        ManifestItemListID = o.ManifestItemListID,
                        RefNo = o.RefNo,
                        CustomerPartyName = o.CustomerPartyName,
                        CustomerName = customers.FirstOrDefault(c => c.UID == o.CustomerUID)?.Name,
                        ItemNo = products.FirstOrDefault(p => p.UID == o.ItemUID)?.ID,
                        EstQty = o.EstQty,
                        FromSlot = o.FromSlot,
                        ToSlot = o.ToSlot,
                        ShipVia = o.ShipVia,
                    };

                    return responseModel;
                });

                var apiResult = this.GetSuccessResult(collection);
                return this.Json(apiResult);
            }
            else
            {
                var apiResult = this.GetFailureResult(-1, result.Message);
                return apiResult;
            }
        }

        // Bulk Pick 構成
        /// <summary>
        /// BulkPick Info  
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetInfo")]
        public IHttpActionResult GetInfo([FromUri]Guid bulkPickUID)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().BulkPickManager;

            if (bulkPickUID == Guid.Empty)
            {
                return this.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            var result = manager.GetBulkPickInfoList(bulkPickUID);
            if (result.Success)
            {
                var products = DrKnowAll.GetProduct();
                var collection = result.Content?.Select(o =>
                {
                    var responseModel = new BulkPickInfoResponseModel()
                    {
                        BulkPickUID = o.BulkPickUID,
                        BulkPickID = o.BulkPickID,
                        PartyName = o.PartyName,
                        ItemNo = products.FirstOrDefault(p => p.UID == o.ItemUID)?.ID,
                        EstQty = o.EstQty ?? 0,
                        ActQty = o.ActQty ?? 0,
                        ShtQty = o.ShtQty ?? 0,
                        SavQty = o.SavQty ?? 0,
                        From = o.From,
                        To = o.To,
                        TicketInfoUID = o.TicketInfoUID,
                        TicketInfoRelationID = o.TicketInfoRelationID,
                    };

                    return responseModel;
                });

                var apiResult = this.GetSuccessResult(collection);
                return this.Json(apiResult);
            }
            else
            {
                var apiResult = this.GetFailureResult(-1, result.Message);
                return apiResult;
            }
        }

        // Bulk Pick 
        /// <summary>
        /// BulkPick Info  
        /// </summary>
        /// <param name="bulkPickUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetInfoView")]
        public IHttpActionResult GetInfoView([FromUri]Guid bulkPickUID)
        {
            InitDIRoot();
            var manager = this.DIContainer.ManifestFactory.CreateManger().BulkPickManager;

            if (bulkPickUID == Guid.Empty)
            {
                return this.GetFailureResult(-1, Resource.COMMON_INCORRECT_PARAMETERS);
            }

            var result = manager.GetBulkPickInfoViewList(bulkPickUID);
            if (result.Success)
            {
                var products = DrKnowAll.GetProduct();
                var collection = result.Content?.Select(o =>
                {
                    var responseModel = new BulkPickInfoViewResponseModel()
                    {
                        BulkPickUID = o.BulkPickUID,
                        ManifestNo = o.ManifestNo,
                        CustomerName = o.CustomerName,
                        TicketInfoUID = o.TicketInfoUID,
                        TicketInfoID = o.TicketInfoID,
                        RefNo = o.RefNo,
                        ItemNo = products.FirstOrDefault(p => p.UID == o.ItemUID)?.ID,
                        EstQty = o.EstQty ?? 0,
                        ActQty = o.ActQty ?? 0,
                        FromSlot = o.FromSlot,
                        ToSlot = o.ToSlot,
                        Shipvia = o.ShipViaUID.ToString(),
                    };

                    return responseModel;
                });

                var apiResult = this.GetSuccessResult(collection);
                return this.Json(apiResult);
            }
            else
            {
                var apiResult = this.GetFailureResult(-1, result.Message);
                return apiResult;
            }
        }

        // TODO: BulkPickController - GetBulkPickList - Assign By
         
        // TODO: BulkPickController- BatchAddWorker

    }
}
