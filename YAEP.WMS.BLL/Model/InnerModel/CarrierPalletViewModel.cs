using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class CarrierPalletViewModel : ICarrierPalletViewModel
    {
        public CarrierPalletViewModel(ShippingService.CarrierPalletViewModel palletViewModel)
        {
            this.CarrierPalletID = palletViewModel.CarrierPalletID;
            this.CarrierPalletName = palletViewModel.CarrierPalletName;
            this.CarrierPalletStatus = palletViewModel.CarrierPalletStatus;
            this.CarrierPalletStatusName = palletViewModel.CarrierPalletStatusName;
            this.CarrierPalletUID = palletViewModel.CarrierPalletUID;
            this.CarrierTruckID = palletViewModel.CarrierTruckID;
            this.CarrierTruckName = palletViewModel.CarrierTruckName;
            this.CarrierTruckUID = palletViewModel.CarrierTruckUID;
            this.CarrierType = palletViewModel.CarrierType;
            this.CreateDate = palletViewModel.CreateDate;
            this.DepartureDate = palletViewModel.DepartureDate;
            this.Locid = palletViewModel.Locid;
            this.PackageCount = palletViewModel.PackageCount;
        }
        public string CarrierType { get; set; }
        public string CarrierTruckName { get; set; }
        public string CarrierTruckID { get; set; }
        public string CarrierPalletName { get; set; }
        public string CarrierPalletID { get; set; }
        public Guid CarrierTruckUID { get; set; }
        public Guid CarrierPalletUID { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public int CarrierPalletStatus { get; set; }
        public int PackageCount { get; set; }
        public string CarrierPalletStatusName { get; set; }
        public string Locid { get; set; }
    }
}
