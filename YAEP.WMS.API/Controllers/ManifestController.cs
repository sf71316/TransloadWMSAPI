using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request; 
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// 清冊相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Manifest")]
    public class ManifestController : AbstractApiController
    {
        /// <summary>
        /// 取得預設目錄夾名稱
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetDefaultFolderName")]
        public IHttpActionResult GetDefaultFolderName(Guid btu, int btp)
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().ManifestManager;
            IActionResult<string> _info = _instance.GetDefaultFolderName(btu, btp);
            if (_info.Success)
            {
                var result = this.GetSuccessResult(_info.Content);
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, _info.Message);
                return result;
            }
        }
        /// <summary>
        /// 查詢Manifest 資料列表
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetManifestList")]
        public IHttpActionResult GetManifestList(ManifestSearchParameters parameters)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            InitDIRoot();
            Debug.WriteLine($"Init di container elapsed:{sw.ElapsedMilliseconds}ms");
            sw.Restart();
            parameters = this.AntiXSSEncode(parameters);
            Debug.WriteLine($"AntiXSS processed elapsed:{sw.ElapsedMilliseconds}ms");
            sw.Restart();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var rs = _instance.GetManifestList<ManifestListViewModel>(parameters);
            Debug.WriteLine($"data processed elapsed:{sw.ElapsedMilliseconds}ms");
            sw.Restart();
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IEnumerable<ManifestListViewModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<ManifestListViewModel>>>(result);
                // return this.JilJson(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
        /// <summary>
        /// 取得Manifest Type 清單
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetManifestTypeList")]
        public IHttpActionResult GetManifestTypeList()
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var rs = _instance.GetManifestTypeList();
            var result = this.GetSuccessResult<List<IEnumFieldInfo>>(rs.ToList());
            return this.Json<APIResult<List<IEnumFieldInfo>>>(result);
        }
        /// <summary>
        /// 取得Manifest Info 資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetManifestInfo")]
        public IHttpActionResult GetManifestInfo(Guid muid)
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var _info = _instance.GetManifestInfo(muid);
            if (_info.Success)
            {
                var result = this.GetSuccessResult<IManifestModel>(_info.Content);
                return this.Json<APIResult<IManifestModel>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, _info.Message);
                return result;
            }
        }

        /// <summary>
        /// 刪除Manifest
        /// </summary>
        /// <param name="muid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteManifest")]
        public IHttpActionResult DeleteManifest([FromUri]Guid[] uid)
        {
            InitDIRoot();
            if (uid != null)
            {
                var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
                var _parameters = this.DIContainer.ManifestFactory.GenerateModel<IManifestDeleteParameters>();
                _parameters.UID = uid;
                var rs = _instance.DeleteManifest(_parameters);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
            else
            {
                return this.BadRequest("Guid is null.");
            }

        }

        /// <summary>
        /// 新增 Manifest
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddManifest")]

        public IHttpActionResult AddManifest(ManinfestRequestModel Model)
        {
            InitDIRoot();
            Model = this.AntiXSSEncode(Model);
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var _model = this.DIContainer.ManifestFactory.GenerateModel<IManifestModel>();
            _model.PartyUID = Model.PartyUID;
            _model.RefNo = Model.RefNo;
            _model.Status = ManifestStatus.Draft;
            _model.Type = Model.Type;
            _model.Volume = Model.Volume;
            _model.WarehouseUID = Model.WarehouseUID;
            _model.Weight = Model.Weight;
            if (Model.PartyUID == Guid.Empty)
            {
                var result = this.GetFailureResult(-1, "Party UID is empty");
                return result;
            }
            else if (Model.WarehouseUID == Guid.Empty)
            {
                var result = this.GetFailureResult(-1, "Warehouse UID is empty");
                return result;
            }
            else
            {
                var rs = _instance.AddManifest(_model);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult<IManifestModel>(rs.Content);
                    return this.Json<APIResult<IManifestModel>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }

        }
        /// <summary>
        ///  編輯 Manifest
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("EditManifest")]
        [ResponseType(typeof(ManinfestRequestModel))]
        public IHttpActionResult EditManifest(ManinfestRequestModel Model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            Model = this.AntiXSSEncode(Model);
            dynamic _model = new ExpandoObject();
            _model.UID = Model.UID;
            _model.Name = Model.Name;
            _model.RefNo = Model.RefNo;
            _model.WarehouseUID = Model.WarehouseUID;
            //TODO 檢查資料是否可以切換Warehouse
            if (Model.Volume.HasValue)
                _model.Volume = Model.Volume;
            if (Model.Weight.HasValue)
                _model.Weight = Model.Weight;
            var rs = _instance.EditManifest(_model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }


        }

        /// <summary>
        /// 取得Manifest Item List 資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetManifestItemList")]
        public IHttpActionResult GetManifestItemList(Guid muid)
        {
            InitDIRoot();
            var _instance = DIContainer.ManifestFactory.CreateManger().ManifestManager;

            var _info = _instance.GetManifestItemList(muid);
            if (_info.Success)
            {
                var result = this.GetSuccessResult<IEnumerable<IManifestItemListModel>>(_info.Content);
                return this.Json<APIResult<IEnumerable<IManifestItemListModel>>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, _info.Message);
                return result;
            }
        }
        /// <summary>
        /// 刪除 Manifest Item
        /// </summary>
        /// <param name="miuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteManinfestItem")]
        public IHttpActionResult DeleteManinfestItem([FromUri]Guid[] uid)
        {
            if (uid != null && uid.Any(p => p != Guid.Empty))
            {
                InitDIRoot();
                var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
                var _parameters = new ManifestItemListDeleteParameters();
                _parameters.UID = uid;
                var rs = _instance.DeleteManifestItem(_parameters);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, "not find manifest item uid");
                return result;
            }

        }
        /// <summary>
        /// 新增 Manifest Item array
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddManinfestItem")]

        public IHttpActionResult AddManinfestItem([FromBody]ManinfestItemRequestModel[] Model)
        {
            InitDIRoot();
            Model = this.AntiXSSEncode(Model);
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var _model = new List<IManifestItemListModel>();
            if (Model != null && Model.Count() > 0)
            {
                foreach (var item in Model)
                {
                    var _m = this.DIContainer.ManifestFactory.GenerateModel<IManifestItemListModel>();
                    _m.ManifestUID = item.ManifestUID;
                    _m.ItemUID = item.ItemUID;
                    _m.PackageUID = item.PackageUID;
                    _m.PackageQty = item.PackageQty;
                    if (item.UID.HasValue)
                    {
                        _m.UID = item.UID.Value;
                    }
                    _m.Type = 1;
                    _m.Status = ManifestItemListStatus.Draft;
                    _model.Add(_m);
                }

                var rs = _instance.AddManifestItems(_model);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, "not find request data.");
                return result;
            }


        }
        /// <summary>
        /// 查詢 Vessel Manifest Item 資料列表
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetVesselManifestItemList")]
        public IHttpActionResult GetVesselManifestItemList(Guid? VesselUID)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            VesselManifestSearchParameters parameters = new VesselManifestSearchParameters();
            if (this.IsLegalGuid(VesselUID))
            {
                parameters.VesselUID = VesselUID;
                var rs = _instance.GetVesselManifestItemList(parameters);
                if (rs.Success)
                {

                    var result = this.GetSuccessResult<IEnumerable<IVesselManifestItemListViewModel>>(rs.Content);
                    return this.Json<APIResult<IEnumerable<IVesselManifestItemListViewModel>>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, rs.Message);
                    return result;
                }
            }
            else
            {
                var result = this.GetFailureResult(-1, "VesselUID is illegal");
                return result;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warhouseUID"></param>
        /// <param name="itemUID"></param>
        /// <param name="packageUID"></param>
        /// <param name="Qty"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("CheckOnhand")]
        public IHttpActionResult CheckOnhand(Guid warhouseUID, Guid itemUID, Guid packageUID, int Qty)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;

            IActionResult<bool> rs = _instance.CheckOnhand(warhouseUID, itemUID, packageUID, Qty);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }


        }
        /// <summary>
        /// Submit Manifest
        /// </summary>
        /// <param name="manifestuid"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SubmitManifest")]
        public IHttpActionResult SubmitManifest(Guid manifestuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var rs = _instance.SubmitManifest(manifestuid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IManifestModel>(rs.Content);
                result.Message = rs.Message;
                return this.Json<APIResult<IManifestModel>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="manifestuid"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("RejectManifest")]
        public IHttpActionResult RejectManifest(Guid manifestuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var rs = _instance.RejectManifest(manifestuid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IManifestModel>(rs.Content);
                result.Message = rs.Message;
                return this.Json<APIResult<IManifestModel>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
        /// <summary>
        /// 取得Ship via 運送形式列表
        /// </summary>
        /// <param name="partyuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetShipMethodList")]
        public IHttpActionResult GetShipMethodList(Guid? partyuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().ManifestManager;
            var rs = _instance.GetShipMethodList(partyuid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IEnumerable<IShipMethodModel>>(rs.Content);
                return this.Json<APIResult<IEnumerable<IShipMethodModel>>>(result);
            }
            else
            {
                var result = this.GetFailureResult(-1, rs.Message);
                return result;
            }
        }
        /// <summary>
        /// Check if item is future allocated
        /// </summary>
        /// <param name="itemUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("CheckItemIsFutureAllocated")]
        public IHttpActionResult CheckItemIsFutureAllocated(Guid itemUID)
        {
            try
            {
                InitDIRoot();
                var manager = DIContainer.ManifestFactory.CreateManger().WorkOrderManager;
                var _payloads = manager.GetWorkOrderPayload(new { ItemUID = itemUID, Type = ((int)PayloadType.FutureAllocated) });

                if (_payloads != null && _payloads.Content != null && _payloads.Content.Count() > 0)
                {
                    var apiResult = this.GetSuccessResult<dynamic>(_payloads.Content);
                    if (!_payloads.Success)
                    {
                        apiResult.IsComplete = _payloads.Success;
                        apiResult.Message = _payloads.Message;
                    }
                    return this.Json(apiResult);
                }

                return GetFailureResult(-1, "Item is not in the FutureAllocated list");
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
        /// <summary>
        /// Do Replenishment
        /// </summary>
        ///  <param name="fillList"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("FillFutureAllocated")]
        public IHttpActionResult FillFutureAllocated(IEnumerable<ReplenishmentModel> fillList)
        {
            InitDIRoot();
            var manager = DIContainer.ManifestFactory.CreateManger().ManifestManager;
            manager.TracingAgent.BeginTracing("FillFutureAllocated", fillList);
            var result = manager.SetFillFutureAllocated(fillList);
            manager.TracingAgent.EndTracing(result);

            if (result != null)
            {
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }

            return base.GetDataNotFoundResult();
        }
        /// <summary>
        /// Do Replenishment
        /// </summary>
        ///  <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("NominateFillFutureAllocated")]
        public IHttpActionResult NominateFillFutureAllocated(NominateReplenishmentModel Model)
        {
            InitDIRoot();
            var manager = DIContainer.ManifestFactory.CreateManger().ManifestManager;
            manager.TracingAgent.BeginTracing("NominateFillFutureAllocated", Model);
            var result = manager.SetNominateFillFutureAllocated(Model);
            manager.TracingAgent.EndTracing(result);

            if (result != null)
            {
                var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                apiResult.Message = result.Message;
                return this.Json(apiResult);
            }

            return base.GetDataNotFoundResult();
        }
        /// <summary>
        /// Sync Replenishment to Varys
        /// </summary>
        /// <param name="WarehouseUID"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("SyncReplenishment")]
        public IHttpActionResult SyncReplenishment(IEnumerable<ReplenishmentModel> Data)
        {
            try
            {
                InitDIRoot();
                var manager = DIContainer.ManifestFactory.CreateManger().ManifestManager;
                var result = manager.CheckReplenishmentSync(Data);
                if (result.Success)
                {
                    var apiResult = this.GetSuccessResult<dynamic>(result.Content);
                    apiResult.Message = result.Message;
                    return this.Json(apiResult);
                }

                return base.GetDataNotFoundResult();
            }
            catch (Exception ex)
            {
                return base.GetFailureResult(-1, ex.Message);
            }
        }
    }
}
