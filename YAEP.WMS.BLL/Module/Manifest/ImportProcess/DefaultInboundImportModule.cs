using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using YAEP.Interfaces;
using CsvHelper;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Language.Resources;
using System.IO;
using CsvHelper.Configuration;
using OfficeOpenXml;
using YAEP.WMS.BLL.Model;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Constant.Enums;
using System.Collections.Concurrent;

namespace YAEP.WMS.BLL.Module
{
    internal class DefaultInboundImportModule : AbstractInboundImportModule
    {
        List<ItemInfo> ItemInfos { get; set; }
        public DefaultInboundImportModule(InboundInitImportParameters parameters) : base(parameters)
        {
            ItemInfos = new List<ItemInfo>();
        }
        public override IActionResult<bool> Execute(HttpPostedFile httpPostedFile)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                if (httpPostedFile.FileName.IndexOf(".xlsx", StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    #region importdata
                    List<string> errors = new List<string>();
                    List<DefaultInboudImportModel> importdata = new List<DefaultInboudImportModel>();
                    using (ExcelPackage ep = new ExcelPackage(httpPostedFile.InputStream))
                    {
                        ExcelWorksheet sheet = ep.Workbook.Worksheets[1];//取得Sheet1
                        int startRowNumber = sheet.Dimension.Start.Row;//起始列編號，從1算起
                        int endRowNumber = sheet.Dimension.End.Row;//結束列編號，從1算起

                        bool isHeader = true;
                        if (isHeader)//有包含標題
                        {
                            startRowNumber += 1;
                        }
                        //var importdata = reader.GetRecords<DefaultInboudImportModel>().ToList();
                        for (int currentRow = startRowNumber; currentRow <= endRowNumber; currentRow++)
                        {
                            try
                            {
                                int _pcpl, _onhand;
                                DefaultInboudImportModel e = new DefaultInboudImportModel();
                                var custid = sheet.Cells[currentRow, 1].Text;
                                var poNo = sheet.Cells[currentRow, 2].Text;
                                var syspon = sheet.Cells[currentRow, 3].Text;
                                var bolNo = sheet.Cells[currentRow, 4].Text;
                                var itemNo = sheet.Cells[currentRow, 5].Text;
                                var pcpl = int.TryParse(sheet.Cells[currentRow, 6].Text, out _pcpl);
                                var onhand = int.TryParse(sheet.Cells[currentRow, 31].Text, out _onhand);
                                e.CustId = custid;
                                e.BOL = bolNo;
                                e.ItemNo = itemNo;
                                if (pcpl)
                                {
                                    e.PC_PL = _pcpl;
                                }
                                else
                                {
                                    if (errors.Any(p => p != syspon))
                                    {
                                        errors.Add(syspon);
                                    }
                                }
                                if (onhand)
                                {
                                    e.Onhand = _onhand;
                                }
                                else
                                {
                                    if (errors.Any(p => p != syspon))
                                    {
                                        errors.Add(syspon);
                                    }
                                }
                                importdata.Add(e);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    #endregion
                    var checkResult = this.checkitem(importdata);
                    if (checkResult.Success)
                    {
                        var importProcessResult = processdata(importdata);
                    }
                    else
                    {
                        rs = checkResult;
                    }
                }
                else
                {
                    rs.Message = string.Format(Resource.COMMON_FILE_INVAILD, ".xlsx ");
                    rs.Success = false;
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
        private IActionResult<bool> checkitem(List<DefaultInboudImportModel> inbounddataCollection)
        {
            var rs = ActionResultTemplates.Result<bool>();
            List<string> notFinditem = new List<string>();
            List<string> duplicateitem = new List<string>();
            List<string> nothavepkg = new List<string>();
            var grpbyItem = inbounddataCollection.GroupBy(g => g.ItemNo);
            foreach (var item in grpbyItem)
            {
                ItemInfo i = new ItemInfo();
                var itemOriginals = this.Provider.ProductCacheManager.GetItem(item.Key,
                    new Guid[] { this.Parameters.CustomerUID }, this.Parameters.GroupUserViews);
                if (itemOriginals != null && itemOriginals.Count() > 1)//是否有重覆的Item#
                {
                    if (!duplicateitem.Any(x => x == item.Key))
                        duplicateitem.Add(item.Key);
                }
                if (itemOriginals != null && itemOriginals.Count() == 1)
                {
                    //Get Package
                    var itemOrg = itemOriginals.FirstOrDefault();
                    var pkg = FindEqualUOMPackage(itemOrg.UID, WMSAPIParameters.PALLET_UOM_KEYNAME);
                    if (pkg != null)
                    {
                        //取得最新版本全部包裝並取得該版本指定包裝
                        i.Item = itemOrg;
                        i.Package = pkg;
                        ItemInfos.Add(i);
                    }
                    else
                    {
                        if (!nothavepkg.Any(x => x == item.Key))
                            nothavepkg.Add(item.Key);
                    }

                }
                if (itemOriginals == null || (itemOriginals != null && itemOriginals.Count() == 0)) //item 是否存在
                {
                    if (!notFinditem.Any(x => x == item.Key))
                        notFinditem.Add(item.Key);
                }
            }
            if (notFinditem.Count == 0 && duplicateitem.Count == 0 && nothavepkg.Count == 0)
            {
                rs.Content = true;
            }
            else
            {
                StringBuilder msg = new StringBuilder();
                if (notFinditem.Count > 0)
                {
                    msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_FIND, string.Join(",", notFinditem)));
                }
                if (duplicateitem.Count > 0)
                {
                    msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_DUPLICATE, string.Join(",", duplicateitem)));
                }
                if (nothavepkg.Count > 0)
                {
                    msg.Append(string.Format(Resource.MANIFEST_ORDER_ITEM_NOT_PACKAGE, string.Join(",", nothavepkg)));
                }
                rs.Message = msg.ToString();
                rs.Content = false;
            }

            return rs;
        }
        private IActionResult<bool> processdata(List<DefaultInboudImportModel> inbounddataCollection)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var datagroupbyOrder = inbounddataCollection.GroupBy(g => new
            {
                g.PoNo,
                g.SysPon
            });
            foreach (var item in datagroupbyOrder)
            {
                ConcurrentStack<Func<IActionResult<bool>>> _action = new ConcurrentStack<Func<IActionResult<bool>>>();
                IManifestModel manifestModel = null;
                List<IManifestItemListModel> manifestItems = new List<IManifestItemListModel>();
                var manifestInfo = this.Provider.ManifestRepository.GetData(new
                {
                    RefNo = item.Key.SysPon,
                    PartyUID = this.Parameters.CustomerUID,
                    WarehouseUID = this.Parameters.WarehouseUID
                });
                if (manifestInfo.Content != null)//manifest exist
                {
                    manifestModel = manifestInfo.Content;
                    var mItems = this.Parameters.ManifestItemListRepository.GetManifestItemList(manifestModel.UID);
                    if (mItems.Content.Count() > 0)
                    {
                        rs.Content = false;
                        rs.Message = Resource.MANIFEST_ORDER_RECEIVING_EXISTITEM;
                        break;
                    }
                }
                #region Manifest 

                if (manifestModel == null)  //manifest unexist
                {
                    var _seq = this.Provider.SequenceAgent.GetManinfestSequence(
                        this.Provider.SequenceAgent.GetManifestRootUID(), ManifestType.Inbound);
                    // create manifest
                    ManinfestInnerModel model = new ManinfestInnerModel();
                    model.UID = Guid.NewGuid();
                    model.ID = _seq;
                    model.Name = item.Key.PoNo;
                    model.RefNo = item.Key.SysPon;
                    model.PartyUID = this.Parameters.CustomerUID;
                    model.Status = ManifestStatus.Open;
                    model.Type = (int)ManifestType.Inbound;
                    model.WarehouseUID = this.Parameters.WarehouseUID;
                    model.Volume = 0;
                    model.Weight = 0;
                    _action.Push(() => this.Provider.ManifestRepository.Add(model));
                    manifestModel = model;
                }
                #endregion
                #region  manifest item
                //create mainifest item 
                var itemCollection = item.GroupBy(p => new
                {
                    ItemNo = p.ItemNo
                });
                var _seqCol = this.Provider.SequenceAgent.GetMainfestItemListSequence(
                    this.Provider.SequenceAgent.GetManifestRootUID(), itemCollection.Count());
                foreach (var itemNo in itemCollection)
                {
                    var itemInfo = this.ItemInfos.FirstOrDefault(p => p.Item.Name == itemNo.Key.ItemNo);
                    if (itemInfo != null)
                    {
                        var _seq = _seqCol.Dequeue();
                        ManifestItemInnerModel m = new ManifestItemInnerModel();
                        m.UID = Guid.NewGuid();
                        m.ID = _seq;
                        m.ItemUID = itemInfo.Item.UID;
                        m.ManifestUID = manifestModel.UID;
                        m.PackageQty = itemNo.Sum(s => s.Onhand);
                        m.PackageUID = itemInfo.Package.UID;
                        m.Volume = 0;
                        m.Weight = 0;
                        m.Status = ManifestItemListStatus.Draft;
                        //var container = request.Container.First(p => p.UID == itemNo.First().ContainerUID);
                        //container.ManifestItemUID = m.UID;
                        manifestItems.Add(m);
                    }
                    else
                    {
                        //?
                    }
                }
                _action.Push(() => this.Parameters.ManifestItemListRepository.Add(manifestItems));
                #endregion
                var grpbybol = item.GroupBy(g => g.BOL);
                foreach (var bitem in grpbybol)
                {
                    /*
                    #region create bol
                    List<IBolModel> bolModels = new List<IBolModel>();
                    List<IVesselModel> vesselModels = new List<IVesselModel>();
                    List<IVesselManifestModel> vesselManifestModels = new List<IVesselManifestModel>();
                    var _bseq = this.Provider.SequenceAgent.GetBOLSeqence(this.Provider.SequenceAgent.GetManifestRootUID());
                    BolInnerModel bolModel = new BolInnerModel();
                    bolModel.UID = Guid.NewGuid();
                    bolModel.ID = _bseq;
                    bolModel.Name = "BOL-" + bitem.Key;
                    bolModel.RefNo = bitem.Key;
                    bolModel.ManifestUID = manifestModel.UID;
                    bolModel.Contact = "";
                    bolModel.ShipViaUID = Guid.Empty;
                    bolModel.ShipMethodUID = Guid.Empty;
                    bolModel.Status = BolStatus.Open;
                    _action.Push(() => this.Provider.BolRepository.AddBol(bolModel));
                    bolModels.Add(bolModel);
                    #endregion
                    #region create vessel

                    var _vseq =// DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    this.SequenceAgent.GetVesselSeqence(SequenceAgent.GetManifestRootUID());
                    VesselInnerModel vesselModel = new VesselInnerModel();
                    vesselModel.UID = Guid.NewGuid();
                    vesselModel.ID = _vseq;
                    vesselModel.Name = "Vessel " + request.RefNo;
                    vesselModel.RefNo = "Vessel " + request.RefNo;
                    vesselModel.Type = 1;
                    vesselModel.BolUID = bolModel.UID;
                    vesselModel.Status = (int)VesselStatus.Open;
                    _action.Push(() => this.VesselRepository.AddVessel(vesselModel));
                    vesselModels.Add(vesselModel);
                    #endregion
                    #region create vessel manifest item
                    var vesselItemSource = request.Container.SelectMany(p => p.Items).GroupBy(p => new
                    {
                        ContainerUID = p.ContainerUID,
                        ItemNo = p.Name
                    }); ;
                    var _seqVCol = this.SequenceAgent.GetVesselManifestSequence(
                       SequenceAgent.GetManifestRootUID(), itemCollection.Count());
                    foreach (var container in request.Container)
                    {
                        //List<Guid> vesselmanifestUID = new List<Guid>();
                        var _viseq = _seqVCol.Dequeue();
                        var mitemInfo = manifestItems.FirstOrDefault(p => p.UID == container.ManifestItemUID);
                        var itemInfo = items.FirstOrDefault(p => p.Item.UID == mitemInfo.ItemUID);
                        VesselManifestItemInnerModel vitem = new VesselManifestItemInnerModel();
                        vitem.UID = Guid.NewGuid();
                        vitem.ID = _viseq;
                        vitem.ItemUID = mitemInfo.ItemUID;
                        vitem.ManifestItemUID = mitemInfo.UID;
                        vitem.PartyUID = request.CustomerUID;
                        vitem.VesselUID = vesselModel.UID;
                        vitem.BolUID = bolModel.UID;
                        vitem.Status = (int)VesselManifestStatus.Open;
                        vitem.Qty = mitemInfo.PackageQty.Value;
                        vitem.Volume = this.ProductUtility.CalculateCUFT(itemInfo.Package, vitem.Qty);
                        vitem.Weight = this.ProductUtility.CaculateTTLWeight(itemInfo.Package, vitem.Qty);
                        vitem.PackageUID = mitemInfo.PackageUID;
                        vesselManifestModels.Add(vitem);
                        _action.Push(() => this.VesselManifestRepository.AddVesselManifest(vitem));
                        container.VesselManifestUID = vitem.UID;
                        //vesselmanifestUID.Add(vitem.UID);
                        foreach (var item in container.Items)
                        {
                            LabelMapping.Add(item.Barcode, vitem.UID);
                        }

                    }


                    #endregion
                    */
                }
            }
            return rs;
        }
        private IPackageModel FindEqualUOMPackage(Guid itemUID, string packageUOM)
        {

            var pkgs = this.Provider.PackageCacheManager.GetPackagesByItem(itemUID).GroupBy(grp => grp.VersionUID);
            var minuomUnique = this.Provider.PackageCacheManager.GetUomUniqueFromName(packageUOM);
            var _result = new List<IPackageModel>();
            foreach (var pkglist in pkgs)
            {
                //1.比對所有版本包裝集合是否有符合的UOM
                var cPkg = pkglist.FirstOrDefault(x => x.UOM == minuomUnique);
                _result.Add(cPkg);
            }
            return _result.OrderByDescending(o => o.CreatedOn).FirstOrDefault();
        }
    }
}
