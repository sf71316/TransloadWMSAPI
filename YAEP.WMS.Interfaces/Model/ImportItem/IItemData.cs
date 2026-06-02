using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
     public interface IItemData
    {
         Guid UID { get; set; }
         string ID { get; set; }
         string Name { get; set; }
         string CategoryName { get; set; }
         string UPC { get; set; }
         string EAN { get; set; }
         bool IsBOM { get; set; }
         string Description { get; set; }
         decimal? LengthInch { get; set; }
         decimal? WidthInch { get; set; }
         decimal? HeightInch { get; set; }
         decimal? LengthCM { get; set; }
         decimal? WidthCM { get; set; }
         decimal? HeightCM { get; set; }
         decimal? NetWeightKG { get; set; }
         decimal? NetWeightLB { get; set; }
         decimal? GrossWeightKG { get; set; }
         decimal? GrossWeightLB { get; set; }
         string StockUOM { get; set; }
         string PurchaseUOM { get; set; }
         string SellingUOM { get; set; }
         string ShipUOM { get; set; }

        // Virtual Item 特有欄位
         string ActualProduct { get; set; }
         bool Is_VirtualItem { get; set; }
         string PUOM { get; set; }
         int BoxQuantity { get; set; }
    }
}
