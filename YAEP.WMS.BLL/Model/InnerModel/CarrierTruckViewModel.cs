using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class CarrierTruckViewModel : ICarrierTruckViewModel
    {

        public CarrierTruckViewModel(ShippingService.CarrierTruckViewModel item)
        {
            this.CarrierTruckID = item.CarrierTruckID;
            this.CarrierTruckName = item.CarrierTruckName;
            this.PalletCount = item.PalletCount;
            this.DepartureDate = item.DepartureDate;
            this.CarrierTruckUID = item.CarrierTruckUID;
            this.CarrierTruckStatus = item.CarrierTruckStatus;
            this.CarrierTruckStatusName = item.CarrierTruckStatusName;
            this.CarrierType = item.CarrierType;
            this.CarrierTypeUID = item.CarrierTypeUID;
        }

        public string CarrierTruckID { get; set; }
        public string CarrierTruckName { get; set; }
        public int PalletCount { get; set; }
        public DateTime? DepartureDate { get; set; }
        public Guid CarrierTruckUID { get; set; }
        public int CarrierTruckStatus { get; set; }
        public string CarrierTruckStatusName { get; set; }
        public string CarrierType { get; set; }
        public Guid CarrierTypeUID { get; set; }
    }
}
