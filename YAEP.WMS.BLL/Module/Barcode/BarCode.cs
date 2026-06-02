using BarcodeLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL
{
    public static class BarCode
    {
        public static byte[] GetBarcode(string content, float fontsize, int? width = null, int? height = null
            , bool hasLabel = false, string labelText = "", BarcodeLib.TYPE bType = BarcodeLib.TYPE.CODE128,
            LabelPositions labelPosition = LabelPositions.BOTTOMCENTER)
        {
            if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(content.Trim()))
            {
                //System.Drawing.Font font = new System.Drawing.Font("verdana", fontsize);
                var barcode = new BarcodeLib.Barcode(content, bType);
                barcode.BackColor = Color.White;
                barcode.ForeColor = Color.Black;
                barcode.ImageFormat = ImageFormat.Png;
                if (hasLabel)
                {
                    barcode.IncludeLabel = true;
                    if (!string.IsNullOrEmpty(labelText))
                        barcode.AlternateLabel = labelText;
                    barcode.LabelFont = new System.Drawing.Font("verdana", fontsize);
                    barcode.LabelPosition = labelPosition;
                }
                if (height.HasValue)
                {
                    barcode.Height = height.Value;
                    if (hasLabel)
                        barcode.Height += barcode.LabelFont.Height;
                }
                else
                {
                    //barcode.Height = font.Height;
                    //if (hasLabel)
                    //    barcode.Height += barcode.LabelFont.Height;
                    //barcode.Height = Convert.ToInt32(barcode.Height * 1.7);
                }
                if (width.HasValue)
                    barcode.Width = width.Value;
                else
                {
                    //var lineSpacing = font.FontFamily.GetLineSpacing(FontStyle.Regular);
                    //barcode.Width = Convert.ToInt32(font.Size * lineSpacing / font.FontFamily.GetEmHeight(FontStyle.Regular) * content.Length);
                    //barcode.Width = Convert.ToInt32(barcode.Width * 0.7);
                }
                return ImageToBytes(barcode.Encode(bType, content));
            }
            else
            {
                return new byte[] { };
            }
        }
        public static byte[] GetPalletBarcode(string content, string LabelText = "")
        {
            if (string.IsNullOrEmpty(LabelText))
                LabelText = content;
            return GetBarcode(content, 30, 700, 220, labelText: LabelText, hasLabel: true);
        }
        public static byte[] GetBoxBarcode(string content, string LabelText = "")
        {
            if (string.IsNullOrEmpty(LabelText))
                LabelText = content;
            return GetBarcode(content, 18, 350, 90, bType: TYPE.CODE128, labelText: LabelText, hasLabel: true);
        }
        public static byte[] GetItemBarcode(string content, string LabelText = "")
        {
            if (string.IsNullOrEmpty(LabelText))
                LabelText = content;
            return GetBarcode(content, 18, 350, 90, bType: TYPE.CODE128, labelText: LabelText, hasLabel: true);
        }
        public static byte[] GetSlotBarcode(string content, string LabelText = "")
        {
            if (string.IsNullOrEmpty(LabelText))
                LabelText = content;
            return GetBarcode(content, 30, 705, 220, labelText: LabelText, hasLabel: true);
        }
        private static byte[] ImageToBytes(Image x)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte;
        }
    }
}
