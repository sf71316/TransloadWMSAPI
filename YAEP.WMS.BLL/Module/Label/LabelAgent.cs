using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces.Model;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Module
{
    internal class LabelAgent
    {
        LabelAgentInitParameter _InitParameter;
        public LabelAgent(LabelAgentInitParameter initParameter)
        {
            _InitParameter = initParameter;
        }
        public IActionResult<bool> GenerateItemLabel(Guid itemUID, Guid packageUID, Guid payloadUID)
        {
            List<dynamic> generateLables = new List<dynamic>();
            List<IActionResult<bool>> result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>(success: true);
            try
            {
                //新增UPC/EAN Label
                var itemInfo = this._InitParameter.ProductCacheManager.GetItem(itemUID) as IProductExtendModel;
                if (itemInfo != null)
                {
                    var _pkg = this._InitParameter.PackageCacheManager.GetPackage(packageUID);
                    var _pkgTree = this._InitParameter.PackageCacheManager.GetPackageTree(_pkg.UID);
                    var _pkgUOM = this._InitParameter.PackageCacheManager.GetUOM(_pkg.UOM);
                    var props = new
                    {
                        UPC = itemInfo.UPC,
                        EAN = itemInfo.EAN
                    };
                    // this._InitParameter.ItemManager.GetProperties(itemInfo.UID);
                    //SCC14 Label
                    var scc14Pkgs = this._InitParameter.PackageCacheManager.GetScc14barcde(_pkgTree.Root);
                    foreach (var pkg in scc14Pkgs)
                    {
                        dynamic label = new ExpandoObject();
                        label.labelType = LabelType.Box_SCC14;
                        label.payloadUID = payloadUID;
                        label.barcode = pkg.SCC14;
                        generateLables.Add(label);
                        //result.Add(
                        //          this._InitParameter.LabelManager.GenerateItemLabel(payloadUID, pkg.SCC14,
                        //          LabelType.Box_SCC14));
                    }
                    //
                    //PUOM
                    var puomPkgs = this._InitParameter.PackageCacheManager.GetPUOMbarcde(_pkgTree.Root);
                    foreach (var pkg in puomPkgs)
                    {
                        //result.Add(
                        //          this._InitParameter.LabelManager.GenerateItemLabel(payloadUID, pkg.PUOM,
                        //          LabelType.Item_PUOM));
                        dynamic label = new ExpandoObject();
                        label.labelType = LabelType.Item_PUOM;
                        label.payloadUID = payloadUID;
                        label.barcode = pkg.PUOM;
                        generateLables.Add(label);
                    }

                    //EAN/UPC Label
                    if (props != null)
                    {
                        var barcodeUPC = props.UPC;
                        var barcodeEAN = props.EAN;
                        if (barcodeUPC != null)
                        {
                            if (WMSAPIParameters.MIN_PACKAGE_UOM
                                .Any(x => _pkgUOM.Name.Equals(x, StringComparison.OrdinalIgnoreCase))
                                && !string.IsNullOrEmpty(barcodeUPC))
                            {
                                //result.Add(
                                //    this._InitParameter.LabelManager.GenerateItemLabel(payloadUID, barcodeUPC.Value, LabelType.Item_UPC));
                                dynamic label = new ExpandoObject();
                                label.labelType = LabelType.Item_UPC;
                                label.payloadUID = payloadUID;
                                label.barcode = barcodeUPC;
                                generateLables.Add(label);
                            }
                            else if (!string.IsNullOrEmpty(barcodeUPC))
                            {
                                //result.Add(
                                //this._InitParameter.LabelManager.GenerateItemLabel(payloadUID, barcodeUPC.Value, LabelType.Box_UPC));
                                dynamic label = new ExpandoObject();
                                label.labelType = LabelType.Box_UPC;
                                label.payloadUID = payloadUID;
                                label.barcode = barcodeUPC;
                                generateLables.Add(label);
                            }
                        }
                        else
                        {
                            //dynamic label = new ExpandoObject();
                            //label.labelType = LabelType.Item_ProductID;
                            //label.payloadUID = payloadUID;
                            //label.barcode = itemInfo.ID;
                            //generateLables.Add(label);
                        }
                        if (barcodeEAN != null)
                        {
                            if (WMSAPIParameters.MIN_PACKAGE_UOM
                                .Any(x => _pkgUOM.Name.Equals(x, StringComparison.OrdinalIgnoreCase))
                                && !string.IsNullOrEmpty(barcodeEAN))
                            {
                                //result.Add(
                                //this._InitParameter.LabelManager.GenerateItemLabel(payloadUID, barcodeEAN.Value, LabelType.Item_EAN));
                                dynamic label = new ExpandoObject();
                                label.labelType = LabelType.Item_EAN;
                                label.payloadUID = payloadUID;
                                label.barcode = barcodeEAN;
                                generateLables.Add(label);
                            }
                            else if (!string.IsNullOrEmpty(barcodeEAN))
                            {
                                //result.Add(
                                //this._InitParameter.LabelManager.GenerateItemLabel(payloadUID, barcodeEAN.Value, LabelType.Box_EAN));
                                dynamic label = new ExpandoObject();
                                label.labelType = LabelType.Box_EAN;
                                label.payloadUID = payloadUID;
                                label.barcode = barcodeEAN;
                                generateLables.Add(label);
                            }
                        }
                    }
                }
                if (generateLables.Count > 0)
                {
                    result.Add(this._InitParameter.LabelManager.GenerateItemLabel(generateLables));
                }
                if (result.Count > 0 && result.All(x => x.Success))
                {
                    rs.Success =
                    rs.Content = true;
                }
                else
                {
                    if (generateLables.Count() == 0)
                    {

                        //rs.Success =
                        //rs.Content = false;
                        //rs.Message = Resource.COMMON_GENERATE_LABEL_NOT_FIND_LABEL + " " +
                        //    string.Join(",", result.Where(x => !x.Success).Select(p => p.Message));
                        rs.Success = true;
                    }
                    else
                    {

                        rs.Success =
                                                rs.Content = false;
                        rs.Message = Resource.COMMON_GENERATE_LABEL_FAILURE + " " +
                            string.Join(",", result.Where(x => !x.Success).Select(p => p.Message));
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
        public IActionResult<bool> GenerateReceivingQtyBarcodeLabel(Guid podUID, string barcode)
        {
            List<dynamic> generateLables = new List<dynamic>();
            List<IActionResult<bool>> result = new List<IActionResult<bool>>();
            var rs = ActionResultTemplates.Result<bool>(success: true);
            try
            {
                LabelInnerModel label = new LabelInnerModel();
                label.BelongToType = LabelBelongType.Pod;
                label.Type = LabelType.Pallet_OrginalTracking;
                label.Content = barcode;
                label.BelongToUID = podUID;
                result.Add(this._InitParameter.LabelManager.AddLabels(new LabelInnerModel[] { label }));

                if (result.Count > 0 && result.All(x => x.Success))
                {
                    rs.Success =
                    rs.Content = true;
                }
                else
                {
                    rs.Success =
                                               rs.Content = false;
                    rs.Message = Resource.COMMON_GENERATE_LABEL_FAILURE + " " +
                        string.Join(",", result.Where(x => !x.Success).Select(p => p.Message));
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
    }
}
