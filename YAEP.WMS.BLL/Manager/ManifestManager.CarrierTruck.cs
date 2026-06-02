using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Manager
{
    public partial class ManifestManager : AbstractManager, IManifestManger
    {
        private ShippingService.WebService GetWebService(string timeZone = "")
        {
            string _timeZone = "";
            ShippingService.WebService webService = new ShippingService.WebService();
            if (string.IsNullOrEmpty(timeZone))
            {
                _timeZone = "Central Standard Time";
            }
            else
            {
                _timeZone = timeZone;
            }
            string shipping_service_url = this.AppConfigure.ShippingManagementWebServiceUrl + $"?TimeZoneformat={_timeZone}";
            webService.Url = shipping_service_url;
            //20220819
            //手機預設timeout 1 min 讓手機端可以吃到error message
            webService.Timeout = 80 * 1000;
            return webService;
        }
        public IActionResult<IEnumerable<ICarrierTruckViewModel>> GetCarrierTruckList(ISearchCarrierTruckParameters parameters)
        {
            List<ICarrierTruckViewModel> carrierTruckViewModels = new List<ICarrierTruckViewModel>();
            var mapping = this.Repository.GetWarehouseMapping();
            var rs = ActionResultTemplates.Result<IEnumerable<ICarrierTruckViewModel>>();
            var searchCarrierTruckParameters = new ShippingService.SearchCarrierTruckParameters();
            var webService = GetWebService(parameters.TimeZone);
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == parameters.WarehouseUID);
            if (warehouseInfo != null)
            {
                if (parameters.CarrierTruckStatus == null)
                {
                    searchCarrierTruckParameters.CarrierTruckStatus = new ShippingService.ShipCarrierTruckStatus[] {
                    ShippingService.ShipCarrierTruckStatus.Open
                    };
                }
                else
                {
                    searchCarrierTruckParameters.CarrierTruckStatus =
                                    parameters.CarrierTruckStatus.Select(p => (ShippingService.ShipCarrierTruckStatus)p).ToArray();
                }

                searchCarrierTruckParameters.EndDateTime = parameters.EndDateTime;
                searchCarrierTruckParameters.StartDateTime = parameters.StartDateTime;
                if (!(searchCarrierTruckParameters.StartDateTime.HasValue && searchCarrierTruckParameters.EndDateTime.HasValue))
                {
                    searchCarrierTruckParameters.StartDateTime = DateTime.UtcNow;
                    searchCarrierTruckParameters.EndDateTime = DateTime.UtcNow.Date;
                }

                searchCarrierTruckParameters.Locid = warehouseInfo.WarehouseID;
                try
                {

                    var collection = webService.GetCarrierTruckList(searchCarrierTruckParameters);
                    if (collection.Count() > 0)
                    {
                        foreach (var item in collection)
                        {
                            var e = new CarrierTruckViewModel(item);
                            carrierTruckViewModels.Add(e);
                        }
                    }
                    rs.Content = carrierTruckViewModels;
                    rs.Success = true;
                }
                catch (Exception ex)
                {
                    rs.Success = false;
                    rs.Message = ex.Message;
                }
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse";
            }
            return rs;
        }

        public IActionResult<bool> AddCarrierTruck(IAddCarrierTruckDTO request)
        {
            var mapping = this.Repository.GetWarehouseMapping();
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == request.WarehouseUID);
            if (warehouseInfo != null)
            {
                var dto = new ShippingService.AddCarrierTruckDTO();
                dto.BatchCount = request.BatchCount;
                dto.CarrierType = request.CarrierType;
                dto.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                dto.Locid = warehouseInfo.WarehouseID;
                dto.TruckName = request.TruckName;
                var wrs = service.AddCarrierTruck(dto);

                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse";
            }
            return rs;
        }
        public IActionResult<bool> AddShipCarrierPallet(IAddCarrierPalletDTO request)
        {
            var mapping = this.Repository.GetWarehouseMapping();
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == request.WarehouseUID);
            if (warehouseInfo != null)
            {
                var dto = new ShippingService.AddCarrierPalletDTO();
                dto.BatchCount = request.BatchCount;
                dto.CarrierType = request.CarrierType;
                dto.CreatedBy = this.AuthProvider.GetAuthenticationInfo().Account;
                dto.Locid = warehouseInfo.WarehouseID;
                dto.PalletName = request.PalletName;
                var wrs = service.AddShipCarrierPallet(dto);

                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse";
            }
            return rs;
        }
        public IActionResult<IEnumerable<IShipCarrierCategory>> GetShipCarrierCategories()
        {
            var rs = ActionResultTemplates.Result<IEnumerable<IShipCarrierCategory>>();
            var service = this.GetWebService();
            var result = new List<ShipCarrierCategory>();
            var wrep = service.GetShipCarrierCategories();
            rs.Content = result;
            rs.Success = true;
            foreach (var item in wrep)
            {
                var e = new ShipCarrierCategory();
                e.ID = item.ID;
                e.Name = item.Name;
                e.UID = item.UID;
                result.Add(e);
            }
            return rs;
        }
        public IActionResult<bool> DeleteCarrierTruck(IEnumerable<Guid> carrierTruckUIds)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            if (carrierTruckUIds != null && carrierTruckUIds.Count() > 0)
            {
                var wrs = service.DeleteCarrierTruck(carrierTruckUIds.ToArray(), this.AuthProvider.GetAuthenticationInfo().Account);
                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Message = "Not find UID";
            }
            return rs;
        }
        public IActionResult<bool> AssignedPalletToTruck(IAssignedPalletToTruckRequest request)
        {
            var mapping = this.Repository.GetWarehouseMapping();
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == request.WarehouseUID);
            if (string.IsNullOrEmpty(request.CarrierTruckName))
            {
                rs.Message = "Truck name can't empty.";
                return rs;
            }
            if (request.carrierPalletIDs == null)
            {
                rs.Message = "Pallet id must have.";
                return rs;
            }
            if (warehouseInfo != null)
            {
                var wrs = service.AssignedPalletToTruck(request.CarrierTruckName, request.CarrierTypeUID, request.carrierPalletIDs.ToArray(),
                     warehouseInfo.WarehouseID, this.AuthProvider.GetAuthenticationInfo().Account);
                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse";
            }
            return rs;
        }
        public IActionResult<bool> CarrierTruckDepartured(List<Guid> carrierTruckUID)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            try
            {
                var wrs = service.CarrierTruckDepartured(carrierTruckUID.ToArray(), this.AuthProvider.GetAuthenticationInfo().Account);
                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
            }
            return rs;
        }
        public IActionResult<bool> ChangePalletToOtherTruck(IChangePalletToOtherTruckRequest request)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var mapping = this.Repository.GetWarehouseMapping();
            var service = this.GetWebService();
            if (request.CarrierTypeUID == null)
            {
                rs.Message = "Must select one truck.";
                return rs;
            }
            if (request.CarrierPalletUIDs == null)
            {
                rs.Message = "Must select one pallet.";
                return rs;
            }
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == request.WarehouseUID);
            if (warehouseInfo != null)
            {
                var wrs = service.ChangePalletToOtherTruck(request.CarrierTruckName, request.CarrierTypeUID,
                request.CarrierPalletUIDs.ToArray(), warehouseInfo.WarehouseID,
                this.AuthProvider.GetAuthenticationInfo().Account);
                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse";
            }


            return rs;
        }
        public IActionResult<bool> RemovePalletFromTruck(List<Guid> carrierPalletUIDs)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            var wrs = service.RemovePalletFromTruck(carrierPalletUIDs.ToArray(),
                this.AuthProvider.GetAuthenticationInfo().Account);
            rs.Content = rs.Success = wrs.Success;
            rs.Message = wrs.Message;
            return rs;
        }
        public IActionResult<IEnumerable<ICarrierPalletViewModel>> GetCarrierPallets(ISearchCarrierPalletParameters parameters)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<ICarrierPalletViewModel>>();
            var carrierPalletViews = new List<ICarrierPalletViewModel>();
            var webService = this.GetWebService();
            var mapping = this.Repository.GetWarehouseMapping();
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == parameters.WarehouseUID);
            if (warehouseInfo != null)
            {
                var scpp = new ShippingService.SearchCarrierPalletParameters();
                scpp.StartDateTime = parameters.StartDateTime;
                scpp.EndDateTime = parameters.EndDateTime;
                scpp.CarrierType = parameters.CarrierType;
                scpp.CarrierTruckUID = parameters.CarrierTruckUID;
                scpp.CarrierTruckID = parameters.CarrierTruckID;
                scpp.locid = warehouseInfo.WarehouseID;
                //要顯示沒有assgine truck 的pallet
                //if (parameters.CarrierTruckStatus == null)
                //{
                //    scpp.CarrierTruckStatus = new ShippingService.ShipCarrierTruckStatus[] { ShippingService.ShipCarrierTruckStatus.Open };
                //}
                //else
                //{
                //    scpp.CarrierTruckStatus = parameters.CarrierTruckStatus.Where(p => p.HasValue).Select(p => (ShippingService.ShipCarrierTruckStatus)p).ToArray();
                //}
                if (parameters.CarrierPalletStatus == null)
                {
                    scpp.CarrierPalletStatus = new ShippingService.ShipCarrierPalletStatus[] { ShippingService.ShipCarrierPalletStatus.Open };
                }
                else
                {
                    scpp.CarrierPalletStatus = parameters.CarrierPalletStatus.Where(p => p.HasValue).Select(p => (ShippingService.ShipCarrierPalletStatus)p).ToArray();
                }
                try
                {

                    var collection = webService.GetCarrierPallets(scpp);
                    if (collection.Count() > 0)
                    {
                        foreach (var item in collection)
                        {
                            var e = new CarrierPalletViewModel(item);
                            carrierPalletViews.Add(e);
                        }
                    }
                    rs.Content = carrierPalletViews;
                    rs.Success = true;
                }
                catch (Exception ex)
                {
                    rs.Success = false;
                    rs.Message = ex.Message;
                }
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse";
            }
            return rs;
        }
        public IActionResult<bool> AssignedPackageToPallet(IAssignedPackageToPalletRequest request)
        {
            var mapping = this.Repository.GetWarehouseMapping();
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == request.WarehouseUID);
            if (warehouseInfo != null)
            {
                var wrs = service.AssignedPackageToPallet(request.CarrierPalletID, request.CarrierPackageid.ToArray(),
              warehouseInfo.WarehouseID, this.AuthProvider.GetAuthenticationInfo().Account);
                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse.";
            }
            return rs;
        }
        public IActionResult<bool> DeleteCarrierPallet(List<Guid> carrierPalletUIds)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            if (carrierPalletUIds != null)
            {
                var wrs = service.DeleteCarrierPallet(carrierPalletUIds.ToArray(), this.AuthProvider.GetAuthenticationInfo().Account);
                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Message = "Must selcet one pallet.";
            }
            return rs;
        }
        public IActionResult<bool> ChangePackageToOtherPallet(IChangePackageToOtherPalletRequest request)
        {
            var mapping = this.Repository.GetWarehouseMapping();
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            var warehouseInfo = mapping.Content.FirstOrDefault(x => x.WarehouseUID == request.WarehouseUID);
            if (request.CarrierPalletInfoUIDs == null)
            {
                rs.Success = false;
                rs.Message = "Must have select package";
                return rs;
            }
            if (string.IsNullOrEmpty(request.CarrierPalletID))
            {
                rs.Success = false;
                rs.Message = "Must scan pallet";
                return rs;
            }
            if (warehouseInfo != null)
            {
                var wrs = service.ChangePackageToOtherPallet(request.CarrierPalletID, request.CarrierPalletInfoUIDs.ToArray()
               , warehouseInfo.WarehouseID, this.AuthProvider.GetAuthenticationInfo().Account);
                rs.Content = rs.Success = wrs.Success;
                rs.Message = wrs.Message;
            }
            else
            {
                rs.Success = false;
                rs.Message = "Not find warehouse";
            }

            return rs;
        }
        public IActionResult<bool> RemovePackageFromPallet(List<Guid> palletinfoUIDs)
        {
            var rs = ActionResultTemplates.Result<bool>();
            var service = this.GetWebService();
            var wrs = service.RemovePackageFromPallet(palletinfoUIDs.ToArray(), this.AuthProvider.GetAuthenticationInfo().Account);
            rs.Content = rs.Success = wrs.Success;
            rs.Message = wrs.Message;
            return rs;
        }
        public IActionResult<IEnumerable<IShipCarrierPalletInfo>> GetCarrierPalletInfos(ISearchCarrierPalletInfoParameters parameters)
        {
            var service = this.GetWebService();
            var rs = ActionResultTemplates.Result<IEnumerable<IShipCarrierPalletInfo>>();
            var result = new List<IShipCarrierPalletInfo>();
            var param = new ShippingService.SearchCarrierPalletInfoParameters();
            if (parameters.CarrierPalletUIDs != null)
            {
                param.CarrierPalletUIDs = parameters.CarrierPalletUIDs.ToArray();
            }
            if (parameters.Syspon != null)
            {
                param.Syspon = parameters.Syspon.ToArray();
            }
            if (parameters.CarrierPalletInfoUIDs != null)
            {
                param.UIDs = parameters.CarrierPalletInfoUIDs.ToArray();
            }
            if (parameters.TrackingNo != null)
            {
                param.TrackingNo = parameters.TrackingNo.ToArray();
            }
            var wrs = service.GetCarrierPalletInfos(param);
            rs.Success = true;
            foreach (var item in wrs)
            {
                var e = new ShipCarrierPalletInfo();
                e.BolNo = item.BolNo;
                e.CarrierPalletUID = item.CarrierPalletUID;
                e.CreatedBy = item.CreatedBy;
                e.CreatedOn = item.CreatedOn;
                e.CustID = item.CustID;
                e.CustPON = item.CustPON;
                e.ModifiedBy = item.ModifiedBy;
                e.ModifiedOn = item.ModifiedOn;
                e.Name = item.Name;
                e.ShipviaUID = item.ShipviaUID;
                e.Status = item.Status;
                e.StatusName = item.StatusName;
                e.Syspon = item.Syspon;
                e.TrackingNo = item.TrackingNo;
                e.UID = item.UID;
                result.Add(e);
            }
            rs.Content = result;
            return rs;
        }
    }
}
