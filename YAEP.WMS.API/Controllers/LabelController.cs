using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using YAEP.WMS.Api.Code;
using YAEP.WMS.Api.Models;
using YAEP.WMS.API.Models;
using YAEP.WMS.API.Models.Request;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Controllers.Api.Attributes;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Model;

namespace YAEP.WMS.API.Controllers
{
    /// <summary>
    /// 標籤相關存取資料API
    /// </summary>
    [EnableCors(origins: "*", headers: "Content-Type, Accept, Authorization", methods: "GET, POST, PUT, DELETE", SupportsCredentials = true)]
    [Authentication]
    [ConnectionLog]
    [RoutePrefix("api/Label")]
    public class LabelController : AbstractApiController
    {
        /// <summary>
        ///  取得 屬於某資料下的Barcode
        /// </summary>
        /// <param name="BelongToUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetBelongtoBarcode")]
        public IHttpActionResult GetBelongtoBarcode(Guid BelongToUID)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateLabelManager();
            var rs = _instance.GetBelongtoBarcode(new Guid[] { BelongToUID });
            var result = this.GetSuccessResult<IEnumerable<ITicketLabelViewModel>>(rs.Content);
            return this.Json<APIResult<IEnumerable<ITicketLabelViewModel>>>(result);

        }
        /// <summary>
        /// 取得 Barcode
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GeneratoreBarcode")]
        public IHttpActionResult GeneratoreBarcode(int t)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger().WarehouseManager;
            BarcodeType _type = BarcodeType.Pallet;
            if (Enum.TryParse<BarcodeType>(t.ToString(), out _type))
            {
                var rs = _instance.GenerateBarcode(_type);
                var result = this.GetSuccessResult(rs);

                return this.Json<APIResult<string>>(result);

            }
            else
            {
                var result = this.GetFailureResult(-1, "invalid barcode type.");
                return result;
            }
        }
        /// <summary>
        /// 批次取得 Barcode
        /// </summary>
        /// <param name="t">Barcode類別</param>
        /// <param name="c">要求Barcode數量</param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("BatchGeneratoreBarcode")]
        public IHttpActionResult BatchGeneratoreBarcode(int t, int c)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateWarehouseManger();
            BarcodeType _type = BarcodeType.Pallet;
            if (Enum.TryParse<BarcodeType>(t.ToString(), out _type) && t < 10)
            {
                List<string> barcode = new List<string>();
                Enumerable.Range(1, c).ToList().ForEach(p => barcode.Add(_instance.WarehouseManager.GenerateBarcode(_type)));
                var rs = barcode;
                var result = this.GetSuccessResult(rs);

                return this.Json<APIResult<List<string>>>(result);

            }
            else
            {
                var result = this.GetFailureResult(-1, "invalid barcode type.");
                return result;
            }
        }
        [HttpPost]
        [ActionName("AttachItemLabel")]
        public IHttpActionResult AttachItemLabel(AttachItemLabelRequest request)
        {
            InitDIRoot();
            try
            {

                var _instance = this.DIContainer.WarehouseFactory.CreateLabelManager();

                var rs = _instance.AttachItemLabelAPI(request.Payloaduid);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult();
                    return this.Json<APIResult<string>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, "attach label failure. " + rs.Message);
                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = this.GetFailureResult(-1, "load parameter error " + ex.Message);
                return result;
            }
        }
        /// <summary>
        /// 產生Label
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        [ActionName("GenerateLabel")]
        public IHttpActionResult GenerateLabel()
        {
            InitDIRoot();
            try
            {
                var Model = LoadParameter();
                if (HttpContext.Current.Request.Files.Count > 0)
                    Model.File = HttpContext.Current.Request.Files[0];
                var _instance = this.DIContainer.WarehouseFactory.CreateLabelManager();

                var rs = _instance.GenerateLabel(Model);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult(rs.Content);
                    return this.Json<APIResult<IEnumerable<ILabelGenerateViewModel>>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, "generate label failure. " + rs.Message);
                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = this.GetFailureResult(-1, "load parameter error " + ex.Message);
                return result;
            }
        }
        private GenerateLabelRequestModel LoadParameter()
        {
            GenerateLabelRequestModel model = new GenerateLabelRequestModel();
            var Form = HttpContext.Current.Request.Form;
            model.BarcodeMethod = (BarcodeMethod)Convert.ToInt32(Form["BarcodeMethod"]);
            model.BarcodeKind = (BarcodeKind)Convert.ToInt32(Form["BarcodeKind"]);
            //model.BarcodeType = (BarcodeType)Convert.ToInt32(Form["BarcodeType"]);
            model.BelongToUID = new Guid(Form["BelongToUID"]);
            model.BelongToType = Convert.ToInt32(Form["BelongtoType"]);
            model.GenerateQty = Convert.ToInt32(Form["GenerateQty"]);
            return model;
        }
        /// <summary>
        /// 產生Slot Label
        /// </summary>
        /// <param name="barCode"></param>
        /// <param name="LabelText"></param>
        /// <param name="barcodeType"></param>
        /// <param name="labelType"></param>
        /// <param name="labelBelongType"></param>
        /// <param name="belongToUID"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GenerateSlotLabel")]
        public IHttpActionResult GenerateSlotLabel(string barCode, string LabelText, BarcodeType barcodeType, LabelType labelType
            , LabelBelongType labelBelongType, Guid belongToUID)
        {
            InitDIRoot();
            try
            {
                var _instance = this.DIContainer.WarehouseFactory.CreateLabelManager();

                var rs = _instance.GenerateGeneralLabel(barCode, LabelText, barcodeType, labelType, labelBelongType, belongToUID);
                if (rs.Success)
                {
                    var result = this.GetSuccessResult(rs.Content);
                    return this.Json<APIResult<ILabelGenerateViewModel>>(result);
                }
                else
                {
                    var result = this.GetFailureResult(-1, "generate label failure. " + rs.Message);
                    return result;
                }
            }
            catch (Exception ex)
            {
                var result = this.GetFailureResult(-1, "load parameter error " + ex.Message);
                return result;
            }
        }
        /// <summary>
        /// 清除Label
        /// </summary>
        /// <param name="btu">Belong to guid array</param>
        /// <returns></returns>
        [HttpDelete]
        [ActionName("ClearLabel")]
        public IHttpActionResult ClearLabel([FromUri]Guid[] btu)
        {
            InitDIRoot();
            var _instance = this.DIContainer.WarehouseFactory.CreateLabelManager();
            var rs = _instance.DeleteLabel(btu);
            var result = this.GetSuccessResult(rs.Content.ToString());
            if (rs.Success)
            {
                return this.Json<APIResult<string>>(result);
            }
            else
            {
                return this.GetFailureResult(-1, "clear failure. " + result.Message);
            }

        }
    }
}
