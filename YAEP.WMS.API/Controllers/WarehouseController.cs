using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API;
using YAEP.WMS.API.Code;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.API.Models.Response;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.DI.Agent;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;
using YAEP.WMS.Model;

namespace YAEP.WMS.Controllers.Api
{
    /// <summary>
    /// 倉庫、儲位相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Warehouse")]
    public class WarehouseController : AbstractApiController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wuid"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSlotNameAutoComplete")]
        public IHttpActionResult GetSlotNameAutocomplete(Guid wuid, string n)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;
            var _parameters = this.DIContainer.WarehouseFactory.GenerateModel<IWarehouseComponentParameters>();
            _parameters.Name = n.GetFilterXSSstring();
            _parameters.UnAssigned = true;
            _parameters.WarehouseUID = wuid;
            var rs = _instance.GetSlotNameList(_parameters);

            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json(result);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wuid"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetPodSelectList")]
        public IHttpActionResult GetPodSelectList([FromUri] Guid wuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            var rs = _instance.GetPodSelectList(wuid);

            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json(result);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wuid"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBinNameAutocomplete")]
        public IHttpActionResult GetBinNameAutocomplete(Guid wuid, string n)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;
            var _parameters = this.DIContainer.WarehouseFactory.GenerateModel<IWarehouseComponentParameters>();
            _parameters.Name = n.GetFilterXSSstring();
            _parameters.UnAssigned = true;
            _parameters.WarehouseUID = wuid;
            var rs = _instance.GetBinNameList(_parameters);

            if (rs.Success)
            {
                var result = this.GetSuccessResult(rs.Content);
                return this.Json(result);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }

        }

        #region Warehouse
        /// <summary>
        /// 取得 Location Info
        /// </summary>
        /// <param name="warehouseUID">
        /// warehouse
        /// <para /><see cref="ISlotModel.WarehouseUID"/>
        /// </param>
        /// <param name="areaUID">
        /// area
        /// <para /><see cref="ISlotModel.AreaUID"/>
        /// </param>
        /// <param name="binUID">
        /// bin
        /// <para /><see cref="ISlotModel.BinUID"/>
        /// </param>
        /// <param name="slotUID">
        /// slot
        /// <para /><see cref="ISlotModel.UID"/>
        /// </param>
        /// <returns></returns>
        [HttpGet]
        [CompressionAttribute]
        [ActionName("GetLocationInfo")]
        public IHttpActionResult GetLocationInfo(Guid warehouseUID, [FromUri] Guid? areaUID = null, [FromUri] Guid? binUID = null, [FromUri] Guid? slotUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;

            var result = manager.GetLocationInfoList(warehouseUID, areaUID, binUID, slotUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);

                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// 取得 Location Info
        /// </summary>
        /// <param name="warehouseUID">
        /// warehouse
        /// <para /><see cref="ISlotModel.WarehouseUID"/>
        /// </param>
        /// <param name="areaUID">
        /// area
        /// <para /><see cref="ISlotModel.AreaUID"/>
        /// </param>
        /// <param name="binUID">
        /// bin
        /// <para /><see cref="ISlotModel.BinUID"/>
        /// </param>
        /// <param name="slotUID">
        /// slot
        /// <para /><see cref="ISlotModel.UID"/>
        /// </param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetLocationInfoTree")]
        public IHttpActionResult GetLocationInfoTree(Guid warehouseUID, [FromUri] Guid? areaUID = null, [FromUri] Guid? binUID = null, [FromUri] Guid? slotUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;

            var result = manager.GetLocationInfoList(warehouseUID, areaUID, binUID, slotUID);

            if (result.Success)
            {
                var data = result.Content;

                var list = new List<object>();
                var groupBin = data.GroupBy(o => new { o.BinUID, o.BinID, o.BinName });
                foreach (var bin in groupBin)
                {
                    var slots = new List<object>();
                    foreach (var slot in bin)
                    {
                        if (slot.SlotUID == Guid.Empty)
                        {
                            continue;
                        }
                        slots.Add(new { slot.SlotUID, slot.SlotID, slot.SlotName, slot.Volume, slot.Weight });
                    }

                    list.Add(new
                    {
                        bin.Key.BinUID,
                        bin.Key.BinID,
                        bin.Key.BinName,
                        Volume = bin.Sum(o => o.Volume),
                        Weight = bin.Sum(o => o.Weight),
                        children = (slots?.Count() > 0 ? slots.ToArray() : null),
                    });
                }

                var actionResult = this.GetSuccessResult(list);

                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }

        /// <summary>
        /// 新增 Warehouse
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddWarehouse")]
        public IHttpActionResult AddWarehouse(WarehouseModel model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            model = this.AntiXSSEncode(model);
            var rs = _instance.AddWarehouse(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_ADD_WAREHOUSE_FAIL);
            }

        }
        /// <summary>
        /// 編輯 Area
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("EditWarehouse")]
        public IHttpActionResult EditWarehouse(WarehouseModel model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            var rs = _instance.EditWarehouse(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_UPDATE_WAREHOUSE_FAIL);
            }
        }
        /// <summary>
        /// 刪除Warehouse
        /// </summary>
        /// <param name="wuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("DeleteWarehouse")]
        public IHttpActionResult DeleteWarehouse(Guid[] wuid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            var parameter = this.DIContainer.WarehouseFactory.GenerateDeleteWarehouseParameters();
            parameter.UID = wuid;
            var rs = _instance.DeleteWarehouse(parameter);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();

                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_WAREHOUSE_FAIL);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetWarehouseList")]
        public IHttpActionResult GetWarehouseList()
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;

            var result = manager.GetWarehouseList();

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);

                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetInfo")]
        public IHttpActionResult GetInfo([FromUri] Guid warehouseUID)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;

            var result = manager.GetWarehouse(warehouseUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);

                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        #endregion
        #region Area

        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetAreaList")]
        public IHttpActionResult GetAreaList([FromUri] Guid? warehouseUID = null, [FromUri] Guid? areaUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().AreaManager;

            var result = manager.GetAreaList(warehouseUID, areaUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);

                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }

        /// <summary>
        /// 新增 Area
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddArea")]
        public IHttpActionResult AddArea(AreaModel model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().AreaManager;
            model = this.AntiXSSEncode(model);
            var rs = _instance.AddArea(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_ADD_AREA_FAIL);
            }

        }
        /// <summary>
        /// 編輯 Area
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("EditArea")]
        public IHttpActionResult EditArea(AreaModel model)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().AreaManager;
            var rs = _instance.EditArea(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_UPDATE_AREA_FAIL);
            }

        }
        /// <summary>
        /// 刪除 Area
        /// </summary>
        /// <param name="auid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteArea")]
        public IHttpActionResult DeleteArea(Guid[] auid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().AreaManager;
            var rs = _instance.DeleteArea(auid);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_AREA_FAIL);
            }

        }
        #endregion
        #region Bin
        /// <summary>
        /// 
        /// </summary>
        /// <param name="areaUID"></param>
        /// <param name="binUID"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("MappingBin")]
        public IHttpActionResult MappingBin([FromUri] Guid areaUID, [FromUri] Guid binUID)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;

            var result = manager.SetMappingToArea(areaUID, binUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }
            else
            {
                return this.GetFailureResult(-1, result.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <param name="areaUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetBinList")]
        public IHttpActionResult GetBinList([FromUri] Guid? warehouseUID = null, [FromUri] Guid? areaUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;

            var result = manager.GetBinList(warehouseUID, areaUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BinUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetBinInfo")]
        public IHttpActionResult GetBinInfo([FromUri] Guid? BinUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;

            var result = manager.GetList(new { UID = BinUID });

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content.FirstOrDefault());
                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// 新增 Area
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddBin")]

        public IHttpActionResult AddBin(BinModel model)
        {
            InitDIRoot();
            model = this.AntiXSSEncode(model);
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;
            var rs = _instance.AddBin(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_ADD_BIN_FAIL);
            }

        }
        /// <summary>
        /// 編輯 Bin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("EditBin")]
        public IHttpActionResult EditBin(BinModel model)
        {
            InitDIRoot();
            model = this.AntiXSSEncode(model);
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;
            var rs = _instance.EditBin(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_UPDATE_BIN_FAIL);
            }

        }
        /// <summary>
        /// 刪除 Bin
        /// </summary>
        /// <param name="buid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteBin")]
        public IHttpActionResult DeleteBin([FromUri] Guid[] buid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().BinManager;
            var rs = _instance.DeleteBin(buid);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_BIN_FAIL);
            }

        }
        #endregion
        #region Slot
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binUID"></param>
        /// <param name="slotUID"></param>
        /// <returns></returns>
        [HttpPut]
        [ActionName("MappingSlot")]
        public IHttpActionResult MappingSlot([FromUri] Guid? areaUID, [FromUri] Guid? binUID, [FromUri] Guid slotUID)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;

            var result = manager.SetMappingToBin(areaUID, slotUID, binUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }
            else
            {
                return this.GetFailureResult(-1, result.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <param name="areaUID"></param>
        /// <param name="binUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetSlotList")]
        public IHttpActionResult GetSlotList([FromUri] Guid? warehouseUID = null, [FromUri] Guid? areaUID = null, [FromUri] Guid? binUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;

            var result = manager.GetSlotList(warehouseUID, areaUID, binUID);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        [HttpGet]
        [ActionName("GetSearchSlotList")]
        public IHttpActionResult GetSearchSlotList([FromUri] string slotid, [FromUri] Guid? warehouseUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;

            var result = manager.GetSearchSlotList(warehouseUID, slotid);

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content);
                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// 新增 Slot
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddSlot")]
        public IHttpActionResult AddSlot(SlotModel model)
        {
            InitDIRoot();
            model = this.AntiXSSEncode(model);
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;
            var rs = _instance.AddSlot(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_ADD_SLOT_FAIL);
            }

        }
        /// <summary>
        /// 編輯 Slot
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("EditSlot")]
        public IHttpActionResult EditSlot(SlotModel model)
        {
            InitDIRoot();
            model = this.AntiXSSEncode(model);
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;
            var rs = _instance.EditSlot(model);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_UPDATE_SLOT_FAIL);
            }

        }
        /// <summary>
        /// 刪除 Slot
        /// </summary>
        /// <param name="suid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteSlot")]
        public IHttpActionResult DeleteSlot([FromUri] Guid[] suid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;
            var rs = _instance.DeleteSlot(suid);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_SLOT_FAIL);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SlotUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetSlotInfo")]
        public IHttpActionResult GetSlotInfo([FromUri] Guid? SlotUID = null)
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;

            var result = manager.GetList(new { UID = SlotUID });

            if (result.Success)
            {
                var actionResult = this.GetSuccessResult(result.Content.FirstOrDefault());
                return this.Json(actionResult);
            }
            else
            {
                return base.GetDataNotFoundResult();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("ClearSlotCache")]
        public IHttpActionResult ClearSlotCache()
        {
            base.InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger().SlotManager;
            manager.ClearSlotCache();
            return this.Ok();
        }
        #endregion
        #region Home Location
        /// <summary>
        /// 取Home location select Area 列表
        /// </summary>
        /// <param name="warehouseUID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomelocationAreaList")]
        public IHttpActionResult GetHomelocationAreaList(Guid warehouseUID, int type)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().HomeAddressRelationManager;
            var rs = _instance.GetHomeAddressAreaList(warehouseUID, type);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_HOMEADDR_FAILURE);
            }

        }
        /// <summary>
        /// 取Home location select Bin 列表
        /// </summary>
        /// <param name="areaUID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomelocationBinList")]
        public IHttpActionResult GetHomelocationBinList(Guid areaUID, int type)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().HomeAddressRelationManager;
            var rs = _instance.GetHomeAddressBinList(areaUID, type);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_HOMEADDR_FAILURE);
            }

        }
        /// <summary>
        /// 取Home location select Slot 列表
        /// </summary>
        /// <param name="binUID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomelocationSlotList")]
        public IHttpActionResult GetHomelocationSlotList(Guid binUID, int type)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().HomeAddressRelationManager;
            var rs = _instance.GetHomeAddressSlotList(binUID, type);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_HOMEADDR_FAILURE);
            }

        }
        /// <summary>
        /// 取得 Home location 列表
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetHomelocationList")]
        public IHttpActionResult GetHomelocationList(GetHomeAddressListParameters parameters)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().HomeAddressRelationManager;
            var rs = _instance.GetHomeAddressList(parameters);
            if (rs.Success)
            {
                var result = this.GetSuccessResult<dynamic>(rs.Content);
                return this.Json<APIResult<dynamic>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_HOMEADDR_FAILURE);
            }

        }
        /// <summary>
        /// 刪除 Home location 資料
        /// </summary>
        /// <param name="auid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteHomelocation")]
        public IHttpActionResult DeleteHomelocation(IEnumerable<Guid> auid)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().HomeAddressRelationManager;
            var rs = _instance.DeleteHomeAddress(auid);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_DELETE_HOMEADDR_FAILURE);
            }

        }
        /// <summary>
        ///  新增 Home location 列表
        /// </summary>
        /// <param name="insertModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddHomelocation")]
        public IHttpActionResult AddHomelocation(IEnumerable<HomeAddressRelationRequest> insertModel)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().HomeAddressRelationManager;
            var models = insertModel.Select(x => new HomeAddressRelationModel
            {
                Sequence = x.Sequence,
                SlotUID = x.SlotUID,
                UID = Guid.NewGuid(),
                ItemUID = x.ItemUID,
                OutboundType = x.OutboundType,
                Type = x.Type,
                Status = (int)HomeAddressRelationStatus.Active,
                CreatedOn = DateTime.Now.ToUniversalTime(),
                ModifiedOn = DateTime.Now.ToUniversalTime(),
                CreatedBy = this.GetAuthenticationInfo().Account
            });
            var rs = _instance.AddHomeAddress(models);
            if (rs.Success)
            {
                var result = this.GetSuccessResult();
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, Resource.WAREHOUSE_ADD_HOMEADDR_FAILURE);
            }

        }
        #endregion
        [HttpPost]
        [ActionName("GetSequence")]
        public IHttpActionResult GetSequence(GetSequenceRequest request)
        {
            InitDIRoot();
            var manager = this.DIContainer.WarehouseFactory.CreateWarehouseManger();
            var result = manager.WarehouseManager.GetSequence(request.BelongToUID, request.BelongToTag);
            var apiResult = this.GetSuccessResult<int>(result.Content);
            return this.Json(apiResult);
        }
    }
}
