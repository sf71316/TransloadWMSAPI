using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.Interfaces;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Model;

namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// 車斗相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Vessel")]
    public class VesselController : AbstractApiController
    {
        /// <summary>
        /// 查詢Vessel 資料列表
        /// </summary>
        /// <param name="boluid">Bol UID</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetVesselList")]
        public IHttpActionResult GetVesselList(Guid boluid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            var _parameters = this.DIContainer.ManifestFactory.GenerateModel<IVesselSearchParameters>();
            _parameters.BolUID = boluid;
            var rs = _instance.GetVesselList(_parameters);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IEnumerable<IVesselModel>>(rs.Content.ToList());
                return this.Json<APIResult<IEnumerable<IVesselModel>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 查詢Vessel Item 新增資料列表
        /// </summary>
        /// <param name="uid">Bol UID</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetAddItemList")]
        public IHttpActionResult GetAddItemList([FromUri]GetAddItemListParameters parameter)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            this.AntiXSSEncode(parameter);
            if (parameter.manifestuid.HasValue || parameter.vesseluid.HasValue)
            {
                var rs = _instance.GetAddItemList(parameter);
                if (rs.Success)
                {

                    var result = this.GetSuccessResult<IEnumerable<IVesselAddItemListVewModel>>(rs.Content.ToList());
                    return this.Json<APIResult<IEnumerable<IVesselAddItemListVewModel>>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, "must have manifest uid or vessel uid");
            }
        }

        /// <summary>
        /// 查詢Vessel assign Item 資料列表
        /// </summary>
        /// <param name="vuid">Vessel UID</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetUnAssignedList")]
        public IHttpActionResult GetUnAssignedList(Guid vuid)
        {
            InitDIRoot();
            VesselManifestSearchParameters _parameters = new VesselManifestSearchParameters();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            _parameters.VesselUID = vuid;
            var rs = _instance.GetUnAssignedList(_parameters);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IEnumerable<IUnAssignedListViewModel>>(rs.Content.ToList());
                return this.Json<APIResult<IEnumerable<IUnAssignedListViewModel>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetOutboundUnAssignedList")]
        public IHttpActionResult GetOutboundUnAssignedList(Guid vuid)
        {
            InitDIRoot();
            VesselManifestSearchParameters _parameters = new VesselManifestSearchParameters();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            _parameters.VesselUID = vuid;
            var rs = _instance.GetOutboundUnAssignedList(vuid);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IEnumerable<dynamic>>(rs.Content.ToList());
                return this.Json<APIResult<IEnumerable<dynamic>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("GetAvailableInventoryList")]
        public IHttpActionResult GetAvailableInventoryList(IGetLocationItemListRequest request)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            if (request.ItemUID == null)
            {
                return this.GetFailureResult(-1, Resource.MANIFEST_MUST_HAVE_ITEM);
            }
            if (request.PackageUID == Guid.Empty)
            {
                return this.GetFailureResult(-1, Resource.MANIFEST_MUST_HAVE_PACKAGE);
            }
            var rs = _instance.GetAvailableInventoryList(request);
            if (rs.Success)
            {

                var result = this.GetSuccessResult<IEnumerable<dynamic>>(rs.Content.ToList());
                result.Message = rs.Message;
                return this.Json<APIResult<IEnumerable<dynamic>>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 新增 Vessel
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddVessel")]
        public IHttpActionResult AddVessel(VesselRequestModel Model)
        {
            InitDIRoot();
            this.AntiXSSEncode(Model);
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            var _model = this.DIContainer.ManifestFactory.GenerateModel<IVesselModel>();
            _model.BolUID = Model.BolUID;
            _model.Status = (int)VesselStatus.Draft;
            _model.Type = 1;
            _model.RefNo = Model.RefNo;
            _model.Name = Model.Name;
            _model.Description = Model.Description;
            if (!Model.UID.HasValue || (Model.UID.HasValue && Model.UID == Guid.Empty))
                _model.UID = Guid.NewGuid();
            else
                _model.UID = Model.UID.Value;

            var rs = _instance.AddVessel(_model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 編輯 Vessel
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("EditVessel")]
        public IHttpActionResult EditVessel(VesselRequestModel Model)
        {
            InitDIRoot();
            this.AntiXSSEncode(Model);
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            if (Model.UID.HasValue)
            {
                dynamic _model = new ExpandoObject();
                _model.UID = Model.UID.Value;
                _model.RefNo = Model.RefNo;
                _model.Name = Model.Name;
                _model.Description = Model.Description;
                var rs = _instance.EditVessel(_model);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, "Vessel UID can not empty.");
            }
        }
        /// <summary>
        /// 刪除 Vessel
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteVessel")]
        public IHttpActionResult DeleteVessel([FromUri]Guid[] uid)
        {
            InitDIRoot();
            if (uid != null)
            {
                var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
                var _parameters = new VesselDeleteParamters();
                _parameters.UID = uid;
                var rs = _instance.DeleteVesselAPI(_parameters);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.BadRequest("Guid is null.");
            }
        }
        /// <summary>
        /// 新增 VesselManifest
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("AddVesselManifestItem")]
        public IHttpActionResult AddVesselManifestItem(VesselManifestRequestModel Model)
        {
            InitDIRoot();
            Model = this.AntiXSSEncode(Model);
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            var _model = this.DIContainer.ManifestFactory.GenerateModel<IVesselManifestModel>();
            if (Model.UID.HasValue)
                _model.UID = Model.UID.Value;
            else
                _model.UID = Guid.NewGuid();
            _model.Status = (int)VesselStatus.Draft;
            _model.Type = 1;
            _model.ManifestItemUID = Model.ManifestItemUID;
            _model.ItemUID = Model.ItemUID;
            _model.Volume = Model.Volume;
            _model.Weight = Model.Weight;
            _model.PackageUID = Model.ReceivePackage;
            _model.VesselUID = Model.VesselUID;
            _model.Qty = Model.ReceiveQty;
            _model.RefNo = Model.RefNo;
            _model.Name = Model.Name;
            var rs = _instance.AddVesselManifest(_model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, rs.Message);
            }
        }
        /// <summary>
        /// 批次新增 VesselManifest
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("BatchAddVesselManifestItem")]
        public IHttpActionResult BatchAddVesselManifestItem([FromBody]VesselManifestRequestModel[] Model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
            List<IActionResult<bool>> _result = new List<IActionResult<bool>>();
            this.AntiXSSEncode(Model);
            foreach (var item in Model)
            {
                if (!item.UID.HasValue)
                {
                    //this.AntiXSSEncode(item);

                    var _model = this.DIContainer.ManifestFactory.GenerateModel<IVesselManifestModel>();
                    if (item.UID.HasValue)
                        _model.UID = item.UID.Value;
                    else
                        _model.UID = Guid.NewGuid();
                    _model.Status = (int)VesselStatus.Draft;
                    _model.Type = 1;
                    _model.ManifestItemUID = item.ManifestItemUID;
                    _model.ItemUID = item.ItemUID;
                    _model.Volume = item.Volume;
                    _model.Weight = item.Weight;
                    _model.PackageUID = item.ReceivePackage;
                    _model.VesselUID = item.VesselUID;
                    _model.Qty = item.ReceiveQty;
                    //_model.RefNo = item.RefNo;
                    _model.Name = item.Name;
                    var rs = _instance.AddVesselManifest(_model);
                    _result.Add(rs);
                }
            }

            if (Model.Count() > 0)
            {
                if (_result.All(r => r.Success))
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, string.Join("\r\n", _result.Where(p => !p.Success).Select(x => x.Message)));
                }
            }
            else
            {
                return this.GetFailureResult(-1, string.Join("\r\n", "not find data."));
            }
        }
        /// <summary>
        /// 刪除 VesselManifest
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteVesselManifestItem")]
        public IHttpActionResult DeleteVesselManifestItem([FromUri]Guid[] uid)
        {
            InitDIRoot();
            if (uid != null)
            {
                var _instance = this.DIContainer.ManifestFactory.CreateManger().VesselManager;
                var _model = new VesselManifestDeleteParameters();
                _model.UID = uid;
                var rs = _instance.DeleteVesselManifestFromUI(_model);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    return this.GetFailureResult(-1, rs.Message);
                }
            }
            else
            {
                return this.GetFailureResult(-1, "not find delete uid");
            }
        }
    }
}
