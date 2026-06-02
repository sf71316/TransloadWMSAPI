using CsvHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Attachment.ClientAPI;
using YAEP.Constants;
using YAEP.Core.Item.Interfaces;
using YAEP.Core.Party.Interfaces;
using YAEP.Data.ORM.Interfaces;
using YAEP.Identities.Interfaces;
using YAEP.Interfaces;
using YAEP.Package.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Module;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Manager
{
    public class LabelManager : AbstractManager, ILabelManager
    {
        private readonly string UPLOAD_FOLDER = "Label";
        public LabelManager(IAuthenticationProvider authenticationInfoProvider,
            ILabelRepository labelRepository, IWarehouseManger warehouseManger, IPayloadRepository payloadRepository,
            IPackageManager packageManager, IPackageUomManager packageUomManager, IItemManager itemManager,
            IWorkOrderPayloadRepository workOrderPayloadRepository,
            IWorkOrderPodRepository workOrderPodRepository,
            ISequenceAgent sequenceAgent, IAppSettings appSettings
            , IGroupManager groupManager, IPartyManager partyManager, IObjectRelationalMappingLayer dbentities
            , Func<YAEP.Core.Item.Interfaces.IItemManager> itemmgmterfunc,
            IRefreshDrKnowAll refreshDKA, IItemRepository itemRepository,
            IPackageVersionRepository packageVersionRepository)
            : base(authenticationInfoProvider, sequenceAgent, appSettings, groupManager, packageManager,
                  packageUomManager, itemManager, partyManager, itemmgmterfunc, dbentities, refreshDKA, itemRepository,
                  packageVersionRepository)
        {
            this._Repository = labelRepository;
            this._WarehouseManger = warehouseManger;
            this.WorkOrderPayloadRepository = workOrderPayloadRepository;
            this.WorkOrderPodRepository = workOrderPodRepository;
            this.PackageManager = packageManager;
            this.PackageUomManager = packageUomManager;
            var labelAgentInitParameter = new LabelAgentInitParameter();
            //labelAgentInitParameter.ItemManager = this.ItemManager;
            labelAgentInitParameter.LabelManager = this;
            labelAgentInitParameter.PackageCacheManager = this.PackageCacheManager;
            labelAgentInitParameter.PackageUomManager = this.PackageUomManager;
            labelAgentInitParameter.ProductCacheManager = this.ProductCacheManager;
            this.LabelAgent = new LabelAgent(labelAgentInitParameter);
            this.PayloadRepository = payloadRepository;
        }
        private LabelAgent LabelAgent { get; set; }
        private IPayloadRepository PayloadRepository { get; set; }
        public IActionResult<bool> AddLabels(ILabelModel[] Models)
        {
            return this._Repository.AddLabelCollection(Models);
        }
        public IActionResult<bool> DeleteLabel(Guid[] belongtouid)
        {
            return this._Repository.DeleteLabel(belongtouid);
        }
        public IActionResult<bool> CloneLabel(Guid sourceBelongToUID, Guid TargetBelongToUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var sourceLabel = this._Repository.GetLabels(new Guid[] { sourceBelongToUID });
            if (sourceLabel.Success && sourceLabel.Content.Count() > 0)
            {
                var targetLabel = sourceLabel.Content.Where(p => p.Status == (int)LabelStatus.Active);
                foreach (var item in targetLabel)
                {
                    item.UID = Guid.NewGuid();
                    item.BelongToUID = TargetBelongToUID;
                    item.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                    item.CreatedOn = DateTime.Now;

                }
                this._Repository.AddLabelCollection(targetLabel.ToArray());
                rs.Success = true;
            }
            else
            {
                rs.Message = Resource.LABEL_NOT_FIND_LABEL;
                rs.Success = true;
            }
            return rs;
        }
        /// <summary>
        /// 複製並回傳指定belongto Label
        /// </summary>
        /// <param name="sourceBelongToUID"></param>
        /// <param name="TargetBelongToUID"></param>
        /// <returns></returns>
        public IActionResult<IEnumerable<ILabelModel>> ReturnCloneLabel(Guid sourceBelongToUID, Guid TargetBelongToUID)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ILabelModel>>();
            var sourceLabel = this._Repository.GetLabels(new Guid[] { sourceBelongToUID });
            if (sourceLabel.Success && sourceLabel.Content.Count() > 0)
            {
                var targetLabel = sourceLabel.Content.Where(p => p.Status == (int)LabelStatus.Active);
                foreach (var item in targetLabel)
                {
                    item.UID = Guid.NewGuid();
                    item.BelongToUID = TargetBelongToUID;
                    item.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                    item.CreatedOn = DateTime.Now;

                }
                this._Repository.AddLabelCollection(targetLabel.ToArray());
                rs.Content = targetLabel;
                rs.Success = true;
            }
            else
            {
                rs.Message = Resource.LABEL_NOT_FIND_LABEL;
                rs.Success = true;
            }

            return rs;
        }
        public IActionResult<IEnumerable<ILabelModel>> BatchReturnCloneLabel(IEnumerable<ICloneLabelModel> cloneLabelModels)
        {
            return this._Repository.BatchReturnCloneLabel(cloneLabelModels);
        }
        public IActionResult<ILabelGenerateViewModel> GenerateGeneralLabel(string barCode, string LabelText, BarcodeType barcodeType,
            LabelType labelType, LabelBelongType labelBelongType, Guid belongToUID)
        {
            dynamic expObj = new ExpandoObject();
            var rs = ActionResultTemplates.Result<ILabelGenerateViewModel>();
            try
            {
                this.DeleteLabel(new Guid[] { belongToUID });
                var api = new HttpClientAgent(
                                this.AuthProvider.GetAuthenticationInfo().Token);
                BarCodeModule barCodeModule = new BarCodeModule(this.AppConfigure);
                var barcode = barCodeModule.GetLabelImage(barcodeType, barCode, LabelText);
                expObj.Barcode = barcode;
                DataTable dt = ConvertToDataTable(expObj);
                var _bpdf = barCodeModule.GeneratoreLabelPdf(barcodeType, new DataTable[] { dt });
                var _fileName = $"{barCode}.pdf";
                var resultstring = api.UploadFile(new MemoryStream(_bpdf.First().Pdf), _fileName,
                                   belongToUID, (int)BelongToTypes.Label, null, null, UPLOAD_FOLDER,
                                   this.AuthProvider.GetAuthenticationInfo().Account);
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadResult>(resultstring);
                if (result.success)
                {
                    List<ILabelModel> _labelModels = new List<ILabelModel>();
                    var entity = new LabelInnerModel();
                    entity.UID = Guid.NewGuid();
                    entity.ID = "";
                    entity.Name = "";
                    entity.Type = labelType;
                    entity.FileUID = result.uids.First();
                    entity.BelongToType = labelBelongType;
                    entity.BelongToUID = belongToUID;
                    entity.Content = barCode;
                    entity.Description = "";
                    entity.Status = (int)LabelStatus.Active;
                    _labelModels.Add(entity);

                    var r = new LabelInnerViewModel();
                    r.Barcode = barCode;
                    r.BarcodeUID = entity.UID;
                    r.AttachmentUID = entity.FileUID;
                    r.FileUID = result.uids.First();
                    r.BarcodeType = (int)barcodeType;
                    r.BarcodeTypeName = barcodeType.ToString();
                    r.BelongToUID = belongToUID;
                    r.BelongToType = (int)labelBelongType;
                    r.Status = entity.Status;
                    r.StatusName = ((LabelStatus)r.Status).ToString();
                    var rs2 = this._Repository.AddLabelCollection(_labelModels.ToArray());
                    if (rs2.Success)
                    {
                        rs.Content = r;
                        rs.Success = true;
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = "Error";
                    }
                }
                else
                {
                    rs.Success = false;
                    rs.Message = "Error";
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        private DataTable ConvertToDataTable(dynamic expObj)
        {
            var dt = new DataTable();
            var obj = expObj as IDictionary<string, object>;
            foreach (var key in obj.Keys)
            {
                object d = null;
                obj.TryGetValue(key, out d);
                dt.Columns.Add(key, d.GetType());
            }
            foreach (var d in obj)
            {
                var dr = dt.NewRow();
                dr[d.Key] = d.Value;
                dt.Rows.Add(dr);
            }
            dt.AcceptChanges();
            return dt;
        }
        public IActionResult<IEnumerable<ITicketLabelViewModel>> GetBelongtoBarcode(Guid[] BelongtoUID)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<ITicketLabelViewModel>>();
            //try
            //{
            List<ITicketLabelViewModel> _result = new List<ITicketLabelViewModel>();
            var _collection = this._Repository.GetLabels(BelongtoUID);
            foreach (var item in _collection.Content)
            {
                TicketLabelInnerModel e = new TicketLabelInnerModel();
                e.AttachmentUID = item.FileUID;
                e.BarcodeType = (int)item.Type;
                e.BarcodeTypeName = item.Type.ToString();
                e.BelongToType = (int)item.BelongToType;
                e.BelongToUID = item.BelongToUID;
                e.Status = item.Status;
                e.StatusName = ((LabelStatus)item.Status).ToString();
                e.Barcode = item.Content;
                _result.Add(e);
            }
            rs.Content = _result;
            rs.Success = true;
            //}
            //catch (Exception ex)
            //{
            //    rs.Message = ex.Message;
            //    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
            //    rs.Success = false;
            //    rs.InnerException = ex;
            //}
            return rs;
        }
        public IActionResult<ILabelGenerateViewModel> GeneratePodLabel(IGeneratePalletLabelModel palletModel, Guid podUID)
        {

            var rs = ActionResultTemplates.Result<ILabelGenerateViewModel>();
            try
            {
                var api = new HttpClientAgent(
                                this.AuthProvider.GetAuthenticationInfo().Token);
                BarCodeModule barCodeModule = new BarCodeModule(this.AppConfigure);

                var barcode = "";
                if (!string.IsNullOrEmpty(palletModel.BarcodeContent))
                {
                    barcode = palletModel.BarcodeContent;
                }
                else
                {
                    barcode = this._WarehouseManger.GenerateBarcode(BarcodeType.Pallet);
                }
                palletModel.Barcode = barCodeModule.GetLabelImage(BarcodeType.Pallet, barcode);

                var _bpdf = barCodeModule.GeneratoreLabelPdf(BarcodeType.Pallet, new dynamic[] { palletModel });
                List<ILabelModel> _labelModels = new List<ILabelModel>();
                var entity = new LabelInnerModel();
                entity.UID = Guid.NewGuid();
                entity.ID = "";
                entity.Name = "";
                entity.Type = LabelType.Pallet_Self;
                entity.BelongToType = LabelBelongType.Pod;
                entity.BelongToUID = podUID;
                entity.Content = barcode;
                entity.Description = "";
                entity.Status = (int)LabelStatus.Active;
                _labelModels.Add(entity);
                var _fileName = $"{barcode}.pdf";
                var resultstring = api.UploadFile(new MemoryStream(_bpdf.First().Pdf), _fileName,
                                   podUID, (int)BelongToTypes.Label, null, null, UPLOAD_FOLDER,
                                   this.AuthProvider.GetAuthenticationInfo().Account);
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadResult>(resultstring);
                if (result.success)
                {
                    entity.FileUID = result.uids.First();
                    var r = new LabelInnerViewModel();
                    r.Barcode = barcode;
                    r.BarcodeUID = entity.UID;
                    r.AttachmentUID = entity.FileUID;
                    r.FileUID = entity.FileUID;
                    r.BarcodeType = (int)BarcodeType.Pallet;
                    r.BarcodeTypeName = BarcodeType.Pallet.ToString();
                    r.BelongToUID = podUID;
                    r.BelongToType = (int)LabelBelongType.Pod;
                    r.Status = entity.Status;
                    r.StatusName = ((LabelStatus)r.Status).ToString();
                    var rs2 = this._Repository.AddLabelCollection(_labelModels.ToArray());
                    if (rs2.Success)
                    {
                        rs.Content = r;
                        rs.Success = true;
                    }
                    else
                    {
                        rs.Success = false;
                        rs.Message = "Error";
                        this.Log($"Insert WMS_Label table failure response:{rs2.Message}", "Upload Label"
                            , this.AuthProvider.GetAuthenticationInfo().Account, Logger.ERROR, (int)BelongToTypes.Label);
                        //throw new Exception(rs2.Message);
                    }
                }
                else
                {
                    rs.Success = false;
                    rs.Message = "Error";
                    this.Log($"upload label failure HTTP response:{resultstring}", "Upload Label"
                            , this.AuthProvider.GetAuthenticationInfo().Account, Logger.ERROR, (int)BelongToTypes.Label);
                    //throw new Exception(resultstring);
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.Log($"upload label failure message:{ex.Message}  StackTrace:{ ex.StackTrace}", "Upload Label"
                           , this.AuthProvider.GetAuthenticationInfo().Account, Logger.ERROR, (int)BelongToTypes.Label);
                if (ex.InnerException != null)
                {
                    this.Log($"upload label failure message:{ex.InnerException.Message}  StackTrace:{ ex.InnerException.StackTrace}", "Upload Label"
                          , this.AuthProvider.GetAuthenticationInfo().Account, Logger.ERROR, (int)BelongToTypes.Label);
                }
                if (ex.InnerException.InnerException != null)
                {
                    this.Log($"upload label failure message:{ex.InnerException.InnerException.Message}  StackTrace:{ ex.InnerException.InnerException.StackTrace}", "Upload Label"
                          , this.AuthProvider.GetAuthenticationInfo().Account, Logger.ERROR, (int)BelongToTypes.Label);
                }
                //throw new Exception(ex.Message + ex.StackTrace);
            }
            return rs;
        }
        public IActionResult<bool> GenerateItemLabel(IEnumerable<ILabelModel> labels)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<ILabelModel> _labelModels = new List<ILabelModel>();
                _labelModels.AddRange(labels);
                rs = this._Repository.AddLabelCollection(_labelModels.ToArray());
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> GenerateItemLabel(IEnumerable<dynamic> itemsparam)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<ILabelModel> _labelModels = new List<ILabelModel>();
                foreach (var item in itemsparam)
                {
                    dynamic exobj = item as ExpandoObject;

                    var entity = new LabelInnerModel();
                    entity.UID = Guid.NewGuid();
                    entity.ID = "";
                    entity.Name = "";
                    entity.Type = exobj.labelType;
                    entity.BelongToType = LabelBelongType.Payload;
                    entity.BelongToUID = exobj.payloadUID;
                    entity.Content = exobj.barcode;
                    entity.Description = "";
                    entity.Status = (int)LabelStatus.Active;
                    _labelModels.Add(entity);
                }

                rs = this._Repository.AddLabelCollection(_labelModels.ToArray());
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> GenerateItemLabel(Guid payloadUID, string barcode, LabelType labelType)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<ILabelModel> _labelModels = new List<ILabelModel>();
                var entity = new LabelInnerModel();
                entity.UID = Guid.NewGuid();
                entity.ID = "";
                entity.Name = "";
                entity.Type = labelType;
                entity.BelongToType = LabelBelongType.Payload;
                entity.BelongToUID = payloadUID;
                entity.Content = barcode;
                entity.Description = "";
                entity.Status = (int)LabelStatus.Active;
                _labelModels.Add(entity);
                rs = this._Repository.AddLabelCollection(_labelModels.ToArray());
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<IEnumerable<ILabelGenerateViewModel>> GenerateLabel(IGenerateLabelRequest request)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ILabelGenerateViewModel>>();
            try
            {
                List<ILabelGenerateViewModel> Result = new List<ILabelGenerateViewModel>();
                List<BarCodeModule.Label> _bpdf = new List<BarCodeModule.Label>();
                if (request.BarcodeMethod == BarcodeMethod.CustomerBarcode && request.File == null)
                {
                    rs.Success = false;
                    rs.Message = Resource.LABEL_NOT_UPLOAD_FILE;

                }
                else
                {
                    var isFinddata = false;
                    var workorderpy = WorkOrderPayloadRepository.GetList(new { PayloadUID = request.BelongToUID });
                    BarcodeType barcodeType = BarcodeType.Pallet;
                    if (workorderpy.Success && workorderpy.Content.Count() > 0)
                    {
                        isFinddata = true;
                        var pkgInfo = PackageManager.GetPackage(workorderpy.Content.First().PackageUID).Content;
                        var uom = this.PackageUomManager.GetPackageUom(pkgInfo.UOM).Content;
                        barcodeType = ConvertBarcodeType(uom.Name);
                    }
                    else
                    {
                        var pod = WorkOrderPodRepository.GetWorkOrderPod(new { PODUID = request.BelongToUID });
                        if (pod.Success && pod != null)
                        {
                            isFinddata = true;
                            barcodeType = BarcodeType.Pallet;
                        }
                    }
                    if (isFinddata)
                    {

                        BarCodeModule barCodeModule = new BarCodeModule(this.AppConfigure);
                        if (request.BarcodeMethod == BarcodeMethod.NewBarcode)
                        {

                            // barcode method =1
                            //generatre barcode
                            //generate barcode file
                            _bpdf = Enumerable.Range(1, request.GenerateQty).Select(
                                p =>
                                {
                                    var content = this._WarehouseManger.GenerateBarcode(barcodeType);
                                    var barcode = barCodeModule.GetLabelImage(barcodeType, content);
                                    var pdf = barCodeModule.GeneratoreLabelPdf(barcodeType, new dynamic[] { barcode });
                                    return new BarCodeModule.Label
                                    {
                                        BarCode = content,
                                        Pdf = pdf.FirstOrDefault().Pdf
                                    };
                                }
                                ).ToList();


                            //_bpdf = barCodeModule.GeneratoreLabelPdf(barcodeType, _barcode);
                            List<ILabelModel> _labelModels = new List<ILabelModel>();

                            var api = new HttpClientAgent(
                                this.AuthProvider.GetAuthenticationInfo().Token);
                            foreach (var item in _bpdf)
                            {
                                //File.WriteAllBytes($"D:\\{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf", item.Pdf);
                                var r = new LabelInnerViewModel();
                                var entity = new LabelInnerModel();

                                var _fileName = $"{item.BarCode}.pdf";
                                var resultstring = api.UploadFile(new MemoryStream(item.Pdf), _fileName,
                                    request.BelongToUID, (int)BelongToTypes.Label, null, null, UPLOAD_FOLDER,
                                    this.AuthProvider.GetAuthenticationInfo().Account);
                                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<UploadResult>(resultstring);
                                if (result.uids != null)
                                {
                                    var deleteRs = this.DeleteLabel(new Guid[] { request.BelongToUID });
                                    if (deleteRs.Success)
                                    {
                                        entity.FileUID = result.uids.FirstOrDefault();
                                        //save data
                                        //var entity = this.GeneratoreObject();
                                        entity.UID = Guid.NewGuid();
                                        entity.ID = "";
                                        entity.Name = "";
                                        if (barcodeType == BarcodeType.Pallet)
                                            entity.Type = LabelType.Pallet_Self;
                                        else if (barcodeType == BarcodeType.Box)
                                            entity.Type = LabelType.Box_Self;
                                        else
                                            entity.Type = LabelType.Item_Self;
                                        entity.BelongToType = (LabelBelongType)request.BelongToType;
                                        entity.BelongToUID = request.BelongToUID;
                                        entity.Content = item.BarCode;
                                        entity.Description = "";
                                        entity.Status = (int)LabelStatus.Active;
                                        _labelModels.Add(entity);
                                        r.Barcode = item.BarCode;
                                        r.BarcodeUID = entity.UID;
                                        r.AttachmentUID = entity.FileUID;
                                        r.FileUID = entity.FileUID;
                                        r.BarcodeType = (int)barcodeType;
                                        r.BarcodeTypeName = barcodeType.ToString();
                                        r.BelongToUID = request.BelongToUID;
                                        r.BelongToType = request.BelongToType;
                                        r.Status = entity.Status;
                                        r.StatusName = ((LabelStatus)r.Status).ToString();
                                        Result.Add(r);
                                    }
                                    else
                                    {
                                        rs.Success = false;
                                        rs.Message = Resource.LABEL_CLEAR_FAILURE;
                                        foreach (var label in _labelModels)
                                        {
                                            string msg = "";
                                            api.DeleteAttachment(label.FileUID, out msg);
                                        }
                                        this.Log("upload label failure :" + result.errormessage, "error", "", "error",
                                       (int)BelongToTypes.Service);
                                    }
                                }
                                else
                                {
                                    //上傳不到Attachment Service
                                    //delete uploaded label
                                    foreach (var label in _labelModels)
                                    {
                                        string msg = "";
                                        api.DeleteAttachment(label.FileUID, out msg);
                                    }
                                    this.Log("upload label failure :" + result.errormessage, "error", "", "error",
                                        (int)BelongToTypes.Service);
                                    _labelModels.Clear();
                                    break;
                                }
                            }
                            if (_labelModels.Count > 0)
                            {
                                var rs2 = this._Repository.AddLabelCollection(_labelModels.ToArray());
                                if (rs2.Success)
                                {
                                    rs.Content = Result;
                                    rs.Success = true;
                                }
                                else
                                {
                                    rs.Success = false;
                                    rs.Message = rs2.Message;
                                }
                            }
                            else
                            {

                                rs.Message = Resource.LABEL_UPLOAD_FAILURE;
                                rs.Success = false;
                            }
                        }
                        else
                        {

                            //todo barcode method =2
                            using (var sr = new StreamReader(request.File.InputStream))
                            using (var reader = new CsvReader(sr))
                            {
                                //read file
                                // parse barcode
                                List<ILabelModel> _labelModels = new List<ILabelModel>();
                                var FileRecords = reader.GetRecords<FileRecord>();
                                //save data
                                foreach (var item in FileRecords)
                                {
                                    var r = new LabelInnerViewModel();
                                    var entity = new LabelInnerModel();
                                    entity.FileUID = Guid.Empty;
                                    entity.UID = Guid.NewGuid();
                                    entity.ID = "";
                                    entity.Name = "";
                                    if (barcodeType == BarcodeType.Pallet)
                                    {
                                        entity.Type = LabelType.Pallet_Other;
                                    }
                                    else if (barcodeType == BarcodeType.Box)
                                    {
                                        if (request.BarcodeKind == BarcodeKind.EAN)
                                            entity.Type = LabelType.Box_EAN;
                                        else
                                            entity.Type = LabelType.Box_UPC;
                                    }
                                    else
                                    {
                                        if (request.BarcodeKind == BarcodeKind.EAN)
                                            entity.Type = LabelType.Item_EAN;
                                        else
                                            entity.Type = LabelType.Item_UPC;
                                    }
                                    entity.BelongToType = (LabelBelongType)request.BelongToType;
                                    entity.BelongToUID = request.BelongToUID;
                                    entity.Content = item.barcode;
                                    entity.Description = "";
                                    entity.Status = (int)LabelStatus.Active;
                                    _labelModels.Add(entity);
                                    r.Barcode = entity.Content;
                                    r.Barcode = item.barcode;
                                    r.BarcodeType = (int)barcodeType;
                                    r.BarcodeTypeName = barcodeType.ToString();
                                    r.BelongToUID = request.BelongToUID;
                                    r.BelongToType = request.BelongToType;
                                    r.Status = entity.Status;
                                    r.StatusName = ((LabelStatus)r.Status).ToString();
                                    r.BarcodeUID = entity.UID;
                                    r.AttachmentUID = entity.FileUID;
                                    r.FileUID = entity.FileUID;
                                    Result.Add(r);
                                }
                                var deleteRs = this.DeleteLabel(new Guid[] { request.BelongToUID });
                                if (deleteRs.Success)
                                {
                                    var rs2 = this._Repository.AddLabelCollection(_labelModels.ToArray());
                                    if (rs2.Success)
                                    {
                                        rs.Content = Result;
                                        rs.Success = true;
                                    }
                                    else
                                    {
                                        rs.Success = false;
                                        rs.Message = rs2.Message;
                                    }
                                }
                                else
                                {
                                    rs.Success = false;
                                    rs.Message = Resource.LABEL_CLEAR_FAILURE;
                                }
                            }


                        }

                    }
                    else
                    {
                        rs.Message = Resource.COMMON_NOT_FIND_WORKORDER_PAYLOAD;
                    }
                }

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        private BarcodeType ConvertBarcodeType(string name)
        {
            if (name == "Pallet")
            {
                return BarcodeType.Pallet;
            }
            else if (name == "Box")
            {
                return BarcodeType.Box;
            }
            else
            {
                return BarcodeType.Item;
            }
        }
        public IActionResult<bool> ChangeLabelStatus(Guid[] barcodeUID, LabelStatus status)
        {
            return this._Repository.ChangeLabelStatus(barcodeUID, status);
        }
        /// <summary>
        /// Label 還原(Ticket 資料需存在)
        /// </summary>
        /// <param name="TicketUIDs"></param>
        /// <returns></returns>
        public IActionResult<bool> RollbackLabel(IEnumerable<Guid> TicketUIDs)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var labels = this._Repository.GetLabelsByTicket(TicketUIDs);
                if (labels.Content.Count() > 0)
                {
                    var rs1 = this.ChangeLabelStatus(labels.Content.Select(p => p.UID).ToArray(), LabelStatus.Active);
                    rs.Success = rs1.Success;
                    rs.Content = rs1.Content;
                }
                else
                {
                    rs.Success = true;
                    rs.Content = false;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public IActionResult<bool> ClearLabelByTickets(IEnumerable<Guid> TicketUIDs)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var labels = this._Repository.GetLabelsByTicket(TicketUIDs);
                if (labels.Content.Count() > 0)
                {
                    var rs1 = this.ChangeLabelStatus(labels.Content.Select(p => p.UID).ToArray(), LabelStatus.Inactive);
                    rs.Success = rs1.Success;
                    rs.Content = rs1.Content;
                }
                else
                {
                    rs.Success = true;
                    rs.Content = false;
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }
        public void DeleteAttachment(IEnumerable<Guid> enumerable)
        {
            var api = new HttpClientAgent(
                            this.AuthProvider.GetAuthenticationInfo().Token);
            foreach (var fileuid in enumerable)
            {
                string msg = "";
                api.DeleteAttachment(fileuid, out msg);
            }
        }
        /// <summary>
        /// 修改Label 狀態 使用前需注意該方法無視delete 狀態一律修改
        /// </summary>
        /// <param name="BelongtoUID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public IActionResult<bool> ChangeLabelStatusByBelongtoUID(IEnumerable<Guid> BelongtoUID, LabelStatus status)
        {
            return this._Repository.ChangeLabelStatusByBelongToUID(BelongtoUID, status);
        }
        /// <summary>
        /// 附加UPC/EAN 重覆性label
        /// </summary>
        /// <param name="payloaduid"></param>
        /// <returns></returns>
        public IActionResult<bool> AttachItemLabelAPI(IEnumerable<Guid> payloaduid)
        {
            using (var db = this.DbEntities.DbAdapter)
            {
                this.DbEntities.BeginTranaction(System.Data.IsolationLevel.Snapshot);
                List<string> result = new List<string>();
                var rs = ActionResultTemplates.Result<bool>();
                rs.Success = true;
                try
                {
                    var lables = this._Repository.GetLabels(payloaduid.ToArray());
                    var collection = this.PayloadRepository.GetList(new { UID = payloaduid });
                    foreach (var item in collection.Content)
                    {
                        var belonglabels = lables.Content.Where(p => p.BelongToUID == item.UID);
                        if (belonglabels.Count() == 0)
                        {
                            var labelrs = this.LabelAgent.GenerateItemLabel(item.ItemUID, item.PackageUID, item.UID);
                            rs.Success &= labelrs.Success;
                            if (!labelrs.Success)
                            {
                                result.Add(labelrs.Message);
                            }
                        }
                    }

                    if (rs.Success)
                        db.Commit();
                    else
                        rs.Message = string.Join(",", result);
                }
                catch (Exception ex)
                {
                    db.Rollback();
                    rs.Message = ex.Message;
                    rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                    rs.Success = false;
                    rs.InnerException = ex;
                }
                return rs;
            }
        }

        private ILabelRepository _Repository;
        private IWarehouseManger _WarehouseManger;
        private IWorkOrderPayloadRepository WorkOrderPayloadRepository;
        private IWorkOrderPodRepository WorkOrderPodRepository;
        private IPackageManager PackageManager;
        private IPackageUomManager PackageUomManager;
    }
}
