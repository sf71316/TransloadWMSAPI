using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Interfaces;
using YAEP.WMS.BLL.Properties;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    public class BarCodeModule
    {
        ReportViewer _Reportviewer = null;
        IAppConfigure _AppConfigure;

        public BarCodeModule(IAppConfigure appConfigure)
        {
            _Reportviewer = new ReportViewer();
            this._AppConfigure = appConfigure;
        }
        public byte[] GetLabelImage(BarcodeType barcodeType, string content, string LabelText = "")
        {
            byte[] _barcodeimg = null;
            //get barcode image
            switch (barcodeType)
            {
                case BarcodeType.Pallet:
                    _barcodeimg = BarCode.GetPalletBarcode(content, LabelText);
                    break;
                case BarcodeType.Box:
                    _barcodeimg = BarCode.GetBoxBarcode(content, LabelText);
                    break;
                case BarcodeType.Item:
                    _barcodeimg = BarCode.GetItemBarcode(content, LabelText);
                    break;
                case BarcodeType.Location_Slot:
                    _barcodeimg = BarCode.GetSlotBarcode(content, LabelText);
                    break;
            }
            return _barcodeimg;
        }
        public List<BarCodeModule.Label> GeneratoreLabelPdf(BarcodeType barcodeType, dynamic[] datasource)
        {
            List<Label> Labels = new List<Label>();
            foreach (var item in datasource)
            {
                Label _label = new Label();
                _Reportviewer = new ReportViewer();

                //File.WriteAllBytes($"D:\\{DateTime.Now.ToString("yyyyMMddHHmmss")}.png", _barcodeimg);
                // convert to pdf
                if (barcodeType == BarcodeType.Pallet)
                {
                    _Reportviewer.LocalReport.ReportPath = this._AppConfigure.PalletLabelRdlcPath;
                    _Reportviewer.LocalReport.DataSources.Add(new ReportDataSource(
                   "DataSet1", new dynamic[] { item }));
                }
                else if (barcodeType == BarcodeType.Box || barcodeType == BarcodeType.Item)
                {
                    _Reportviewer.LocalReport.ReportPath = this._AppConfigure.BoxLabelRdlcPath;
                    _Reportviewer.LocalReport.DataSources.Add(new ReportDataSource(
                   "DataSet1", new dynamic[] { new { Barcode = item } }));
                }
                else if (barcodeType == BarcodeType.Location_Slot)
                {
                    _Reportviewer.LocalReport.ReportPath = this._AppConfigure.SlotLabelRdlcPath;
                    _Reportviewer.LocalReport.DataSources.Add(new ReportDataSource(
                   "DataSet1", item));
                }
                _Reportviewer.LocalReport.EnableExternalImages = true;
                _label.Pdf = _Reportviewer.LocalReport.Render("PDF", "");
                Labels.Add(_label);

            }
            return Labels;
        }
        public class Label
        {
            public string BarCode { get; set; }
            public byte[] Pdf { get; set; }
        }
    }
}
